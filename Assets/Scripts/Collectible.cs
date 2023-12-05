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

    public void Awake(){
        startPos = transform.position;
    }

    public void Reset(){
        
        gameObject.SetActive(true);
        collected = false;
        deposited = false;
        done = false;

        if(!hasSpawnpoint){
            transform.position = startPos;
        }else{
            transform.position = spawnPoint.Pos;
        }
    }

    public void OnTriggerEnter(Collider col){
        if(!Services.PlayerBehaviour.hasCollectible){
            Pickup();
        }
    }

    public void Update(){
        
        if(flocking){
            boidBehaviour.SteerWithNeighbours();
        }

        if(!collected){
            if(hasSpawnpoint){
                //transform.position = spawnPoint.Pos;
            }
        }else{
            
            if(deposited && !done){
                
                if(Vector3.Distance(transform.position, depositPoint.Pos) < 0.05f){
                    HitPoint(); 
                }
            }else{
                //transform.position = Services.PlayerBehaviour.visualRoot.position;
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
        flocking = false;
        transform.position = depositPoint.Pos;
        done = true;
        
    }
    public void Pickup(){
        
        Debug.Log("caught");

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
        boidBehaviour.target = p.transform;
        depositPoint = p;
        deposited = true;
        Services.PlayerBehaviour.hasCollectible = false;
    }
}
