using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 


Crawler : MonoBehaviour
{
    public bool useSpline = true;
    public bool forward = true;
    protected Spline spline;

    //this is merely the reference to the point we base movement off
    public Point point;

    //this is the last point that we arrived at
    protected Point curPoint;
    public int curIndex;
    public float baseSpeed = 1;
    protected float boost;
    protected float speed;
    public bool running;
    public bool moving;
    public bool faceForward = false;
    protected float progress;
    protected float distance;
    protected bool startDir;
    protected Spline startSpline;
    protected Vector3 lastPos;
    protected Vector3 delta;
    protected int startIndex;
    protected int dir;
    protected int index;
    protected CrawlerManager controller;
    
    public virtual void Init(CrawlerManager c){
        controller = c;
        running = false;
        index = c.GetCrawlerIndex(this);
    }

    public virtual void Setup(Spline s, bool f, int i = 0)
    {
        baseSpeed = s.crawlerSpeed;
        forward = f;
        startIndex = i;
        startSpline = s;
        startDir = f;
        Restart();
    }

    public virtual void Restart(){   
        moving = true;
        curIndex = startIndex;
        spline = startSpline;
        speed = baseSpeed;
        point = spline.SplinePoints[curIndex];
        progress = forward ? 0 : 1;
        running = true;
        dir = forward ? 1 : -1;
        transform.position = spline.SplinePoints[curIndex].Pos;
        distance = spline.GetDistance(curIndex);
        lastPos = transform.position;
    }

    public virtual void Step()
    {
        if (running)
        {
            if(moving){
                if(progress < 0 || progress > 1){
                    
                    if(point.pointType != PointTypes.ghost){
                        OnHitPoint();
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
                
                if(faceForward && delta.sqrMagnitude > 0){
                    transform.forward = delta.normalized;
                }
            }else{
                Switching();
            }
        }
    }

    public virtual void Switching(){

    }

    public virtual void OnTriggerEnter(Collider col){
        
    }

    public virtual bool CollidedOnSpline(){
         if(Services.PlayerBehaviour.state != PlayerState.Traversing || !spline.isPlayerOn) return false;

        if(curIndex != spline.selectedIndex) return false;

        return true; 
    }

    public virtual void ReverseDir(){
        forward = !forward;
        dir = forward ? 1 : -1;
    }

    public virtual void BreakOff(){
    }

    public virtual void OnHitPoint(){
        
        // boost = Point.boostAmount;
    }
    
    public virtual void SetNextPoint()
    {
        curPoint = spline.GetNextPoint(curIndex, forward);

        EnterPoint(curPoint);
        GetNextPoint();
    }

    public void GetNextPoint(){
        
        progress = forward ? 0 : 1;
        
        bool looping = false;

        if(forward){
        
            curIndex++;

            if(curIndex < spline.SplinePoints.Count - (spline.closed ? 0 : 1)){
                //we good
            }else{
                if(spline.closed){
                    curIndex = 0;
                }else{
                    curIndex --;
                    ReverseDir();
                    progress = 1;
                }
            }
        }else{
            if(curIndex > 0)
            {
                curIndex--;
            }
            else
            {
                if(spline.closed){
                    looping = true;
                    curIndex = spline.SplinePoints.Count-1;
                }else{
                    ReverseDir();
                    progress = 0;
                }
            }
        }

        distance = spline.GetSegmentDistance(curIndex);
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
