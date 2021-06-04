using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CustomEditor( typeof( StellationManager ) )]
public class SceneManager : Editor
{
    // Start is called before the first frame update
    
    public override void OnInspectorGUI()
    {
        
        DrawDefaultInspector();   

        StellationManager script = (StellationManager)target;

        if(GUILayout.Button("Load"))
        {
            Undo.RecordObject(script, "savedFile");
            StellationController c = Services.main.editor.Load(script.fileName);
            if(script.controllers == null){
                script.controllers = new List<StellationController>();
            }

            script.controllers.Add(c);
            c.transform.parent = script.transform;
            //put it in the right transform and asign it to my list of stellations
        }
     
    }
  
    
}
