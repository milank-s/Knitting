using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crawler : MonoBehaviour
{
    public bool forward = true;
    public float progress;
    private Spline spline;
    private int curIndex;
    public float speed;
    public Spline startSpline;

    public bool running;

    int dir;
    [SerializeField] private SpriteRenderer mesh;
    
    void Step()
    {
        if (running)
        {
            if(progress < 0 || progress > 1){
                GetNextPoint();
            }

          
            progress += Time.deltaTime * speed * dir; // spline.distance;
               
            transform.position = spline.GetPointAtIndex(curIndex, progress);
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
        mesh.enabled = false;
    }

    public virtual void Init(Spline s)
    {
        curIndex = 0;
        progress = 0;
        spline = s;
        running = true;
        mesh.enabled = true;
        dir = forward ? 1 : -1;
    }
}
