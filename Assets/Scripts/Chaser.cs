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

    public override void SetNextPoint(){

        GetNextPoint();

        path = Pathfinding.FindPlayer(point);

        if(path != null){
            Point dest = path[1];
            spline = point.GetConnectingSpline(path[1]);
            forward = SplineUtil.GetDirection(point, dest, spline);
            curIndex = spline.GetPointIndex(point);
            dir = forward ? 1 : -1;
        }

        distance = spline.GetSegmentDistance(curIndex);
        EnterPoint(point);

    }
}
