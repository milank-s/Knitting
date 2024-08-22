using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Runner : Crawler
{
    [SerializeField] SphereCollider collider;
    [SerializeField] Collectible collectible;
    float timer = 0;
    bool caught = false;

    public override void Setup(Spline s, bool f, int startIndex = 0)
    {
        base.Setup(s, f);
        caught = false;
        timer = 0;
        collider.enabled = false;
        collectible.collider.enabled = false;

        collectible.SetTarget(transform);
        collectible.transform.position = transform.position;
        speed *= (float)(index + 1)/(float)controller.crawlerCount;
        collectible.flocking = false;
        Services.main.activeStellation.collectibles.Add(collectible);
    }

    public override void Stop(){

        //remove collectible from controller list, necessary?
        //maybe not
        collectible.Reset();

        base.Stop();
    }

    public override void Step()
    {
        if(!caught){
            base.Step();
            timer += Time.deltaTime;
            if(timer > 0.1f){
                collider.enabled = true;
            }
        }else{
            //transform.position = Services.PlayerBehaviour.visualRoot.position;
        }
    }

    public void Caught(){

        controller.CheckCrawlers();
        collectible.Pickup();
        running = false;
        collider.enabled = false;
        caught = true;

    }
    
    public override void OnTriggerEnter(Collider col){
        if(Services.PlayerBehaviour.state != PlayerState.Traversing) return;

        if(Services.PlayerBehaviour.curSpeed > speed && Services.PlayerBehaviour.curPoint == point && forward == Services.PlayerBehaviour.goingForward){
           Caught();
        }
    }

}
