using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum PointTypes{normal, fly, boost, leaf, straight}
//fly points enable flying
//boost add additional boostAmount. tension = ?
//leaves cannot be connected to
//end points force new spline creation
//straight continuity = 0;
//biased bias = ?
public class Point : MonoBehaviour
{

	#region

	public bool visited = false;
	public PointTypes pointType = PointTypes.normal;
	[Space(10)]

	public List<Point> _neighbours;
	public List<Spline> _connectedSplines;

	public static float hitColorLerp;
	public static int pointCount = 0;

	[Space(10)]
	[Header("Curve")]
	public float tension;
	public float bias;
	public float continuity;
	[Space(10)]

	[HideInInspector]
	public bool isKinematic;
	public static float damping = 600f;
	public static float stiffness = 100f;
	public static float mass = 50f;
	[HideInInspector]
	public Vector3 originalPos;
	[Space(10)]

	[HideInInspector]
	public string text;
	[HideInInspector]
	public PointCloud pointCloud;
	[HideInInspector]
	public bool hasPointcloud;
	[Space(10)]

	[Header("Interaction")]
	public bool hit = false;
	public float lockAmount;
	private float cooldown;
	[HideInInspector]
	public float timeOffset;
	[HideInInspector]
	public float proximity = 0;
	[HideInInspector]
	public bool locked = false;
	[HideInInspector]
	public Color color;
	[HideInInspector]
	public float c = 0;
	public static Point Select;
	private FadeSprite activationSprite;
	private SpriteRenderer SR;

	public bool isSelect
	{
		get
		{
			return this==Select;
		}
	}
	private Vector3 _velocity;

	public Vector3 velocity
	{
		set
		{
			_velocity = value;
		}
	}

	public Vector3 Pos
	{
		get
		{
			return transform.position;
		}
	}
	#endregion

	void Awake(){
		stiffness = 1600;
		damping = 500;
		color = Color.black;
		Point.pointCount++;
		activationSprite = GetComponentInChildren<FadeSprite> ();
		timeOffset = Point.pointCount;

		if (_neighbours.Count == 0) {
			_neighbours = new List<Point> ();
		}

		if (_connectedSplines.Count == 0) {
			_connectedSplines = new List<Spline> ();
		}

		c = 0;
		cooldown = 0;
		SR = GetComponent<SpriteRenderer> ();
		originalPos = transform.position;
	}

	public void Start(){

		switch(pointType){

			case PointTypes.fly:
				SR.sprite = Services.Prefabs.pointSprites[(int)PointTypes.fly];
			break;

			case PointTypes.boost:
				SR.sprite = Services.Prefabs.pointSprites[(int)PointTypes.boost];
			break;

			case PointTypes.leaf:
				SR.sprite = Services.Prefabs.pointSprites[(int)PointTypes.leaf];
			break;

			case PointTypes.straight:
				SR.sprite = Services.Prefabs.pointSprites[(int)PointTypes.straight];
				tension = 1;
			break;

			default:
			break;
		}

		if(locked){
			SR.sprite = Services.Prefabs.pointSprites[1];
		}
	}

	public void Update(){

			SetSprite();
			SetColor();

			if(!isKinematic){
				Movement();
			}
	}

	void Movement(){
		Vector3 stretch = transform.position - originalPos;
		Vector3 force = -stiffness * stretch - damping * _velocity;
		Vector3 acceleration = force / mass;

		_velocity += acceleration * Time.deltaTime;
		transform.position += _velocity * Time.deltaTime;
	}

	public void AddSpline(Spline s){
		if (!_connectedSplines.Contains (s)) {
			_connectedSplines.Add (s);
		}else{
			Debug.Log("trying to add a spline twice. DONT DO THAT");
		}
	}

	public void AddPoint(Point p){
		if (!_neighbours.Contains (p)) {
			_neighbours.Add (p);
		}else{
			Debug.Log("trying to add a point twice. DONT DO THAT");
		}
	}

	public void ResetCooldown(){

	}

	public bool isUnlocked(){
		 if(!locked || PointManager._pointsHit.Count >= lockAmount){
			locked = false;
			return true;
		}else{
			return false;
		}
	}

	public float NeighbourCount(){
		return _connectedSplines.Count;
	}

	public void RemoveSpline(Spline s){
		_connectedSplines.Remove (s);
	}

	public void RemovePoint(Point p){
		_neighbours.Remove (p);
	}

	public void OnPointEnter(){
		if (!visited) {

			if(hasPointcloud && pointCloud.text != null){
			GameObject newText = (GameObject)Instantiate (Services.Prefabs.spawnedText, transform.position - Vector3.forward/5f + Vector3.up/10f, Quaternion.identity);
			newText.GetComponent<TextMesh>().text = pointCloud.GetWord();
			newText.GetComponent<FadeTextOnPoint>().p = this;
			newText.transform.parent = transform;
			}
		}

		PutOnCooldown ();
	}

	public void OnPointExit(){

	}

	public bool HasSplines(){
		return _connectedSplines.Count > 0 ? true : false;
	}

	public Spline GetConnectingSpline(Point p){
		foreach (Spline s in _connectedSplines) {
			if (s.IsPointConnectedTo(p))
				return s;
		}
		return null;
	}

	public bool IsAdjacent(Point n){
		return _neighbours.Contains (n);
	}

	public List<Spline> GetSplines(){
		return _connectedSplines;
	}

	public bool IsOffCooldown(){
		return !hit;
	}

	public float GetCooldown(){
		return cooldown;
	}

	public void PutOnCooldown(){

		if (!visited) {
			visited = true;
			PointManager._connectedPoints.Add (this);
		}

		if (!hit) {
			PointManager.AddPointHit (this);
			hit = true;
			GameObject fx = Instantiate (Services.Prefabs.circleEffect, transform.position, Quaternion.identity);
			fx.transform.parent = transform;
		}

	}

	public List<Point> GetNeighbours(){
		return _neighbours;
	}

	void SetColor(){

		c = (Mathf.Sin (3 * (Time.time + timeOffset))/2 + 0.6f) + proximity;
		c = Mathf.Pow (c, 1);

		if (hit) {
			color = Color.Lerp (SR.color, Color.white * 2, Time.deltaTime * 5);
			SR.color = color;
		} else {
			SR.color = Color.Lerp (SR.color, new Color (c, c, c), Time.deltaTime * 5);
		}
	}

	void SetSprite(){
		if(locked && PointManager._pointsHit.Count < lockAmount){
			SR.sprite = Services.Prefabs.pointSprites[1];
		}else{
			SR.sprite = Services.Prefabs.pointSprites[0];
		}
	}

	// public void SetDirectionalArrows(){
	// 	int index = 0;
	//
	// 	foreach (Spline s in _connectedSplines) {
	// 		foreach (Point p in _neighbours) {
	//
	// 			if (!p._connectedSplines.Contains (s)) {
	// 				//do nothing if the point is in another spline
	// 			} else {
	// 				if (index > _directionalSprites.Count - 1) {
	// 					GameObject newSprite = (GameObject)Instantiate (directionalSprite, Vector3.zero, Quaternion.identity);
	// 					newSprite.transform.parent = transform;
	// 					_directionalSprites.Add (newSprite);
	// 				}
	// 				SetPosAndVelocity (_directionalSprites [index], 0, s, p);
	// 				float cc = c + Mathf.Clamp01 (cooldown);
	// 				_directionalSprites[index].GetComponent<SpriteRenderer>().color =  new Color (cc,cc,cc);
	// 				index++;
	// 			}
	// 		}
	// 	}
	// }
}
