using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    public BoidFlocking boidBehaviour;

    public bool collected = false; 
    public bool deposited = false;
    public bool done = false;
    public bool flocking;

    public bool hasSpawnpoint;
    public Point spawnPoint;
    Point depositPoint;
    public SphereCollider collider;
    Vector3 startPos;
    float speed;
    
    public void Awake(){
        startPos = transform.position;
    }

    public void Reset(){
        
        gameObject.SetActive(true);
        collected = false;
        deposited = false;
        done = false;
        flocking = false;
        depositPoint = null;

        if(!hasSpawnpoint){
            transform.position = startPos;
        }else{
            transform.position = spawnPoint.Pos;
        }
    }

    public void Update(){
        
        if(flocking){
            boidBehaviour.SteerWithNeighbours();
        }

        if(!collected){
            if(hasSpawnpoint){
                transform.position = spawnPoint.Pos;
            }
        }else{
            
            if(deposited){
                
                if(done){
                     transform.position = depositPoint.Pos;
                }else{
                    speed += Time.deltaTime;
                    transform.position = Vector3.MoveTowards(transform.position, depositPoint.Pos, speed * Time.deltaTime);
                    if(Vector3.Distance(transform.position, depositPoint.Pos) < 0.025f) HitPoint();
                }

            }
        }
    }

    public void SetPoint(Point p){
        spawnPoint = p;
        hasSpawnpoint = true;
        boidBehaviour.target = p.transform;
    }

    public void SetTarget(Transform t){
        boidBehaviour.target = t;
    }
    
    void HitPoint(){
        
        Services.fx.PlayAnimationAtPosition(FXManager.FXType.burst, depositPoint.transform);
        Services.fx.EmitRadialBurst(20, 1, depositPoint.transform);
        done = true;
    }

    public void Pickup(){

        if(Services.main.activeStellation.OnPickup != null){
            Services.main.activeStellation.OnPickup.Invoke();
        }

        Services.PlayerBehaviour.AddCollectible(this);

        collected = true;
        collider.enabled = false;

        flocking = true;
        boidBehaviour.SetVelocity(transform.forward * Services.PlayerBehaviour.curSpeed);
        boidBehaviour.target = Services.Player.transform;

    }

    public void Deposit(Point p){
        p.collectible = this;
        flocking = false;
        boidBehaviour.target = p.transform;
        speed = boidBehaviour.speed;
        depositPoint = p;
        deposited = true;
        Services.PlayerBehaviour.RemoveCollectible(this);
    }
}
