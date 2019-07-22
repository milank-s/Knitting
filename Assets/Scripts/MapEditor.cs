using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;

public class MapEditor : MonoBehaviour
{

    private Point activePoint
    {
        get
        {
            if (selectedPoints.Count > 0)
            {
                return selectedPoints[selectedPoints.Count-1];
            }
            else
            {
                return null;
            }
        }
    }

    private Spline activeSpline
    {
        get
        {
            if(selectedSplines.Count > 0){
                return selectedSplines[selectedSplines.Count-1];
   
            }else{
                return null;
            }
        }
    }
    
    public Transform pointsParent;
    public Transform splinesParent;
    
    public static bool editing = false;
    public GameObject selector;
    public Transform canvas;
    public GameObject selectedPointIndicator;
    private List<Image> selectors;
    private LineRenderer l;
    private Camera cam;
    public enum Tool
    {
        select,
        draw,
        connect
    }

    
    private List<Point> selectedPoints;
    private List<Spline> selectedSplines;
    
    //add insert tool. inserts after the currently selected point
    //add delete tool. 
    //
    
    private static Tool curTool;

    public Image[] tools;
    public Text[] tooltips;
    public Sprite[] cursors;
    
    public Image cursor;
    // Start is called before the first frame update
    void Awake()
    {
        editing = true;
 
        selectors = new List<Image>();
        for (int i = 0; i < 20; i++)
        {
            Image newSelector = Instantiate(selector, Vector3.zero, Quaternion.identity).GetComponent<Image>();
            selectors.Add(newSelector);
            newSelector.transform.parent = canvas;
        }

        cam = GetComponentInChildren<Camera>();
        selectedPoints = new List<Point>();
        selectedSplines = new List<Spline>();
        Point.editMode = true;
        l = GetComponent<LineRenderer>();
    }

    void Start()
    {
        l.enabled = false;
        cam.enabled = editing;
        Services.main.EnterEditMode(editing);
    }

    void AddSelectedPoint(Point p)
    {
        if (!selectedPoints.Contains(p))
        {
            selectedPoints.Add(p);
            selectors[selectedPoints.Count - 1].color = Color.white;
            
        }
        else
        {
            selectedPoints.Remove(p);
            selectedPoints.Add(p);
        }
    }
    
    void AddSelectedSpline(Spline p)
    {
        if (!selectedSplines.Contains(p))
        {
            selectedSplines.Add(p);
        }
       
    }
    
    void RemoveSelectedPoint(Point p)
    {
        if (selectedPoints.Contains(p))
        {
            
            selectors[selectedPoints.Count - 1].color = Color.clear;
            selectedPoints.Remove(p);
        }
    }
    
    public void SetTension(float t)
    {
        activePoint.tension = t;
    }
    
    
    public void SetBias(float t)
    {
        activePoint.bias = t;
    }
    // Update is called once per frame
    void Update()
    {
        Cursor.visible = false;
        Vector3 curPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, cam.nearClipPlane);
        Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(curPos.x, curPos.y,
            Mathf.Abs(cam.transform.position.z)));
        cursor.transform.position = curPos;

        Ray r = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Point hitPoint = null;
        if (Physics.Raycast(r.origin, r.direction, out hit))
        {
            hitPoint = hit.transform.GetComponent<Point>();

            if (hitPoint != null)
            {
                if (Input.GetMouseButtonDown(1))
                {
                    RemoveSelectedPoint(hitPoint);
                }

                
            }
        }

      
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (selectedPoints.Count > 0)
            {
                Services.StartPoint = activePoint;
            }
            editing = !editing;
            canvas.gameObject.SetActive(editing);
            cam.enabled = editing;
            Services.main.EnterEditMode(editing);
        }

        if (editing)
        {

//            Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 2));
            for (int i = 0; i < tools.Length; i++)
            {
                if (i == (int) curTool)
                {
                    tools[i].color = Color.white;
                    tooltips[i].color = Color.white;
                    cursor.sprite = cursors[i];
                }
                else
                {
                    if (curTool != Tool.connect)
                    {
                        l.enabled = false;
                    }
                    tools[i].color = Color.gray;
                    tooltips[i].color = Color.clear;
                }
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                curTool = Tool.select;
            }
            else if (Input.GetKeyDown(KeyCode.W))
            {
                curTool = Tool.draw;

            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                curTool = Tool.connect;
                l.enabled = true;
            }

            if (selectedPoints.Count == 0)
            {
                selectedPointIndicator.SetActive(false);
            }

            switch (curTool)
            {
                case Tool.select:

                    if (hitPoint != null)
                    {
                        if (Input.GetMouseButtonDown(0))
                        {

                            AddSelectedPoint(hitPoint);
                        }
                        
                        if (selectedPoints.Contains(hitPoint) && Input.GetMouseButton(0))
                        {
                            Vector3 pos = cam.ScreenToWorldPoint(new Vector3(curPos.x, curPos.y,
                                Mathf.Abs(cam.transform.position.z) - hitPoint.Pos.z));
                            hitPoint.transform.position = new Vector3(pos.x, pos.y, hitPoint.Pos.z);
                        }
                    }

                    break;

                case Tool.connect:

                   
                    
                    
                    break;

                case Tool.draw:

                    if (selectedPoints.Count > 0)
                    {
                        l.SetPosition(0, selectedPoints[selectedPoints.Count - 1].transform.position);
                        l.SetPosition(1, worldPos);
                    }

                    if (hitPoint != null)
                    {
                        l.SetPosition(1, hitPoint.Pos);
                        if (selectedPoints.Count > 0)
                        {
                            if (Input.GetMouseButtonDown(0) && hitPoint != activePoint)
                            {
                                SplinePointPair spp = SplineUtil.ConnectPoints(activeSpline,
                                    activePoint, hitPoint);
                                if (spp.s != null)
                                {
                                    spp.s.transform.parent = splinesParent;
                                    spp.p.transform.parent = pointsParent;
                                    RemoveSelectedPoint(activePoint);
                                    AddSelectedPoint(hitPoint);
                                    
                                    AddSelectedSpline(spp.s);
                                }


                            }
                        }
                        else
                        {
                            AddSelectedPoint(hitPoint);
                        }
                    }
                    else if (hitPoint == null)
                    {
                        if (Input.GetMouseButtonDown(0))
                        {
                            if (selectedPoints.Count > 0)
                            {
                                Point newPoint = SplineUtil.CreatePoint(worldPos);
                                newPoint.transform.parent = pointsParent;

                                SplinePointPair spp = SplineUtil.ConnectPoints(activeSpline,
                                    activePoint, newPoint);
                                if (spp.s != null)
                                {
                                    spp.s.transform.parent = splinesParent;
                                    spp.p.transform.parent = pointsParent;
                                    RemoveSelectedPoint(activePoint);
                                    AddSelectedPoint(newPoint);
                                    AddSelectedSpline(spp.s);
                                }

                            }
                            else
                            {
                                Point newPoint = SplineUtil.CreatePoint(worldPos);
                                newPoint.transform.parent = pointsParent;
                                AddSelectedPoint(newPoint);
                            }
                        }
                    }

                    break;

            }

            int index = 0;
            foreach (Point p in selectedPoints)
            {
              
                    selectors[index].transform.Rotate(Vector3.forward);
                    selectors[index].transform.position = cam.WorldToScreenPoint(p.Pos);

                    if (index == selectedPoints.Count-1)
                    {
                        selectedPointIndicator.transform.position = cam.WorldToScreenPoint(p.Pos);
                        selectedPointIndicator.SetActive(true);    
                    }

                    p.proximity = Mathf.Sin(Time.time);
                index++;
            }

            foreach (Spline s in selectedSplines)
            {
                    s.DrawSplineOverride();
            }

        }
    }
}
