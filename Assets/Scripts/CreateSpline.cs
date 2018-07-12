using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vectrosity;

public class CreateSpline : MonoBehaviour
{

	Spline curSpline;
	Point curPoint;
	List<Vector3> vertices;

	public TextAsset shape;

	public float continuity;

	// Use this for initialization
	void Start ()
	{
		vertices = VectorLine.BytesToVector3List (shape.bytes);
		Make ();
	}

	void Make ()
	{

		Point firstPoint = SplineUtil.CreatePoint (transform.TransformPoint (vertices [0]));
		Point secondPoint = SplineUtil.CreatePoint (transform.TransformPoint (vertices [1]));
		curPoint = secondPoint;
		curSpline = SplineUtil.CreateSpline (firstPoint, secondPoint);
		for (int i = 2; i < vertices.Count; i++) {
			
			Point nextPoint = SplineUtil.CreatePoint (transform.TransformPoint (vertices [i]));

			SplinePointPair spp = SplineUtil.ConnectPoints (curSpline, curPoint, nextPoint);
			curSpline = spp.s;
			curPoint = nextPoint;
		}

	}
}
