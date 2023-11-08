using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Runner : Crawler
{
    [SerializeField] SphereCollider collider;
    float timer = 0;
    public override void Setup(Spline s, bool f)
    {
        base.Setup(s, f);
        timer = 0;
        collider.enabled = false;
        speed *= (float)(index + 1)/(float)controller.crawlerCount;
    }

    public override void Step()
    {
        base.Step();
        timer += Time.deltaTime;
        if(timer > 0.1f){
            collider.enabled = true;
        }
    }

    public void Caught(){

        //particle effect
        //tell player something
        Debug.Log("caught");

        controller.HasCrawlers();
        Services.fx.EmitRadialBurst(100, 10, Services.Player.transform);
        Stop();

    }

    public override void GetNextPoint()
    {
        // if(curIndex < spline.SplinePoints.Count - 1){
            base.GetNextPoint();
        // }else{
        //     Stop();
        // }
    }
    public override void OnTriggerEnter(Collider col){
        if(Services.PlayerBehaviour.state == PlayerState.Switching) return;

        if(Services.PlayerBehaviour.curSpeed > speed && Services.PlayerBehaviour.curPoint == point && Services.PlayerBehaviour.progress < progress && forward == Services.PlayerBehaviour.goingForward){
            Caught();
        }
    }

}
