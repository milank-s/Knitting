using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Node : MonoBehaviour {

	public GameObject edgePrefab;

	private List<Node> _adjacents;
	private List<Edge> _edges;

	public static int nodeCount = 0;


	 void Awake(){
		gameObject.name = "v" + Node.nodeCount;
		_adjacents = new List<Node> ();
		_edges = new List<Edge> ();
	}

	public Edge InsertEdge(Node n){

		GameObject newEdge = (GameObject)Instantiate (edgePrefab, n.transform.position, Quaternion.identity);

		Edge e = newEdge.GetComponent<Edge> ();
		e.SetVerts (n, this);

		e.name = n.name + "—" + this.name;

		_edges.Add(e);
		_adjacents.Add (n);
		n.AddEdge (e);
		n.AddNode (this);

		return e;
	}

	public void AddEdge(Edge e){
		_edges.Add (e);
	}

	public void AddNode(Node n){
		_adjacents.Add (n);
	}

	public Edge ConnectingEdge(Node n){
		foreach (Edge e in _edges) {
			if (e.GetVert2 () == n) {
				return e;
			}
		}
		return null;
	}

	public Edge GetClosestEdge(Vector3 cursorAngle){

		float minAngle = Mathf.Infinity;
		Edge closestEdge = null;

		foreach (Edge e in _edges) {

			float curAngle;
			if (e.GetVert1() == this) {
				curAngle = Vector3.Angle (cursorAngle, e.curve.GetDirection (0));
			} else {
				curAngle = Vector3.Angle(cursorAngle, -e.curve.GetDirection(1));
			}

			if (curAngle < minAngle){
				minAngle = curAngle;
				closestEdge = e;
			}
		}

		if (minAngle <= 25) {
			return closestEdge;
		}
		return null;
	}

	public bool HasEdges(){
		return _edges.Count > 0 ? true : false;
	}

	public List<Edge> GetEdges(){
		return _edges;
	}

	public List<Node> GetAdjacents(){
		return _adjacents;
	}

	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
