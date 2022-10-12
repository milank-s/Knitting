using UnityEngine;
using UnityEditor;

[CustomEditor( typeof( Spline ) )]
[CanEditMultipleObjects]
public class SplineEditor : Editor {

  private const int stepsPerCurve = 5;
  private const float directionScale = 0.5f;
  private const float handleSize = 0.075f;
  private const float pickSize = 0.02f;
  private static GUILayoutOption miniButtonWidth = GUILayout.Width(20f);

  private static Color[] modeColors = {
    Color.white,
    Color.yellow,
    Color.cyan
  };

  private Spline spline;
  private Transform handleTransform;
  private Quaternion handleRotation;
  private int selectedIndex = -1;
  private Point PointInsert;

  SerializedObject GetTarget;
  SerializedProperty SplinePoints;
  int ListSize;

	    // draw lines between a chosen game object
	    // and a selection of added game objects


    public void OnEnable(){
          GetTarget = new SerializedObject(target);
          SplinePoints = GetTarget.FindProperty("SplinePoints"); // Find the List in our script and create a refrence of it
      }

    public override void OnInspectorGUI()
    {
	  
	    
        spline = (Spline)target;

        GetTarget.Update();

        
        DrawDefaultInspector();
        GUILayout.Space(25);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Insert",GUILayout.Width(50));
        PointInsert = (Point)EditorGUILayout.ObjectField(PointInsert, typeof(Point), true,GUILayout.Width(140));
        if(PointInsert != null){
          Undo.RecordObject(spline, "added point");
          spline.InsertPoint(PointInsert, 0);
          PointInsert = null;
        }
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(15);
        ShowList();
        GUILayout.Space(15);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Append", GUILayout.Width(50));
        PointInsert = (Point)EditorGUILayout.ObjectField(PointInsert, typeof(Point), true, GUILayout.Width(140));
        if(PointInsert != null){
          Undo.RecordObject(spline, "added point");
          spline.InsertPoint(PointInsert, spline.SplinePoints.Count);
          PointInsert = null;
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(15);
        Undo.RecordObject(spline, "closed");
        spline.closed = EditorGUILayout.Toggle("closed", spline.closed);
        
        //add closed function;
        //add lock button;
        GUILayout.Space(15);

        if(GUILayout.Button("Add Point"))
        {
            Undo.RecordObject(spline, "added point");
            spline.AddNewPoint(spline.SplinePoints.Count);
        }


        if(GUILayout.Button("Reverse Direction"))
        {
            Undo.RecordObject(spline, "reversed direction");
            spline.ReverseSpline();

        }
        GUILayout.Space(15);

        if(selectedIndex > -1){
          EditorGUILayout.LabelField("Selected Point");
          spline.SplinePoints[selectedIndex].tension = EditorGUILayout.Slider("Tension", spline.SplinePoints[selectedIndex].tension, -1, 1);
          spline.SplinePoints[selectedIndex].bias = EditorGUILayout.Slider("Bias", spline.SplinePoints[selectedIndex].bias, -1, 1);
          spline.SplinePoints[selectedIndex].continuity = EditorGUILayout.Slider("Continuity", spline.SplinePoints[selectedIndex].continuity, -1, 1);
        }

        GetTarget.ApplyModifiedProperties();

    }


  private void ShowList () {

		    for (int i = 0; i < SplinePoints.arraySize; i++) {
				      EditorGUILayout.BeginHorizontal();
              if(i == selectedIndex){
                EditorGUILayout.LabelField(">>>", GUILayout.Width(50));
              }else{
                if (GUILayout.Button(">",  GUIStyle.none, miniButtonWidth)) {
            			   selectedIndex = i;
                     ShowPoint(i);
            		}
              }
              EditorGUILayout.PropertyField(SplinePoints.GetArrayElementAtIndex(i), GUIContent.none);
				      ShowButtons(i);
				      EditorGUILayout.EndHorizontal();
			}
		}

	private void ShowButtons (int index) {
		if (GUILayout.Button("v", EditorStyles.miniButtonLeft, miniButtonWidth)) {
			   SplinePoints.MoveArrayElement(index, index + 1);
		}
		if (GUILayout.Button("+", EditorStyles.miniButtonMid, miniButtonWidth)) {
        // SplinePoints.InsertArrayElementAtIndex(index);
        spline.AddNewPoint(index);
		}
		if (GUILayout.Button("x", EditorStyles.miniButtonRight, miniButtonWidth)) {
      int oldSize = SplinePoints.arraySize;
      SplinePoints.DeleteArrayElementAtIndex(index);
      if (SplinePoints.arraySize == oldSize) {
      SplinePoints.DeleteArrayElementAtIndex(index);
  }
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

					for (int i = 0; i < spline.SplinePoints.Count - (spline.closed? 0 : 0); i++) {

            if(i < spline.SplinePoints.Count){
              ShowPoint(i);
            }

    				Handles.color = new Color(0.2f, 0.2f, 0.2f);

            if(i < spline.SplinePoints.Count - 1 || spline.closed){
    				      Handles.DrawDottedLine(spline.SplinePoints[i].Pos, spline.SplinePoints[(i + 1) % spline.SplinePoints.Count].Pos, 5f);
            }

            Handles.color = new Color(1,1,1);
	
		 		 			for (int k = 0; k < Spline.curveFidelity + 1; k++) {

								float j = (float)k / (float)(Spline.curveFidelity);
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

  			if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotHandleCap)) {
  				selectedIndex = index;
  				Repaint();
  			}

  			// check that the point is the one I want. Need to also get the spline
  			if (selectedIndex == index) {

  				EditorGUI.BeginChangeCheck();
  				point = Handles.DoPositionHandle(point, handleRotation);
  				if (EditorGUI.EndChangeCheck()) {
  					EditorUtility.SetDirty(spline);
            Undo.RecordObject(spline, "Move Point");
  					spline.SetPointPosition(index, point);
  				}
  			}
  		}
	}
