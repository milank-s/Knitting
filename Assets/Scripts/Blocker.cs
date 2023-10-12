using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blocker : Crawler
{
   
   public float speedLoss;

   public override void EnterPoint(Point p){
        p.distortion += 1;
   }
   
   public void OnTriggerEnter(Collider col){
    
    //lets just make them only collide with the player so we dont need to waste time sorting tags
    //
    float p = Services.PlayerBehaviour.progress;

    if(forward == Services.PlayerBehaviour.goingForward){
        if((forward && progress > p) || (!forward && progress < p)){
            //destroyed by player
            DamageMe();
        }else{
            DamagePlayer();
        }
    }else{
        DamagePlayer();
    }

   }

    void DamageMe(){
        Stop();
    }

   void DamagePlayer(){
        Services.PlayerBehaviour.AddFlow(-speedLoss);
        Stop();
   }

   public override void Stop(){
    
        Services.fx.EmitRadialBurst(25, 1, transform);
        base.Stop();
   }
}
