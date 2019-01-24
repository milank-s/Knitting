using UnityEngine;
using UnityEditor;

[CustomEditor( typeof( SplineTurtle ) )]
public class SplineTurtleCreator : Editor {

	public override void OnInspectorGUI()
	{
			DrawDefaultInspector();

			SplineTurtle myScript = (SplineTurtle)target;
			if(GUILayout.Button("Generate Spline"))
			{
					myScript.Generate();
					Undo.RecordObject(myScript, "generated");
			}
	}
}
