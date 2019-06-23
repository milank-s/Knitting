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
	SplineManager s;
//Doesnt work because it's targeting splines
//use this to select individual splines.

	// public override void OnInspectorGUI () {

	// 	spline = target as Spline;
	// 	EditorGUI.BeginChangeCheck();
	// 	bool loop = EditorGUILayout.Toggle("Loop", spline.closed);
	// 	if (EditorGUI.EndChangeCheck()) {
	// 		Undo.RecordObject(spline, "Toggle Loop");
	// 		EditorUtility.SetDirty(spline);
	// 		spline.closed = loop;
	// 	}
	// 	if (selectedIndex >= 0 && selectedIndex < spline.SplinePoints.Count) {
	// 		DrawSelectedPointInspector();
	// 	}
	// 	if (GUILayout.Button("Add Curve")) {
	// 		Undo.RecordObject(spline, "Add Curve");
	// 		//spline.AddCurve();
	// 		EditorUtility.SetDirty(spline);
	// 	}
	// }

	public void OnEnable(){
		s = target as SplineManager;
		s.splines = s.GetComponentsInChildren<Spline>();
	}

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
		//draw all the splines. NOTHING INTELLIGENT HERE
		Handles.color = Color.white;
		handleTransform = s.transform;
		handleRotation = handleTransform.rotation;
		foreach (Spline spliney in s.splines) {

			Handles.color = Color.gray;
			// Handles.Label(spliney.SplinePoints[0].Pos - Vector3.up/5 + Vector3.right/8f,spliney.name);

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


			int Count = spliney.SplinePoints.Count;
			Vector3 lastPosition = spliney.GetPointAtIndex (0, 0);
			for (int i = 0; i < Count - (spliney.closed ? 0 : 1); i++) {

				GUIStyle style = new GUIStyle();
		    style.fontSize = 12;
		    style.normal.textColor = Color.white;
		    //Handles.Label(spliney.SplinePoints[i].Pos, spliney.SplinePoints[i].gameObject.name, style);

				//Draw Point handles

				Handles.color = new Color(0.2f, 0.2f, 0.2f);
				Handles.DrawDottedLine(spliney.SplinePoints[i].Pos, spliney.SplinePoints[(i + 1) % spliney.SplinePoints.Count].Pos, 5f);


				for (int k = 0; k < spliney.curveFidelity; k++) {
					float t = (float)k / (float)(spliney.curveFidelity - 1);

					Vector3 v = spliney.GetPointAtIndex (i, t);
					Handles.color = Color.white;
					Handles.DrawLine (lastPosition, v);

					lastPosition = v;


					//  v = spliney.GetPointAtIndex(i, t);
					// Handles.DrawLine(v, v + spliney.GetVelocityAtIndex(i, t).normalized/5);
				}
			}
		}
	}


}
