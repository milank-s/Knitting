using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bird : Crawler
{
    [SerializeField] BoidFlocking boidBehaviour;
    public void OnLeaveSpline(){
        //to do
    }

    public override void Step(){
        base.Step();

        bool onPlayerSpline = spline.isPlayerOn && spline.selectedIndex == curIndex; 
        //this might be broken for loops and reverse dirs
        
        if(onPlayerSpline){
            float playerSpeed = Services.PlayerBehaviour.actualSpeed;
            float playerProgress = Services.PlayerBehaviour.progress;

            bool sameDir = Services.PlayerBehaviour.goingForward == forward;
            bool inFront = (progress > playerProgress && Services.PlayerBehaviour.goingForward) || (progress < playerProgress && !Services.PlayerBehaviour.goingForward);
            bool evade = inFront;
            bool brake = !evade;

            float distanceToPlayer = Mathf.Abs(progress - Services.PlayerBehaviour.progress);
            float desiredSpeed = (playerSpeed + 1) * (sameDir ? 1 : -1);
            
            if(evade){ 
                speed = Mathf.Lerp(desiredSpeed, baseSpeed, distanceToPlayer);
            }else{
                speed = Mathf.Lerp(0, baseSpeed, distanceToPlayer);
            }
          
            //we reversed direction, act accordingly;

            if(speed < 0){
                ReverseDir();
            }
        }else{
            speed = Mathf.Lerp(speed, baseSpeed, Time.deltaTime * 2);
        }
    }

    //can fly off points in boid like behaviour
}
