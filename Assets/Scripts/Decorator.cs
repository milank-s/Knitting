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
        float distance = spline.distance;
        float step = distance/(float)amount;
        float progress = 0;
        float f = 0;
        Vector3 lastPoint = spline.GetPointAtIndex(0, 0);

        InstantiateDecor(progress, 0);

       for(int i = 0; i < spline.numPoints; i++){
         for(int j = 0; j < spline.curveFidelity; j++){
             float l = 1f/j;
             f += Vector3.Distance(lastPoint, spline.GetPointAtIndex(i, l));

             if(f > step){
               progress = l;
               f = 0;
               InstantiateDecor(progress, i);
           }
         }

           
           
       }
   }

    void Update(){
        if(!done){
            done = true;
            Setup();
        }

        if(!spline.drawingIn && Services.main.state != Main.GameState.paused){
            foreach(Decoration d in decorations){
                d.Step();
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
