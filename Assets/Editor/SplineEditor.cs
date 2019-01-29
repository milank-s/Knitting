using UnityEngine;
using UnityEditor;

[CustomEditor( typeof( Spline ) )]
[CanEditMultipleObjects]
public class SplineEditor : Editor {

	    // draw lines between a chosen game object
	    // and a selection of added game objects

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Spline myScript = (Spline)target;
        if(GUILayout.Button("Make Spline from points"))
        {
            myScript.SetupSpline();
            Undo.RecordObject(myScript, "built spline");
        }
    }

	    void OnSceneGUI( )
	    {

	        Spline t = target as Spline;

	        if( t == null)
	            return;

					Vector3 pos = Vector3.zero;
					Vector3 curPos = Vector3.zero;
					for (int i = 0; i < t.SplinePoints.Count - (t.closed ? 0 : 1); i++) {
		 		 			for (int k = 0; k < t.curveFidelity; k++) {
                
								float j = (float)k / (float)(t.curveFidelity);
								curPos = t.GetPointAtIndex (i, j);

								if(i == 0 && k ==0){
									Handles.DrawLine(curPos, curPos);
									pos = curPos;
								}else{
									Handles.DrawLine(curPos, pos);
									pos = curPos;
								}

		 		 			}
		 		 		}

	        // iterate over game objects added to the array...
	        // for( int i = 0; i < t.GameObjects.Length; i++ )
	        // {
	        //     // ... and draw a line between them
	        //     if( t.GameObjects[i] != null )
	        //         Handles.DrawLine( center, t.GameObjects[i].transform.position );
	        // }
	    }
	}
