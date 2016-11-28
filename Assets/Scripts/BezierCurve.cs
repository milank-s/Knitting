using UnityEngine;

public class BezierCurve : MonoBehaviour {

	public Vector3[] points;
	
	public Vector3 GetPoint (float t) {
		return transform.TransformPoint(Bezier.GetPoint(points[0], points[1], points[2], points[3], t));
	}
	
	public Vector3 GetVelocity (float t) {
		return transform.TransformPoint(Bezier.GetFirstDerivative(points[0], points[1], points[2], points[3], t)) - transform.position;
	}
	
	public Vector3 GetDirection (float t) {
		return GetVelocity(t).normalized;
	}

	//if continuing from an edge find the closest edge opposite it and add the velocity of the last point to the second point of this curve

	public void CreateCurve (Transform p1, Transform p2, Vector3 v1 = default(Vector3), Vector3 v2 = default(Vector3)) {
		Vector3 target = p2.position - p1.position;
		points = new Vector3[4];

		points [0] = Vector3.zero;

		if (v1 != Vector3.zero) {
			points [1] = v1;
		} else {
			points [1] = Vector3.Lerp (Vector3.zero, target, 0.33f);
		}

		if (v2 != Vector3.zero) {
			points [2] = v2;

		}else if (v1 != Vector3.zero){

			points[2] = Vector3.Lerp(points[1], target, 0.5f);
		} else {
			points[2] = Vector3.Lerp(points[1], target, 0.5f);
		}

		points[3] = target;
	}

	
	public void Reset () {
		points = new Vector3[] {
			new Vector3(1f, 0f, 0f),
			new Vector3(2f, 0f, 0f),
			new Vector3(3f, 0f, 0f),
			new Vector3(4f, 0f, 0f)
		};
	}
}