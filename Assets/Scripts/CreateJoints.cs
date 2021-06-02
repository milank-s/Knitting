using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateJoints : MonoBehaviour
{
    // Start is called before the first frame update
  
    bool instantiated;
    void Update(){
        if(!instantiated){
            Setup();
            instantiated = true;
        }
    }

    void Setup(){
        Point[] points;
        List<Point> usedPoints = new List<Point>();
        points = GetComponentsInChildren<Point>();

        for(int i = 0; i < points.Length; i++){
            
            foreach(Point p in points[i]._neighbours){
                if(!usedPoints.Contains(p)){
                    SplineUtil.CreateJoint(points[i], p);
                }  
            }

            usedPoints.Add(points[i]);
        }
    }
}
