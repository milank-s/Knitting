using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spark : Crawler
{
    //start at point
    //at each intersection, add to list of points
    //spawn a new spark for any branching paths
    //doesn't loop

    List<Point> visited;

    public override void Setup(Spline s, bool f, int startIndex = 0){
        base.Setup(s, f, startIndex);
        
        visited = new List<Point>();
        visited.Add(point);
    }
    
    public override void SetNextPoint()
    {
        bool curDir = forward;

        base.SetNextPoint();

        if(curPoint != null){
            foreach(Point p in curPoint._neighbours){

                Spline s = p.GetConnectingSpline(curPoint);
                if(s == spline) continue;
                
                // spawn a spark going to this point

                Spark newCrawler = (Spark)controller.SpawnCrawler(CrawlerType.spark);
                bool f = s.IsGoingForward(curPoint, p);
                int i = f ? s.GetPointIndex(curPoint) : s.GetPointIndex(p);
                newCrawler.Setup(s, f, i);
            }
        }

        if(visited.Contains(point) || curDir != forward){
            //I'm done
            Stop();
            //play particle effect
        }
        
        visited.Add(point);
    }
}
