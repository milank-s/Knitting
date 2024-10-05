using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public enum Shapes{polygon, spiral, wave}
public class SplineTurtle : MonoBehaviour {

	public MapEditor editor;

	[Header("UI")] 
	
	[SerializeField] private ReadSliderValue numPointsUI;
	[SerializeField] private ReadSliderValue minDistUI;
	[SerializeField] private ReadSliderValue maxDistUI;
	[SerializeField] private ReadSliderValue distScaleUI;
	[SerializeField] private ReadSliderValue angleeUI;
	[SerializeField] private ReadSliderValue angleDeltaUI;
	[SerializeField] private ReadSliderValue angleScaleUI;
	
	[SerializeField] private ReadSliderValue continuityUI;
	[SerializeField] private ReadSliderValue tensionUI;
	
	[SerializeField] private ReadSliderValue pivotAngleUI;
	[SerializeField] private ReadSliderValue pivotDistanceUI;
	[SerializeField] private ReadSliderValue stepSpeedUI;

	[SerializeField] private InputField xOffsetUI;
	[SerializeField] private InputField yOffsetUI;
	[SerializeField] private InputField zOffsetUI;

	[SerializeField] private ReadToggleValue zigzagUI;
	[SerializeField] private ReadToggleValue randomlyZagUI;
	[SerializeField] private ReadToggleValue closeToggle;
	[SerializeField] private ReadToggleValue connectUI;
	[SerializeField] private ReadToggleValue ghostToggle;
	
	public static float maxTotalPoints = 1000;
	public static float maxCrawlers = 1;

	public string name;

	int pointCount;
	public List<Point> points;
	public List<Spline> splines;

	public bool createSplines;
	public bool Randomize;

	public int initialAmount;
	public float initialAngleMax;
	public float initialAngleMin;

	public Transform pivot;
	public Transform turtle;
	
	public float stepSpeed = 0.1f;
	
	public float angleChange = 0;
	public float angleVariance = 10;
	public float angle = 30;
	public float scaleChange = 0;
	public float maxDist = 2;
	public float minDist = 1;
	public float branchFactor = 0;
	public int maxPoints = 50;
	public float continuity = 0;
	public float tension;
	public bool Raycast = true;
	
	public bool alternateAngle = false;

	private float angleRandom;
	private float ang;
	private float mxDist;
	private float mDist;

	
	public float  PivotSpeed;
	public bool closed;
	public bool childrenInherit = false;
	private bool turnleft = true;

	public bool ghostPoints;
	
	private bool running;
	private bool redraw;
	public bool updatePoints;
	private bool randomlyZag;
	
	public Vector3 offsetDirection = Vector3.zero;

	private float timeSinceRedraw;
	
	
	private Coroutine drawing;
	
	
	Mesh mesh;
	Spline curSpline;
	public Point curPoint;


	public void Clear(){
		if(points.Count > 0){
			for(int i = points.Count-1; i >= 0; i--){
				editor.DeletePoint(points[i]);
			}
		}

		Reset();
	}

	//for toggling between play modes
	public void Reset()
	{
		points.Clear();
		splines.Clear();
		pointCount = 0;
		turtle.position = Vector3.zero;
		turtle.rotation = Quaternion.identity;
		pivot.position = turtle.position + Vector3.up * pivotDistanceUI.val;

	}

	public void Generate()
	{
		Clear();
		
		if (Randomize) {

			initialAngleMax = Random.Range(-90, 45);
			initialAngleMin = Random.Range(initialAngleMax, 90);

			angleChange = Random.Range (0,3);
			angleVariance = Random.Range (-90, 10);
			angle = Random.Range (angleVariance, 90);
			scaleChange = Random.Range (0.98f, 1.02f);
			if (Random.Range (0, 100) < 90) {
				maxDist = Random.Range (1f, 2f);
				minDist = Random.Range (1, maxDist);
				maxPoints = Random.Range (5, 10);
				initialAmount = 1;
			} else {
				maxDist = Random.Range (3f, 5f);
				minDist = Random.Range (2, maxDist);
				initialAmount = Random.Range (20,25);
				maxPoints = Random.Range (8, 10);
			}
			branchFactor = Random.Range(0,0);
			continuity = Random.Range(0,2);

			
			alternateAngle = Random.Range (0f, 100f) > 50 ? true : false;
			PivotSpeed = Random.Range (0f, 2f);
		}
		
		InitializeSpline();
	}

	public void UpdateTurtle()
	{
		UpdateValues();

		if (redraw)
		{
			timeSinceRedraw += Time.deltaTime;
			Debug.Log("redrawing");

			Generate();
			redraw = false;
		}

		if(updatePoints){
			ChangePointSettings();
			updatePoints = false;
		}
	}

	public void RedrawTurtle()
	{
		
		redraw = true;
		timeSinceRedraw = 0;
	}

	void UpdateValues()
	{
		closed = closeToggle.val;
		alternateAngle = zigzagUI.val;
		randomlyZag = randomlyZagUI.val;
		Raycast = connectUI.val;
		maxPoints = (int)numPointsUI.val;
		minDist = minDistUI.val;
		maxDist = maxDistUI.val;
		scaleChange = distScaleUI.val;
		
		angle = angleeUI.val;
		angleVariance = angleDeltaUI.val;
		angleChange = angleScaleUI.val;

		ghostPoints = ghostToggle.val;
		continuity = continuityUI.val;
		tension = tensionUI.val;

		stepSpeed = stepSpeedUI.val;
		pivot.position = editor.splinesParent.position + Vector3.up * pivotDistanceUI.val;
		PivotSpeed = pivotAngleUI.val;
		
		float.TryParse(xOffsetUI.text, out offsetDirection.x);
		float.TryParse(yOffsetUI.text, out offsetDirection.y);
		float.TryParse(zOffsetUI.text, out offsetDirection.z);
	}
	
	public void Draw(){
		
		
		for(int i = pointCount; i < maxPoints; i++) {
			Step ();
			NewPoint ();
		}

		if (createSplines && closed) {

			SplinePointPair spp;

			spp = SplineUtil.ConnectPoints (curSpline, curSpline.SplinePoints[curSpline.SplinePoints.Count-1], curSpline.SplinePoints[0]);

			AddSpline(spp.s);
			
			curSpline = spp.s;
			curPoint = spp.p;
			curPoint.transform.parent = editor.pointsParent.transform;
			curSpline.transform.parent = editor.splinesParent;

		
		}

		if (maxCrawlers < 100) {
			for (int i = 0; i < initialAmount; i++) {
				SpawnTurtle ().transform.Rotate (0, 0, turtle.eulerAngles.z + Random.Range (initialAngleMin, initialAngleMax) * i);

			}
		}
		
		running = false;
	}

	public void Complete(){
		Reset();
	}

	void AddSpline(Spline s){
		if(!splines.Contains(s)){
			splines.Add(s);
			editor.AddSpline(s);
		}
	}
	Point CreatePoint(){
		Point p = SplineUtil.CreatePoint (turtle.position);
		p.transform.parent = editor.pointsParent.transform;
		editor.controller.AddPoint(p);
		points.Add(p);
		pointCount ++;
		return p;
	}
	void InitializeSpline(){
		
		//parent.name = name;
		
		ang = angle;
		angleRandom = angleVariance;
		mxDist = maxDist;
		mDist = minDist;

//		if(Raycast){
//			curPoint = SplineUtil.RaycastDownToPoint (turtle.position, Mathf.Infinity, 1000f);
//			if (curPoint == null)
//			{
//				
//			}

			curPoint = CreatePoint();

			Step ();
			NewPoint();
			
//			Point secondPoint = SpawnPointPrefab.CreatePoint (turtle.position);
//
//			if (createSplines) {
//				curSpline = SplineUtil.CreateSpline (curPoint, secondPoint);
//			}
			
			//curPoint = secondPoint;
			//curPoint.transform.parent = editor.pointsParent.transform;
		

		if (ghostPoints)
		{
			curPoint.SetPointType(PointTypes.ghost);
		}

		curSpline.transform.parent = editor.splinesParent;
		Draw();
	}

	public GameObject SpawnTurtle(){
		GameObject newTurtle = Instantiate (gameObject, turtle.position, Quaternion.Euler(turtle.eulerAngles));

		SplineTurtle newTurtleScript = newTurtle.GetComponent<SplineTurtle> ();

		newTurtle.transform.Rotate (0,0,Random.Range (initialAngleMin, initialAngleMax));

		if (!childrenInherit) {
			newTurtleScript.angle = angle;
			newTurtleScript.angleVariance = angleVariance;
			newTurtleScript.maxDist = maxDist;
			newTurtleScript.minDist = minDist;
		}
		maxCrawlers++;
		// newTurtleScript.Generate();
		return newTurtle;
	}

	public void TryConnectPoints(){
		//iterate through all points, raycast down, create new splines
		
	}

	public void ChangeClosed(){
		if(curSpline != null){
			curSpline.closed = closed;
		}
	}

	public void ChangePointAmount(){
		if(!running){
			if(pointCount < maxPoints){
				Draw();
			}else if(pointCount > maxPoints){
				

				for(int i = 0; i < pointCount - maxPoints; i++){
					pointCount --;
					Point p = points[points.Count - 1 -i];
					points.Remove(p);
					Services.main.editor.DeletePoint(p);
				}

				if(points.Count > 0){
					curPoint = points[points.Count -1];
					turtle.transform.position = curPoint.Pos;
					turtle.transform.up = curPoint.transform.up;
				}else{
					curPoint = null;
				}
			}

		}
	}
	public void ChangePointSettings(){
		foreach(Point p in Point.Points){
			p.continuity = continuity;
			p.tension = tension;

			if(p._connectedSplines.Count > 1){
				p.SetPointType(PointTypes.stop);
			}else if(ghostPoints){
				p.SetPointType(PointTypes.ghost);
			}else{
				p.SetPointType(PointTypes.normal);
			}
		}
	}

	public void NewPoint(){


		if (Random.Range (0f, 100f) < branchFactor) {
			if (maxTotalPoints < 100) {
				SpawnTurtle ();
			}
		}

		SplinePointPair spp;

		Point newPoint = null;

		// if (Raycast) {
		// 	newPoint = SplineUtil.RaycastDownToPoint (turtle.position, Mathf.Infinity, 100f);
		// 	if (newPoint == null) {
		// 		newPoint = CreatePoint();
		// 	}
		// } else {

		newPoint = CreatePoint ();
		

		if (createSplines) {
			spp = SplineUtil.ConnectPoints (curSpline, curPoint, newPoint);
			
			AddSpline(spp.s);

			curSpline = spp.s;
			curPoint = spp.p;
			curPoint.transform.parent = editor.pointsParent.transform;
			curSpline.transform.parent = editor.splinesParent;

		} else {
			newPoint.transform.parent = editor.pointsParent.transform;
		}

		if (ghostPoints)
		{
			newPoint.SetPointType(PointTypes.ghost);
		}

		
		
	}

	public void Rotate(){
		float rotation;
		//if (LockAngle) {
			if (alternateAngle) {
				if (turnleft) {
					rotation = -ang;
					turnleft = !turnleft;
				} else {
					rotation = ang;
					turnleft = !turnleft;
				}
			}else if (randomlyZag)
			{
				if (Random.Range(0, 100) < 50) {
					rotation = -ang;
					turnleft = !turnleft;
				} else {
					rotation = ang;
					turnleft = !turnleft;
				}
			}
			else
			{
				rotation = ang;
			}
		//} else {
			rotation = rotation + Random.Range (-angleRandom/2f, angleRandom/2f);
		//}

		ang *= angleChange;
		if (Mathf.Abs (angleRandom) > angleVariance) {
//			angleChange = -angleChange;
//			angleRandom = angleRandom % angleVariance;
		}
		if (Mathf.Abs (ang) > angle) {
//			ang = angle % angle;
//			angleChange = -angleChange;
		}

		turtle.Rotate (0, 0, rotation);
	}

	void Step(){
		
		maxTotalPoints++;

		Rotate ();

		turtle.RotateAround (pivot.position, Vector3.forward, PivotSpeed);
		
		float moveDistance = Random.Range (mDist, mxDist);
		mDist *= scaleChange;
		mxDist *= scaleChange;
		turtle.localPosition += turtle.up * moveDistance + offsetDirection;
		curPoint.continuity = continuity;
		curPoint.tension = tension;
		
	}
}
