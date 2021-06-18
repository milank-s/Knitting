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
    bool done;
   public void Start(){
       
   }
   public void Setup(){
    
        decorations = new List<Decoration>();
        float step = spline.distance/(float)amount;
        float f = 0;
        Vector3 lastPoint = spline.SplinePoints[0].Pos;

        InstantiateDecor(0, 0);

       for(int i = 0; i < spline.numPoints; i++){
         for(int j = 0; j < Spline.curveFidelity; j++){

             float l = (float)j/Spline.curveFidelity;
             Vector3 curPoint = spline.GetPointAtIndex(i, l);
             f += Vector3.Distance(lastPoint, curPoint);
             lastPoint = curPoint;

             if(f > step){
               f = 0;
               InstantiateDecor(l, i);
           }
         }
       }
   }

    void Update(){
        

        if(!spline.drawingIn && Services.main.state != Main.GameState.paused){

            if(!done){
                done = true;
                Setup();
            }else{
                foreach(Decoration d in decorations){
                    d.Step();
                }
            }
        }
    }

   public void InstantiateDecor(float progress, int pointIndex){

        //use a segment distance taken from the vectrosity line
        //so that during distortion the decoration follow the line
        
        Decoration newD = Instantiate(prefab);
        newD.transform.parent = transform;
        newD.Init(spline, progress, pointIndex, speed);
        newD.mesh.sprite = sprite;
        decorations.Add(newD);
   }
}
