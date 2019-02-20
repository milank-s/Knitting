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
					myScript.Generate();
					Undo.RecordObject(myScript, "generated");
			}

			if(GUILayout.Button("Reset"))
			{

				Transform[] ts = myScript.GetComponentsInChildren<Transform>();
					for(int i = 0; i < ts.Length; i++){
						if (ts[i] != myScript.transform){
							DestroyImmediate(ts[i].gameObject);
						}
					}
					// Undo.RecordObject(myScript, "reset");
			}

			if(GUILayout.Button("Save"))
			{
				foreach(Transform t in myScript.transform){
						if (t!= myScript.transform){
							t.parent = null;
						}
					}
					Undo.RecordObject(myScript, "saved");
			}

	}
}
