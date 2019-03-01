using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum PointTypes{normal, fly, boost}
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

	[HideInInspector]
	public List<Point> _neighbours;
	[HideInInspector]
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
	public static float damping = 1000f;
	public static float stiffness = 1000f;
	public static float mass = 50f;
	[HideInInspector]
	public Vector3 originalPos;
	[Space(10)]

	[HideInInspector]
	public string text;
	private TextMesh textMesh;
	[HideInInspector]
	public List<PointCloud> pointClouds;
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
		pointClouds = new List<PointCloud>();
		stiffness = 1600;
		damping = 500;
		color = Color.white/10;
		Point.pointCount++;
		activationSprite = GetComponentInChildren<FadeSprite> ();
		timeOffset = Point.pointCount;

		_neighbours = new List<Point> ();
		_connectedSplines = new List<Spline> ();

		c = 0;
		cooldown = 0;
		SR = GetComponent<SpriteRenderer> ();
		SR.color = Color.black;
		originalPos = transform.position;
	}

	public void Start(){

		text = gameObject.name;
		switch(pointType){

			case PointTypes.fly:
				SR.sprite = Services.Prefabs.pointSprites[(int)PointTypes.fly];
			break;

			case PointTypes.boost:
				SR.sprite = Services.Prefabs.pointSprites[(int)PointTypes.boost];
			break;

			default:
			break;
		}

		if(locked){
			SR.sprite = Services.Prefabs.pointSprites[1];
		}
	}

	public void Update(){
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

	public void Reset(){
		for (int i = 0; i < _connectedSplines.Count; i++){
			RemoveSpline(_connectedSplines[i]);
		}
		for (int i = 0; i < _neighbours.Count; i++){
			RemovePoint(_neighbours[i]);
		}
	}

	public void RemoveSpline(Spline s){
		_connectedSplines.Remove (s);
	}

	public void RemovePoint(Point p){
		_neighbours.Remove (p);
	}

	public void OnPointEnter(){
		color += Color.white/5;
		SR.color = color;

		if(textMesh != null){
			textMesh.GetComponent<FadeTextOnPoint>().alpha = 1;
		}

		if (!visited) {
			visited = true;

			GameObject newText = (GameObject)Instantiate (Services.Prefabs.spawnedText, transform.position - Vector3.forward/5f + Vector3.up/10f, Quaternion.identity);
			textMesh = newText.GetComponent<TextMesh>();
			textMesh.text = text;
			newText.GetComponent<FadeTextOnPoint>().p = this;
			newText.transform.parent = transform;
		}

		if(hasPointcloud){
			foreach(PointCloud p in pointClouds){
			p._pointshit.Add(this);
			PointManager._connectedPoints.Add (this);
			p.CheckCompleteness();
		 }
		}

		if (!hit) {
			PointManager.AddPointHit (this);
			hit = true;
			GameObject fx = Instantiate (Services.Prefabs.circleEffect, transform.position, Quaternion.identity);
			fx.transform.parent = transform;
		}
	}

	public void OnPointExit(){

		switch(pointType){
			case PointTypes.boost:
				// Services.PlayerBehaviour.boost += 1;
				Services.PlayerBehaviour.flow += 0.1f;
				Services.PlayerBehaviour.boost = Services.PlayerBehaviour.boostAmount;
			break;

			case PointTypes.fly:
				Services.PlayerBehaviour.boost = Services.PlayerBehaviour.boostAmount;
			break;

			case PointTypes.normal:
				if(!hit){
					Services.PlayerBehaviour.boost = Services.PlayerBehaviour.boostAmount;
				}
			break;

		}

		/*
			if(curPoint.IsOffCooldown()){
			// flow += flowAmount;
			}
			if(Mathf.Abs(flow) < 1){
				boost = boostAmount;
			}
		*/
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

	public List<Point> GetNeighbours(){
		return _neighbours;
	}

	void SetColor(){

		if(visited){
		// c = (Mathf.Sin (3 * (Time.time + timeOffset))/4 + 0.3f) + proximity;
		c = proximity + 0.1f;
		c = Mathf.Pow (c, 1);
		color = new Color(c, c, c);
		SR.color = Color.Lerp (color, new Color (c, c, c), Time.deltaTime * 5);
	}else{
		color = new Color(1, 1, 1, proximity);
		SR.color = color;
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
