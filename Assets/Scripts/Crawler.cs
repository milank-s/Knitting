using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 


Crawler : MonoBehaviour
{
    public bool forward = true;
    protected Spline spline;
    protected int curIndex;
    public float speed = 1;

    public bool running;
    protected float progress;

    protected Vector3 lastPos;
    protected Vector3 delta;
    int dir;
    [SerializeField] private SpriteRenderer mesh;
    
    public virtual void Step()
    {
        if (running)
        {
            if(progress < 0 || progress > 1){
                GetNextPoint();
            }

            progress += Time.deltaTime * speed * dir; // spline.distance;
               
            transform.position = spline.GetPointAtIndex(curIndex, progress);
            delta = (transform.position - lastPos).normalized;
            lastPos = transform.position;
            transform.forward = delta;
        }
    }

    void GetNextPoint()
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
        
        EnterPoint( spline.SplinePoints[curIndex]);
        
    }

    public virtual void EnterPoint(Point p){
        p.OnPointEnter();
    }

    public virtual void Stop()
    {
        running = false;
        gameObject.SetActive(false);
    }

    public virtual void Init(){
        running = false;

    }

    public virtual void Setup(Spline s, bool f)
    {
        curIndex = f? 0 : s.SplinePoints.Count - 1;
        forward = f;
        progress = forward ? 0 : 1;
        spline = s;
        running = true;
        dir = forward ? 1 : -1;
        transform.position = s.SplinePoints[curIndex].Pos;
        lastPos = transform.position;
    }
}
