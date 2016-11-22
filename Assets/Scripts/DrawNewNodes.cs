using UnityEngine;
using System.Collections;

public class DrawNewNodes : MonoBehaviour {

	public GameObject NodePrefab;
	public Node curNode;

	// Use this for initialization
	void Start () {
		StartCoroutine (createNode());
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	IEnumerator createNode(){
		Node newNode = new Node ();
		newNode.InsertEdge (curNode);
//		GameObject newNode = (GameObject)Instantiate (NodePrefab, curNode.transform.position + transform.up * Mathf.Cos (Time.time * 3) * Time.time/3 + transform.right * Mathf.Sin(Time.time * 3) * Time.time/3 , Quaternion.identity);
//		newNode.GetComponent<Node> ().Initialize (curNode);

		yield return new WaitForSeconds (.25f);

		curNode = newNode.GetComponent<Node>();

		StartCoroutine (createNode());
	}
}
