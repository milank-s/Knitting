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

         EditorGUILayout.Space();

        
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

         EditorGUILayout.Space();

        if(GUILayout.Button("Readout Submeshes")) {

            t.SubmeshReadout();
            //add everthing the button would do.
        }
   }

   public void OnSceneGUI(){
        MeshToSpline t = (MeshToSpline) target;
		Mesh m;
        if(t.meshTarget == null) return;
        //foreach(MeshFilter f in t.GetComponentsInChildren<MeshFilter>()){
            m = t.meshTarget.sharedMesh;
            
            int numSubmeshes = m.subMeshCount;
            MeshTopology topo;
           
            for(int subMeshIndex = 0; subMeshIndex < numSubmeshes; subMeshIndex++){
                UnityEngine.Rendering.SubMeshDescriptor sub = m.GetSubMesh(subMeshIndex);

                topo = sub.topology;
                int start = sub.indexStart;
                int amount = sub.indexCount;
                int end = start + amount;
                int label = 0;


                //   Handles.Label(v2 + Vector3.up/10f, label.ToString());
                //         label++;
                int[] indices = m.GetIndices(subMeshIndex);

                for(int i = 0; i < indices.Length; i+= amount){
                    for(int j = 0; j < amount-1; j+= amount){
                        Vector3 v1 = m.vertices[indices[i + j]];
                        Vector3 v2 = m.vertices[indices[i + j + 1]];
                        v1 = t.transform.TransformPoint(v1);
                        v2 = t.transform.TransformPoint(v2);

                        Handles.DrawLine(v1, v2);
                        
                    }

                    if(amount > 1){
                        Vector3 v1 = m.vertices[indices[i]];
                        Vector3 v2 = m.vertices[indices[i + amount - 1]];
                        v1 = t.transform.TransformPoint(v1);
                        v2 = t.transform.TransformPoint(v2);
                    }
                }
            }
        
		    // Debug.Log("tris = " + tris);
		//}
   }
}
