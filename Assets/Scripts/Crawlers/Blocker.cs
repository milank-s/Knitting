using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blocker : Crawler
{
   
   public float speedLoss;
   public float force = 100;

   public override void EnterPoint(Point p){
        base.EnterPoint(p);

        //we need to figure out what these boys are getting up to

        if(p.pointType == PointTypes.stop){
            p.AddForce(Random.onUnitSphere * speed * force);
            p.distortion += 0.2f;
            Stop();
        }
   }

   public override void OnTriggerEnter(Collider col){
    
    //lets just make them only collide with the player so we dont need to waste time sorting tags
    //
    
    base.OnTriggerEnter(col);
    
    if(spawnTimer < spawnDelay) return;

    float p = Services.PlayerBehaviour.progress;
    
    //if(Services.PlayerBehaviour.state != PlayerState.Traversing) return;

    //this is very naive DOES NOT WORK ACROSS POINTS

    // if(forward == Services.PlayerBehaviour.goingForward){
    //     if((forward && progress > p) || (!forward && progress < p)){
    //         //destroyed by player from behind
    //         DamageMe();
    //     }else{
            
    //         DamagePlayer();
    //     }
    // }else{
    //     DamagePlayer();
    // }

    DamagePlayer();
   }

    void DamageMe(){
        Services.fx.EmitRadialBurst(10, 1, transform);
        Stop();
    }

   void DamagePlayer(){
    
        Services.fx.EmitRadialBurst(10, 1, transform);
        // Services.PlayerBehaviour.AddFlow(-speedLoss);
        Services.PlayerBehaviour.Lose();
        // base.spline.distortion += 0.2f;
        // Stop();
   }

   public override void Stop(){
    
        base.Stop();
   }
}
