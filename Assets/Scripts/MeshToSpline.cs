using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshToSpline : MonoBehaviour {

	Mesh mesh;
	Spline curSpline;
	Point curPoint;
	Vector3[] vertices;

	void Start () {
		mesh = GetComponent<MeshFilter> ().mesh;
		vertices = mesh.vertices;
		Debug.Log (mesh.vertices.Length);

		Point firstPoint = Services.PlayerBehaviour.CreatePoint (transform.TransformPoint(vertices [0]));
		Point secondPoint = Services.PlayerBehaviour.CreatePoint (transform.TransformPoint(vertices [1]));
		curPoint = secondPoint;
		curSpline = Services.PlayerBehaviour.CreateSpline (firstPoint, secondPoint);
		for(int i = 2; i < vertices.Length; i++) {
			Point nextPoint = Services.PlayerBehaviour.CheckIfOverPoint(transform.TransformPoint(vertices [i]));
			SplinePointPair spp = Services.PlayerBehaviour.ConnectNewPoint(curSpline, curPoint, nextPoint, transform.position);
			curSpline = spp.s;
			curPoint = spp.p;
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
