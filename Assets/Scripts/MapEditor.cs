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

    public Tool _curTool
    {
        set
        {
            int dummy = (int)curTool;
            curTool = value;
            if (dummy != (int)value)
            {
                ChangeTool();
            }
        }

        get { return curTool; }
    }
    public Transform pointsParent;
    public Transform splinesParent;
    
    public static bool editing = false;
    public GameObject selector;
    public Transform canvas;
    public GameObject selectedPointIndicator;
    public GameObject pointOptions;
    private List<Image> selectors;
    private LineRenderer l;
    private Camera cam;

    private bool dragging;
    private bool pointSelected
    {
        get { return selectedPoints.Count > 0; }
    }
    private Vector3 curPos;
    private Vector3 lastPos;
    private Vector3 worldPos;
    public enum Tool
    {
        select,
        move,
        draw,
        connect,
        marquee
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

    
    void Awake()
    {
        editing = true;
 
        selectedPointIndicator.SetActive(false);
        pointOptions.SetActive(false);
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
        curPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, cam.nearClipPlane);
        worldPos = cam.ScreenToWorldPoint(new Vector3(curPos.x, curPos.y,
            Mathf.Abs(cam.transform.position.z)));
    }

    void SelectPoint(Point p)
    {
        if (p != null && Input.GetMouseButtonDown(0))
        {
            if ((!Input.GetKey(KeyCode.LeftShift) && pointSelected) && activePoint != p)
            {
                Deselect();
            }

            if (curTool == Tool.select && pointSelected && activePoint == p)
            {
                RemoveSelectedPoint(p);
                if (!pointSelected && curTool == Tool.select)
                {
                    pointOptions.SetActive(false);
                }
            }else{
                
                AddSelectedPoint(p);
                
                if (pointSelected && curTool == Tool.select)
                {
                    pointOptions.SetActive(true);
                }
            }
        }
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
   
        selectedPointIndicator.SetActive(pointSelected);
        
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

        selectedPointIndicator.SetActive(pointSelected);
    }
    
    public void SetTension(float t)
    {
        activePoint.tension = t;
    }
    
    
    public void SetBias(float t)
    {
        activePoint.bias = t;
    }

    void ChangeTool()
    {
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
                if (curTool != Tool.draw)
                {
                    l.enabled = false;
                }

                if (curTool != Tool.select)
                {
                    pointOptions.SetActive(false);  
                }

                if (curTool != Tool.move)
                {
                    dragging = false;
                }
                
                tools[i].color = Color.gray;
                tooltips[i].color = Color.clear;
            }
        }
    }

    void Deselect()
    {
        selectedPoints.Clear();
        selectedPointIndicator.SetActive(false);
        foreach (Image i in selectors)
        {
            i.color = Color.clear;
        }
    }
    
    IEnumerator MarqueeSelect(Vector3 pos)
    {

        l.enabled = true;
        l.positionCount = 5;
        while (!Input.GetMouseButtonUp(0))
        {
            Vector3 pos2 = new Vector3(pos.x, worldPos.y, 0);
            Vector3 pos3 = new Vector3(worldPos.x, pos.y, 0);
            
            l.SetPosition(0, pos);
            l.SetPosition(1, pos2);
            l.SetPosition(2, worldPos);
            l.SetPosition(3, pos3);
            l.SetPosition(4, pos);
            yield return null;
        }

        Vector3 center = Vector3.Lerp(pos, worldPos, 0.5f);
        Vector3 size = worldPos - pos;
        
       

        if (Vector3.Distance(worldPos, pos) > 0.25f)
        {
            if (!Input.GetKey(KeyCode.LeftShift))
            {
                Deselect();
            }
            foreach (Collider c in Physics.OverlapBox(center, new Vector3(Mathf.Abs(size.x), Mathf.Abs(size.y), 10) / 2))
            {
                Point p = c.GetComponent<Point>();
                if (p != null)
                {
                    if (pointSelected && selectedPoints.Contains(p))
                    {
                        RemoveSelectedPoint(p);

                        if (!pointSelected)
                        {
                            pointOptions.SetActive(false);
                        }
                    }
                    else
                    {
                        AddSelectedPoint(p);

                        if (pointSelected && curTool == Tool.select)
                        {
                            pointOptions.SetActive(true);
                        }
                    }
                }
            }
        }

        l.positionCount = 2;
        l.enabled = false;

    }
    // Update is called once per frame
    void Update()
    {
        Cursor.visible = false;
        Point hitPoint = null;

        lastPos = worldPos;
        curPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, cam.nearClipPlane);
        worldPos = cam.ScreenToWorldPoint(new Vector3(curPos.x, curPos.y,
            Mathf.Abs(cam.transform.position.z)));
        cursor.transform.position = curPos;

        
            Ray r = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Input.GetMouseButtonDown(1))
            {
                //RemoveSelectedPoint(hitPoint);
                Deselect();
            }
            

            if (Physics.Raycast(r.origin, r.direction, out hit))
            {
                hitPoint = hit.transform.GetComponent<Point>();

                if (hitPoint != null)
                {
                    if (Input.GetMouseButtonDown(1))
                    {
                        RemoveSelectedPoint(hitPoint);
                        
                        if (!pointSelected && curTool == Tool.select)
                        {
                            pointOptions.SetActive(false);
                        }
                    }


                }
            }
        


        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (pointSelected)
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
            
           

            if (Input.GetKeyDown(KeyCode.Q))
            {

                _curTool = Tool.select;
                if (pointSelected)
                {
                    pointOptions.SetActive(true);
                }
            }
            else if (Input.GetKeyDown(KeyCode.W))
            {
                _curTool = Tool.move;

            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                _curTool = Tool.draw;
                l.enabled = true;
            } else if (Input.GetKeyDown(KeyCode.M)){
                _curTool = Tool.marquee;
              
            }

            if (selectedPoints.Count == 0)
            {
                selectedPointIndicator.SetActive(false);
            }

            
            switch (curTool)
            {
                
                case Tool.move:
                    
                        SelectPoint(hitPoint);

                        if (hitPoint != null || dragging || (pointSelected && Input.GetKey(KeyCode.LeftShift)))
                        {
                            if (Input.GetMouseButton(0))
                            {
                                dragging = true;
//                            if (hitPoint != activePoint)
//                            {
//                                selectedPoints.Remove(hitPoint);
//                                selectedPoints.Add(hitPoint);
//                            }

                                Vector3 pos = cam.ScreenToWorldPoint(new Vector3(curPos.x, curPos.y,
                                    Mathf.Abs(cam.transform.position.z) - activePoint.Pos.z));

                                Vector3 delta = worldPos - lastPos;

                                foreach (Point p in selectedPoints)
                                {
                                    p.transform.position += new Vector3(delta.x, delta.y, 0);
                                }
                            }
                        }

                        if (dragging && Input.GetMouseButtonUp(0))
                        {
                            dragging = false;
                        }
                    

                  
                    break;
                case Tool.select:

                    SelectPoint(hitPoint);

                    
                    //implement marquee box 

                    break;

                case Tool.connect:

                   
                    
                    
                    break;

                case Tool.marquee:
                    
                    if (Input.GetMouseButtonDown(0))
                    {
                        StartCoroutine(MarqueeSelect(worldPos));
                    }
                    
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
                        if (hitPoint == null)
                        {
                            selectedPointIndicator.transform.position = cam.WorldToScreenPoint(p.Pos);
                        }
                        else
                        {
                            selectedPointIndicator.transform.position = cam.WorldToScreenPoint(hitPoint.Pos);
                        }

                        pointOptions.transform.position = cam.WorldToScreenPoint(p.Pos);
                    }

                    p.proximity = Mathf.Sin(Time.time);
                index++;
            }

            if (!pointSelected)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    StartCoroutine(MarqueeSelect(worldPos));
                }
            }
            
            foreach (Spline s in selectedSplines)
            {
                    s.DrawSplineOverride();
            }

        }
    }
}
