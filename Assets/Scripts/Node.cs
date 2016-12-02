using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Node : MonoBehaviour {

	public GameObject edgePrefab;

	private List<Node> _adjacents;
	private List<Edge> _edges;
	public Color c;
	public float timeOffset;
	public static int nodeCount = 0;
	public float proximity = 0;

	 void Awake(){
//		c = new Color (1, 1, 1, 0);
		c = new Color(Random.Range(0.50f , 1.00f),Random.Range(0.50f , 1.00f),Random.Range(0.50f , 1.00f), 0);
		gameObject.name = "v" + Node.nodeCount;
		_adjacents = new List<Node> ();
		_edges = new List<Edge> ();
	}

	void FixedUpdate(){
		SetColour ();
	}

	public Edge CreateEdge(Node n){

		GameObject newEdge = (GameObject)Instantiate (edgePrefab, transform.position, Quaternion.identity);

		Edge e = newEdge.GetComponent<Edge> ();
		e.CreateSpline (this, n);
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

	public void DestroyEdges(){
		foreach (Node n in _adjacents) {
			n.GetConnectingEdge (this).GetComponent<EdgeDecorator> ().DestroySpline (this, n);
			n._edges.Remove (n.GetConnectingEdge (this));
			n._adjacents.Remove (this);
		}
//		foreach (Edge e in _edges) {
//			e.GetComponent<EdgeDecorator>().DestroySpline (this);
//		}
	}

	public void SetColour(){
		c.a = proximity + Mathf.Abs(Mathf.Sin (Time.time * 3 + timeOffset)/5) + 0.1f;
		GetComponent<SpriteRenderer>().color = c;

		foreach (Edge e in _edges) {
			if (e.GetVert1 () == this) {
				e.GetComponent<LineRenderer> ().SetColors (c, e.GetVert2().c); 
			} else {
				e.GetComponent<LineRenderer> ().SetColors (e.GetVert1().c, c);
			}
		}
	}
}
