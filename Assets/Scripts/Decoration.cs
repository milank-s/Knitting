using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Spline;
public class Decoration : MonoBehaviour
{
    public float progress;
    private Spline spline;
    private int curIndex;
    public float speed;
    public bool running;
    float distance;
    [SerializeField] public SpriteRenderer mesh;
    
        public void Init(Spline s, float p, int i, float sp)
    {
        curIndex = i;
        progress = p;
        spline = s;
        speed = sp;
        running = true;
        mesh.enabled = true;
        distance = s.GetDistance(i);
        
    }

    public void Step()
    {
        if (running)
        {
            
            progress += Time.deltaTime * speed / distance;    

            if (progress < 1)
            {    
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
        transform.position = spline.line.points3[Mathf.Clamp(segmentIndex, 0, spline.line.points3.Count-1)];
        transform.up = spline.GetVelocityAtIndex(curIndex, progress);
        // transform.up = spline.line.points3[Mathf.Clamp(segmentIndex + 1, 0, spline.line.points3.Count -1)]- transform.position;
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
    
        distance = spline.GetDistance(curIndex);
    
        //spline.SplinePoints[curIndex].OnPointEnter();
        
    }

    public void Stop()
    {
        running = false;
        mesh.enabled = false;
    }

}
