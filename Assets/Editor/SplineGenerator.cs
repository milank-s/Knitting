using UnityEngine;
using UnityEditor;

[CustomEditor( typeof( SplineTurtle ) )]
public class SplineGenerator : Editor {

	public override void OnInspectorGUI()
	{
			DrawDefaultInspector();

			SplineTurtle myScript = (SplineTurtle)target;
			if(GUILayout.Button("Generate Spline"))
			{
					myScript.Generate();
			}
	}
}
