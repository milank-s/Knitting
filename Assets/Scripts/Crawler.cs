using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crawler : MonoBehaviour
{
    public float progress;
    public List<Point> points;
    private Spline spline;
    private int curIndex;
    public float speed;
    public Spline startSpline;

    public bool running;

    [SerializeField] private SpriteRenderer mesh;
    
    private Point curPoint
    {
        get { return points[curIndex]; }
    }
    
    private Point nextPoint
    {
        get { return points[curIndex + 1]; }
    }
    
    void Update()
    {
        if (running)
        {
            if (progress < 1)
            {
                progress += Time.deltaTime * speed;
                transform.position = spline.GetPointAtIndex(curIndex, progress);
            }
            else
            {
                GetNextPoint();
            }
        }
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
        
        
        curPoint.OnPointEnter();
        
    }

    public void Stop()
    {
        running = false;
        mesh.enabled = false;
    }

    public void Init(Spline s)
    {
        curIndex = 0;
        spline = s;
        speed = Services.main.activeStellation.speed;
        running = true;
        mesh.enabled = true;
    }
}
