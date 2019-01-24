using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BezierSpline))]
public class SplineEditor : Editor
{
	private SerializedObject _spline;
	private SerializedProperty __numSubDivisions;
	private SerializedProperty _numSubDivisions;

	BezierSpline spline;

	public void OnEnable()
	{
		_spline = new SerializedObject(target);

		spline = (BezierSpline)target;

		_numSubDivisions = _spline.FindProperty("NumSubDivisions");
		__numSubDivisions = _numSubDivisions;
	}

	public override void OnInspectorGUI()
	{
		EditorGUILayout.PropertyField(_numSubDivisions);

		_spline.ApplyModifiedProperties();

		if (SerializedProperty.EqualContents(_numSubDivisions, __numSubDivisions))
		{
			__numSubDivisions = _numSubDivisions;
			spline = (BezierSpline)target;
			spline.UpdateNumberSubDivisions();
		}

		//DrawDefaultInspector();

		if (GUILayout.Button("Add Point"))
		{
			BezierSpline spline = (BezierSpline)target;
			spline.AddPoint(false);
		}

	}
}
