using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bird : Crawler
{
    // [SerializeField] BoidFlocking boidBehaviour;

    bool flying = false;
    float minSpeed = 1;
    float maxSpeed = 5;
    Point pointDest;
    Vector3 velocity;
    Vector3 dest;
    Vector3 scale;
    float lerp;
    
    [SerializeField] TrailRenderer trail;

    public override void Init(CrawlerManager c)
    {
        base.Init(c);
        scale = transform.localScale;
    }
    public override void BreakOff()
    {
        base.BreakOff();
        lerp = 0;
        flying = true;
        velocity = (transform.forward * (speed + boost));
        dest = GetClosestPoint();
        trail.emitting = true;
        // boidBehaviour.SetVelocity(transform.forward * speed);
        // boidBehaviour.pointTarget = GetClosestPoint();
        // boidBehaviour.enabled = true;

    }

    Vector3 GetClosestPoint(){
        
        float minDist = Mathf.Infinity;
        Vector3 pos = transform.position + velocity;
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
        if(Services.PlayerBehaviour.state == PlayerState.Switching) return;

        if(!flying){
            BreakOff();
        }
        //fly awAY 

    }

    void FlyStep(){

        lerp += Time.deltaTime / 2f;
        Vector3 pointPos = pointDest.Pos;
        Vector3 diff = pointPos - transform.position;

        velocity = Vector3.Lerp(velocity, diff.normalized, Mathf.Sin(lerp * (Mathf.PI/2f)));
        
        float mag = velocity.magnitude;
        float dist = diff.magnitude;
        
        if(mag > maxSpeed) velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

        transform.position += velocity * Time.deltaTime;
        transform.forward = velocity;
       
        if(lerp > 1){
            trail.emitting = false;
        }

        if(dist < 0.01f){
            
            // boidBehaviour.enabled = false;
            trail.emitting = false;
            //Services.fx.BakeTrail(trail, Services.fx.flyingTrailMesh);
            curIndex = spline.GetPointIndex(pointDest);
            flying = false;
            progress = forward ? 0 : 1;

        }
    }

    
    public override void Step(){

        transform.localScale = Vector3.Lerp(scale, new Vector3(scale.x, scale.y, scale.z * 2), speed/baseSpeed);

        if(!flying){
            
            base.Step();
            

            bool onPlayerSpline = spline.isPlayerOn && spline.selectedIndex == curIndex; 
            //this might be broken for loops and reverse dirs
            
            if(onPlayerSpline){
                float playerSpeed = Services.PlayerBehaviour.curSpeed;
                float playerProgress = Services.PlayerBehaviour.progress;

                bool sameDir = Services.PlayerBehaviour.goingForward == forward;
                bool inFront = (progress > playerProgress && Services.PlayerBehaviour.goingForward) || (progress < playerProgress && !Services.PlayerBehaviour.goingForward);
                bool evade = inFront;

                float distanceToPlayer = Mathf.Abs(progress - Services.PlayerBehaviour.progress);
                float desiredSpeed = Mathf.Clamp(playerSpeed + 1, 0, Services.main.activeStellation.maxSpeed) * (sameDir ? 1 : -1);
                
                if(evade){ 
                    speed = Mathf.Lerp(desiredSpeed, baseSpeed, distanceToPlayer);
                }else{
                    speed = Mathf.Lerp(0, baseSpeed, distanceToPlayer);
                
                }
            
                //we reversed direction, act accordingly;

                if(speed < 0){
                    ReverseDir();
                    speed = -speed;
                }
            }else{
                speed = Mathf.Lerp(speed, baseSpeed, Time.deltaTime * 2);
            }
        }else{
            
            FlyStep();
        }
    }

}
