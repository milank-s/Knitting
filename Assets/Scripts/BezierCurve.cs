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

	public void CreateCurve (Transform p1, Transform p2) {
		Vector3 target = p2.position - p1.position;
		points = new Vector3[] {
			Vector3.zero,
//			new Vector3 (Vector3.Lerp(Vector3.zero, target, 0.66f).x, 0, 0),
//			new Vector3 (Vector3.Lerp(Vector3.zero, target, 0.33f).x, target.y, 0),	
			Vector3.Lerp(Vector3.zero, target, 0.33f),
			Vector3.Lerp(Vector3.zero, target, 0.66f),
			target
		};
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