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

    public void OnDrawGizmos(){
        for(int i = 0; i < path.Count; i++){
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(path[i].Pos, 3);
            if(i < path.Count -1){
                Gizmos.DrawLine(path[i].Pos, path[i+1].Pos);
            }
        }
    }
    public override void SetNextPoint(){

        //if you have reached the end going forward, update your current point
        //if you have reached the end going backward, you are already there

        int next = curIndex;
        if(forward){
            if(curIndex < spline.SplinePoints.Count - 1){
                next ++;
            }else{
                if(spline.closed){
                    next = 0;
                }
            }
        }

        Point nextPoint = spline.SplinePoints[next];
        path = Pathfinding.FindPlayer(nextPoint);
        
        //what if the player is on the same point that we are?
        //what do we do?
        if(path != null && path.Count > 1){
            point = nextPoint;
            Point p = path[1];
            spline = point.GetConnectingSpline(p);
            curIndex = spline.GetPointIndex(point);
            bool newDir = SplineUtil.GetDirection(point, p, spline);

            if(newDir != forward){

                forward = newDir;
                
                if(!forward){
                    if(curIndex == 0){
                        if(spline.closed){
                            curIndex = spline.numPoints - 1;
                        }
                    }else{
                        curIndex --;
                    }
                }
            }

            dir = forward ? 1 : -1;
            progress = forward ? 0 : 1;

        }else{
            GetNextPoint();
        }

        distance = spline.GetSegmentDistance(curIndex);
        EnterPoint(point);

    }
}
