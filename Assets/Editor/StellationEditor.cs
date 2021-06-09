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

		if (GUILayout.Button("Open in Editor"))
		{
			
			Undo.RecordObject(controller, "reloaded");
			
			Services.main.OpenEditorFileOnLoad(controller.name);
			
			EditorUtility.SetDirty(controller);
			EditorUtility.SetDirty(Services.main);

			EditorApplication.ExecuteMenuItem("Edit/Play");
			
			
		}

		if (GUILayout.Button("Reload From File"))
		{
			
			Undo.RecordObject(controller, "reloaded");
			controller.ReloadFromEditor();
			StellationManager m = controller.GetComponentInParent<StellationManager>();


			EditorUtility.SetDirty(controller);
			EditorUtility.SetDirty(m);
		}
		
	}
}