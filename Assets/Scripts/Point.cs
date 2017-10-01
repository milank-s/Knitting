using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Point : MonoBehaviour
{
	public Vector3 Pos
	{
		get
		{
			return transform.position;
		}
	}
	public static int pointCount = 0;

	public float tension;
	public float bias;
	public float continuity;

	public GameObject splinePrefab;

	public List<Point> _neighbours;
	public List<Spline> _connectedSplines;

	public Color color;
	public float timeOffset;
	public float proximity = 0;

	public static Point Select;
	
	public bool isSelect
	{
		get
		{
			return this==Select;
		}
	}


	void Awake(){
		Point.pointCount++;
		color = new Color (1, 1, 1, 0);
		gameObject.name = "v" + Point.pointCount;
		_neighbours = new List<Point> ();
		_connectedSplines = new List<Spline> ();

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

	public void AddSpline(Spline s){
		if (!_connectedSplines.Contains (s)) {
			_connectedSplines.Add (s);
		}
	}

	public void AddPoint(Point p){
		if (!_neighbours.Contains (p)) {
			_neighbours.Add (p);
		}
	}

	public void RemoveSpline(Spline s){
		_connectedSplines.Remove (s);
	}

	public void RemoveNode(Point p){
		_neighbours.Remove (p);
	}

	public void OnPointEnter(){
		continuity = Mathf.Clamp01 (continuity - 0.1f);
//		bias  = Mathf.Clamp01 (bias + 0.1f);
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
