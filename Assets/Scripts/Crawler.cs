using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crawler : MonoBehaviour
{
    public float progress;
    private Spline spline;
    private int curIndex;
    public float speed;
    public Spline startSpline;

    public bool running;

    [SerializeField] private SpriteRenderer mesh;
    
    void Update()
    {
        if (running)
        {
            if (progress < 1)
            {
                progress += Time.deltaTime * speed; // spline.distance;
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
        
        
        spline.SplinePoints[curIndex].OnPointEnter();
        
    }

    public void Stop()
    {
        running = false;
        mesh.enabled = false;
    }

    public void Init(Spline s)
    {
        curIndex = 0;
        progress = 0;
        spline = s;
        speed = 2;
        running = true;
        mesh.enabled = true;
    }
}
