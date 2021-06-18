using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Spline;
public class Decorator : MonoBehaviour
{
   public Spline spline;
   public Decoration prefab;
    public Sprite sprite;
    public float speed;
    public int amount = 10;
   List<Decoration> decorations;

   public void Setup(){

       int numPerSegment = amount / spline.numPoints;
       float step = 1f/(float)numPerSegment;
        float progress = 0;
       for(int i = 0; i < spline.numPoints; i++){
           while(progress < 1){
                int segmentIndex = i * curveFidelity + (int)(Spline.curveFidelity * progress);
                InstantiateDecor(segmentIndex);
                progress += step;
           }
       
       }
   }

   public void InstantiateDecor(int segmentIndex, float progress, int pointIndex){

        //use a segment distance taken from the vectrosity line
        //so that during distortion the decoration follow the line
        
        Decoration newD = Instantiate(prefab);
        newD.transform.parent = transform;
        newD.Init(progress, pointIndex, speed);
        newD.mesh = sprite;
   }
}
