using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CirclePoints : MonoBehaviour {

	StellationController c;
	List<Vector3> pointVectors;
	private List<float> distance;
	void Start ()
	{
		distance = new List<float>();
		pointVectors = new List<Vector3> ();
		c = GetComponent<StellationController> ();
		for(int i = 0; i < c._points.Count; i++){
			Vector3 diff =  c._points[i].transform.position - transform.position;
			pointVectors.Add(Vector3.Cross(diff, Quaternion.Euler(Vector3.up * 90) * diff));
			distance.Add(diff.magnitude);
		}
	}
	
	// Update is called once per frame
	void Update () {
		for(int i = 0; i < c._points.Count; i++){

			Vector3 dir = c._points[i].transform.position - transform.position; // get point direction relative to pivot
//			c._points [i].transform.position = dir + transform.position; // calculate rotated point
//			pointVectors[i] = Vector3.Cross(dir, Quaternion.Euler(Vector3.up * 90) * dir);
			c._points [i].transform.position = (Quaternion.Euler(pointVectors[i] * 10) * dir).normalized * distance[i] + transform.position;

//			c._points [i].transform.RotateAround (transform.position, pointVectors [i], Time.deltaTime * 100);
		}
	}
}
