using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MeshToSpline))]
public class MeshToSplineEditor : Editor
{
    
    public override void OnInspectorGUI () {
    //Called whenever the inspector is drawn for this object.
        MeshToSpline t = (MeshToSpline) target;
        DrawDefaultInspector();

        
        //This draws the default screen.  You don't need this if you want
        //to start from scratch, but I use this when I'm just adding a button or
        //some small addition and don't feel like recreating the whole inspector.

        if(GUILayout.Button("Points")) {
            
            t.ConvertMesh(ConvertMode.None);
            //add everthing the button would do.
        }

        if(GUILayout.Button("Linear")) {
            
            t.ConvertMesh(ConvertMode.Linear);
            //add everthing the button would do.
        }

        if(GUILayout.Button("Complete")) {
            
            t.ConvertMesh(ConvertMode.Complete);
            //add everthing the button would do.
        }

        if(GUILayout.Button("Faces")) {
            
            t.ConvertMesh(ConvertMode.Quads);
            //add everthing the button would do.
        }
   }

   public void OnSceneGUI(){
        MeshToSpline t = (MeshToSpline) target;
		Mesh m;
        if(t.meshTarget == null) return;
        //foreach(MeshFilter f in t.GetComponentsInChildren<MeshFilter>()){
            m = t.meshTarget.mesh;
            
			int num = 0;
            Vector3 lastPos = m.vertices[0] + t.transform.position;
			foreach(Vector3 v in m.vertices){
                Vector3 v2 = v + t.transform.position;
				 Handles.Label(v2 + Vector3.up/10f,
                 num.ToString());
                 num++;
                 Handles.DrawLine(lastPos, v2);
                 lastPos = v2;
			}

			string tris = "";
			foreach(int i in m.triangles){
				tris += i;
			}
        
		    // Debug.Log("tris = " + tris);
		//}
   }
}
