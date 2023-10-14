using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bird : Crawler
{
    // [SerializeField] BoidFlocking boidBehaviour;

    bool flying = false;

    Point pointDest;
    Vector3 velocity;
    Vector3 dest;
    float lerp;
    public override void BreakOff()
    {
        base.BreakOff();
        lerp = 0;
        flying = true;
        velocity = (transform.forward * speed);
        dest = GetClosestPoint();
        // boidBehaviour.SetVelocity(transform.forward * speed);
        // boidBehaviour.pointTarget = GetClosestPoint();
        // boidBehaviour.enabled = true;

    }

    Vector3 GetClosestPoint(){
        
        float minDist = Mathf.Infinity;
        Vector3 pos = transform.position;
        foreach(Point p in spline.SplinePoints){
            if(p == point) continue;

            float dist = Vector3.Distance(p.Pos, pos);
            if(dist < minDist){
                minDist = dist;
                pointDest = p;
            }
        }

        return pointDest.Pos;
    }
    public override void OnTriggerEnter(Collider col){
        base.OnTriggerEnter(col);

        if(!flying){
            BreakOff();
        }
        //fly awAY 

    }

    void FlyStep(){

        lerp += Time.deltaTime;
        Vector3 diff = dest - transform.position;

        velocity = Vector3.Lerp(velocity, diff, lerp);
        if(velocity.magnitude < 1) velocity.Normalize();
        transform.position += velocity * Time.deltaTime;

        if(Vector3.Distance(pointDest.Pos, transform.position) < 0.05f){
            
            // boidBehaviour.enabled = false;
            Debug.Log("back on lines");
            curIndex = spline.GetPointIndex(pointDest);
            flying = false;
            progress = forward ? 0 : 1;

        }
    }

    public override void Step(){

        if(!flying){
            
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
        }else{
            // boidBehaviour.Steer();
            FlyStep();
        }
    }

}
