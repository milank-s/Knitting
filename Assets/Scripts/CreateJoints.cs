using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateJoints : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Setup();
    }

    void Setup(){
        Point[] points;
        List<Point> usedPoints = new List<Point>();
        points = GetComponentsInChildren<Point>();

        for(int i = 0; i < points.Length; i++){
            
            foreach(Point p in points[i]._neighbours){
                if(!usedPoints.Contains(points[i])){
                    SplineUtil.CreateJoint(points[i], p);
                }  
            }

            usedPoints.Add(points[i]);
        }
    }
}
