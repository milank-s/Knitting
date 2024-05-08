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
    public override void Switching()
    {
        transform.position = point.Pos;
        SetNextPoint();
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

        point = spline.SplinePoints[next];
        path = Pathfinding.FindPlayer(point);
        
        //what if the player is on the same point that we are?
        //what do we do?
        if(path != null){
            moving = true;
            //we need to get to the player
            if(path.Count > 1){

            Point p = path[1];
            spline = point.GetConnectingSpline(p);
            curIndex = spline.GetPointIndex(point);
            bool newDir = SplineUtil.GetDirection(point, p, spline);
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

            dir = forward ? 1 : -1;
            progress = forward ? 0 : 1;
            
            }else{
                moving = false;
            }

        }else{
            //we cant get to the player, just idle

            GetNextPoint();
        }

        if(moving){
            distance = spline.GetSegmentDistance(curIndex);
            EnterPoint(point);
        }
    }
}
