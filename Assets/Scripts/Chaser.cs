using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chaser : Crawler
{
    List<Point> path;
    void Start()
    {
        path = new List<Point>();
    }

    public void Update(){
        for(int i = 0; i < path.Count; i++){
            if(i < path.Count -1){
                Debug.DrawLine(path[i].Pos, path[i+1].Pos);
            }
        }
    }
    public override void SetNextPoint(){

        GetNextPoint();

        path = Pathfinding.FindPlayer(point);
        
        //what if the player is on the same point that we are?
        //what do we do?
        if(path != null){

            if(path.Count > 0 && path[path.Count-1] != point){

                Point nextPoint = path[1];
                spline = point.GetConnectingSpline(nextPoint);
                curIndex = spline.GetPointIndex(point);
                forward = SplineUtil.GetDirection(point, nextPoint, spline);
                dir = forward ? 1 : -1;
                progress = forward ? 0 : 1;
                
                if(!forward){
                    
                    if(curIndex == 0){
                        if(spline.closed){
                            curIndex = spline.numPoints - 1;
                        }else{
                            curIndex = spline.numPoints - 1;
                        }
                    }else{
                        curIndex --;
                    }
                }
            }
        }

        Debug.Log("index = " + curIndex);
        distance = spline.GetSegmentDistance(curIndex);
        EnterPoint(point);

    }
}
