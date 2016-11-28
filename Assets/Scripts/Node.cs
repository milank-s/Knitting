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

		GameObject newEdge = (GameObject)Instantiate (edgePrefab, transform.position, Quaternion.identity);

		Edge e = newEdge.GetComponent<Edge> ();
		e.SetVerts (this, n);

		Color c = new Color(Random.Range(0.50f , 1.00f),Random.Range(0.50f , 1.00f),Random.Range(0.50f , 1.00f));
		e.GetComponent<LineRenderer>().SetColors(c, c);
		e.name = this.name + "—" + n.name;
		
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

	public Edge GetClosestEdgeDirection(Vector3 direction, bool reversed = false){

		float minAngle = Mathf.Infinity;
		Edge closestEdge = null;

		foreach (Edge e in _edges) {

			float curAngle;

			curAngle = e.GetAngleAtNode (direction, this, reversed);

			if (curAngle < minAngle){
				minAngle = curAngle;
				closestEdge = e;
			}
		}

		return closestEdge;
	}

	public void Improve(){
		GetComponent<SpriteRenderer> ().color *= 1.5f;
	}

	public bool HasEdges(){
		return _edges.Count > 0 ? true : false;
	}
		
	public Edge GetConnectingEdge(Node n){
		foreach (Edge e in _edges) {
			if (e.ConnectedTo (this) && e.ConnectedTo (n))
				return e;
		}
		return null;
	}

	public bool IsAdjacent(Node n){
		return _adjacents.Contains (n);
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
