using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(StellationController))]
[CanEditMultipleObjects]
public class StellationEditor : Editor
{
	private StellationController controller;

	// draw lines between a chosen game object
	// and a selection of added game objects
	public override void OnInspectorGUI()
	{
		
		controller = (StellationController) target;
		
		DrawDefaultInspector();

		if (GUILayout.Button("ReloadFromFile"))
		{
			
			Undo.RecordObject(controller, "reloaded");
			controller.ReloadFromEditor();
			
		}
		
	}
}