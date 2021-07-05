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
    int lastStep;

    [SerializeField] public SpriteRenderer mesh;
    
    public void Init(Spline s, float p, int i, float sp)
    {
        curIndex = i;
        progress = p;
        spline = s;
        speed = sp;
        mesh.enabled = true;

        StartPosition();
    }

    void StartPosition(){
        distance = spline.GetDistance(curIndex);
        curStep = curIndex * curveFidelity + (int)(Spline.curveFidelity * progress); 
        lastStep = curStep;

        transform.position = spline.line.points3[Mathf.Clamp(curStep, 0, spline.line.points3.Count-1)];

        nextPos = transform.position;
        lastPos = nextPos;
        running = true;
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
    
        int c = curIndex * (curveFidelity) + (int)(Spline.curveFidelity * progress);

        if(curStep != c){
            lastStep = curStep;
            curStep = c;
            lastPos = nextPos;
        }
        
        int upperBound = spline.line.points3.Count-1;
        lastPos = spline.line.points3[Mathf.Clamp(lastStep, 0, upperBound)];
        nextPos = spline.line.points3[Mathf.Clamp(curStep, 0, upperBound)];

        float p = Spline.curveFidelity * progress;
        float step = Mathf.Floor(p);
        float diff = p - step;

        transform.position = Vector3.Lerp(lastPos, nextPos, diff);
        Vector3 up = spline.GetVelocityAtIndex(curIndex, progress);
        up.z = 0;
        transform.up = up;

    }

    void GetNextPoint()
    {   
        int i = spline.closed ? 1 : 2;
        if(curIndex < spline.SplinePoints.Count - i)
        {
            curIndex++;
        }
        else
        {
            curIndex = 0;
            if(!spline.closed){
                curStep = 0;
                lastStep = 0;
            }
            // transform.position = spline.SplinePoints[curIndex].Pos;
            // lastPos = transform.position;
            // nextPos = lastPos;
        }
        
        if(spline.SplinePoints[curIndex].pointType != PointTypes.ghost){
             Services.main.OnPointEnter.Invoke(spline.SplinePoints[curIndex]);
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
