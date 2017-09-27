using UnityEngine;
using System;
using System.Collections;

public class BezierSpline : MonoBehaviour {

	[SerializeField]
	private Vector3[] points;

	[SerializeField]
	private BezierControlPointMode[] modes;

	[SerializeField]
	private bool loop;

	[SerializeField]


	public bool Loop {
		get {
			return loop;
		}
		set {
			loop = value;
			if (value == true) {
				modes[modes.Length - 1] = modes[0];
				SetControlPoint(0, points[0]);
			}
		}
	}

	public int ControlPointCount {
		get {
			return points.Length;
		}
	}

	public Vector3 GetControlPoint (int index) {
		return points[index];
	}

	public void SetControlPoint (int index, Vector3 point) {
		if (index % 3 == 0) {
			Vector3 delta = point - points[index];
			if (loop) {
				if (index == 0) {
					points[1] += delta;
					points[points.Length - 2] += delta;
					points[points.Length - 1] = point;
				}
				else if (index == points.Length - 1) {
					points[0] = point;
					points[1] += delta;
					points[index - 1] += delta;
				}
				else {
					points[index - 1] += delta;
					points[index + 1] += delta;
				}
			}
			else {
				if (index > 0) {
					points[index - 1] += delta;
				}
				if (index + 1 < points.Length) {
					points[index + 1] += delta;
				}
			}
		}
		points[index] = point;
		EnforceMode(index);
	}

	public BezierControlPointMode GetControlPointMode (int index) {
		return modes[(index + 1) / 3];
	}

	public void SetControlPointMode (int index, BezierControlPointMode mode) {
		int modeIndex = (index + 1) / 3;
		modes[modeIndex] = mode;
		if (loop) {
			if (modeIndex == 0) {
				modes[modes.Length - 1] = mode;
			}
			else if (modeIndex == modes.Length - 1) {
				modes[0] = mode;
			}
		}
		EnforceMode(index);
	}

	private void EnforceMode (int index) {
		int modeIndex = (index + 1) / 3;
		BezierControlPointMode mode = modes[modeIndex];
		if (mode == BezierControlPointMode.Free || !loop && (modeIndex == 0 || modeIndex == modes.Length - 1)) {
			return;
		}

		int middleIndex = modeIndex * 3;
		int fixedIndex, enforcedIndex;
		if (index <= middleIndex) {
			fixedIndex = middleIndex - 1;
			if (fixedIndex < 0) {
				fixedIndex = points.Length - 2;
			}
			enforcedIndex = middleIndex + 1;
			if (enforcedIndex >= points.Length) {
				enforcedIndex = 1;
			}
		}
		else {
			fixedIndex = middleIndex + 1;
			if (fixedIndex >= points.Length) {
				fixedIndex = 1;
			}
			enforcedIndex = middleIndex - 1;
			if (enforcedIndex < 0) {
				enforcedIndex = points.Length - 2;
			}
		}

		Vector3 middle = points[middleIndex];
		Vector3 enforcedTangent = middle - points[fixedIndex];
		if (mode == BezierControlPointMode.Aligned) {
			enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, points[enforcedIndex]);
		}
		points[enforcedIndex] = middle + enforcedTangent;
	}

	public int CurveCount {
		get {
			return (points.Length - 1) / 3;
		}
	}

	public Vector3 GetPoint (float t) {
		int i;
		if (t >= 1f) {
			t = 1f;
			i = points.Length - 4;
		}
		else {
			t = Mathf.Clamp01(t) * CurveCount;
			i = (int)t;
			t -= i;
			i *= 3;
		}
		return transform.TransformPoint(Bezier.GetPoint(points[i], points[i + 1], points[i + 2], points[i + 3], t));
	}
	
	public Vector3 GetVelocity (float t) {
		int i;
		if (t >= 1f) {
			t = 1f;
			i = points.Length - 4;
		}
		else {
			t = Mathf.Clamp01(t) * CurveCount;
			i = (int)t;
			t -= i;
			i *= 3;
		}
		return transform.TransformPoint(Bezier.GetFirstDerivative(points[i], points[i + 1], points[i + 2], points[i + 3], t)) - transform.position;
	}
	
	public Vector3 GetDirection (float t) {
		return GetVelocity(t).normalized;
	}

	public void AddCurve () {
		Vector3 point = points[points.Length - 1];
		Array.Resize(ref points, points.Length + 3);
		point.x += 1f;
		points[points.Length - 3] = point;
		point.x += 1f;
		points[points.Length - 2] = point;
		point.x += 1f;
		points[points.Length - 1] = point;

		Array.Resize(ref modes, modes.Length + 1);
		modes[modes.Length - 1] = modes[modes.Length - 2];
		EnforceMode(points.Length - 4);

		if (loop) {
			points[points.Length - 1] = points[0];
			modes[modes.Length - 1] = modes[0];
			EnforceMode(0);
		}
	}


//	for (k=1; k<(points.Length-2); k++)
//	{
//		for (i=0; i<maxVerticesCurve; i++)
//		{
//			u = (double)i / (double)(maxVerticesCurve-1);
//			spline2D[maxVerticesCurve*(k-1)+i][0] = (2*u*u*u - 3*u*u + 1)*control[k][0] 
//				+ (-2*u*u*u + 3*u*u)*control[k+1][0]
//				+ (u*u*u - 2*u*u + u)*(0.5*(1-tension)*((1+bias)*(1-continuity)*(control[k][0]-control[k-1][0])
//					+ (1-bias)*(1+continuity)*(control[k+1][0]-control[k][0])))
//				+ (u*u*u - u*u)*(0.5*(1-tension)*((1+bias)*(1+continuity)*(control[k+1][0]-control[k][0])
//					+ (1-bias)*(1-continuity)*(control[k+2][0]-control[k+1][0])));
//			spline2D[maxVerticesCurve*(k-1)+i][1] = (2*u*u*u - 3*u*u + 1)*control[k][1] 
//				+ (-2*u*u*u + 3*u*u)*control[k+1][1]
//				+ (u*u*u - 2*u*u + u)*(0.5*(1-tension)*((1+bias)*(1-continuity)*(control[k][1]-control[k-1][1])
//					+ (1-bias)*(1+continuity)*(control[k+1][1]-control[k][1])))
//				+ (u*u*u - u*u)*(0.5*(1-tension)*((1+bias)*(1+continuity)*(control[k+1][1]-control[k][1])
//					+ (1-bias)*(1-continuity)*(control[k+2][1]-control[k+1][1])));
//			spline2D[maxVerticesCurve*(k-1)+i][2] = (2*u*u*u - 3*u*u + 1)*control[k][2] 
//				+ (-2*u*u*u + 3*u*u)*control[k+1][2]
//				+ (u*u*u - 2*u*u + u)*(0.5*(1-tension)*((1+bias)*(1-continuity)*(control[k][2]-control[k-1][2])
//					+ (1-bias)*(1+continuity)*(control[k+1][2]-control[k][2])))
//				+ (u*u*u - u*u)*(0.5*(1-tension)*((1+bias)*(1+continuity)*(control[k+1][2]-control[k][2])
//					+ (1-bias)*(1-continuity)*(control[k+2][2]-control[k+1][2])));
//		}
//	}


	public void CreateCurve (Transform p1, Transform p2, float distance, Vector3 v1 = default(Vector3), Vector3 v2 = default(Vector3)) {
		Vector3 target = p2.position - p1.position;


		points = new Vector3[4];
		points [0] = Vector3.zero;
//		points [1] = Vector3.Lerp (Vector3.zero, target, 0.33f);
//		points [2] = Vector3.Lerp (points[1], target, 0.5f);
//
		if (v1 == Vector3.zero) {
			points [1] = Vector3.Lerp (Vector3.zero, target, 0.33f);
		} else {
			points [1] = v1; //multiplicant should range between 0 and 1. Should also scale with line length
		}

		if (v2 != Vector3.zero) {
			points [2] = target + v2;
	
//		Lerp Second point with the inverse of Point 1's velocity
		} else if (v1 != Vector3.zero) {
//			v2 = Quaternion.Euler(0, 0, 0) * v1; //rotate v1 180 degrees
			points [2] = target + ((v1 - target));
//			points[2] = Vector3.Reflect(v1, (target-v1));
		} else {
			points [2] = Vector3.Lerp (points[1], target, 0.66f);
		
		}

		points[3] = target;

		modes = new BezierControlPointMode[] {
			BezierControlPointMode.Aligned,
			BezierControlPointMode.Aligned
		};
	}

	public void SetSpline(Vector3[] newPoints, BezierControlPointMode[] newModes){
		points = newPoints;
		modes = newModes;
	}

}