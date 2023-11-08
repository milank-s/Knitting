using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    bool collected = false; 
    public bool deposited = false;
    public SphereCollider collider;
    Vector3 startPos;

    public void Reset(){
        transform.position = startPos;
        gameObject.SetActive(true);
        collected = false;
        deposited = false;
    }

    public void OnTriggerEnter(Collider col){
        if(!Services.PlayerBehaviour.hasCollectible){
            Pickup();
        }
    }

    public void Update(){
        if(collected){
            transform.position = Services.PlayerBehaviour.visualRoot.position;
        }
    }
    public void Pickup(){
        Services.PlayerBehaviour.hasCollectible = true;
        Services.PlayerBehaviour.collectible = this;
        collected = true;
        collider.enabled = false;
    }
    public void Deposit(){
        deposited = true;
        Services.PlayerBehaviour.hasCollectible = false;
        Services.fx.PlayAnimationAtPosition(FXManager.FXType.burst, transform);
        Services.fx.EmitRadialBurst(20, 1, transform);
        gameObject.SetActive(false);
    }
}
