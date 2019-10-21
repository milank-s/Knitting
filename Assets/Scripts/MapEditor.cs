using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;
using System.IO;
//###################################################
//###################################################


//						TO DO					   


//deleting and inserting like in editor script
//better method to select/deselect splines 
//to get the spline that a point is on right now you have to iterate through all the splines?
// a way to switch points, ones that you cant insert because they're already placed and connected


//###################################################
//###################################################


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
    private String sceneName;
    public Slider tensionSlider;
    public Slider biasSlider;
    public GameObject pointSelectedTip;
    public GameObject splineSelectedTip;
    public GameObject marqueeTip;
    public GameObject deselectTip;
    public GameObject splinePointTip;
    public Text pointType;
    public Text xPos;
    public Text yPos;
    public Text zPos;

    public InputField sceneTitle;
    public Dropdown levelList;
    
    private static float cameraDistance = 2;
    private List<GameObject> text;

    public bool typing;
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

    private Spline selectedSpline
    {
        get
        {
            if (splineindex < 0)
            {
                return null;
            }
            if (Spline.Splines.Count > 0)
            {
                return Spline.Splines[splineindex % Spline.Splines.Count];
            }
            else
            {
                return null;
            }
        }
    }

    private int splineindex = 0;
    //add insert tool. inserts after the currently selected point
    //add delete tool. 
    //
    
    private static Tool curTool;

    public Image[] tools;
    public GameObject[] tooltips;
    public Sprite[] cursors;
    
    public Image cursor;

    
    void Awake()
    {
        
        sceneName = "Stellation 1";
        editing = true;

        text = new List<GameObject>();
        foreach (Text t in canvas.GetComponentsInChildren<Text>())
        {
            text.Add(t.gameObject);
        }
        selectedPointIndicator.SetActive(false);
        pointOptions.SetActive(false);
        selectors = new List<Image>();
        for (int i = 0; i < 50; i++)
        {
            Image newSelector = Instantiate(selector, Vector3.zero, Quaternion.identity).GetComponent<Image>();
            selectors.Add(newSelector);
            newSelector.transform.parent = canvas;
        }
        selectedPoints = new List<Point>();
 
        l = GetComponent<LineRenderer>();
        
       
    }

    public void Typing()
    {
        typing = true;
    }

    public void StopTyping(String name)
    {
        typing = false;
        sceneName = name;
    }
    void Start()
    {
        cam = Services.mainCam;
        l.enabled = false;
        Services.main.EnterEditMode(editing);
        curPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, cam.nearClipPlane);
        worldPos = cam.ScreenToWorldPoint(new Vector3(curPos.x, curPos.y,
            cameraDistance));
        for(int i = 0; i < tooltips.Length; i++)
        {
            tooltips[i].SetActive(false);
        }
        
        UnityEngine.Object[] files = Resources.LoadAll ("Levels");
        for (int i = 0; i < files.Length; i++) {
            levelList.options.Add(new Dropdown.OptionData(files[i].name));
        }

        _curTool = Tool.draw;

    }

    void Update()
    {

        if (!typing)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (pointSelected)
                {
                    Services.StartPoint = activePoint;
                }

                editing = !editing;
                canvas.gameObject.SetActive(editing);
//            cam.enabled = editing;
                Services.main.EnterEditMode(editing);
            }

            if (editing)
            {
                if (Input.GetKeyDown(KeyCode.H))
                {

                    foreach (GameObject t in text)
                    {
                        t.SetActive(!t.activeSelf);
                    }
                }


                if (splineindex >= 0 && splineindex < Spline.Splines.Count)
                {
                    splineSelectedTip.SetActive(true);
                    if (pointSelected)
                    {
                        splinePointTip.SetActive(true);
                    }

                    if (Input.GetKeyDown(KeyCode.X))
                    {
                        selectedSpline.ReverseSpline();
                    }


                    if (Input.GetKeyDown(KeyCode.C))
                    {
                        selectedSpline.closed = !selectedSpline.closed;
                    }


                    if (Input.GetKeyDown(KeyCode.LeftArrow))
                    {
                        if (pointSelected)
                        {

                            Point curPoint = activePoint;

                            if (!Input.GetKey(KeyCode.LeftShift))
                            {
                                Deselect();
                            }

                            if (selectedSpline.SplinePoints.Contains(curPoint))
                            {
                                int pointIndex = (selectedSpline.SplinePoints.IndexOf(curPoint) - 1) %
                                                 selectedSpline.SplinePoints.Count;



                                if (pointIndex < 0)
                                {
                                    AddSelectedPoint(selectedSpline.EndPoint);
                                }
                                else
                                {
                                    AddSelectedPoint(selectedSpline.SplinePoints[pointIndex]);
                                }

                            }
                            else
                            {


                                AddSelectedPoint(selectedSpline.EndPoint);
                            }
                        }
                        else
                        {

                            AddSelectedPoint(selectedSpline.EndPoint);
                        }
                    }
                    else if (Input.GetKeyDown(KeyCode.RightArrow))
                    {


                        if (pointSelected)
                        {

                            Point curPoint = activePoint;

                            if (!Input.GetKey(KeyCode.LeftShift))
                            {
                                Deselect();
                            }

                            if (selectedSpline.SplinePoints.Contains(curPoint))
                            {

                                int pointIndex = (selectedSpline.SplinePoints.IndexOf(curPoint) + 1) %
                                                 selectedSpline.SplinePoints.Count;

                                if (pointIndex >= selectedSpline.SplinePoints.Count)
                                {
                                    AddSelectedPoint(selectedSpline.StartPoint);
                                }
                                else
                                {
                                    AddSelectedPoint(selectedSpline.SplinePoints[pointIndex]);
                                }

                            }
                            else
                            {


                                AddSelectedPoint(selectedSpline.StartPoint);
                            }
                        }
                        else
                        {

                            AddSelectedPoint(selectedSpline.StartPoint);
                        }
                    }

                    if (pointSelected && selectedSpline.SplinePoints.Contains(activePoint))
                    {
                        if (Input.GetKeyDown(KeyCode.R))
                        {
                            selectedSpline.RemovePoint((selectedSpline.SplinePoints.IndexOf(activePoint)));
                        }

                        if (Input.GetKeyDown(KeyCode.I))
                        {
                            selectedSpline.AddNewPoint(selectedSpline.SplinePoints.IndexOf(activePoint));
                        }
                    }

                }

                if ((Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow)) &&
                    Spline.Splines.Count > 0)
                {

                    int i = 0;
                    if (Input.GetKeyDown(KeyCode.UpArrow))
                    {
                        i = splineindex + 1;
                    }
                    else if (Input.GetKeyDown(KeyCode.DownArrow))
                    {

                        i = splineindex - 1;
                        if (i < 0)
                        {
                            i = Spline.Splines.Count - 1;
                        }
                    }

                    ChangeSelectedSpline(i);
                }

                Cursor.visible = false;
                Point hitPoint = null;

                if (!Input.GetMouseButton(0))
                {
                    PanCamera();
                }

                lastPos = worldPos;
                curPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, cam.nearClipPlane);
                worldPos = cam.ScreenToWorldPoint(new Vector3(curPos.x, curPos.y,
                    cameraDistance));
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
                }
                else if (Input.GetKeyDown(KeyCode.M))
                {
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

                        if (pointSelected)
                        {
                            if (hitPoint != null && Input.GetMouseButtonDown(0))
                            {
                                dragging = true;
                            }

                            if (dragging || Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButton(0))
                            {


//                                dragging = true;
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

                                    p.transform.position += new Vector3(delta.x, delta.y,
                                        -Input.mouseScrollDelta.y * Time.deltaTime * 10f);
                                    p.initPos = p.Pos;
                                }
                            }
                        }

                        if (!dragging && hitPoint == null && Input.GetMouseButton(0) &&
                            !Input.GetKey(KeyCode.LeftShift))
                        {
                            DragCamera();
                        }


                        if (dragging && Input.GetMouseButtonUp(0))
                        {
                            dragging = false;
                        }



                        break;
                    case Tool.select:

                        SelectPoint(hitPoint);


                        Vector3 screenPos = cam.WorldToViewportPoint(lastPos);
                        Vector3 viewPortPos = cam.ScreenToViewportPoint(curPos);
                        if (Input.GetMouseButton(0) && Input.GetKey(KeyCode.LeftShift))
                        {
                            if (pointSelected)
                            {
                                biasSlider.value += (viewPortPos.x - screenPos.x) * 1000 * Time.deltaTime;
                                tensionSlider.value += (viewPortPos.y - screenPos.y) * 1000 * Time.deltaTime;
                            }
                        }
                        else if (Input.GetMouseButton(0))
                        {
                            DragCamera();
                        }

                        break;

                    case Tool.connect:




                        break;

                    case Tool.marquee:

                        if (pointSelected && Input.GetMouseButtonDown(0))
                        {
                            StartCoroutine(MarqueeSelect(worldPos));
                        }

                        break;

                    case Tool.draw:

                        if (pointSelected)
                        {
                            l.SetPosition(0, selectedPoints[selectedPoints.Count - 1].transform.position);
                            l.SetPosition(1, worldPos);
                        }
                        else
                        {
                            l.SetPosition(0, Vector3.one * 1000);
                            l.SetPosition(1, Vector3.one * 1000);
                        }

                        if (hitPoint != null)
                        {

                            if (pointSelected)
                            {
                                l.SetPosition(1, hitPoint.Pos);
                                if (Input.GetMouseButtonDown(0) && hitPoint != activePoint)
                                {
                                    SplinePointPair spp = SplineUtil.ConnectPoints(selectedSpline,
                                        activePoint, hitPoint);
                                    if (spp.s != null)
                                    {
                                        spp.s.transform.parent = splinesParent;
                                        spp.p.transform.parent = pointsParent;
                                        RemoveSelectedPoint(activePoint);
                                        AddSelectedPoint(hitPoint);
                                        ChangeSelectedSpline(Spline.Splines.IndexOf(spp.s));
                                    }


                                }
                            }
                            else
                            {
                                if (Input.GetMouseButtonDown(0))
                                {
                                    AddSelectedPoint(hitPoint);
                                }
                            }
                        }
                        else if (hitPoint == null)
                        {
                            if (Input.GetMouseButtonDown(0))
                            {
                                if (selectedPoints.Count > 0)
                                {
                                    Point newPoint = SplineUtil.CreatePoint(worldPos);
                                    if (Input.GetKey(KeyCode.LeftShift))
                                    {
                                        newPoint.tension = 1;
                                    }

                                    newPoint.transform.parent = pointsParent;

                                    SplinePointPair spp = SplineUtil.ConnectPoints(selectedSpline,
                                        activePoint, newPoint);
                                    if (spp.s != null)
                                    {
                                        spp.s.transform.parent = splinesParent;
                                        spp.p.transform.parent = pointsParent;
                                        RemoveSelectedPoint(activePoint);
                                        AddSelectedPoint(newPoint);
                                        ChangeSelectedSpline(Spline.Splines.IndexOf(spp.s));
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

                    if (index == selectedPoints.Count - 1)
                    {
                        if (hitPoint == null)
                        {
                            selectedPointIndicator.transform.position = cam.WorldToScreenPoint(p.Pos);
                        }
                        else
                        {
                            if (Input.GetMouseButton(0))
                            {
                                selectedPointIndicator.transform.position = cam.WorldToScreenPoint(p.Pos);
                            }
                            else
                            {
                                selectedPointIndicator.transform.position = cam.WorldToScreenPoint(hitPoint.Pos);
                            }
                        }

                        pointOptions.transform.position = cam.WorldToScreenPoint(p.Pos);
                    }

                    p.proximity = Mathf.Sin(Time.time);
                    index++;
                }

                if (!pointSelected && curTool != Tool.marquee)
                {
                    if (Input.GetMouseButtonDown(0) && Input.GetKey(KeyCode.LeftShift))
                    {
                        StartCoroutine(MarqueeSelect(worldPos));
                    }
                }

                if (pointSelected)
                {
                    marqueeTip.SetActive(false);
                    deselectTip.SetActive(true);
                    pointSelectedTip.SetActive(true);


                    if (Input.GetKeyDown(KeyCode.T))
                    {

                        foreach (Point p in selectedPoints)
                        {
                            p.tension = Mathf.PingPong(p.tension + 1, 1);
                        }
                    }

//                string input = Input.inputString;
                    pointType.text = "Type - " + activePoint.pointType.ToString();
                    xPos.text = "x  " + activePoint.Pos.x.ToString("F2");
                    yPos.text = "y  " + activePoint.Pos.y.ToString("F2");
                    zPos.text = "z  " + activePoint.Pos.z.ToString("F2");

                    if (Input.GetKeyDown(KeyCode.Alpha1))
                    {

                        activePoint.SetPointType(PointTypes.normal);

                    }
                    else if (Input.GetKeyDown(KeyCode.Alpha2))
                    {

                        activePoint.SetPointType(PointTypes.stop);

                    }
                    else if (Input.GetKeyDown(KeyCode.Alpha3))
                    {
                        activePoint.SetPointType(PointTypes.connect);

                    }
                    else if (Input.GetKeyDown(KeyCode.Alpha4))
                    {
                        activePoint.SetPointType(PointTypes.fly);
                    }


                }
                else
                {
                    marqueeTip.SetActive(true);
                    deselectTip.SetActive(false);

                }

            }
        }
    }
    

    public void Save()
    {

        Debug.Log("save me");
        
        JSONObject level = new JSONObject();

        level["name"] = sceneName;
        level["pointCount"].AsInt = Point.Points.Count;
        
        for(int i = 0; i < Point.Points.Count; i++)
        {
            level["p" + i] = Point.Points[i].Save(i);
        }

        level["splineCount"].AsInt = Spline.Splines.Count;
        
        for(int j = 0; j < Spline.Splines.Count; j++)
        {
            JSONObject splineData = new JSONObject();
            //record if its closed
            splineData["closed"].AsBool = Spline.Splines[j].closed;
            splineData["numPoints"] = Spline.Splines[j].SplinePoints.Count;
            
            JSONObject pointIndices = new JSONObject();

            int pi = 0;
            foreach (Point sp in  Spline.Splines[j].SplinePoints)
            {
                
                for(int i = 0; i < Point.Points.Count; i++)
                {
                    if (sp == Point.Points[i])
                    {
                        pointIndices["p" + pi] = i;
                        pi++;
                    }
                }
            }

            splineData["points"] = pointIndices;
            level["spline" + j] = splineData;
        }
        
        WriteJSONtoFile ("Assets/Resources/Levels", gameObject.name + ".json", level);
        
        levelList.options.Add(new Dropdown.OptionData(gameObject.name));
    }
    
    static void WriteJSONtoFile(string path, string fileName, JSONObject json){
        StreamWriter sw = new StreamWriter(path + "/" + fileName);
        sw.Write(json.ToString());
        sw.Close();
    }

    static JSONNode ReadJSONFromFile(string path, string fileName){
        StreamReader sr = new StreamReader (path + "/" + fileName);

        string resultstring = sr.ReadToEnd ();

        sr.Close ();

        JSONNode result = JSON.Parse (resultstring);

        return result;
    }


    public void LoadFromDropDown(Int32 i)
    {
        Load(levelList.options[i].text);
    }
    public void Load(string fileName)
    {   
        //Delete everything already in the scene
        //take care of any local variables in here that reference shit in the scene
        
        ClearSelection();
        
        JSONNode json = ReadJSONFromFile("Assets/Resources/Levels", fileName + ".json");

        List<Point> newPoints = new List<Point>();
        sceneName = json["name"];
        sceneTitle.text = sceneName;
        
        for (int i = 0; i < json["pointCount"]; i++)
        {
            
            Vector3 spawnPos = new Vector3(json["p" + i]["x"],json["p" + i]["y"],json["p" + i]["z"]);
            
            Point newPoint;
            if (i < Point.Points.Count)
            {
                newPoint = Point.Points[i];
                newPoint.Clear();
                newPoint.transform.position = spawnPos;
            }    
            else
            {
                //make new Point
                newPoint = SplineUtil.CreatePoint(spawnPos);
            }

            newPoint.tension = json["p" + i]["tension"];
            newPoint.bias = json["p" + i]["bias"];
            newPoint.continuity = json["p" + i]["continuity"];
            int t = json["p" + i]["pointType"];
            newPoint.pointType = (PointTypes)t;
            
            newPoints.Add(newPoint);
        }

        
        for (int i = 0; i < json["splineCount"]; i++)
        {    
            Point p1 = newPoints[json["spline" + i]["points"]["p" + 0]];
            Point p2 = newPoints[json["spline" + i]["points"]["p" + 1]];

            Spline newSpline;
                
            if (i < Spline.Splines.Count)
            {
                newSpline = Spline.Splines[i];
                newSpline.Reset();
                newSpline.SplinePoints.Add(p1);
                newSpline.SplinePoints.Add(p2);
            }
            else
            {
                newSpline = SplineUtil.CreateSpline(p1, p2);
            }

            
            int numPoints = json["spline" + i]["numPoints"];
            
            if (json["spline" + i]["numPoints"] > 2)
            {
                for (int k = 2; k < numPoints; k++)
                {
                    newSpline.SplinePoints.Add(newPoints[json["spline" + i]["points"]["p" + k]]);
                }
            }
            
            newSpline.closed = json["spline" + i]["closed]"];
            
        }

        for (int i = Spline.Splines.Count-1; i >= json["splineCount"]; i--)
        {
            Spline.Splines[i].Destroy();
        }

        for (int i = Point.Points.Count -1 ; i >= json["pointCount"]; i--)
        {
            Point.Points[i].Destroy();
        }
    }
    
 
    
    
    void DragCamera()
    {
        Vector3 delta = worldPos - lastPos;
        Services.mainCam.transform.position -= delta;

        curPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, cam.nearClipPlane);
        worldPos = cam.ScreenToWorldPoint(new Vector3(curPos.x, curPos.y,
            cameraDistance));
        lastPos = worldPos;
    }
    
    void PanCamera()
    {
        cam.transform.position -= Vector3.forward * Input.mouseScrollDelta.y * Time.deltaTime * 10f;
        
        Vector3 viewportPos = cam.ScreenToViewportPoint(curPos);
        
        if (viewportPos.y > 0.95f && viewportPos.y < 1)
        {
            cam.transform.position += Vector3.up * Time.deltaTime * (0.1f - (1-viewportPos.y)) * 5f;
           
        }else if (viewportPos.y < 0.05f && viewportPos.y > 0)
        {
            cam.transform.position -= Vector3.up * Time.deltaTime * (0.1f - viewportPos.y) * 5f;
        }

        if (viewportPos.x > 0.95f && viewportPos.x < 1 )
        {
            cam.transform.position += Vector3.right * Time.deltaTime * (0.1f - (1-viewportPos.x)) * 5f;
        }else if (viewportPos.x < 0.05f && viewportPos.x > 0)
        {
            cam.transform.position -= Vector3.right * Time.deltaTime * (0.1f - viewportPos.x) * 5f;
        }
    }
    void SelectPoint(Point p)
    {
        if (p != null && Input.GetMouseButtonDown(0))
        {
            if ((!Input.GetKey(KeyCode.LeftShift) && pointSelected) && activePoint != p)
            {
                Deselect();
            }

            if (pointSelected && activePoint == p)
            {
                if (curTool == Tool.select)
                {
                    RemoveSelectedPoint(p);
                    if (!pointSelected && curTool == Tool.select)
                    {
                        pointOptions.SetActive(false);
                    }
                }else if (curTool == Tool.move)
                {
                    Deselect();
                    AddSelectedPoint(p);
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

        biasSlider.value = activePoint.bias;
        tensionSlider.value = activePoint.tension;
        selectedPointIndicator.SetActive(pointSelected);
        
    }
    
    void ChangeSelectedSpline(int i)
    {
        Deselect();
        
        if (selectedSpline != null)
        {
            selectedSpline.ChangeMaterial(3);
        }

        if (Spline.Splines.Count > 0)
        {
            splineindex = i;
            if (splineindex < 0)
            {
                splineindex = Spline.Splines.Count - 1;
            }
            else if (splineindex >= Spline.Splines.Count)
            {
                splineindex = 0;
            }

                splineSelectedTip.SetActive(true);
                selectedSpline.ChangeMaterial(0);

                foreach (Point p in selectedSpline.SplinePoints)
                {
                    AddSelectedPoint(p);
                }
        }
        else
        {
            splineSelectedTip.SetActive(false);
        }

    }

    void ClearSelection()
    {

           for (int i = selectedPoints.Count - 1; i >= 0; i--)
            {
                RemoveSelectedPoint(selectedPoints[i]);
           }
        

        splineindex = 0;
    }
    void RemoveSelectedPoint(Point p)
    {
        if (selectedPoints.Contains(p))
        {
            
            selectors[selectedPoints.Count - 1].color = Color.clear;
            selectedPoints.Remove(p);
        }

        selectedPointIndicator.SetActive(pointSelected);
        pointSelectedTip.SetActive(pointSelected);
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
                tooltips[i].SetActive(true);
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
                    cursor.rectTransform.pivot = new Vector3(0f, 0f);
                    dragging = false;
                }
                else
                {
                    cursor.rectTransform.pivot = new Vector3(0.5f, 0.5f);
                }
                
                tools[i].color = Color.gray;
                tooltips[i].SetActive(false);
            }
        }
    }

    void Deselect()
    {
        selectedPoints.Clear();
        selectedPointIndicator.SetActive(false);
        pointSelectedTip.SetActive(false);
        foreach (Image i in selectors)
        {
            i.color = Color.clear;
        }

        pointOptions.SetActive(false);
        
    }
    
    IEnumerator MarqueeSelect(Vector3 pos)
    {
        l.enabled = true;
        l.positionCount = 5;
        while (!Input.GetMouseButtonUp(0))
        {
            Vector3 pos2 = new Vector3(pos.x, worldPos.y, worldPos.z);
            Vector3 pos3 = new Vector3(worldPos.x, pos.y, worldPos.z);
            
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
   
}
