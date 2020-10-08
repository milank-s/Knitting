using UnityEditor;
using UnityEngine;
using System.Collections;
using System.IO;

[CustomEditor( typeof( Main ) )]
public class MainEditor : Editor
{
    // Start is called before the first frame update
    
    public override void OnInspectorGUI()
    {
        
        DrawDefaultInspector();

        
        
        Main mainScript = (Main)target;


        if (GUILayout.Button("INIT"))
        {
            mainScript.Awake();
        }
        if(GUILayout.Button("LOAD"))
        {
            Undo.RecordObject(mainScript, "savedFile");
            mainScript.editor.Load(mainScript.loadFileName);
        }
     
    }
  
    
}
