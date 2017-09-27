using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerPickups : MonoBehaviour {

	public GameObject nodePrefab;
	public GameObject RipplePrefab;

	private List<Point> nodeInv;
	private PlayerBehaviour p;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
		CirclePlayer ();
	}

	public void OnTriggerEnter(Collider col){
		if (col.tag == "Point") {
//			GetComponent<TextHolder> ().CreateWord (transform.position + transform.right/3 + -transform.up/3);

			if(!col.GetComponent<Point>().HasSplines()){CollectPoint (col.GetComponent<Point> ());}

//			Instantiate (RipplePrefab, col.transform.position, Quaternion.identity);
		}
	}

	IEnumerator CollectPoint(Point n){
		float t = 0;
		Vector3 originalPos = n.transform.position;

		while (t <= 1) {
			n.transform.position = Vector3.Lerp (originalPos, transform.position, t);
			t += Time.deltaTime;
			yield return null;
		}

		AddPoint (n);
	}

	void AddPoint(Point n){
		nodeInv.Add (n);
		n.transform.parent = transform;
		n.GetComponent<Collider> ().enabled = false;
	}

//	public Spline CreatePoint(){
//		if (nodeInv.Count > 0) {
//			Point newPoint = nodeInv [0];
//			nodeInv.Remove (newPoint);
//			newPoint.transform.parent = null;
//			newPoint.transform.position = ServicesManager.Cursor.transform.position;
//			newPoint.timeOffset = Time.time;
//			newPoint.GetComponent<Collider> ().enabled = true;
//			return p.CreateSpline (p.curPoint, newPoint);
//		} else {
//			return null;
//		}
//	}

	void CirclePlayer(){
		int i = 0;
		foreach (Point n in nodeInv) {
			i++;
			//g.transform.RotateAround (transform.position, Vector3.forward, (GetComponent<SplineWalker>().GetFlow()*numPickups + 1)/(i/5 + 1));
			n.transform.RotateAround (transform.position, Vector3.forward, (nodeInv.Count * Mathf.Abs(p.GetFlow()) + 10)/(i));
			Vector3 direction = (n.transform.position - transform.position).normalized;
			direction = direction * (i);
			n.transform.position = transform.position + (direction / Mathf.Clamp((Mathf.Abs(p.GetFlow()) * nodeInv.Count), 5, 1000)); 
		}
	}
}
