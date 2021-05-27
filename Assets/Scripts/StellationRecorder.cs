using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vectrosity;
public class StellationRecorder : MonoBehaviour
{

    //list of splines in stellation
    //hooks for on point enter etc
    VectorLine line;
    public float minAngleDiff = 30f;
    public float maxAngleDiff = 90;
    public float minDistance = 0.1f;
    public float maxDistance = 0.66f;

    public List<Vector3> positions;
    public int steps = 5;
    public float stepSize => 1/(float) steps;

    float lastProgress = 0;
    //dictionary for point pairs to make doubling impossible

    void Start()
    {
        line = new VectorLine (name, new List<Vector3> (0), 2, LineType.Continuous, Vectrosity.Joins.Weld);
        line.color = new Color(1,1,1,0.25f);
        line.smoothWidth = true;
        line.smoothColor = true;

        positions = new List<Vector3>();
        Services.PlayerBehaviour.OnStartTraversing += StartRecording;
        Services.PlayerBehaviour.OnStoppedTraversing += EnterPoint;
        Services.PlayerBehaviour.OnTraversing += RecordLine;
    }

    public void StartRecording(){
        lastProgress = Services.PlayerBehaviour.progress;
        line.Draw3DAuto();
    }

    public void EnterPoint(){
        positions.Add(Services.PlayerBehaviour.cursorPos);
    }
    public void RecordLine(){
        //we can plug this into a Vectrosity thing now, dont need to use a trailrenderer

        float progress = Services.PlayerBehaviour.progress;
        if(Mathf.Abs(progress - lastProgress) > stepSize){
            positions.Add(Services.PlayerBehaviour.cursorPos);
            lastProgress = progress;
            line.points3 = positions;
        }
    }
    public void GenerateStellation(){
        //use cursortrail

        Services.main.state = Main.GameState.paused;

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
        Vector3 lastDir = positions[1] - positions[0];
        Vector3 curTangent;
        int splineCount = 0;
        for(int i = 1; i < positions.Count; i++){
            Vector3 dir = positions[i] - curPoint.Pos;
            float angle = Vector3.Angle(lastDir, dir);

            if((angle > minAngleDiff && angle < maxAngleDiff && dir.magnitude > minDistance) || dir.magnitude > maxDistance){

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
        line.points3 = positions;
        
        Services.main.state = Main.GameState.playing;

        //now we're ready for the normal level start logic
    }
}
