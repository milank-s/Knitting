using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blocker : Crawler
{
   
   public float speedLoss;
   public float force = 100;

   public override void EnterPoint(Point p){
        base.EnterPoint(p);

        if(p.pointType != PointTypes.ghost){
            p.AddForce(Random.onUnitSphere * speed * force);
            p.distortion += 0.2f;
            Stop();
        }
   }

   public void OnTriggerEnter(Collider col){
    
    //lets just make them only collide with the player so we dont need to waste time sorting tags
    //

    if(Services.PlayerBehaviour.state != PlayerState.Traversing || spline != Services.PlayerBehaviour.curSpline) return;

    if(base.curIndex != spline.selectedIndex) return; 

    float p = Services.PlayerBehaviour.progress;

    if(forward == Services.PlayerBehaviour.goingForward){
        if((forward && progress > p) || (!forward && progress < p)){
            //destroyed by player from behind
            DamageMe();
        }else{
            Debug.Log("ow");
            DamagePlayer();
        }
    }else{
        DamagePlayer();
    }

   }

    void DamageMe(){
        Services.fx.EmitLinearBurst(10, 1, transform, Services.PlayerBehaviour.transform.forward);
        Stop();
    }

   void DamagePlayer(){
    
        Services.fx.EmitRadialBurst(10, 1, transform);
        Services.PlayerBehaviour.AddFlow(-speedLoss);
        base.spline.distortion += 0.2f;
        Stop();
   }

   public override void Stop(){
    
        base.Stop();
   }
}
