using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 


Crawler : MonoBehaviour
{
    public bool useSpline = true;
    public bool forward = true;
    protected Spline spline;
    protected Point point;
    protected int curIndex;
    public float baseSpeed = 1;
    protected float boost;
    protected float speed;
    public bool running;
    protected float progress;
    protected float distance;

    protected Vector3 lastPos;
    protected Vector3 delta;
    protected int dir;
    protected int index;
    protected CrawlerManager controller;
    
    public virtual void Init(CrawlerManager c){
        controller = c;
        running = false;
        index = c.GetCrawlerIndex(this);
    }

    public virtual void Setup(Spline s, bool f)
    {
        speed = baseSpeed;
        curIndex = f? 0 : s.SplinePoints.Count - 1;
        point = s.SplinePoints[curIndex];
        forward = f;
        progress = forward ? 0 : 1;
        spline = s;
        running = true;
        dir = forward ? 1 : -1;
        transform.position = s.SplinePoints[curIndex].Pos;
        lastPos = transform.position;

    }

    public virtual void Step()
    {
        if (running)
        {
            if(progress < 0 || progress > 1){
                if(point.pointType != PointTypes.ghost){
                    OnPoint();
                }
                SetNextPoint();
            }

            boost = Mathf.Lerp(boost, 0, Time.deltaTime * 2);
            //why arent we dividing by distance?
            progress += (((speed + boost) * dir)/(distance)) * Time.deltaTime;
               
            //hacky fix to spline distances not being populated at first

            if(distance == 0) return;
            
            transform.position = spline.GetPointAtIndex(curIndex, progress);
            delta = (transform.position - lastPos);
            lastPos = transform.position;
            
            // if(delta.sqrMagnitude > 0){
            //     transform.forward = delta.normalized;
            // }
        }
    }

    public virtual void OnTriggerEnter(Collider col){
        
    }

    public virtual bool CollidedOnSpline(){
         if(Services.PlayerBehaviour.state != PlayerState.Traversing || !spline.isPlayerOn) return false;

        if(curIndex != spline.selectedIndex) return false;

        return true; 
    }

    public void ReverseDir(){
        forward = !forward;
        dir = forward ? 1 : -1;
        speed = Mathf.Abs(speed);
    }

    public virtual void BreakOff(){
    }

    public virtual void OnPoint(){
        
        boost += Point.boostAmount;
    }
    public virtual void SetNextPoint()
    {
        
        GetNextPoint();

        distance = spline.GetSegmentDistance(curIndex);
        EnterPoint(point);
        
    }

    public void GetNextPoint(){
        progress = forward ? 0 : 1;
        
        if(forward){
            if(curIndex < spline.SplinePoints.Count - 1)
            {
                curIndex++;
            }
            else
            {
                curIndex = 0;
            }
        }else{
            if(curIndex > 0)
            {
                curIndex--;
            }
            else
            {
                curIndex = spline.SplinePoints.Count -1;
            }
        }
        
        point = spline.SplinePoints[curIndex];
    }

    public virtual void EnterPoint(Point p){
        p.OnPointEnter();
    }

    public virtual void Stop()
    {
        running = false;
        gameObject.SetActive(false);
    }
}
