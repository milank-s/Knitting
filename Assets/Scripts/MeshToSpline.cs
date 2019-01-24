using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshToSpline : MonoBehaviour {

	Mesh mesh;
	Spline curSpline;
	Point curPoint;
	Vector3[] vertices;

	public float raycastDist;
	public float raycastHeight;

	void Start () {
		mesh = GetComponent<MeshFilter> ().mesh;
		vertices = mesh.vertices;
		Debug.Log (mesh.vertices.Length);

		Point firstPoint = SplineUtil.CreatePoint (transform.TransformPoint(vertices [0]));
		Point secondPoint = SplineUtil.CreatePoint (transform.TransformPoint(vertices [1]));
		curPoint = secondPoint;
		curSpline = SplineUtil.CreateSpline (firstPoint, secondPoint);
		for(int i = 2; i < vertices.Length; i++) {
			Point nextPoint = SplineUtil.RaycastDownToPoint(transform.TransformPoint(vertices [i]), raycastDist, raycastHeight);
			if (nextPoint == null) {
				SplineUtil.CreatePoint (transform.TransformPoint (vertices [i]));
			}
			SplinePointPair spp = SplineUtil.ConnectPoints(curSpline, curPoint, nextPoint);
			curSpline = spp.s;
			curPoint = spp.p;
		}
	}

}
