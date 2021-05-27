using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StellationRecorder : MonoBehaviour
{

    //list of splines in stellation
    //hooks for on point enter etc
    
    public float angleDiffThreshold = 10f;
    //dictionary for point pairs to make doubling impossible
    

    void Start()
    {
        
    }

    public void GenerateStellation(){
        //use cursortrail

        Point.Points.Clear();
        Spline.Splines.Clear();
        StellationController curStellation = Services.main.activeStellation;
        
        int children = curStellation.transform.childCount;
        for(int i = 0; i < children; i++){
            if( curStellation.transform.GetChild(i) != curStellation.transform){
                Destroy(curStellation.transform.GetChild(i).gameObject);
            }
        }

        Vector3[] positions = new Vector3[Services.fx.cursorTrail.numPositions];
        Services.fx.cursorTrail.GetPositions(positions);
        
        //we need to reset a bunch of other bollocks. I guess you can call the Reset delegate

        Transform pointParent = new GameObject().transform;
        pointParent.parent = curStellation.transform;

        //put point on start points;
        Point curPoint = SplineUtil.CreatePoint(positions[0]);
        curPoint.transform.parent = pointParent;
        curPoint.SetPointType(PointTypes.start);
        curStellation.start = curPoint;
        Spline curSpline = null;
        Point lastPoint;
        Vector3 lastDir = positions[0] - positions[1];
        Vector3 curTangent;
        int splineCount = 0;
        for(int i = 1; i < positions.Length; i++){
            Vector3 dir = positions[i] - positions[i-1];

            if(Vector3.Angle(lastDir, dir) > angleDiffThreshold){
                lastDir = dir;
                lastPoint = curPoint;
                curPoint = SplineUtil.CreatePoint(positions[i]);
                curPoint.pointType = PointTypes.ghost;
                Spline newSpline = SplineUtil.ConnectPoints(curSpline, lastPoint, curPoint).s;

                if(newSpline != curSpline || curSpline == null){
                    newSpline.order = splineCount;
                    newSpline.gameObject.name = splineCount.ToString();
                    splineCount ++;
                }

                curSpline = newSpline;
                curSpline.transform.parent = curStellation.transform;
                curPoint.transform.parent = pointParent;
            }
        }

        curPoint.pointType = PointTypes.end;

        Services.main.InitializeLevel();
        //now we're ready for the normal level start logic
    }

    void Update()
    {
        
    }
}
