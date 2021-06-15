using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor( typeof( MapEditor ) )]
public class LevelEditorInspector : Editor
{
    // Start is called before the first frame update
    
    public override void OnInspectorGUI()
    {
        
        MapEditor mapEditor = (MapEditor)target;

     
        if(GUILayout.Button("SAVE"))
        {
            Undo.RecordObject(mapEditor, "savedFile");
            mapEditor.Save(mapEditor.controller);
        }
        
        if(GUILayout.Button("LOAD"))
        {
            Undo.RecordObject(mapEditor, "savedFile");
            mapEditor.LoadInEditor(mapEditor.controller.name);
        }
        
        
        DrawDefaultInspector();

        
        
    }
  
    
}
