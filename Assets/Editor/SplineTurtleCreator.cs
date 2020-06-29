using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor( typeof( SplineTurtle ) )]
public class SplineTurtleCreator : Editor {

	public override void OnInspectorGUI()
	{
			DrawDefaultInspector();

			SplineTurtle myScript = (SplineTurtle)target;
			if(GUILayout.Button("Generate Spline"))
			{
				
					myScript.Reset();
					myScript.Generate();
					Undo.RecordObject(myScript, "generated");
			}

			if(GUILayout.Button("Reset"))
			{

				myScript.Reset();
					// Undo.RecordObject(myScript, "reset");
			}

			if(GUILayout.Button("Save"))
			{

				myScript.editor.Save();
				
				//MapEditor.Save();
//				foreach(Transform t in myScript.parent.transform){
//						if (t!= myScript.transform){
//							t.parent = null;
//						}
//					}
//					Undo.RecordObject(myScript, "saved");


			}

	}
}
