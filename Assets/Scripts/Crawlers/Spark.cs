using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spark : Crawler
{
    //start at point
    //at each intersection, add to list of points
    //spawn a new spark for any branching paths

    List<Point> visited;
    public void Start(){
        visited = new List<Point>();
    }
    
    public override void SetNextPoint()
    {
        
        Debug.Log("spark setting next point");

        Point lastPoint = point;
        base.SetNextPoint();
        
        if(visited.Contains(point)){
            //I'm done
            Stop();
            //play particle effect
        }else{

            visited.Add(point);
            Point nextPoint = spline.GetNextPoint(curIndex, forward);
    
            foreach(Point p in point._neighbours){

                    Spline s = p.GetConnectingSpline(point);
                    if(s == spline) continue;
                    
                    Debug.Log("spawning new spark");
                    // spawn a spark going to this point

                    Spark newCrawler = (Spark)controller.SpawnCrawler(CrawlerType.spark);
                    bool f = s.IsGoingForward(point, p);
                    newCrawler.Setup(s, f);
                    
                    newCrawler.curIndex = f ? s.GetPointIndex(point) : s.GetPointIndex(p);
                    newCrawler.point = f ? point : p;
                    
                
            }
        }
    }
}
