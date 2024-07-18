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
        Point lastPoint = point;
        base.SetNextPoint();
        
        if(visited.Contains(point)){
            //I'm done
        }else{

            visited.Add(point);

            foreach(Point p in point._neighbours){
                
                if(p != lastPoint){
                    Debug.Log("spawning new spark");
                    // spawn a spark going to this point
                    // Spark newCrawler = (Spark)controller.SpawnCrawler(CrawlerType.spark);
                    // Spline s = p.GetConnectingSpline(point);
                    // bool f = s.IsGoingForward(point, p);
                    // newCrawler.Setup(s, f);
                    
                    // newCrawler.curIndex = f ? s.GetPointIndex(point) : s.GetPointIndex(p);
                    // newCrawler.point = f ? point : p;
                    
                }
            }
        }
    }
}
