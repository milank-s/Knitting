using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;
using System.IO;
using System.Linq;
using UnityEditor;

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


    [SerializeField] public string sceneName;



    private Point activePoint
    {
        get
        {
            if (selectedPoints.Count > 0)
            {
                return selectedPoints[selectedPoints.Count - 1];
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
            int dummy = (int) curTool;
            curTool = value;
            if (dummy != (int) value)
            {
                ChangeTool();
            }
        }

        get { return curTool; }
    }

    [Space(25)] public Transform pointsParent;
    public Transform splinesParent;

    public  Image textCursor;
    public static bool editing = false;
    public GameObject selector;
    public Transform canvas;
    public GameObject selectedPointIndicator;
    public GameObject pointOptions;
    private List<Image> selectors;
    
    private LineRenderer l;
    private Camera cam;
    public Slider tensionSlider;
    public Slider biasSlider;
    public GameObject pointSelectedTip;
    public GameObject splineSelectedTip;
    public GameObject marqueeTip;
    public GameObject deselectTip;
    public GameObject splinePointTip;
    public Transform pointCoords;
    public Text pointType;
    public Text xPos;
    public Text yPos;
    public Text zPos;

    public InputField sceneTitle;
    public Dropdown levelList;

    private static float cameraDistance = 2;
    private List<GameObject> text;

    private Vector3 center
    {
        get { return Vector3.Lerp(upperRight, lowerLeft, 0.5f); }
    }
    private Vector3 upperRight, lowerLeft;

    private Point hitPoint;
    
    public static bool typing;
    private bool dragging;
    private string pointText;
    private bool pointSelected
    {
        get { return selectedPoints.Count > 0; }
    }

    private Vector3 delta
    {
        get
        {
           return worldPos - lastPos;
        }
    }
    private Vector3 curPos;
    private Vector3 lastPos;
    private Vector3 worldPos;
    private Vector3 rotationPivot;
    
    public enum Tool
    {
        select,
        move,
        draw,
        marquee,
        clone, 
        rotate,
        text
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
                if (splineindex >= Spline.Splines.Count)
                {
                    splineindex = Spline.Splines.Count - 1;
                }

                return Spline.Splines[splineindex];
            }
            else
            {
                return null;
            }
        }
    }

    private List<Spline> selectedSplines = new List<Spline>();
    
    private int splineindex = -1;
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
            Image newSelector = Instantiate(selector, Vector3.one * 1000, Quaternion.identity).GetComponent<Image>();
            selectors.Add(newSelector);
            newSelector.color = Color.clear;
            newSelector.transform.SetParent(canvas, false);
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
        editing = true;
        cam = Services.mainCam;
        l.enabled = false;
        Services.main.EnterEditMode(editing);
        curPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, cam.nearClipPlane);
        worldPos = cam.ScreenToWorldPoint(new Vector3(curPos.x, curPos.y,
            cameraDistance));
        for (int i = 0; i < tooltips.Length; i++)
        {
            tooltips[i].SetActive(false);
        }

        UnityEngine.Object[] files = Resources.LoadAll("Levels");
        for (int i = 0; i < files.Length; i++)
        {
            levelList.options.Add(new Dropdown.OptionData(files[i].name));
        }

        _curTool = Tool.draw;

    }

    void TogglePlayMode()
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
    }

    void HideUI()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {

            foreach (GameObject t in text)
            {
                t.SetActive(!t.activeSelf);
            }
        }
    }

    void EditSelectedSpline()
    {
        if (splineindex >= 0 && splineindex < Spline.Splines.Count)
        {


            if (Input.GetKeyDown(KeyCode.Alpha5))
            {

                selectedSpline.ChangeMaterial(selectedSpline.lineMaterial + 1);
            }

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
                        DeselectPoints();
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
                        DeselectPoints();
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
                if (Input.GetKeyDown(KeyCode.Minus))
                {
                    selectedSpline.RemovePoint((selectedSpline.SplinePoints.IndexOf(activePoint)));
                    selectedSpline.ResetLineLength();

                    if (selectedSpline.SplinePoints.Count < 2)
                    {
                        Destroy(selectedSpline);
                    }
                }

                if (Input.GetKeyDown(KeyCode.Equals))
                {
                    selectedSpline.AddNewPoint(selectedSpline.SplinePoints.IndexOf(activePoint));
                }
            }

        }
    }

    void ChangeSelectedSpline()
    {
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

            }

            if (i >= Spline.Splines.Count)
            {
                i = 0;
            }

            if (i < 0)
            {
                i = Spline.Splines.Count - 1;
            }

            AddSelectedSpline(Spline.Splines[i]);


            if (!Input.GetKey(KeyCode.LeftShift))
            {
                DeselectPoints();
            }


            foreach (Point p in selectedSpline.SplinePoints)
            {
                AddSelectedPoint(p);
            }

        }
    }

    void SetCursorPosition()
    {
        if (Input.GetMouseButton(0))
        {
            PanCamera();
        }

        cam.fieldOfView = Mathf.Clamp(cam.fieldOfView - Input.mouseScrollDelta.y * Time.deltaTime * 100f, 10,
            160);

        lastPos = worldPos;
        curPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, cam.nearClipPlane);
        worldPos = cam.ScreenToWorldPoint(new Vector3(curPos.x, curPos.y,
            cameraDistance));
        cursor.transform.position = curPos;
    }

    void DeselectAll()
    {
        if (Input.GetMouseButtonDown(1))
        {
            //RemoveSelectedPoint(hitPoint);
            DeselectPoints();
            DeselectSpline();
        }
    }

    void RaycastFromCursor()
    {
        hitPoint = null;

        Ray r = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

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
    }

    void TryChangeTool()
    {
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
        else if (Input.GetKeyDown(KeyCode.R))
        {
            _curTool = Tool.rotate;

        }
        else if (Input.GetKeyDown(KeyCode.V))
        {
            _curTool = Tool.clone;
        }
        else if (Input.GetKeyDown(KeyCode.T))
        {
            _curTool = Tool.text;
        }
    }

    void Update()
    {

        if (!typing)
        {
            TogglePlayMode();
        }

        if (editing)
            {

                Cursor.visible = false;
                SetCursorPosition();
                
                if (!typing)
                {
                    
                    HideUI();

                    if (curTool != Tool.text)
                    {
                        EditSelectedSpline();
                    }
                    
                    ChangeSelectedSpline();
                    TryChangeTool();
                    
                    if (Input.GetKeyDown(KeyCode.S))
                    {
                        Save();
                    }
                    
                }

                DeselectAll();
                RaycastFromCursor();


                
                UseTool();

                
                if (curTool != Tool.clone && curTool != Tool.rotate)
                {
                    SelectPoint(hitPoint);
                }
                

                //update point selection UI
                if (selectedPoints.Count == 0)
                {
                    selectedPointIndicator.SetActive(false);
                }

                

                ManageSelectionUI();


                if (dragging && Input.GetMouseButtonUp(0))
                {
                    dragging = false;
                }


                if (!typing)
                {
                    if (!pointSelected && curTool != Tool.marquee)
                    {
                        if (Input.GetMouseButtonDown(0) && Input.GetKey(KeyCode.LeftShift))
                        {
                            StartCoroutine(MarqueeSelect(worldPos));
                        }
                    }
                    
                    EditSelectedPoint();
                }
                
            }
        }


    void EditSelectedPoint()
    {
        if (pointSelected)
        {

            if (Input.GetKeyDown(KeyCode.F))
            {
                cam.transform.position = new Vector3(center.x, center.y, center.z - cameraDistance);
            }

            marqueeTip.SetActive(false);
            deselectTip.SetActive(true);
            pointSelectedTip.SetActive(true);


            if (Input.GetKeyDown(KeyCode.Z))
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

            else if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                activePoint.SetPointType(PointTypes.start);
            }else if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                activePoint.SetPointType(PointTypes.end);
                
            }

            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                Point pointToDelete = activePoint;
                RemoveSelectedPoint(activePoint);
                foreach (Spline s in Spline.Splines)
                {
                    if (s.SplinePoints.Contains(pointToDelete))
                    {
                        s.SplinePoints.Remove(pointToDelete);
                        selectedSpline.ResetLineLength();
                    }

                    if (s.SplinePoints.Count < 2)
                    {
                        Destroy(s);
                    }
                }

                pointToDelete.Destroy();
            }
        }
        else
        {
            marqueeTip.SetActive(true);
            deselectTip.SetActive(false);
        }
    }

    void ManageSelectionUI()
    {
        int index = 0;

                lowerLeft = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
                upperRight = new Vector3(-Mathf.Infinity, -Mathf.Infinity, -Mathf.Infinity);


                foreach (Point p in selectedPoints)
                {
                    if (p.Pos.x > upperRight.x)
                    {
                        upperRight.x = p.Pos.x;
                    }

                    if (p.Pos.x < lowerLeft.x)
                    {
                        lowerLeft.x = p.Pos.x;
                    }

                    if (p.Pos.y > upperRight.y)
                    {
                        upperRight.y = p.Pos.y;
                    }

                    if (p.Pos.y < lowerLeft.y)
                    {
                        lowerLeft.y = p.Pos.y;
                    }

                    if (p.Pos.z > upperRight.z)
                    {
                        upperRight.z = p.Pos.z;
                    }

                    if (p.Pos.z < lowerLeft.z)
                    {
                        lowerLeft.z = p.Pos.z;
                    }


                    if (index < selectors.Count)
                    {
                        selectors[index].transform.Rotate(Vector3.forward);
                        selectors[index].transform.position = cam.WorldToScreenPoint(p.Pos);
                    }


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
                        pointCoords.position = cam.WorldToScreenPoint(p.Pos);
                    }

                    index++;
                }
    }
    void MoveSelectedPoints()
    {
        Vector3 pos = cam.ScreenToWorldPoint(new Vector3(curPos.x, curPos.y,
            Mathf.Abs(cam.transform.position.z) - activePoint.Pos.z));

        foreach (Point p in selectedPoints)
        {

            p.transform.position += new Vector3(delta.x, delta.y,
                -Input.mouseScrollDelta.y * Time.deltaTime * 10f);
            p.initPos = p.Pos;
        }
    }


    public void Save()
    {

        JSONObject level = new JSONObject();

        level["name"] = sceneName;
        level["pointCount"].AsInt = Point.Points.Count;

        for (int i = 0; i < Point.Points.Count; i++)
        {
            level["p" + i] = Point.Points[i].Save(i);
            
        }

        level["splineCount"].AsInt = Spline.Splines.Count;

        for (int j = 0; j < Spline.Splines.Count; j++)
        {
            JSONObject splineData = new JSONObject();
            //record if its closed
            splineData["closed"].AsBool = Spline.Splines[j].closed;
            splineData["numPoints"] = Spline.Splines[j].SplinePoints.Count;
            splineData["lineTexture"] = Spline.Splines[j].lineMaterial;
            JSONObject pointIndices = new JSONObject();

            int pi = 0;
            foreach (Point sp in Spline.Splines[j].SplinePoints)
            {

                for (int i = 0; i < Point.Points.Count; i++)
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

        WriteJSONtoFile("Assets/Resources/Levels", sceneName + ".json", level);

        bool contains = false;
        foreach (Dropdown.OptionData d in levelList.options)
        {
            if (d.text == sceneName)
            {
                contains = true;
            }
        }

        if (!contains)
        {
            levelList.options.Add(new Dropdown.OptionData(sceneName));
        }
    }

    static void WriteJSONtoFile(string path, string fileName, JSONObject json)
    {
        StreamWriter sw = new StreamWriter(path + "/" + fileName);
        sw.Write(json.ToString());
        sw.Close();
    }

    static JSONNode ReadJSONFromFile(string path, string fileName)
    {
        StreamReader sr = new StreamReader(path + "/" + fileName);

        string resultstring = sr.ReadToEnd();

        sr.Close();

        JSONNode result = JSON.Parse(resultstring);

        return result;
    }


    public void LoadFromDropDown(Int32 i)
    {
        LoadInEditor(levelList.options[i].text);
    }

    public void LoadInEditor(string fileName)
    {
        //Delete everything already in the scene
        //take care of any local variables in here that reference shit in the scene

        Spline[] splines = splinesParent.GetComponentsInChildren<Spline>();
        Point[] points = pointsParent.GetComponentsInChildren<Point>();

        ClearSelection();

        JSONNode json = ReadJSONFromFile("Assets/Resources/Levels", fileName + ".json");
        
        sceneName = json["name"];
        sceneTitle.text = sceneName;
        List<Point> newPoints = new List<Point>();
        
        for (int i = 0; i < json["pointCount"]; i++)
        {
            
            Vector3 spawnPos = new Vector3(json["p" + i]["x"],json["p" + i]["y"],json["p" + i]["z"]);
            
            Point newPoint;
            if (i < points.Length)
            {
                newPoint = points[i];
                newPoint.Clear();
                newPoint.transform.position = spawnPos;
            }    
            else
            {
                //make new Point
                newPoint = SplineUtil.CreatePoint(spawnPos);
            }
            
            if (json["p" + i]["word"] != "")
            {
                TextMesh newText;
                    
                Vector3 textPos = new Vector3(json["p" + i]["text"]["x"], json["p" + i]["text"]["y"],
                    json["p" + i]["text"]["z"]);
                if (newPoint.textMesh == null)
                {
                     newText = Instantiate(Services.Prefabs.spawnedText, newPoint.transform)
                        .GetComponent<TextMesh>();
                }
                else
                {
                    newText = newPoint.textMesh;
                }
                newPoint.textMesh = newText;
                newText.transform.position = textPos;
                Services.Prefabs.SetFont(newText, json["p" + i]["text"]["font"]);
                newText.fontSize = json["p" + i]["text"]["fontSize"];
            }else if (newPoint.textMesh != null)
            {
                newPoint.textMesh.text = "";   
            }

            newPoint.tension = json["p" + i]["tension"];
            newPoint.bias = json["p" + i]["bias"];
            newPoint.continuity = json["p" + i]["continuity"];
            int t = json["p" + i]["pointType"];
            newPoint.pointType = (PointTypes)t;
            newPoint.transform.parent = pointsParent;
            newPoints.Add(newPoint);
        }

        
        for (int i = 0; i < json["splineCount"]; i++)
        {    
            Point p1 = newPoints[json["spline" + i]["points"]["p" + 0]];
            Point p2 = newPoints[json["spline" + i]["points"]["p" + 1]];

            Spline newSpline;
                
            if (i < splines.Length)
            {
                newSpline = splines[i];
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

            newSpline.lineMaterial = json["lineTexture"];
            newSpline.closed = json["spline" + i]["closed"];
            newSpline.transform.parent = splinesParent;
        }

        for (int i = splines.Length - 1; i >= json["splineCount"]; i--)
        {
            Destroy(splines[i]);
        }

        for (int i = points.Length - 1; i >= json["pointCount"]; i--)
        {
            points[i].Destroy();
        }

        StopTyping(sceneName);
    }

    public static void Load(string fileName)
    {

        GameObject parent = new GameObject();
        GameObject pointsParent = new GameObject();
        pointsParent.name = "Points";
        GameObject splineParent = new GameObject();
        splineParent.name = "Splines";

        pointsParent.transform.parent = parent.transform;
        splineParent.transform.parent = parent.transform;
        
        JSONNode json = ReadJSONFromFile("Assets/Resources/Levels", fileName + ".json");

        parent.name = json["name"];
        
        List<Point> newPoints = new List<Point>();
        
        for (int i = 0; i < json["pointCount"]; i++)
        {
            
            
            Vector3 spawnPos = new Vector3(json["p" + i]["x"],json["p" + i]["y"],json["p" + i]["z"]);
            
            
            Point newPoint;
            newPoint = SplineUtil.CreatePoint(spawnPos);


            if (json["p" + i]["word"] != "")
            {
                Vector3 textPos = new Vector3(json["p" + i]["text"]["x"], json["p" + i]["text"]["y"],
                    json["p" + i]["text"]["z"]);
                TextMesh newText = Instantiate(Services.Prefabs.spawnedText, newPoint.transform).GetComponent<TextMesh>();
                newText.transform.position = textPos;
                Services.Prefabs.SetFont(newText, json["p" + i]["text"]["font"]);
                newText.fontSize = json["p" + i]["text"]["fontSize"];
                newPoint.textMesh = newText;
            }
            
            newPoint.tension = json["p" + i]["tension"];
            newPoint.bias = json["p" + i]["bias"];
            newPoint.continuity = json["p" + i]["continuity"];
            int t = json["p" + i]["pointType"];
            newPoint.pointType = (PointTypes)t;
            
            
            if(i == 0)
            {
                parent.transform.position = newPoint.transform.position;
            }
            
            newPoint.transform.parent = pointsParent.transform;
            newPoints.Add(newPoint);
        }

        
        for (int i = 0; i < json["splineCount"]; i++)
        {    
            Point p1 = newPoints[json["spline" + i]["points"]["p" + 0]];
            Point p2 = newPoints[json["spline" + i]["points"]["p" + 1]];

            Spline newSpline;
            
            newSpline = SplineUtil.CreateSpline(p1, p2);
            
            int numPoints = json["spline" + i]["numPoints"];
            
            if (json["spline" + i]["numPoints"] > 2)
            {
                for (int k = 2; k < numPoints; k++)
                {
                    newSpline.SplinePoints.Add(newPoints[json["spline" + i]["points"]["p" + k]]);
                }
            }
            
            newSpline.closed = json["spline" + i]["closed"];
            newSpline.transform.parent = splineParent.transform;
        }

}

void DragCamera()
    {
        Services.mainCam.transform.position -= delta;

        curPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, cam.nearClipPlane);
        worldPos = cam.ScreenToWorldPoint(new Vector3(curPos.x, curPos.y,
            cameraDistance));
        lastPos = worldPos;
    }
    
    void PanCamera()
    {
        cam.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, Mathf.Clamp(cam.transform.position.z, -20, -2));
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
                DeselectPoints();
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
                    DeselectPoints();
                    pointCoords.gameObject.SetActive(true);
                    AddSelectedPoint(p);
                }
            }else{
                
                AddSelectedPoint(p);
                
                for (int i = Spline.Splines.Count -1; i >= 0; i--)
                {
                    if (Spline.Splines[i].SplinePoints.Contains(activePoint))
                    {
                        AddSelectedSpline(Spline.Splines[i]);
                        break;
                    }
                }
                
                if (pointSelected && curTool == Tool.select)
                {
                    pointOptions.SetActive(true);
                }

                if (pointSelected && curTool == Tool.move)
                {
                    pointCoords.gameObject.SetActive(true);
                }
            }
        }
    }
    void AddSelectedPoint(Point p)
    {
        if (!selectedPoints.Contains(p))
        {
            selectedPoints.Add(p);
            selectors[Mathf.Clamp(selectedPoints.Count - 1, 0, selectors.Count-1)].color = Color.white;
            
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

    void RemoveSelectedSpline(Spline s)
    {
        s.ChangeMaterial(s.lineMaterial);
        selectedSplines.Remove(s);
        if (selectedSplines.Count < 1)
        {
            splineSelectedTip.SetActive(false);
        }
    }
    void AddSelectedSpline(Spline s, bool add = false)
    {

        if (Input.GetKey(KeyCode.LeftShift))
        {
            add = true;
        }
            if (!add && selectedSpline != null)
            {
                RemoveSelectedSpline(selectedSpline);
            }


            if (!selectedSplines.Contains(s))
            {
                splineindex = Spline.Splines.IndexOf(s);
                selectedSpline.SwitchMaterial(3);
                selectedSplines.Add(selectedSpline);
                splineSelectedTip.SetActive(true);
            }


    }

    void UseTool()
    {
        switch (curTool)
        {

            case Tool.move:

                

                if (pointSelected)
                {
                    if (hitPoint != null && Input.GetMouseButtonDown(0))
                    {
                        dragging = true;
                    }

                    if (dragging || Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButton(0))
                    {

                        MoveSelectedPoints();
//                                dragging = true;
//                            if (hitPoint != activePoint)
//                            {
//                                selectedPoints.Remove(hitPoint);
//                                selectedPoints.Add(hitPoint);
//                            }


                    }
                }

                if (!dragging && hitPoint == null && Input.GetMouseButton(0) &&
                    !Input.GetKey(KeyCode.LeftShift))
                {
                    DragCamera();
                }

                break;
            case Tool.select:



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

            case Tool.clone:


                if (Input.GetMouseButtonDown(0))
                {
                    dragging = true;



                    //get all the splines and points you want to copy
                    //keep track of all the points that are copied to avoid duplicates

                    List<Point> pointsToCopy = selectedPoints;
                    List<Point> newPoints = new List<Point>();
                    List<Spline> newSplines = new List<Spline>();


                    foreach (Spline s in selectedSplines)
                    {
                        List<Point> splinePoints = new List<Point>();
                        foreach (Point p in s.SplinePoints)
                        {
                            Point newPoint = null;
                            bool alreadyCopied = false;
                            foreach (Point pew in newPoints)
                            {
                                if (Vector3.Distance(p.Pos, pew.Pos) < 0.05f)
                                {
                                    newPoint = pew;
                                    alreadyCopied = true;
                                    break;
                                }
                            }

                            if (!alreadyCopied)
                            {
                                newPoint = Instantiate(p.gameObject, pointsParent.transform)
                                    .GetComponent<Point>();
                                newPoints.Add(newPoint);
                                if (pointsToCopy.Contains(p))
                                {
                                    pointsToCopy.Remove(p);
                                }

                            }

                            splinePoints.Add(newPoint);
                        }

                        Spline newSpline = SplineUtil.CreateSplineFromPoints(splinePoints);
                        newSpline.closed = selectedSpline.closed;
                        newSpline.transform.parent = splinesParent;
                        newSplines.Add(newSpline);
                    }

                    foreach (Point p in pointsToCopy)
                    {
                        newPoints.Add(Instantiate(p.gameObject, pointsParent.transform)
                            .GetComponent<Point>());
                    }

                    if (!Input.GetKey(KeyCode.LeftShift))
                    {
                        DeselectSpline();
                        DeselectPoints();
                    }

                    foreach (Spline s in newSplines)
                    {
                        AddSelectedSpline(s, true);
                    }

                    foreach (Point p in newPoints)
                    {
                        AddSelectedPoint(p);
                    }


                }

                if (dragging)
                {
                    MoveSelectedPoints();
                }


                break;

            case Tool.rotate:

                if (Input.GetMouseButtonDown(0))
                {
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        rotationPivot = worldPos;
                    }

                    else
                    {
                        rotationPivot = center;
                    }
                }

                if (Input.GetMouseButton(0))
                {
                    cursor.transform.position = cam.WorldToScreenPoint(rotationPivot);
                    float angle = Mathf.Sign(delta.x) * delta.magnitude * 100f;
                    cursor.transform.Rotate(0, 0, angle);

                    foreach (Point p in selectedPoints)
                    {
                        Vector3 pivot = new Vector3(rotationPivot.x, rotationPivot.y, p.Pos.z);

                        p.transform.RotateAround(pivot, Vector3.forward, angle);

                    }

                }

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
                                splineindex = Spline.Splines.IndexOf(spp.s);

                                RemoveSelectedPoint(activePoint);
                                AddSelectedSpline(spp.s);
                                AddSelectedPoint(hitPoint);
                                
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

            case Tool.text:

                if (pointSelected)
                {
                    
                    pointText = activePoint.text;
                    
                    bool stopTyping = false;

                    if (!typing && Input.GetMouseButtonDown(0))
                    {
                        typing = true;

                    }else if (typing && Input.GetMouseButtonDown(0) && hitPoint == null)
                    {
                        dragging = true;
                    }

                    if (activePoint.textMesh == null && typing)
                    {
                        GameObject newText = Instantiate(Services.Prefabs.spawnedText,
                            activePoint.transform);
                        newText.transform.position = worldPos;
                        activePoint.textMesh = newText.GetComponent<TextMesh>();
                    }

                    if (typing)
                    {
                        textCursor.transform.position = cam.WorldToScreenPoint(activePoint.textMesh.transform.position);

                        textCursor.enabled = Mathf.PingPong(Time.time, 0.5f) > 0.25f;
                        textCursor.transform.localScale = new Vector3(0.1f, 1, 1) * (activePoint.textMesh.fontSize /64f);
                        foreach (char c in Input.inputString)
                        {
                            if (c == '\b') // has backspace/delete been pressed?
                            {
                                if (pointText.Length != 0)
                                {
                                    pointText = pointText.Substring(0, pointText.Length - 1);
                                }
                                else
                                {
                                    activePoint.text = "";
                                    GameObject textToDestroy = activePoint.textMesh.gameObject;
                                    activePoint.textMesh = null;
                                    Destroy(textToDestroy);
                                    typing = false;
                                    return;
                                }
                            }
                            else if ((c == '\n') || (c == '\r'))
                            {
                                typing = false;
                                
                                return;
                            }
                            else
                            {
                                pointText += Input.inputString;
                            }
                        }
                        activePoint.text = pointText;
                        activePoint.textMesh.text = pointText;
                        
                    }
                    else
                    {
                        textCursor.enabled = false;
                        
                        if (Input.GetKeyDown(KeyCode.Equals))
                        {
                            activePoint.textMesh.fontSize = Mathf.Clamp((int)(activePoint.textMesh.fontSize * 1.5f), 24, 712);
                        }

                        if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
                        {
                            activePoint.textMesh.fontSize =
                                Mathf.Clamp((int) (activePoint.textMesh.fontSize / 1.5f), 24, 712);
                        }
                    }

                    if (dragging)
                    {
                        activePoint.textMesh.transform.position = worldPos;
                    }
                    
                    

                }
                else
                {
                    typing = false;

                }

                break;

        }
    }
    void ClearSelection()
    {

           for (int i = selectedPoints.Count - 1; i >= 0; i--)
            {
                RemoveSelectedPoint(selectedPoints[i]);
           }
        

        splineindex = -1;
    }
    void RemoveSelectedPoint(Point p)
    {
        if (selectedPoints.Contains(p))
        {
            selectors[Mathf.Clamp(selectedPoints.Count - 1, 0, selectors.Count -1)].color = Color.clear;
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

                if (curTool != Tool.rotate)
                {
                    cursor.transform.rotation = Quaternion.identity;
                    
                }
                if (curTool != Tool.select)
                {
                    pointOptions.SetActive(false);  
                }


                if (curTool == Tool.draw || curTool == Tool.select)
                {
                    pointCoords.gameObject.SetActive(false);
                    cursor.rectTransform.pivot = new Vector3(0f, 0f);
                    dragging = false;
                }else
                {
                    cursor.rectTransform.pivot = new Vector3(0.5f, 0.5f);
                }
                

                if (curTool == Tool.text)
                {
                    textCursor.enabled = false;
                }
                
                
                
                tools[i].color = Color.gray;
                tooltips[i].SetActive(false);
            }
        }
    }

    void DeselectSpline()
    {   
        foreach (Spline s in selectedSplines)
        {
            s.ChangeMaterial(s.lineMaterial);
        }
        selectedSplines.Clear();
        splineSelectedTip.SetActive(false);
        
    }
    void DeselectPoints()
    {
        selectedPoints.Clear();
        selectedPointIndicator.SetActive(false);
        pointSelectedTip.SetActive(false);
        foreach (Image i in selectors)
        {
            i.color = Color.clear;
        }
        
        pointOptions.SetActive(false);
        pointCoords.gameObject.SetActive(false);
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

        Vector3 marQueecenter = Vector3.Lerp(pos, worldPos, 0.5f);
        Vector3 size = worldPos - pos;
        
       

        if (Vector3.Distance(worldPos, pos) > 0.25f)
        {
            if (!Input.GetKey(KeyCode.LeftShift))
            {
                DeselectPoints();
            }
            foreach (Collider c in Physics.OverlapBox(marQueecenter, new Vector3(Mathf.Abs(size.x), Mathf.Abs(size.y), 10) / 2))
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
   
}
