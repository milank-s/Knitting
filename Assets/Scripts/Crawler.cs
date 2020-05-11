using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crawler : MonoBehaviour
{
    private float progress;
    public List<Point> points;
    private Spline curSpline;
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
                transform.position = curSpline.GetPointAtIndex(curSpline.SplinePoints.IndexOf(curPoint), progress);
            }
            else
            {
                GetNextPoint();
            }
        }
    }

    void GetNextPoint()
    {
        
        if(curIndex < points.Count - 1)
        {
            curSpline = curPoint.GetConnectingSpline(nextPoint);
            curIndex++;
        }
        else
        {
            curIndex = 0;
            curSpline = startSpline;
        }
        
        curPoint.OnPointEnter();
    }

    public void Stop()
    {
        running = false;
        mesh.enabled = false;
    }

    public void Init()
    {
        curSpline = startSpline;
        running = true;
        mesh.enabled = true;
    }
}
