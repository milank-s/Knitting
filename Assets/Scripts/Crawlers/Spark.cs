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

    public override void Setup(Spline s, bool f, int startIndex = 0){
        base.Setup(s, f, startIndex);
        if(visited == null){
            visited = new List<Point>();
        }
    }
    
    public override void SetNextPoint()
    {
        
        bool curDir = forward;

        base.SetNextPoint();

        if(visited.Contains(curPoint)){
            Debug.Log("already visited, stopping");
            Stop();
            return;
        }

        //wait why the fuck would curpoint be null

        foreach(Point p in curPoint._neighbours){
            foreach(Spline s in p.GetConnectingSplines(curPoint)){
                if(s == spline) continue;
                
                // spawn a spark going to this point

                Spark newCrawler = (Spark)controller.SpawnCrawler(CrawlerType.spark);
                bool f = s.IsGoingForward(curPoint, p);
                int i = f ? s.GetPointIndex(curPoint) : s.GetPointIndex(p);
                newCrawler.Setup(s, f, i);
            }
        }

        visited.Add(curPoint);

        if(curDir != forward){
            Debug.Log("reached end of not-closed line");
            Stop();
        }
    }
}
