using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine.UI;

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
	[SerializeField] private ReadToggleValue closeToggle;
	[SerializeField] private ReadToggleValue connectUI;
	[SerializeField] private ReadToggleValue ghostToggle;
	
	public static float maxTotalPoints = 1000;
	public static float maxCrawlers = 1;

	public string name;

	
	public bool createSplines;
	public bool Randomize;

	public int initialAmount;
	public float initialAngleMax;
	public float initialAngleMin;

	public Transform pivot;
	public Transform turtle;
	
	public float stepSpeed;
	
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
	
	
	public Vector3 offsetDirection = Vector3.zero;

	private float timeSinceRedraw;
	
	
	private Coroutine drawing;
	
	
	Mesh mesh;
	Spline curSpline;
	public Point curPoint;


	public void Reset()
	{
		if (drawing != null)
		{
			StopCoroutine(drawing);
		}
	
		
		turtle.position = Vector3.zero;
		turtle.rotation = Quaternion.identity;
		pivot.position = turtle.position + Vector3.up * pivotDistanceUI.val;

		editor.Reset();

		//parent.name = "Untitled";

	}

	IEnumerator WaitOneFrameThenGenerate()
	{
		yield return null;
		InitializeSpline();
	}
	public void Generate()
	{

		Reset();
		StartCoroutine(WaitOneFrameThenGenerate());
		
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

	
	}

	public void UpdateTurtle()
	{
		UpdateValues();

		if (redraw)
		{
			timeSinceRedraw += Time.deltaTime;
			
			Generate();
			redraw = false;

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
	
	IEnumerator Draw(){
		
		
		for(int i = 2; i < maxPoints; i++) {
			Step ();
			NewPoint ();

			


				if (stepSpeed > 0)
				{
					yield return new WaitForSeconds(stepSpeed);
				}
		}

		if (createSplines && closed) {

			SplinePointPair spp;

			spp = SplineUtil.ConnectPoints (curSpline, curSpline.SplinePoints[curSpline.SplinePoints.Count-1], curSpline.SplinePoints[0]);
			curSpline = spp.s;
			curPoint = spp.p;
			curPoint.transform.parent = editor.pointsParent.transform;
			curSpline.transform.parent = editor.splinesParent;
			editor.AddSpline(curSpline);
		}

		if (maxCrawlers < 100) {
			for (int i = 0; i < initialAmount; i++) {
				SpawnTurtle ().transform.Rotate (0, 0, turtle.eulerAngles.z + Random.Range (initialAngleMin, initialAngleMax) * i);

			}
		}

		
		running = false;
		turtle.rotation = Quaternion.identity;

		foreach (Spline s in Spline.Splines)
		{
			editor.AddSpline(s);
		}

		yield return null;
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

			curPoint = SpawnPointPrefab.CreatePoint (turtle.position);
			curPoint.transform.parent = editor.pointsParent.transform;

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
		drawing = StartCoroutine(Draw());
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

	public void NewPoint(){

		if (Random.Range (0f, 100f) < branchFactor) {
			if (maxTotalPoints < 100) {
				SpawnTurtle ();
			}
		}

		SplinePointPair spp;

		Point newPoint = null;

		if (Raycast) {
			newPoint = SplineUtil.RaycastDownToPoint (turtle.position, Mathf.Infinity, 100f);
			if (newPoint == null) {
				newPoint = SpawnPointPrefab.CreatePoint (turtle.position);
			}
		} else {
			newPoint = SpawnPointPrefab.CreatePoint (turtle.position);
		}

		if (createSplines) {
			spp = SplineUtil.ConnectPoints (curSpline, curPoint, newPoint);
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
