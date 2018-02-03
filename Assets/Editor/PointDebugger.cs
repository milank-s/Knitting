using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor( typeof( Point ) )]
public class PointDebugger : Editor {


	private void OnSceneGUI () {

		Point point = target as Point;

		Handles.color = Color.green;
		DrawSelectedPoint (point);

		Handles.color = Color.red;



		foreach (Point p in point._neighbours) {
			DrawSelectedPoint (p);
		}
		int m = 0;
		foreach (Spline spline in point._connectedSplines) {

			 
			switch (m) {
			case 0:
				Handles.color = Color.blue;
				break;

			case 1:
				Handles.color = Color.yellow;
				break;

			case 2:
				Handles.color = Color.magenta;
				break;

			case 3:
				Handles.color = Color.green;

				break;

			default:
				Handles.color = Color.cyan;
				break;
			}


			int Count = spline.SplinePoints.Count;
			Vector3 lastPosition = spline.GetPointAtIndex (0, 0);

			for (int i = 0; i < Count - (spline.closed ? 0 : 1); i++) {
				for (int k = 0; k < spline.curveFidelity; k++) {

					float t = (float)k / (float)(spline.curveFidelity - 1);

					Vector3 v = spline.GetPointAtIndex (i, t);

					Handles.DrawLine (lastPosition, v);

					lastPosition = v;
				}
			}

//			Handles.color = Color.yellow;
		

//			for (int i = 0; i < Count - (spline.closed?0:1); i++){
//				for (int k = 0; k < spline.curveFidelity ; k++){
//
//					float t = (float)k / (float)(spline.curveFidelity-1);
//
//					Vector3 v= spline.GetPointAtIndex(i, t);
//
//					Handles.DrawLine(v, v + spline.GetVelocityAtIndex(i, t).normalized/5);
//				}
//			}

			m++;

		}
	}

	private void DrawSelectedPoint(Point p) {
		Handles.DrawSolidDisc (p.Pos, -Vector3.forward, 0.1f); 
	}

	private Vector3 ShowPoint (int index) {
		//			Vector3 point = handleTransform.TransformPoint(spline.GetControlPoint(index));
		//			float size = HandleUtility.GetHandleSize(point);
		//			if (index == 0) {
		//				size *= 2f;
		//			}
		//			Handles.color = Color.gray;
		//			return point;
		//		}
		return Vector3.zero;
	}
}
