using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum PointTypes{fly, boost, leaf, straight, biased, tension, normal}
//fly points enable flying
//boost add additional boostAmount. tension = ?
//leaves cannot be connected to
//end points force new spline creation
//straight continuity = 0;
//biased bias = ?

public class Point : MonoBehaviour
{

	public PointTypes pointType = PointTypes.normal;

	public Vector3 Pos
	{
		get
		{
			return transform.position;
		}
	}


	public static float hitColorLerp;
	public bool visited = false;

	public static int pointCount = 0;
	public string text;
	public float tension;
	public float bias;
	public float continuity;
	public bool isKinematic;

	public float damping = 600f;
	public float stiffness = 100f;
	public float mass = 50f;
	public float boostCooldown;

	public GameObject activatedSprite;
	public GameObject directionalSprite;

	public List<Point> _neighbours;
	public List<Spline> _connectedSplines;

	public Color color;
	public float timeOffset;
	public float proximity = 0;
	public bool locked = false;

	public static Point Select;
	private float cooldown;
	private SpriteRenderer SR;
	private List<GameObject> _directionalSprites;
	public float c = 0;
	public bool hit = false;
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

	public Vector3 originalPos;
	FadeSprite activationSprite;

	void Awake(){

		stiffness = 1600;
		damping = 500;
		color = Color.black;
		Point.pointCount++;
		activationSprite = GetComponentInChildren<FadeSprite> ();
		timeOffset = Point.pointCount;
		_directionalSprites = new List<GameObject> ();

		if (_neighbours.Count == 0) {
			_neighbours = new List<Point> ();
		}
		if (_connectedSplines.Count == 0) {
			_connectedSplines = new List<Spline> ();
		}

		cooldown = (((float)Point.pointCount) % boostCooldown)/3f;
		SR = GetComponent<SpriteRenderer> ();

		originalPos = transform.position;

		c = 0;
	}
//	void OnMouseDown()
//	{
//		Select=this;
//		screenPoint=CameraControler.MainCamera.WorldToScreenPoint(transform.position);
//	}
//	void OnMouseDrag()
//	{
//		Vector3 curentScreenPoint=new Vector3(Input.mousePosition.x,Input.mousePosition.y,screenPoint.z);
//		Vector3 curentPos=CameraControler.MainCamera.ScreenToWorldPoint(curentScreenPoint);
//		transform.position=curentPos;
//
//	}


	//HELPER FUNCTIONS

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

			case PointTypes.biased:
				SR.sprite = Services.Prefabs.pointSprites[(int)PointTypes.biased];
				bias = 1;
			break;

			case PointTypes.tension:
				tension = -1;
				SR.sprite = Services.Prefabs.pointSprites[(int)PointTypes.tension];
			break;

			default:
			break;
		}
	}

	public void Update(){

//		if (hit) {
//			activationSprite.time = Mathf.Lerp (activationSprite.time, 1, Time.deltaTime * 5);
//		} else {
//			activationSprite.time = Mathf.Lerp (activationSprite.time, 0, Time.deltaTime * 2);
//		}

		c = (Mathf.Sin (Time.time + timeOffset)/2 + 0.6f)/10f + proximity;

		c = Mathf.Pow (c, 1);

		if (!visited) {
//			SR.color = Color.white;
			c = 1;
		} else {

			if(!isKinematic){
				Movement();
			}
//			if (hit) {
//				color = Color.Lerp (color, Color.white, Time.deltaTime * 5);
//				SR.color = color;
//			} else {
				color = Color.Lerp (color, new Color (c, c, c), Time.deltaTime * 5);
//				SR.color = Color.Lerp (SR.color, Color.black, Time.deltaTime * 5);
				SR.color = color * 10;
//			}
		}

//		l.SetPosition (0, transform.position);
//		l.SetPosition (1, GetComponent<SpringJoint>().connectedBody.transform.position);

//		if (_neighbours.Count > 2) {
//			SetDirectionalArrows ();
//		}
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

	public void SetDirectionalArrows(){
		int index = 0;

		foreach (Spline s in _connectedSplines) {
			foreach (Point p in _neighbours) {

				if (!p._connectedSplines.Contains (s)) {
					//do nothing if the point is in another spline
				} else {
					if (index > _directionalSprites.Count - 1) {
						GameObject newSprite = (GameObject)Instantiate (directionalSprite, Vector3.zero, Quaternion.identity);
						newSprite.transform.parent = transform;
						_directionalSprites.Add (newSprite);
					}
					SetPosAndVelocity (_directionalSprites [index], 0, s, p);
					float cc = c + Mathf.Clamp01 (cooldown);
					_directionalSprites[index].GetComponent<SpriteRenderer>().color =  new Color (cc,cc,cc);
					index++;
				}
			}
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

	public void SetPosAndVelocity(GameObject g, float t, Spline s, Point p){
		int indexdiff = s.SplinePoints.IndexOf (p) - s.SplinePoints.IndexOf (this);
		int index = 0;

		if (indexdiff == -1 || indexdiff > 1) {
			index = s.SplinePoints.IndexOf (p);
			t = 1 - t;
			g.transform.up = -s.GetVelocityAtIndex (index, t);
		} else {
			index = s.SplinePoints.IndexOf (this);
			g.transform.up = s.GetVelocityAtIndex (index, t);
		}
		g.transform.position = s.GetPointAtIndex (index, t);

	}

	public float NeighbourCount(){
		return _connectedSplines.Count;
	}

	public void RemoveSpline(Spline s){
		_connectedSplines.Remove (s);
	}

	public void RemoveNode(Point p){
		_neighbours.Remove (p);
	}

	public void OnPointEnter(){

//		continuity = Mathf.Clamp(continuity + 0.01f, 0, 1);

		if (!visited && GetComponentInParent<PointCloud>() != null) {
			PointCloud p = GetComponentInParent<PointCloud> ();
			GameObject newText = (GameObject)Instantiate (Services.Prefabs.spawnedText, transform.position - Vector3.forward/2f + Vector3.up/8f, Quaternion.identity);
			newText.GetComponent<TextMesh>().text = p.GetWord ();
			newText.GetComponent<FadeTextOnPoint>().p = this;
			newText.transform.parent = transform;
		}

		PutOnCooldown ();
		// GameObject fx = Instantiate (activatedSprite, transform.position, Quaternion.identity);
		// fx.transform.parent = transform;
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

	public bool isConnectedTo(Point p){
		if (_neighbours.Contains (p)) {
			return true;
		}
		return false;
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
		}

	}

	public List<Point> GetNeighbours(){
		return _neighbours;
	}

	public void DestroySplines(){
		foreach (Spline s in _connectedSplines) {
//			s.GetVert1 ().RemoveNode (s.GetVert2 ());
//			s.GetVert2 ().RemoveNode (s.GetVert1 ());
//			s.GetVert1 ().RemoveSpline (e);
//			s.GetVert2 ().RemoveSpline (e);
			Destroy (s.gameObject);
			Destroy (this);
		}
		//		foreach (Spline s in _connectedSplines) {
		//			s.GetComponent<SplineDecorator>().DestroySpline (this);
		//		}
	}


}
