using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class Point : MonoBehaviour
{
	public Vector3 Pos
	{
		get
		{
			return transform.position;
		}
	}

	public static float hitColorLerp;

	public static int pointCount = 0;
	public Rigidbody rb;
	public string text;
	public float tension;
	public float bias;
	public float continuity;

	public float boostCooldown;

	public GameObject activatedSprite;
	public GameObject directionalSprite;

	public List<Point> _neighbours;
	public List<Spline> _connectedSplines;

	public bool isPlaced = false;
	public Color color;
	public float timeOffset;
	public float proximity = 0;
	public bool locked = false; 

	public static Point Select;
	private float cooldown;
	private SpriteRenderer SR;
	private LineRenderer l;
	private List<GameObject> _directionalSprites;
	public float c = 0;
	public bool hit;
	public bool isSelect
	{
		get
		{
			return this==Select;
		}
	}




	void Awake(){
		Point.pointCount++;
		timeOffset = Point.pointCount * 0.1f;
		gameObject.name = "v" + Point.pointCount;
		_directionalSprites = new List<GameObject> ();
		color = new Color (1, 1, 1, 1);
		rb = GetComponent<Rigidbody> ();
		if (_neighbours.Count == 0) {
			_neighbours = new List<Point> ();
		}
		if (_connectedSplines.Count == 0) {
			_connectedSplines = new List<Spline> ();
		}

		_connectedSplines =  new List<Spline> ();
		_neighbours = new List<Point> ();

		cooldown = (((float)Point.pointCount) % boostCooldown)/3f;
		SR = GetComponent<SpriteRenderer> ();

//		l = GetComponent<LineRenderer> ();
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

	public void Update(){

		c = proximity + Mathf.Clamp01((Mathf.Sin (3 * Time.time + timeOffset)/5)) + 0.1f;
		c = Mathf.Pow (c, 1);
		if (hit) {
			SR.color = Color.Lerp (SR.color, Color.Lerp (new Color (c, c, c), Color.red, Mathf.Clamp01 (hitColorLerp)), Time.deltaTime);
		} else {
			SR.color = new Color (c, c, c);
		}
		color = SR.color;

		if (_connectedSplines.Count == 0) {
			c = 1;
		}

//		l.SetPosition (0, transform.position);
//		l.SetPosition (1, GetComponent<SpringJoint>().connectedBody.transform.position);

//		if (_neighbours.Count > 2) {
//			SetDirectionalArrows ();
//		}
	}

	public void AddSpline(Spline s){
		if (!_connectedSplines.Contains (s)) {
			_connectedSplines.Add (s);
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

	public void OnPointEnter(Spline s){
		if (GetComponentInParent<WordBank>() != null) {
			GameObject newText = (GameObject)Instantiate (Services.Prefabs.SpawnedText, transform.position + Vector3.up, Quaternion.identity);
			newText.GetComponent<TextMesh>().text = GetComponentInParent<WordBank>().GetWord ();
			newText.transform.parent = transform;
		}
	}

	public void OnPointExit(){
		proximity = 0.5f;
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
		GameObject fx = Instantiate (activatedSprite, transform.position, Quaternion.identity);
		fx.transform.parent = transform;
		hit = true;
		if (!PointManager._pointsHit.Contains (this)) {
			PointManager._pointsHit.Add (this);
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
