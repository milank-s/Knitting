using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;
using System.IO;
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

    PointTypes curPointType = PointTypes.normal;

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
                
            tooltips[(int)value].SetActive(true);   
        }

        get { return curTool; }
    }

    
    [Space(25)] public Transform pointsParent;
    public GameObject tooltipParent;
    public SplineTurtle splineTurtle;
    public GameObject turtleUI;
    public Transform splinesParent;
    public Transform stellationsParent;
    public PrefabManager prefabs;
    public Main main;
    public  Image textCursor;
    public static bool editing = false;
    public GameObject selector;
    public Transform canvas;
    public GameObject selectedPointIndicator;
    public GameObject pointOptions;
    private List<Image> selectors;
    [SerializeField] private Text[] pointOrder;
    public Transform container;
    private bool raycastNull;
    private Camera cam;
    //public Camera zCam;
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

    public ReadSliderValue lineWidthSlider;
    public ReadSliderValue biasSliderVal;
    public ReadSliderValue continuitySliderVal;
    public ReadSliderValue tensionSliderVal;
    public Text startSpeed;
    public Text splineSpeedReadout;
    public Text accelerationReadout;
    public Text maxSpeedReadout;
    public Slider splineSpeedVal;
    public Slider speedSlider;
    public Slider accelerationSlider;
    public Slider maxSpeedSlider;
    public Text fov;
    public Slider fovSlider;
    public Toggle useCamPos;
    public Toggle splineLockToggle;
    public Slider scoreSlider;
    public Text scoreText;
    public Toggle lockXToggle;
    public Toggle lockYToggle;
    public Toggle lockZToggle;
    public Text splineOrder;
    public Text splineTypeReadout;
    public Text splineDirectionReadout;
    public InputField sceneTitle;
    public InputField controllerText;
    public InputField controllerTitle;
    public Dropdown levelList;
    public Dropdown unlockTypes;

    [Header("Crawler Settings")]
    public GameObject crawlerOptions;
    public Dropdown crawlerType;
    public ReadSliderValue crawlerSpeed;
    public ReadSliderValue crawlerIndex;
    public ReadToggleValue crawlerDir;

    private List<GameObject> text;

    public StellationController controller;

    public LineRenderer l;
    
    public static MapEditor instance;

    private Vector3 center
    {
        get { return Vector3.Lerp(upperRight, lowerLeft, 0.5f); }
    }

    private Vector3 bounds{
          get { return upperRight - lowerLeft; }
    }

    private Vector3 extents{
          get { return bounds/2f; }
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

    private Vector3 deltaWorld
    {
        get
        {
           return worldPos - lastPos;
        }
    }

    Vector2 deltaScreen{
         get
        {
           return (mousePos - lastMousePos)/100f;
        }
    }
    private Vector3 mousePos;
    private Vector3 lastPos;
    private Vector3 worldPos;
    private Vector3 lastMousePos;
    private Vector3 rotationPivot;
    private Vector3 scalePivot;
    private Vector3 scaleDelta;

    Vector3 viewDir = Vector3.forward;
    
    private bool recenterScalePivot;

    
    public enum Tool
    {
        select,
        move,
        draw,
        marquee,
        clone, 
        rotate,
        text,
        scale
    }


    private List<Point> selectedPoints = new List<Point>();

    private Spline selectedSpline
    {
        get
        {
            if (splineindex < 0)
            {
                return null;
            }

            if (controller._splines.Count > 0)
            {
                if (splineindex >= controller._splines.Count)
                {
                    splineindex = controller._splines.Count - 1;
                }

                splineOrder.text = "spline " + controller._splines[splineindex].order;
                splineTypeReadout.text = controller._splines[splineindex].type.ToString();
                splineDirectionReadout.text = controller._splines[splineindex].bidirectional ? "<—>" : "—>";
                splineOrder.transform.position = cam.WorldToScreenPoint(controller._splines[splineindex].SplinePoints[0].Pos + Vector3.up*0.15f);
                splineTypeReadout.transform.position =  cam.WorldToScreenPoint(controller._splines[splineindex].SplinePoints[0].Pos + Vector3.up*0.05f);
                splineDirectionReadout.transform.position =  cam.WorldToScreenPoint(controller._splines[splineindex].SplinePoints[0].Pos + Vector3.up*0.1f + Vector3.right * 0.5f);
                return controller._splines[splineindex];
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
    
    string userLevelPath;
    string devLevelPath;

    void Awake()
    {

        instance = this;

        userLevelPath = Application.streamingAssetsPath + "/UserLevels";
        devLevelPath = Application.streamingAssetsPath + "/Stellations";

        text = new List<GameObject>();
        
        foreach (Text t in canvas.GetComponentsInChildren<Text>())
        {
            text.Add(t.gameObject);
        }

        selectedPointIndicator.SetActive(false);
        selectedPointIndicator.SetActive(false);
        pointOptions.SetActive(false);
        selectors = new List<Image>();
        
        selectedPoints = new List<Point>();
    }

    public void Typing()
    {
        typing = true;
    }

    public void SetCrawlerDir(bool b){
        
        if(!selectedSpline) return;
        selectedSpline.crawlerDir = b;
    }

    public void SetCrawlerType(Int32 i){
        
        if(!selectedSpline) return;
        selectedSpline.crawlerType = (CrawlerType)i;
        Debug.Log("setting crawlerType to " + (CrawlerType)i + " " + i);
    }

    void SetCrawlerInfo(Spline s){
         
        if(!crawlerOptions.activeSelf){
            ToggleCrawlerOptions();
        }

        crawlerIndex.ChangeValue(s.crawlerIndex);
        crawlerSpeed.ChangeValue(s.crawlerSpeed);
        crawlerDir.ChangeValue(s.crawlerDir);
        crawlerType.SetValueWithoutNotify((int)s.crawlerType);
        crawlerIndex.slider.maxValue = s.SplinePoints.Count -1;
    }

    public void SetCrawlerIndex(float i){
        if(!selectedSpline) return;
        int ind = (int)i;
        selectedSpline.crawlerIndex = ind;
    }

    public void SetCrawlerSpeed(float f){
        if(!selectedSpline) return;
        selectedSpline.crawlerSpeed = f;
    }

    public void ChangeFOV(System.Single s)
    {
        controller.desiredFOV = (int)s;
        CameraFollow.instance.cam.fieldOfView = s;
        fov.text = s.ToString("F0");
    }

    public void ChangeStartSpeed(System.Single s)
    {
        controller.startSpeed = s;
        startSpeed.text = s.ToString("F1");
    }

    public void ChangeAcceleration(System.Single s)
    {
        controller.acceleration = s;
        accelerationReadout.text = s.ToString("F1");
    }

    public void ChangeMaxSpeed(System.Single s)
    {
        controller.maxSpeed = s;
        maxSpeedReadout.text = s.ToString("F1");
    }


    public void ChangeSplineSpeed(System.Single s)
    {
        selectedSpline.speed = s;
        splineSpeedReadout.text = s.ToString("F1");
    }
    public void SetStellationLock(bool b){
		controller.lockSplines = b;
	}

    public void LockX(bool b){
        controller.lockX = b;
    }

    public void LockY(bool b){
        controller.lockY = b;
    }

    public void LockZ(bool b){
        controller.lockZ = b;
    }


    IEnumerator Start()
    {
        cam = Services.mainCam;
        l.enabled = false;
        mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, cam.nearClipPlane);
        worldPos = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y,
            Main.cameraDistance));

        string filePath = Services.main.devMode ? devLevelPath : userLevelPath;

        DirectoryInfo directoryInfo = new DirectoryInfo(filePath);
        
        FileInfo[] allFiles = directoryInfo.GetFiles("*.*");
        
        foreach (FileInfo file in allFiles)
        {
            if (!file.Name.Contains("json") || file.Name.Contains("meta") )
            {
                
            }
            else
            {
                string fileName = Path.GetFileNameWithoutExtension(file.ToString());
                levelList.options.Add(new Dropdown.OptionData(fileName));
            }
        }

        int val = 0;

        foreach(CrawlerType c in Enum.GetValues(typeof(CrawlerType))){
            string crawlername = Enum.GetName(typeof(CrawlerType), (int)c);
            crawlerType.options.Add(new Dropdown.OptionData(crawlername));
        }
        
        unlockTypes.options.Add(new Dropdown.OptionData("None"));
        unlockTypes.options.Add(new Dropdown.OptionData("Laps"));
        unlockTypes.options.Add(new Dropdown.OptionData("Speed"));
        unlockTypes.options.Add(new Dropdown.OptionData("Pickups"));
        
        _curTool = Tool.draw;

        for (int i = 0; i < tooltips.Length; i++)
        {
            tooltips[i].SetActive(false);
        }
        
        for (int i = 0; i < 50; i++)
        {
            Image newSelector = Instantiate(selector, Vector3.one * 1000, Quaternion.identity).GetComponent<Image>();
            selectors.Add(newSelector);
            newSelector.color = Color.clear;
            newSelector.transform.SetParent(container, false);
            yield return null;
        }
    }

    
    public void EnterEditMode()
    {
        fov.text = controller.desiredFOV.ToString("F0");
        fovSlider.value = controller.desiredFOV;
        speedSlider.value = controller.startSpeed;
        accelerationSlider.value = controller.acceleration;
        maxSpeedSlider.value = controller.maxSpeed;
        useCamPos.isOn = controller.setCameraPos;
        splineLockToggle.isOn = controller.lockSplines;
        lockXToggle.isOn = controller.lockX;
        lockYToggle.isOn = controller.lockY;
        lockZToggle.isOn = controller.lockZ;

        ChangeWinCondition((int)controller.unlockMethod);
        
        //load from controller name
        sceneTitle.text = controller.name;

        if(controller.text != null){
            
            controllerText.text = controller.text;
        }else{
            controller.text = "";
            controllerText.text = "stellation text";
        }

        if(controller.title != null){
            controllerTitle.text = controller.title;
        }else{
            controller.title = "";
            controllerTitle.text = "stellation title";
        }

        typing = false;
        controller.Initialize();
        controller.Setup();
        controller.OnPlayerEnter();
        
        Services.main.EnterUIMode();
    }

    public void ToggleTurtleMode()
    {
        bool on =  turtleUI.activeSelf;
        turtleUI.SetActive(!on);
        tooltipParent.gameObject.SetActive(on);
    }

    
    public void TogglePlayMode()
    {
        Services.main.OnReset.Invoke();
        Services.main.activeStellation = controller;
        Services.main.activeStellation.Initialize();
        controller.isPlayerOn = true;
        
        if (pointSelected)
        {
            Services.StartPoint = activePoint;
        }

        if (Point.Points.Count > 0)
        {
            Services.main.InitializeLevel();
            Services.main.ActivatePlayer();
        }

        canvas.gameObject.SetActive(!editing);
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

    public void SetLineWidth(){
        
        selectedSpline.SetLineWidth((int) lineWidthSlider.val);
    }

    void EditSelectedSpline()
    {
        if (splineindex >= 0 && splineindex < controller._splines.Count)
        {

            if (Input.GetKeyDown(KeyCode.L))
            {
               
                bool locked = selectedSpline.type == Spline.SplineType.locked;

                if (locked)
                {
                    selectedSpline.SetSplineType(Spline.SplineType.normal);

                    if (controller._escapeSplines.Contains(selectedSpline))
                    {
                        controller._escapeSplines.Remove(selectedSpline);
                    }
                }
                else
                {
                    selectedSpline.SetSplineType(Spline.SplineType.locked);
                    if (!controller._escapeSplines.Contains(selectedSpline))
                    {
                        controller._escapeSplines.Add(selectedSpline);
                    }
                }
                
            }
            
            
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                selectedSpline.ChangeMaterial(selectedSpline.lineMaterial + 1);
            }

            for (int i = 0; i < pointOrder.Length; i++)
            {
                if (i < selectedSpline.SplinePoints.Count)
                {
                    pointOrder[i].text = i.ToString();
                    pointOrder[i].transform.position = cam.WorldToScreenPoint(selectedSpline.SplinePoints[i].Pos + Vector3.left * 0.1f);
                }
                else
                {
                    pointOrder[i].text = "";
                }
            }

            if (pointSelected)
            {
                splinePointTip.SetActive(true);
            }


            if (Input.GetKeyDown(KeyCode.I))
            {
                selectedSpline.ReverseSpline();
            }
            
            if (Input.GetKeyDown(KeyCode.O))
            {
                selectedSpline.bidirectional = !selectedSpline.bidirectional;
            }


            if (Input.GetKeyDown(KeyCode.C))
            {
                selectedSpline.closed = !selectedSpline.closed;
                selectedSpline.ResetLineLength();
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
                    
                    Services.fx.PlayAnimationAtPosition(FXManager.FXType.burst, activePoint.transform);
                    SynthController.instance.keys[0].PlayNote(40, 0.5f, 0.5f);
                    
                    selectedSpline.RemovePoint((selectedSpline.SplinePoints.IndexOf(activePoint)));
                    selectedSpline.ResetLineLength();

                    if (selectedSpline.SplinePoints.Count < 2)
                    {
                        DeselectSpline();
                        Destroy(selectedSpline);
                        ReassignSplineOrder();
                    }
                }

                if (Input.GetKeyDown(KeyCode.Equals))
                {
                    Point p = selectedSpline.AddNewPoint(selectedSpline.SplinePoints.IndexOf(activePoint));
                    AudioManager.instance.helmAudio.PlayNoteOnPoint(p);
                    p.transform.parent = pointsParent;
                }
            }
        }
    }

    void ChangeSelectedSpline()
    {
        if ((Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow)) &&
            controller._splines.Count > 0)
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

            if (i >= controller._splines.Count)
            {
                i = 0;
            }

            if (i < 0)
            {
                i = controller._splines.Count - 1;
            }

            AddSelectedSpline(controller._splines[i]);

            if (!Input.GetKey(KeyCode.LeftShift))
            {
                DeselectPoints();
            }


            foreach (Point p in selectedSpline.SplinePoints)
            {
                AddSelectedPoint(p);
            }

            SetCrawlerInfo(controller._splines[i]);

        }
    }

    void SetCursorPosition()
    {
        if (Input.GetMouseButton(0))
        {
            PanCamera();
        }

        lastPos = worldPos;
        lastMousePos = mousePos;
        mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, cam.nearClipPlane);
        worldPos = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y,
            Main.cameraDistance));
        
        cursor.transform.position = mousePos;
    }

    public void DeselectAll()
    {
            //RemoveSelectedPoint(hitPoint);
            DeselectPoints();
            DeselectSpline();
        
    }
    void RaycastFromCursor()
    {
        hitPoint = null;

        Ray r = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(r.origin, r.direction, out hit, 50))
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

            raycastNull = false;
        }
        else
        {    
            
            raycastNull = true;
        }

    }

    void TryChangeTool()
    {
        if (Input.GetKey(KeyCode.Q))
        {

            _curTool = Tool.select;
            if (pointSelected)
            {
                pointOptions.SetActive(true);
            }
        }
        else if (Input.GetKey(KeyCode.W))
        {
            _curTool = Tool.move;

        }
        else if (Input.GetKey(KeyCode.D))
        {
            _curTool = Tool.draw;
            l.enabled = true;
        }
        else if (Input.GetKey(KeyCode.M))
        {
            _curTool = Tool.marquee;

        }
        else if (Input.GetKey(KeyCode.E))
        {
            _curTool = Tool.rotate;

        }
        else if (Input.GetKey(KeyCode.V))
        {
            _curTool = Tool.clone;
        }
        else if (Input.GetKey(KeyCode.T))
        {
            _curTool = Tool.text;
        }  else if (Input.GetKey(KeyCode.R))
        {
            _curTool = Tool.scale;
        }
    }

     public void Step()
    {
        if (editing)
            {
                
                Cursor.visible = false;
                
                SetCursorPosition();

                if(Input.GetKeyDown(KeyCode.Comma))
                {
                    ShuffleSplineOrder(-1);
                }else if (Input.GetKeyDown(KeyCode.Period))
                {
                    ShuffleSplineOrder(1);
                }
               

                if (!Input.GetKey(KeyCode.LeftShift))
                {
                    cam.fieldOfView = Mathf.Clamp(cam.fieldOfView - Input.mouseScrollDelta.y * Time.deltaTime * 100f, 10, 160);
                }
                else if(!Input.GetMouseButton(0))
                {
                    //never move camera on the z, its annoying af

                    //cam.transform.position += Vector3.forward * Input.mouseScrollDelta.y * Time.deltaTime * 10;
                }

                if (!typing)
                {
                     bool recenter = false;
                
                if(Input.GetKeyDown(KeyCode.X)){
                    if(viewDir == Vector3.right){
                         viewDir = Vector3.left;
                    }else{
                        viewDir = Vector3.right;
                    }

                    recenter = true;
                }

                if(Input.GetKeyDown(KeyCode.Y)){
                     recenter = true;
                    if(viewDir == Vector3.down){
                         viewDir = Vector3.up;
                    }else{
                        viewDir = Vector3.down;
                    }
                }

                if(Input.GetKeyDown(KeyCode.Z)){
                     recenter = true;
                    if(viewDir == Vector3.forward){
                         viewDir = Vector3.back;
                    }else{
                        viewDir = Vector3.forward;
                    }
                }
                
                if(recenter){
                    Services.mainCam.transform.forward = viewDir;
                    CenterCamera();
                }

                    
                    HideUI();

                    if (curTool != Tool.text)
                    {
                        EditSelectedSpline();
                    }
                    
                    ChangeSelectedSpline();
                    
                    
                    tooltips[(int)curTool].SetActive(true);
                    
                    TryChangeTool();
                    
                    if (Input.GetKeyDown(KeyCode.S))
                    {
                        PlaySavedEffect();
                        Save(controller);
                    }
                    
                }

                if (Input.GetMouseButtonDown(1))
                {
                    DeselectAll();
                }

                RaycastFromCursor();

                UseTool();

                
                if (curTool != Tool.clone && curTool != Tool.rotate && !Input.GetKey(KeyCode.LeftAlt))
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
                
                
                if (Input.GetKeyDown(KeyCode.Delete))
                {
                    // File.Delete (Application.streamingAssetsPath + "/Levels/"+ controller.name + ".json");
                    // #if UNITY_EDITOR
                    //     AssetDatabase.Refresh();
                    // #endif
                    
                    // Reset();
                    // levelList.options.RemoveAll(d => d.text.Contains(controller.name));
                    // controller.name = "Untitled";
                    // sceneTitle.text = controller.name;
                }
                
                if (turtleUI.activeSelf)
                {
                    splineTurtle.UpdateTurtle();
                }
                   
            }
        
        
        }

     void SetPointType(PointTypes t)
     {
         activePoint.SetPointType(t);
         
         Point samplePoint = null;
         foreach(Point p in selectedPoints){
             p.SetPointType(t);
             samplePoint = p;
         }

         //play effects
        //  SynthController.instance.keys[0].PlayNote((int)t * 4 + 60, 0.5f, 0.1f);
        if(samplePoint){
         AudioManager.instance.helmAudio.PlayNoteOnPoint(samplePoint);
        }
         Services.fx.PlayAnimationAtPosition(FXManager.FXType.pulse, activePoint.transform);
     }

     void PlaySavedEffect()
    {
        Services.fx.title.text = "SAVED";
        StartCoroutine(Services.fx.FlashWord());
    }

    void CenterCamera(){
        //we want to play the camera at the closest bound based on its view dir
        //and then add the offset
        cam.transform.position = center;

        if(viewDir == Vector3.left){
            cam.transform.position += (extents.x + Main.cameraDistance) * Vector3.right;
        }else if(viewDir == Vector3.right){
            cam.transform.position -= (extents.x + Main.cameraDistance) * Vector3.right;
        }

         if(viewDir == Vector3.up){
            cam.transform.position -= (extents.y + Main.cameraDistance) * Vector3.up;
        }else if(viewDir == Vector3.down){
            cam.transform.position += (extents.y + Main.cameraDistance) * Vector3.up;
        }

         if(viewDir == Vector3.forward){
            cam.transform.position -= (extents.z + Main.cameraDistance) * Vector3.forward;
        }else if(viewDir == Vector3.back){
            cam.transform.position += (extents.z + Main.cameraDistance) * Vector3.forward;
        }
    }
    void EditSelectedPoint()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (Point.Points.Count > 0)
            {
                //no more going on z
                CenterCamera();
            }
        }
        
        if (pointSelected)
        {

            marqueeTip.SetActive(false);
            deselectTip.SetActive(true);
            pointSelectedTip.SetActive(true);
            selectedPointIndicator.SetActive(true);
            pointCoords.gameObject.SetActive(true);

            if (Input.GetKeyDown(KeyCode.U))
            {

                foreach (Point p in selectedPoints)
                {
                    p.tension = Mathf.PingPong(p.tension + 1, 1);
                }
            }

            tensionSliderVal.val = activePoint.tension;

            pointType.text = activePoint.pointType.ToString();
            xPos.text = "x  " + activePoint.Pos.x.ToString("F2");
            yPos.text = "y  " + activePoint.Pos.y.ToString("F2");
            zPos.text = "z  " + activePoint.Pos.z.ToString("F2");

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SetPointType(PointTypes.normal);
                curPointType = PointTypes.normal;

            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {

                 SetPointType(PointTypes.stop);
                 curPointType = PointTypes.stop;

            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                 SetPointType(PointTypes.pickup);
                 curPointType = PointTypes.pickup;

            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                 SetPointType(PointTypes.fly);
                 curPointType = PointTypes.fly;
            }

            else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                 SetPointType(PointTypes.start);

            }else if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                 SetPointType(PointTypes.end);
                
            }else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                 SetPointType(PointTypes.ghost);
                 curPointType = PointTypes.ghost;
            }
              else if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                SetPointType(PointTypes.reset);
            }else if(Input.GetKeyDown(KeyCode.Alpha9))
            {
                SetPointType(PointTypes.connect);
            }
             
            
            if (Input.GetKeyDown(KeyCode.Backspace) && curTool != Tool.text)
            {
                Services.fx.PlayAnimationAtPosition(FXManager.FXType.burst, activePoint.transform);
                SynthController.instance.keys[0].PlayNote(40, 0.5f, 0.5f);
                
                DeletePoint(activePoint);
            }
        }
        else
        {
            marqueeTip.SetActive(true);
            deselectTip.SetActive(false);
        }
    }

    public void DeletePoint(Point pointToDelete){
        
                RemoveSelectedPoint(activePoint);
                
                foreach (Spline s in controller._splines)
                {
                    if (s.SplinePoints.Contains(pointToDelete))
                    {
                        s.SplinePoints.Remove(pointToDelete);
                        if(splineindex != -1){
                            selectedSpline.ResetLineLength();
                        }
                    }

                    if (s.SplinePoints.Count < 2)
                    {
                         if(splineindex != -1){
                            if (selectedSplines.Contains(s))
                            {
                                selectedSplines.Remove(s);
                            }
                         }
                        
                        Destroy(s);
                        
                        ReassignSplineOrder();
                    }
                }

                controller.RemovePoint(pointToDelete);

                Destroy(pointToDelete.gameObject);
    }

    void ReassignSplineOrder()
    {
       int i = 0;
       
       foreach (Spline s in controller._splines)
       {
           s.order = i;
           i++;
       } 
    }
    
    void OnDrawGizmos(){
        
        Gizmos.DrawWireCube(center, bounds);
    }
    void ManageSelectionUI()
    {
        int index = 0;

        lowerLeft = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
        upperRight = new Vector3(-Mathf.Infinity, -Mathf.Infinity, -Mathf.Infinity);

        if (pointSelected)
        {
            foreach (Point p in selectedPoints)
            {
                ComparePointPosition(p);
                SetPointInfo(p, index);
                p.Step();
                index++;
            }
        }
        else
        {
            foreach (Point p in Point.Points)
            {
                p.Step();
                ComparePointPosition(p);
            }
        }
    }

    void ComparePointPosition(Point p)
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
        }

        void SetPointInfo(Point p, int i)
        {
            if (i < selectors.Count)
            {
                selectors[i].transform.Rotate(Vector3.forward);
                selectors[i].transform.position = cam.WorldToScreenPoint(p.Pos);
            }


            if (i == selectedPoints.Count - 1)
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
        }
        
    void MoveSelectedPoints()
    {

        foreach (Point p in selectedPoints)
        {

            p.transform.position += new Vector3(deltaWorld.x, deltaWorld.y,
                deltaWorld.z);
            p.initPos = p.Pos;
        }
    }


    public void Save(StellationController c)
    {
        instance = this;
        
        JSONObject level = new JSONObject();

        Point[] points = c.GetComponentsInChildren<Point>();
        
        level["name"] = c.name;
        level["pointCount"].AsInt = points.Length;

        for (int i = 0; i < points.Length; i++)
        {
            level["p" + i] = points[i].Save(i);
        }
        
        level["splineCount"].AsInt = c._splines.Count;
        level["unlockType"].AsInt = (int) c.unlockMethod;
        level["speed"].AsFloat = c.speed; 
        level["acceleration"].AsFloat = c.acceleration; 
        level["maxSpeed"].AsFloat = c.maxSpeed; 
        level["time"].AsFloat = c.time;
        level["laps"].AsInt = c.laps;
        level["startSpeed"].AsFloat = c.startSpeed;
        level["text"] = c.text;
        level["title"] = c.title;
        
        level["forceOrder"] = c.lockSplines;
        JSONObject cameraData = new JSONObject();
        cameraData["lockX"] = c.lockX;
        cameraData["lockY"] = c.lockY;
        cameraData["lockZ"] = c.lockZ;
        cameraData["x"].AsFloat = c.cameraPos.x;
        cameraData["y"].AsFloat =  c.cameraPos.y;
        cameraData["z"].AsFloat =  c.cameraPos.z;
        cameraData["setPos"].AsBool = c.setCameraPos;
        cameraData["fov"].AsInt = c.desiredFOV;
        level["camera"] = cameraData;

        for (int j = 0; j < c._splines.Count; j++)
        {
            Spline s = c._splines[j];
            
            JSONObject splineData = new JSONObject();
            //record if its closed
            splineData["order"].AsInt = s.order;
            splineData["closed"].AsBool = s.closed;
            splineData["numPoints"] = s.SplinePoints.Count;
            splineData["type"].AsInt = (int)s.type;
            splineData["lineTexture"] = s.lineMaterial;
            splineData["lineWidth"] = s.lineWidth;
            splineData["bidirectional"].AsBool = s.bidirectional;
            splineData["speed"].AsFloat = s.speed;
            splineData["crawlerType"].AsInt = (int)s.crawlerType;
            splineData["crawlerIndex"].AsInt = s.crawlerIndex;
            splineData["crawlerDir"].AsBool = s.crawlerDir;
            splineData["crawlerSpeed"].AsFloat = s.crawlerSpeed;
            JSONObject pointIndices = new JSONObject();

            int pi = 0;
            foreach (Point sp in s.SplinePoints)
            {

                for (int i = 0; i < points.Length; i++)
                {
                    if (sp == points[i])
                    {
                        pointIndices["p" + pi] = i;
                        pi++;
                    }
                }
            }

            splineData["points"] = pointIndices;
            level["spline" + j] = splineData;
        }

        string filePath = Services.main.devMode ? devLevelPath : userLevelPath;
        WriteJSONtoFile(filePath, c.name + ".json", level);

        
        bool contains = false;
        foreach (Dropdown.OptionData d in levelList.options)
        {
            if (d.text == c.name)
            {
                contains = true;
            }
        }

        if (!contains)
        {
            //we should also select this?
            levelList.options.Add(new Dropdown.OptionData(c.name));
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

    public void SetCameraPos(){
        Vector3 pos = Services.mainCam.transform.position;
        pos.z += Main.cameraDistance;
        controller.cameraPos = pos;
    }

    public void UseCameraPos(bool b){
        controller.setCameraPos = b;
    }

    public void LoadFromDropDown(Int32 i)
    {
        LoadInEditor(levelList.options[i].text);
    }

    public void LoadInEditor(string fileName)
    {
        //Delete everything already in the scene
        //take care of any local variables in here that reference shit in the scene

        if (!Input.GetKey(KeyCode.LeftShift))
        {
            Services.main.FullReset();
        }

        Load(fileName, null, false);
        
        EnterEditMode();
    }

    public void Reset()
    {
        DeselectAll();
		
        string levelName = controller.title;
        Destroy(controller.gameObject);
		
        splinesParent = new GameObject().transform;
        splinesParent.transform.parent = stellationsParent;
        
        controller = splinesParent.gameObject.AddComponent<StellationController>();
        controller._points = new List<Point>();
        controller._splines = new List<Spline>();
        controller.title = levelName;
        controller.name = levelName;

        //this is also wiping all your fov settings... text... win con....
        //why use the nuclear option on the controller?
        //just remove points and splines while leaving the controller intact


        pointsParent = new GameObject().transform;
        pointsParent.name = "points";
        pointsParent.parent = splinesParent;
    }

    public StellationController LoadIntoStellation(StellationController c){

        //destroy children

        for(int i = 0; i < c.transform.childCount; i++){
            DestroyImmediate(c.transform.GetChild(i).gameObject);
        }

        Load(c.name, c);

        return c;
    }
    public StellationController Load(string fileName, StellationController stellationController = null, bool isGameLevel = true)
    {
        GameObject parent;
        GameObject pointParent;

        userLevelPath = Application.streamingAssetsPath + "/UserLevels";
        devLevelPath = Application.streamingAssetsPath + "/Stellations";

        //if we're in the application OR if a level set is asking, use streamingassets
        bool isDev = Application.isEditor;
        string filePath = isDev ? devLevelPath : userLevelPath;

        // loading a level for the main game, not for editor use, use game files
        filePath = isGameLevel ? devLevelPath : filePath; 

        JSONNode json = ReadJSONFromFile(filePath, fileName + ".json");
   
        if(stellationController == null){
            parent = new GameObject();
            parent.transform.parent = stellationsParent;
        }else{
            parent = stellationController.gameObject;
        }

        pointParent = new GameObject();
        pointParent.transform.parent = parent.transform;
        pointParent.name = "points";
        parent.name = fileName;

        pointsParent = pointParent.transform;
        splinesParent = parent.transform;
        
        StellationController c;

        if(stellationController == null){
            c = parent.AddComponent<StellationController>();
        }else{
            c = stellationController;
        }
        

        List<Point> newPoints = new List<Point>();
        List<Spline> newSplines = new List<Spline>();
        
        for (int i = 0; i < json["pointCount"]; i++)
        {
            
            Vector3 spawnPos = new Vector3(json["p" + i]["x"],json["p" + i]["y"],json["p" + i]["z"]);
            
            Point newPoint = SplineUtil.CreatePoint(spawnPos);
            
            if (json["p" + i]["word"] != "")
            {
                TextMesh newText;
                    
                Vector3 textPos = new Vector3(json["p" + i]["text"]["x"], json["p" + i]["text"]["y"],
                    json["p" + i]["text"]["z"]);
                if (newPoint.textMesh == null)
                {
                     newText = Instantiate(prefabs.spawnedText, newPoint.transform)
                        .GetComponent<TextMesh>();
                }
                else
                {
                    newText = newPoint.textMesh;
                }

                newPoint.textMesh = newText;
                newText.transform.position = textPos;
                prefabs.SetFont(newText, json["p" + i]["text"]["font"]);
                newText.fontSize = json["p" + i]["text"]["fontSize"];
                newText.text = json["p" + i]["word"];
                newPoint.text = newText.text;

                
            }else if (newPoint.textMesh != null)
            {
                
                newPoint.CleanText();
                
            }

            //newPoint.spawnCollectible = json["p" + i]["collect"];
            newPoint.tension = json["p" + i]["tension"];
            newPoint.bias = json["p" + i]["bias"];
            newPoint.continuity = json["p" + i]["continuity"];
            int t = json["p" + i]["pointType"];

            newPoint.SetPointType((PointTypes)t);
            
            newPoint.transform.parent = pointParent.transform;
            newPoint.gameObject.name = i.ToString();
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

                    int index = json["spline" + i]["points"]["p" + k].AsInt;
                    Point pointToAdd = newPoints[index];
                    if (!newSpline.SplinePoints.Contains(pointToAdd))
                    {
                        newSpline.SplinePoints.Add(newPoints[index]);
                    }
                }
                newSpline.name = newSpline.SplinePoints[0].name + " - " + newSpline.SplinePoints[numPoints-1].name;
            }

            int splineType = json["spline" + i]["type"];
            newSpline.type = (Spline.SplineType) splineType;
            newSpline.lineWidth = Mathf.Clamp(json["spline" + i]["lineWidth"], 1, 10);
            newSpline.lineMaterial = json["spline" + i]["lineTexture"];
            newSpline.closed = json["spline" + i]["closed"];
            newSpline.transform.parent = parent.transform;
            newSpline.order =  json["spline" + i]["order"];
            newSpline.bidirectional = json["spline" + i]["bidirectional"];
            newSpline.crawlerType = (CrawlerType) ((int)json["spline" + i]["crawlerType"]);
            newSpline.crawlerSpeed = json["spline" + i]["crawlerSpeed"];
            newSpline.crawlerIndex =  json["spline" + i]["crawlerIndex"];
            newSpline.crawlerDir =  json["spline" + i]["crawlerDir"];
            newSpline.ChangeMaterial(newSpline.lineMaterial);
            newSplines.Add(newSpline);
        }

        c.name = parent.name;

        //tuning

        c.speed = json["speed"];
        c.acceleration = json["acceleration"];
        
        c.maxSpeed = json["maxSpeed"];
        if(c.maxSpeed == 0){
            c.maxSpeed = 3f;
        }

        //win conditions
        c.laps = json["laps"];
        c.text = json["text"];
        c.time = json["time"];

        c.startSpeed = json["startSpeed"];
        int unlock = json["unlockType"];
        c.unlockMethod = (StellationController.UnlockType) unlock;

        c.title = fileName;
        // c.text = ???;

        //camera settings
        c.setCameraPos = json["camera"]["setPos"];
        c.cameraPos.x = json["camera"]["x"];
        c.cameraPos.y = json["camera"]["y"];
        c.cameraPos.z = json["camera"]["z"];
        c.lockX = json["camera"]["lockX"];
        c.lockY = json["camera"]["lockY"];
        c.lockZ = json["camera"]["lockZ"];
        c.desiredFOV = json["camera"]["fov"];

        //title
        c.lockSplines = json["forceOrder"];

        if (c.desiredFOV == 0)
        {
            c.desiredFOV = 40;
        }
        
        c._splines = newSplines;
        c._points = newPoints;
        c.GetBounds();

        controller =  c;
        return c;
    }


    public void ChangeScore(Single i)
    {
        
        switch (controller.unlockMethod)
        {
            case StellationController.UnlockType.laps:
                controller.laps = (int)i;
                scoreText.text = scoreSlider.value.ToString("F0");
                
                break;
            
            case StellationController.UnlockType.speed:
                controller.speed = i;
                scoreText.text = scoreSlider.value.ToString("F1");

                break;
        }
    }
    
    public void ChangeWinCondition(Int32 i)
    {
        controller.unlockMethod = (StellationController.UnlockType) i;

        if(controller.unlockMethod == StellationController.UnlockType.pickups){
            scoreSlider.gameObject.SetActive(false);
            scoreText.gameObject.SetActive(false);
            scoreSlider.gameObject.SetActive(false);
        }else{
            scoreSlider.gameObject.SetActive(true);
            scoreSlider.gameObject.SetActive(true);
            scoreText.gameObject.SetActive(true);
        }

        switch ((StellationController.UnlockType) i)
        {
            case StellationController.UnlockType.laps:
            
                scoreSlider.wholeNumbers = true;
                scoreSlider.value = controller.laps;
                scoreSlider.minValue = 0;
                scoreSlider.maxValue = 10;
                break;
            
            case StellationController.UnlockType.speed:
                
                scoreSlider.wholeNumbers = false;
                scoreSlider.minValue = 0;
                scoreSlider.maxValue = 10;
                scoreSlider.value = controller.speed;
                break;
        }

        unlockTypes.SetValueWithoutNotify(i);
    }
    
void DragCamera()
    {
        Services.mainCam.transform.localPosition -= deltaWorld;

        mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, cam.nearClipPlane);
        worldPos = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y,
            Main.cameraDistance));
        lastPos = worldPos;
    }
    
    void PanCamera()
    {
        //these directions need to be rotated by the viewdir

       Vector3 viewportPos = cam.ScreenToViewportPoint(mousePos);
        
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
                pointCoords.gameObject.SetActive(true);
                
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
                    AddSelectedPoint(p);
                    
                    pointCoords.gameObject.SetActive(true);
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
                
                if (pointSelected)
                {
                    if (curTool == Tool.select)
                    {
                        pointOptions.SetActive(true);
                        pointCoords.gameObject.SetActive(true);
                    }else if (curTool == Tool.move)
                    {
                        pointCoords.gameObject.SetActive(true);
                    }
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

            
        biasSlider.SetValueWithoutNotify(activePoint.bias);
        tensionSlider.SetValueWithoutNotify(activePoint.tension);
        biasSliderVal.ChangeValue(activePoint.bias);
        continuitySliderVal.ChangeValue(activePoint.continuity);
        tensionSliderVal.ChangeValue(activePoint.tension);
        selectedPointIndicator.SetActive(pointSelected);
        
    }

    public void AddSpline(Spline s)
    {
        controller.AddSpline(s);
    }
    
    void RemoveSelectedSpline(Spline s)
    {
        s.ChangeMaterial(s.lineMaterial);
        selectedSplines.Remove(s);
        if (selectedSplines.Count == 0)
        {
            splineSelectedTip.SetActive(false);
            splineOrder.text = "";
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

        if (s == null) return;
        
        if (!selectedSplines.Contains(s))
        {
            splineindex = controller._splines.IndexOf(s);
            selectedSplines.Add(selectedSpline);
            
            //draw locked stuff diff ? if(selectedSpline.)
            selectedSpline.SwitchMaterial(2);
            lineWidthSlider.ChangeValue(selectedSpline.lineWidth);
            splineSpeedVal.SetValueWithoutNotify(selectedSpline.speed);
            splineSpeedReadout.text = selectedSpline.speed.ToString("F1");
            splineSelectedTip.SetActive(true);

            SetCrawlerInfo(selectedSpline);
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

                    if (dragging || (Input.GetMouseButton(0) && Input.GetKey(KeyCode.LeftShift)))
                    {

                        MoveSelectedPoints();
                        if (Input.mouseScrollDelta.y != 0)
                        {

                            //zCam.gameObject.SetActive(true);
                        }
//                                dragging = true;
//                            if (hitPoint != activePoint)
//                            {
//                                selectedPoints.Remove(hitPoint);
//                                selectedPoints.Add(hitPoint);
//                            }
                    }
                    else
                    {
                       //zCam.gameObject.SetActive(false);
                    }
}
                else
                {
                    
                   // zCam.gameObject.SetActive(false);
                }

                if (!dragging && hitPoint == null && Input.GetMouseButton(0) &&
                    !Input.GetKey(KeyCode.LeftShift))
                {
                    DragCamera();
                }

                break;
            case Tool.select:
                
                Vector3 screenPos = cam.WorldToViewportPoint(lastPos);
                Vector3 viewPortPos = cam.ScreenToViewportPoint(mousePos);

                
                if (Input.GetMouseButton(0) && hitPoint == null && !pointSelected &&  !dragging)
                {
                    DragCamera();
                }
                else if(Input.GetMouseButton(0) && pointSelected && hitPoint == null  && !dragging)
                {
                    if (pointSelected)
                    {
                        biasSlider.value += (viewPortPos.x - screenPos.x) * 1000 * Time.deltaTime;
                        tensionSlider.value += (viewPortPos.y - screenPos.y) * 1000 * Time.deltaTime;
                    }
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
                            Point newP = null;
                            bool alreadyCopied = false;
                            foreach (Point pew in newPoints)
                            {
                                if (Vector3.Distance(p.Pos, pew.Pos) < 0.05f)
                                {
                                    newP = pew;
                                    alreadyCopied = true;
                                    break;
                                }
                            }

                            if (!alreadyCopied)
                            {
                                newP = Instantiate(p.gameObject, pointsParent.transform)
                                    .GetComponent<Point>();
                                newPoints.Add(newP);
                                if (pointsToCopy.Contains(p))
                                {
                                    pointsToCopy.Remove(p);
                                }
                                
                            }

                            splinePoints.Add(newP);
                        }

                        Spline newSpline = SplineUtil.CreateSplineFromPoints(splinePoints);
                        newSpline.closed = s.closed;
                        newSpline.bidirectional = s.bidirectional;
                        newSpline.speed = s.speed;
                        newSpline.lineWidth = s.lineWidth;
                        newSpline.lineMaterial = s.lineMaterial;
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
                        AddSpline(s);
                        s.order = controller._splines.IndexOf(s);
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
                    float xAngle = Mathf.Sign(deltaScreen.x) * deltaScreen.magnitude * 100f;
                    float yAngle = Mathf.Sign(deltaScreen.y) * deltaScreen.magnitude * 100f;
                    
                    cursor.transform.Rotate(0, 0, xAngle);

                    foreach (Point p in selectedPoints)
                    {
                        
                        p.transform.RotateAround(rotationPivot, viewDir, xAngle);

                        // if(Input.GetKey(KeyCode.LeftAlt)){
                        //     Vector3 pivot = new Vector3(rotationPivot.x, rotationPivot.y, rotationPivot.z);
                        //     p.transform.RotateAround(pivot, Vector3.up, -xAngle);
                        // }else if(Input.GetKey(KeyCode.RightAlt)){
                        //     Vector3 pivot = new Vector3(rotationPivot.x, rotationPivot.y, rotationPivot.z);
                        //     p.transform.RotateAround(pivot, Vector3.right, yAngle);
                        // }else{
                        //     Vector3 pivot = new Vector3(rotationPivot.x, rotationPivot.y, p.Pos.z);
                        //     p.transform.RotateAround(pivot, Vector3.forward, xAngle);
                        // }

                    }

                }

                break;

            case Tool.marquee:

                if (Input.GetMouseButtonDown(0))
                {
                    StartCoroutine(MarqueeSelect(worldPos));
                }
                
                break;

            case Tool.draw:

                bool pointCreated = false;
                
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
                
                Point newPoint = null;

                if (hitPoint != null && !Input.GetKey(KeyCode.LeftAlt))
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
                                
                                if (spp.s.controller == null)
                                {
                                    AddSpline(spp.s);
                                    spp.s.order = controller._splines.IndexOf(spp.s);
                                    spp.s.controller = controller;
                                }
                                
                                splineindex = Spline.Splines.IndexOf(spp.s);

                                RemoveSelectedPoint(activePoint);
                                AddSelectedSpline(spp.s);
                                AddSelectedPoint(hitPoint);
                                
                                pointCreated = true;
                                
                            }
                        }
                    }
                    else
                    {
                        if (Input.GetMouseButtonDown(0))
                        {
                            
                            AudioManager.instance.helmAudio.PlayNoteOnPoint(hitPoint);
                            AddSelectedPoint(hitPoint);
                        }
                    }
                }
                else if (raycastNull || Input.GetKey(KeyCode.LeftAlt))
                {
                        
                    if (Input.GetMouseButtonDown(0)){

                        pointCreated = true;
                        newPoint = SplineUtil.CreatePoint(worldPos);
                        newPoint.SetPointType(curPointType);
                        controller.AddPoint(newPoint);
                        newPoint.transform.parent = pointsParent;

                        if (Input.GetKey(KeyCode.LeftShift))
                        {
                            newPoint.tension = 1;
                        }
                        
                        if (selectedPoints.Count > 0)
                        {

                            SplinePointPair spp = SplineUtil.ConnectPoints(selectedSpline,
                                activePoint, newPoint);
                            if (spp.s != null)
                            {
                                spp.s.transform.parent = splinesParent;
                                spp.p.transform.parent = pointsParent;

                                if (spp.s.controller == null)
                                {
                                    AddSpline(spp.s);
                                    spp.s.order = controller._splines.IndexOf(spp.s);
                                    spp.s.controller = controller;
                                }
                                
                                RemoveSelectedPoint(activePoint);
                                AddSelectedSpline(spp.s);
                                
                            }
                            

                        }
                       
                        AddSelectedPoint(newPoint);

                    }
                }


                if (pointCreated)
                {
                    if (!(Point.Points.Count > 0))
                    {
                        newPoint.SetPointType(PointTypes.start);
                    }
                    AudioManager.instance.helmAudio.PlayNoteOnPoint(newPoint);
                }

                break;

            case Tool.text:

                if (pointSelected)
                {
                    
                    pointText = activePoint.text;

                    if (!typing && Input.GetMouseButtonDown(0) && hitPoint != null)
                    {
                        typing = true;
                    }else 

                    if (Input.GetMouseButtonDown(0) && hitPoint == null)
                    {
                        dragging = true;
                        typing = false;
                    }

                    if (activePoint.textMesh == null)
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
                                pointText += System.Environment.NewLine;
                                // pointText += '\n';
                                
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

                        if (Input.GetKeyDown(KeyCode.T))
                        {
                            Services.Prefabs.SetFont(activePoint.textMesh, Services.Prefabs.FindFontIndex(activePoint.textMesh.font) + 1);
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

                case Tool.scale:

                if (Input.GetMouseButtonDown(0))
                {
                    scaleDelta = Vector3.zero;
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        scalePivot = worldPos;
                        recenterScalePivot = false;
                    }

                    else
                    {
                        scalePivot = center;
                        recenterScalePivot = true;
                    }

                    foreach (Point p in selectedPoints)
                    {
                       p.initPos = p.transform.position;
                    }
                }

                if (Input.GetMouseButton(0))
                {
                    //if(recenterScalePivot) scalePivot = center;  //this leads to an infinite loop

                    cursor.transform.position = cam.WorldToScreenPoint(scalePivot);
                    scaleDelta += (Vector3)deltaScreen;
                    foreach (Point p in selectedPoints)
                    {
                        Vector3 diff = p.initPos - scalePivot;

                        p.transform.position = p.initPos + diff * Mathf.Sign(scaleDelta.x) * scaleDelta.magnitude;
                    }

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
    
    public void SetTensionWithSlider(){
        foreach(Point p in selectedPoints){
            p.tension = tensionSliderVal.val;
        }
    }

    public void SetBiasWithSlider(){
        foreach(Point p in selectedPoints){
            p.bias = biasSliderVal.val;
        }
    }

    public void SetContinuityWithSlider(){
        foreach(Point p in selectedPoints){
            p.continuity = continuitySliderVal.val;
        }
    }

    public void SetTension(float t)
    {
        
        foreach(Point p in selectedPoints){
            p.tension = t;
        }
        tensionSliderVal.ChangeValue(t);

    }
    
    
    public void SetBias(float t)
    {
        foreach(Point p in selectedPoints){
            p.bias = t;
        }
        biasSliderVal.ChangeValue(t);
    }

    
    public void SetContinuity(float t)
    {
        foreach(Point p in selectedPoints){
            p.continuity = t;
        }
        continuitySliderVal.ChangeValue(t);
    }

    public void SetStellationText(String s){
        controller.text = s;
        typing = false;
    }

    public void SetStellationTitle(String s){
        controller.title = s;
        controller.name = s;
        
        typing = false;
    }

    void ShuffleSplineOrder(int i)
    {
        
        if (controller._splines.Count > 0)
        {
            int newPosition = splineindex + i;
            
            if (newPosition < 0)
            {
                newPosition = controller._splines.Count - 1;
            }

            if (newPosition >= controller._splines.Count)
            {
                newPosition = 0;
            }
            
            Spline selected = selectedSpline;
            Spline splineToSwap = controller._splines[newPosition];

            //Debug.Log("spline " + selected + " with order " + selected.order + " in position " + controller._splines.IndexOf(selected).ToString());
            //Debug.Log("spline " + splineToSwap + " with order " + splineToSwap.order  +" in position " + controller._splines.IndexOf(splineToSwap).ToString());
            
            //Debug.Log("swapping");
            controller._splines[newPosition] = controller._splines[splineindex];
            controller._splines[splineindex] = splineToSwap;
            
            splineindex = newPosition;
            
            selected.order = controller._splines.IndexOf(selected);
            splineToSwap.order = controller._splines.IndexOf(splineToSwap);

            
            //Debug.Log("spline " + selected + " with order " + selected.order + " in position " + controller._splines.IndexOf(selected).ToString());
            //Debug.Log("spline " + splineToSwap + " with order " + splineToSwap.order  +" in position " + controller._splines.IndexOf(splineToSwap).ToString());
            
            AddSelectedSpline(selected);
        }
    }
    
    void ChangeTool()
    {
        for (int i = 0; i < tools.Length; i++)
        {
            if (i == (int) curTool)
            {
                tools[i].color = Color.white;
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
//                  pointCoords.gameObject.SetActive(false);
                    cursor.rectTransform.pivot = new Vector3(0f, 0f);
                    dragging = false;
                }else
                {
                    cursor.rectTransform.pivot = new Vector3(0.5f, 0.5f);
                }
                
               

                if (curTool != Tool.text)
                {
                    textCursor.enabled = false;
                }
                
                
                
                tools[i].color = Color.gray;
                tooltips[i].SetActive(false);
            }
        }
    }

    public void DeselectSpline()
    {   
        foreach (Spline s in selectedSplines)
        {
            if(s != null) s.ChangeMaterial(s.lineMaterial);
        }
        selectedSplines.Clear();
        splineSelectedTip.SetActive(false);
        splineOrder.text = "";
        if(crawlerOptions.activeSelf){
            ToggleCrawlerOptions();
        }
    }
    
    public void ToggleCrawlerOptions(){
        crawlerOptions.SetActive(!crawlerOptions.activeSelf);
    }
    public void DeselectPoints()
    {
        selectedPoints.Clear();
        foreach (Image i in selectors)
        {
            i.color = Color.clear;
        }
        selectedPointIndicator.SetActive(false);
        pointSelectedTip.SetActive(false);
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
