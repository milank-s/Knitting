using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 


Crawler : MonoBehaviour
{
    public bool forward = true;
    protected Spline spline;
    protected Point point;
    protected int curIndex;
    public float baseSpeed = 1;
    public float boostAmount = 0.25f;
    float boost;
    protected float speed;
    public bool running;
    protected float progress;

    protected Vector3 lastPos;
    protected Vector3 delta;
    protected int dir;
    
        public virtual void Init(){
        running = false;

    }

    public virtual void Setup(Spline s, bool f)
    {
        speed = baseSpeed;
        curIndex = f? 0 : s.SplinePoints.Count - 1;
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
                OnPoint();
                GetNextPoint();
            }

            boost = Mathf.Lerp(boost, 0, Time.deltaTime);
            progress += Time.deltaTime * (speed + boost) * dir; // spline.distance;
               
            transform.position = spline.GetPointAtIndex(curIndex, progress);
            delta = (transform.position - lastPos);
            lastPos = transform.position;
            transform.forward = delta.normalized;
        }
    }

    public virtual void OnTriggerEnter(Collider col){
        if(Services.PlayerBehaviour.state != PlayerState.Traversing || !spline.isPlayerOn) return;

        if(curIndex != spline.selectedIndex) return; 
    }

    public void ReverseDir(){
        forward = !forward;
        dir = forward ? 1 : -1;
        speed = Mathf.Abs(speed);
    }

    public virtual void BreakOff(){
    }

    public virtual void OnPoint(){
        boost += boostAmount;
    }
    public virtual void GetNextPoint()
    {
        
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
        EnterPoint(point);
        
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
