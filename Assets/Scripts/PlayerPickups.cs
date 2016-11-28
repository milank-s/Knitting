using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerPickups : MonoBehaviour {

	public GameObject nodePrefab;
	public GameObject RipplePrefab;

	private List<Node> nodeInv;
	private float numPickups;
	private PlayerTraversal p;
	// Use this for initialization
	void Start () {
		nodeInv = new List<Node>();
		p = GetComponent<PlayerTraversal> ();
	}
	
	// Update is called once per frame
	void Update () {
		CirclePlayer ();

		if(Input.GetButtonDown("x")){
			InsertVertex();
		} 
	}

	public void OnTriggerEnter(Collider col){
		if (col.tag == "Node") {
//			GetComponent<TextHolder> ().CreateWord (transform.position + transform.right/3 + -transform.up/3);
			Instantiate (RipplePrefab, col.transform.position, Quaternion.identity);
		}
	}

	public void Pickup(Node n){
		
		numPickups++;
		nodeInv.Add (n);
		n.transform.parent = transform;
		n.transform.position = transform.position + (transform.up * numPickups)/10;
		n.GetComponent<Collider> ().enabled = false;
	}

	void InsertVertex(){

		if (!p.GetTraversing()) {

			Ray ray = Camera.main.ScreenPointToRay(Camera.main.WorldToScreenPoint(p.cursor.transform.position));
			RaycastHit hit;

			if (Physics.Raycast (ray, out hit)) {
				if (hit.collider.tag == "Node") {
					Node hitNode = hit.collider.GetComponent<Node> ();

					if (hitNode == p.curNode) {
						hitNode.Improve ();
					} else if (p.curNode.IsAdjacent (hitNode)) {
						hitNode.GetConnectingEdge (p.curNode).Reinforce ();
					} else {
						p.curNode.InsertEdge (hitNode);
					}
				}
			} else {
				Node.nodeCount++;
				GameObject newNode = (GameObject)Instantiate (nodePrefab, p.cursor.transform.position, Quaternion.identity);
				p.curEdge = p.curNode.InsertEdge (newNode.GetComponent<Node> ());
			}
		}
	}

	void CirclePlayer(){
		int i = 0;
		foreach (Node n in nodeInv) {
			i++;
			//g.transform.RotateAround (transform.position, Vector3.forward, (GetComponent<SplineWalker>().GetFlow()*numPickups + 1)/(i/5 + 1));
			n.transform.RotateAround (transform.position, Vector3.forward, (numPickups * p.GetFlow() + 1)/(i));
			Vector3 direction = (n.transform.position - transform.position).normalized;
			direction = direction * (i);
			n.transform.position = transform.position + (direction / Mathf.Clamp((p.GetFlow() * numPickups), 5, 1000)); 
		}
	}
}
