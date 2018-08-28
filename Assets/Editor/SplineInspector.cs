using UnityEditor;
using UnityEngine;

[CustomEditor(typeof( SplineManager ))]
public class SplineInspector : Editor {

	private const int stepsPerCurve = 5;
	private const float directionScale = 0.5f;
	private const float handleSize = 0.04f;
	private const float pickSize = 0.06f;

	private static Color[] modeColors = {
		Color.white,
		Color.yellow,
		Color.cyan
	};

	private Spline spline;
	private Transform handleTransform;
	private Quaternion handleRotation;
	private int selectedIndex = -1;

//	public override void OnInspectorGUI () {
//		spline = target as Spline;
//		EditorGUI.BeginChangeCheck();
//		bool loop = EditorGUILayout.Toggle("Loop", spline.closed);
//		if (EditorGUI.EndChangeCheck()) {
//			Undo.RecordObject(spline, "Toggle Loop");
//			EditorUtility.SetDirty(spline);
//			spline.closed = loop;
//		}
//		if (selectedIndex >= 0 && selectedIndex < spline.SplinePoints.Count) {
//			DrawSelectedPointInspector();
//		}
//		if (GUILayout.Button("Add Curve")) {
//			Undo.RecordObject(spline, "Add Curve");
//			//spline.AddCurve();
//			EditorUtility.SetDirty(spline);
//		}
//	}

	private void DrawSelectedPointInspector() {
		GUILayout.Label("Selected Point");
		EditorGUI.BeginChangeCheck();
		Vector3 point = EditorGUILayout.Vector3Field("Position", spline.SplinePoints[selectedIndex].Pos);
		if (EditorGUI.EndChangeCheck()) {
			Undo.RecordObject(spline, "Move Point");
			EditorUtility.SetDirty(spline);
			spline.SplinePoints [selectedIndex].transform.position = point;
		}
	}

	private void OnSceneGUI () {

		Handles.color = Color.white;

		SplineManager s = target as SplineManager;

		foreach (Spline spline in s.splines) {


			//		for (int i = 0; i < spline.SplinePoints.Count; i ++) {
			//			Vector3 p1 = ShowPoint(i);
			//			Vector3 p2 = ShowPoint(i + 1);
			//			Vector3 p3 = ShowPoint(i + 2);
			//
			//			Handles.color = Color.gray;
			//			Handles.DrawLine(p0, p1);
			//			Handles.DrawLine(p2, p3);
			//			Handles.DrawBezier(p0, p3, p1, p2, Color.white, null, 2f);
			//			p0 = p3;
			//		}


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

//			Handles.color = Color.green;
//
//			Count = spline.SplinePoints.Count;
//
//			for (int i = 0; i < Count - (spline.closed?0:1); i++){
//				for (int k = 0; k < stepsPerCurve ; k++){
//
//					float t = (float)k / (float)(stepsPerCurve-1);
//
//					Vector3 v= spline.GetPointAtIndex(i, t);
//
//					Handles.DrawLine(v, v + spline.GetVelocityAtIndex(i, t).normalized/5);
//				}
//			}
		}
	}

//	private void ShowDirections () {
//		Handles.color = Color.green;
//
//		int Count = spline.SplinePoints.Count;
//
//		for (int i = 0; i < Count - (spline.closed?0:1); i++){
//			for (int k = 0; k < stepsPerCurve ; k++){
//
//				float t = (float)k / (float)(stepsPerCurve-1);
//
//				Vector3 v= spline.GetPointAtIndex(i, t);
//
//				Handles.DrawLine(v, v + spline.GetVelocityAtIndex(i, t).normalized/5);
//			}
//		}
//	}

//	private Vector3 ShowPoint (int index) {
//		Vector3 point = handleTransform.TransformPoint(spline.GetControlPoint(index));
//		float size = HandleUtility.GetHandleSize(point);
//		if (index == 0) {
//			size *= 2f;
//		}
//		Handles.color = modeColors[(int)spline.GetControlPointMode(index)];
//		if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotCap)) {
//			selectedIndex = index;
//			Repaint();
//		}
//		if (selectedIndex == index) {
//			EditorGUI.BeginChangeCheck();
//			point = Handles.DoPositionHandle(point, handleRotation);
//			if (EditorGUI.EndChangeCheck()) {
//				Undo.RecordObject(spline, "Move Point");
//				EditorUtility.SetDirty(spline);
//				spline.SetControlPoint(index, handleTransform.InverseTransformPoint(point));
//			}
//		}
//		return point;
//	}
}
