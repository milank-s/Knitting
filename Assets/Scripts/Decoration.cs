using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Decoration : MonoBehaviour
{
    public float progress;
    private Spline spline;
    private int curIndex;
    public float speed;

    public bool running;

    [SerializeField] private SpriteRenderer mesh;
    
    void Step()
    {
        if (running)
        {
            if (progress < 1)
            {
                progress += Time.deltaTime * speed / spline.segmentDistance;    
                SetPosition();
            }
            else
            {
                GetNextPoint();
            }
        }
    }

    void SetPosition(){
        
        int segmentIndex = curIndex * curveFidelity + (int)(Spline.curveFidelity * progress);
        transform.position = spline.line.points3[segmentIndex];
        transform.forward = spline.line.points3[Mathf.Clamp(segmentIndex + 1, 0, spline.line.points3.Count -1)]- transform.position;
        

    }

    void GetNextPoint()
    {
        progress = 0;
        
        if(curIndex < spline.SplinePoints.Count - 1)
        {
            curIndex++;
        }
        else
        {
            curIndex = 0;
        }
        
        
        //spline.SplinePoints[curIndex].OnPointEnter();
        
    }

    public void Stop()
    {
        running = false;
        mesh.enabled = false;
    }

    public void Init(Spline s, float p, int i, float s)
    {
        curIndex = i;
        progress = p;
        spline = s;
        speed = s;
        running = true;
        mesh.enabled = true;
    }
}
