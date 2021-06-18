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
    
    Vector3 nextPos;

    Vector3 lastPos;

    int curStep;

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
        curStep = curIndex * curveFidelity + (int)(Spline.curveFidelity * progress); 
    }

    public void Step()
    {
        if (running)
        {
            
            progress += Time.deltaTime * speed / distance;    

            if (progress > 1)
            {    
                progress = progress - 1;
                GetNextPoint();
            }
            
            
            SetPosition();
        }   
    }

    void SetPosition(){
    
        int c = curIndex * (curveFidelity + 1) + (int)(Spline.curveFidelity * progress);

        if(curStep != c){
            curStep = c;
            lastPos = nextPos;
            nextPos = spline.line.points3[Mathf.Clamp(c, 0, spline.line.points3.Count-1)];
        }

        float p = Spline.curveFidelity * progress;
        float step = Mathf.Floor(p);
        float diff = p - step;

        transform.position = Vector3.Lerp(lastPos, nextPos, diff);
        transform.up = spline.GetVelocityAtIndex(curIndex, progress);

        // transform.up = spline.line.points3[Mathf.Clamp(segmentIndex + 1, 0, spline.line.points3.Count -1)]- transform.position;
    }

    void GetNextPoint()
    {
        
        if(curIndex < spline.SplinePoints.Count - 1)
        {
            curIndex++;
        }
        else
        {
            curIndex = 0;
        }
    
            
        transform.position = spline.SplinePoints[curIndex].Pos;
        lastPos = transform.position;
        distance = spline.GetDistance(curIndex);
        
    
        //spline.SplinePoints[curIndex].OnPointEnter();
        
    }

    public void Stop()
    {
        running = false;
        mesh.enabled = false;
    }

}
