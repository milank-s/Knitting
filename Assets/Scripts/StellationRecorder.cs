using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StellationRecorder : MonoBehaviour
{

    //list of splines in stellation
    //hooks for on point enter etc
    
    public float minAngleDiff = 10f;
    public float minDistance = 0.05f;

    public List<Vector3> positions;
    public int steps = 50;
    public float stepSize => 1/(float) steps;

    float lastProgress = 0;
    //dictionary for point pairs to make doubling impossible

    void Start()
    {
        positions = new List<Vector3>();
        Services.PlayerBehaviour.OnStartTraversing += StartRecording;
        Services.PlayerBehaviour.OnTraversing += RecordLine;
    }

    public void StartRecording(){
        lastProgress = Services.PlayerBehaviour.progress;
    }
    public void RecordLine(){
        //we can plug this into a Vectrosity thing now, dont need to use a trailrenderer

        float progress = Services.PlayerBehaviour.progress;
        if(Mathf.Abs(progress - lastProgress) > stepSize){
            positions.Add(Services.PlayerBehaviour.cursorPos);
            lastProgress = progress;
        }
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

        //Vector3[] positions = new Vector3[Services.fx.cursorTrail.numPositions];
       // Services.fx.cursorTrail.GetPositions(positions);
        
        //we need to reset a bunch of other bollocks. I guess you can call the Reset delegate

        Transform pointParent = new GameObject().transform;
        pointParent.parent = curStellation.transform;

        //put point on start points;
        Point curPoint = SplineUtil.CreatePoint(positions[0]);
        curPoint.transform.parent = pointParent;
        curPoint.SetPointType(PointTypes.start);
        curStellation.start = curPoint;
        Spline curSpline = null;
        Point lastPoint= curPoint;
        Vector3 lastDir = positions[0] - positions[1];
        Vector3 curTangent;
        int splineCount = 0;
        for(int i = 1; i < positions.Count; i++){
            Vector3 dir = positions[i] - lastPoint.Pos;

            if(Vector3.Angle(lastDir, dir) > minAngleDiff && dir.magnitude > minDistance){
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

        positions.Clear();
        //now we're ready for the normal level start logic
    }
}
