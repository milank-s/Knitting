using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spark : Crawler
{
    //start at point
    //at each intersection, add to list of points
    //spawn a new spark for any branching paths
    //doesn't loop
    public static List<Point> visited;
    Point lastPoint;
    public override void Setup(Spline s, bool f, int startIndex = 0){
        base.Setup(s, f, startIndex);
        if(visited == null){
            visited = new List<Point>();
        }
    }
    
    public override void SetNextPoint()
    {
        
        bool curDir = forward;

        lastPoint = curPoint;

        base.SetNextPoint();

        if(visited.Contains(curPoint)){
            Stop();
            return;
        }

        bool emitted = false;

        Debug.Log("curPoint = " + curPoint.name);

        foreach(Point p in curPoint._neighbours){
            foreach(Spline s in curPoint.GetConnectingSplines(p)){
                if(s == spline) continue;
                // spawn a spark going to this point

                emitted = true;
                Spark newCrawler = (Spark)controller.SpawnCrawler(CrawlerType.spark);
                bool f = s.IsGoingForward(curPoint, p);
                int i = f ? s.GetPointIndex(curPoint) : s.GetPointIndex(p);
                Debug.Log("going from " + curPoint.name + " to " + p.name + " dir = " + f + " index = " + i);
                newCrawler.Setup(s, f, i);
            }
        }

        visited.Add(curPoint);

        if(curDir != forward){
            
            Stop();
        }
    }
}
