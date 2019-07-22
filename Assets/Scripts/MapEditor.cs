using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;

public class MapEditor : MonoBehaviour
{

    public static bool editing = false;
    public GameObject selector;
    public Transform canvas;
    private List<Image> selectors;
    
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
    }

    void Start()
    {
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
    }
    
    void RemoveSelectedPoint(Point p)
    {
        if (selectedPoints.Contains(p))
        {
            selectors[selectedPoints.Count - 1].color = Color.clear;
            selectedPoints.Remove(p);
        }
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
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (selectedPoints.Count > 0)
            {
                Services.StartPoint = selectedPoints[0];
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
            }

            switch (curTool)
            {
                case Tool.select:

                    if (hitPoint != null)
                    {
                        if (Input.GetMouseButtonDown(1))
                        {
                            RemoveSelectedPoint(hitPoint);
                        }

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

                case Tool.draw:

                    if (hitPoint == null && Input.GetMouseButtonDown(0))
                    {
                        SplineUtil.CreatePoint(worldPos);
                    }

                    break;

                case Tool.connect:
                    break;

            }

            int index = 0;
            foreach (Point p in selectedPoints)
            {
                selectors[index].transform.Rotate(Vector3.forward);
                selectors[index].transform.position = cam.WorldToScreenPoint(p.Pos);
                p.proximity = Mathf.Sin(Time.time);
                index++;
            }

        }
    }
}
