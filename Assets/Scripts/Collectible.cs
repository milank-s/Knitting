using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    
    [SerializeField] BoidFlocking boidBehaviour;

    bool collected = false; 
    public bool deposited = false;
    public bool hasSpawnpoint;
    public Point spawnPoint;
    public SphereCollider collider;
    Vector3 startPos;

    public void Awake(){
        startPos = transform.position;
    }

    public void Reset(){
        
        gameObject.SetActive(true);
        collected = false;
        deposited = false;

        if(!hasSpawnpoint){
            //transform.position = startPos;
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
        if(!collected){
            if(hasSpawnpoint){
                transform.position = spawnPoint.Pos;
            }
        }else{
            boidBehaviour.Steer();
            
            if(deposited){
                //transform.Rotate(0, 0, Time.deltaTime * 60);
                //transform.position = target.position;
            }else{
                //transform.position = Services.PlayerBehaviour.visualRoot.position;
            }
        }
    }

    public void SetPoint(Point p){
        spawnPoint = p;
        hasSpawnpoint = true;
    }
    
    public void Pickup(){
        if(Services.main.activeStellation.OnPickup != null){
            Services.main.activeStellation.OnPickup.Invoke();
        }

        Services.PlayerBehaviour.hasCollectible = true;
        Services.PlayerBehaviour.collectible = this;

        collected = true;
        collider.enabled = false;

        boidBehaviour.enabled = true;
        boidBehaviour.SetVelocity(transform.forward * Services.PlayerBehaviour.curSpeed);
        boidBehaviour.target = Services.Player.transform;

    }
    public void Deposit(Point p){
        boidBehaviour.target = p.transform;
        deposited = true;
        Services.PlayerBehaviour.hasCollectible = false;
        Services.fx.PlayAnimationAtPosition(FXManager.FXType.burst, transform);
        Services.fx.EmitRadialBurst(20, 1, transform);

        boidBehaviour.enabled = false;
    }
}
