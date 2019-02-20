using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor( typeof( Point ) )]
[CanEditMultipleObjects]
public class PointDebugger : Editor {



	public override void OnInspectorGUI()
	{
			DrawDefaultInspector();

			Point myScript = (Point)target;
			if(GUILayout.Button("Clear Connections"))
			{
					myScript.Reset();
					Undo.RecordObject(myScript, "reset point");
			}

	}

	private void OnSceneGUI () {

		Point point = target as Point;

		Handles.color = Color.green;
		DrawSelectedPoint (point);

		Handles.color = Color.blue;


		foreach (Point p in point._neighbours) {
			DrawSelectedPoint (p);
		}
		int m = 0;

		foreach (Spline spline in point._connectedSplines) {


		Handles.color = Color.white;

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


		}
	}

	private void DrawSelectedPoint(Point p) {
		Handles.DrawSolidDisc (p.Pos, -Vector3.forward, 0.05f);
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
