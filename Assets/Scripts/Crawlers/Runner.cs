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
        //speed *= (float)(index + 1)/(float)controller.crawlerCount;
        collectible.flocking = false;
        Services.main.activeStellation.collectibles.Add(collectible);
    }

    public override void Stop(){

        //remove collectible from controller list, necessary?
        //maybe not
        collectible.Reset();

        base.Stop();
    }
    public override void Switching()
    {
        transform.position = point.Pos;
        if(Pathfinding.furthestPoint != null && Pathfinding.furthestPoint != point){
            SetNextPoint();
        }
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

    public override void SetNextPoint(){

        //if you have reached the end going forward, update your current point
        //if you have reached the end going backward, you are already there
        
        int next = curIndex;
        if(forward){
            if(curIndex < spline.SplinePoints.Count - 1){
                next ++;
            }else{
                if(spline.closed){
                    next = 0;
                }
            }
        }

        point = spline.SplinePoints[next];
        List<Point> path = Pathfinding.GetLongestPath(point, Pathfinding.furthestPoint);
        
        
        //what if the player is on the same point that we are?
        //what do we do?
        if(path != null){
        
            moving = true;
            //we need to get to the player
            if(path.Count > 1){

            Point p = path[1];
            spline = point.GetConnectingSpline(p);
            curIndex = spline.GetPointIndex(point);
            bool newDir = SplineUtil.GetDirection(point, p, spline);
            forward = newDir;

            if(!forward){
                if(curIndex == 0){
                    if(spline.closed){
                        curIndex = spline.numPoints - 1;
                    }
                }else{
                    curIndex --;
                }
            }

            dir = forward ? 1 : -1;
            progress = forward ? 0 : 1;
            
            }else{
                moving = false;
            }

        }else{
            //we cant get to the player, just idle

            GetNextPoint();
        }

        if(moving){
            distance = spline.GetSegmentDistance(curIndex);
            EnterPoint(point);
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

        //why?

        // if(Services.PlayerBehaviour.curSpeed > speed && Services.PlayerBehaviour.curPoint == point && forward == Services.PlayerBehaviour.goingForward){
           Caught();
        // }
    }

}
