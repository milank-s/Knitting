using UnityEngine;
using UnityEditor;

[CustomEditor( typeof( Spline ) )]
[CanEditMultipleObjects]
public class SplineEditor : Editor {

  private const int stepsPerCurve = 5;
  private const float directionScale = 0.5f;
  private const float handleSize = 0.075f;
  private const float pickSize = 0.02f;

  private static Color[] modeColors = {
    Color.white,
    Color.yellow,
    Color.cyan
  };

  private Spline spline;
  private Transform handleTransform;
  private Quaternion handleRotation;
  private int selectedIndex = -1;
	    // draw lines between a chosen game object
	    // and a selection of added game objects

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Spline myScript = (Spline)target;
        if(GUILayout.Button("Add Point"))
        {
            myScript.AddNewPoint();
            Undo.RecordObject(myScript, "added point");
        }
        if(GUILayout.Button("Remove End Point"))
        {
            myScript.RemoveEndPoint();
            Undo.RecordObject(myScript, "removed end");
        }
        if(GUILayout.Button("Make Spline from points"))
        {
            myScript.SetupSpline();
            Undo.RecordObject(myScript, "built spline");
        }
    }

	    void OnSceneGUI( )
	    {

	        spline = target as Spline;
          handleTransform = spline.transform;
          handleRotation = handleTransform.rotation;

	        if( spline == null)
	            return;

					Vector3 pos = Vector3.zero;
					Vector3 curPos = Vector3.zero;

					for (int i = 0; i < spline.SplinePoints.Count; i++) {

            selectedIndex = i;
            ShowPoint(i);

    				Handles.color = new Color(0.2f, 0.2f, 0.2f);
    				Handles.DrawDottedLine(spline.SplinePoints[i].Pos, spline.SplinePoints[(i + 1) % spline.SplinePoints.Count].Pos, 5f);

            Handles.color = new Color(1,1,1);

		 		 			for (int k = 0; k < spline.curveFidelity + 1; k++) {

								float j = (float)k / (float)(spline.curveFidelity);
								curPos = spline.GetPointAtIndex (i, j);

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


      private void ShowPoint (int index) {
  			//All functionality for selected points
  			Handles.color = new Color(1,1,1);

  			Vector3 point = spline.SplinePoints[index].Pos;
  			float size = HandleUtility.GetHandleSize(point);

  			if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotCap)) {
  				selectedIndex = index;
  				Repaint();
  			}

  			//check that the point is the one I want. Need to also get the spline
  			if (selectedIndex == index) {

  				EditorGUI.BeginChangeCheck();
  				point = Handles.DoPositionHandle(point, handleRotation);
  				if (EditorGUI.EndChangeCheck()) {
  					Undo.RecordObject(spline, "Move Point");
  					EditorUtility.SetDirty(spline);
  					spline.SetPointPosition(index, point);
  				}
  			}
  		}
	}
