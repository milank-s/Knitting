using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class SplineTurtle : MonoBehaviour {

	public MapEditor editor;

	[Header("UI")] 
	private ReadSliderValue numPointsUI;
	private ReadSliderValue minDistUI;
	private ReadSliderValue maxDistUI;
	private ReadSliderValue distScaleUI;
	private ReadSliderValue angleeUI;
	private ReadSliderValue angleDeltaUI;
	private ReadSliderValue angleScaleUI;
	
	private ReadSliderValue continuityUI;
	private ReadSliderValue tensionUI;
	
	private ReadSliderValue pivotAngleUI;
	private ReadSliderValue pivotDistanceUI;
	private ReadSliderValue stepSpeedUI;

	private InputField xOffsetUI;
	private InputField yOffsetUI;
	private InputField zOffsetUI;

	
	public static float maxTotalPoints = 1;
	public static float maxCrawlers = 1;

	public string name;

	public GameObject parent;
	public GameObject pointsParent;
	
	public bool createSplines;
	public bool Randomize;

	public int initialAmount;
	public float initialAngleMax;
	public float initialAngleMin;

	public Transform pivot;
	
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
	public bool LockAngle = false;
	public bool alternateAngle = false;

	private float angleRandom;
	private float ang;
	private float mxDist;
	private float mDist;

	public bool PivotAroundCenter;
	public float  PivotSpeed;
	public bool closed;
	public bool childrenInherit = false;
	private bool turnleft = true;

	private bool running;
	private bool redraw;
	
	public Vector3 startDirection;
	public Vector3 offsetDirection = Vector3.zero;


	Mesh mesh;
	Spline curSpline;
	Point curPoint;


	public void Reset()
	{
		Transform[] ts = parent.GetComponentsInChildren<Transform>();
		for(int i = 0; i < ts.Length; i++){
			if (ts[i].GetComponent<Spline>() != null || ts[i].GetComponent<Point>() != null){
				DestroyImmediate(ts[i].gameObject);
			}
		}

		editor.controller._splines.Clear();
		//parent.name = "Untitled";

	}
	public void Generate(){
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

			LockAngle = Random.Range (0f, 100f) > 50 ? true : false;
			alternateAngle = Random.Range (0f, 100f) > 50 ? true : false;



			PivotAroundCenter = Random.Range (0f, 100f) > 50 ? true : false;
			PivotSpeed = Random.Range (0f, 2f);

		}

	InitializeSpline ();

	}

	public void Update()
	{
		UpdateValues();
		
	
	}

	public void UpdateTurtle()
	{
		if (!running)
		{
			running = true;
			StartCoroutine(Draw());
		}
	}

	void UpdateValues()
	{
		maxPoints = (int)numPointsUI.val;
		minDist = minDistUI.val;
		maxDist = maxDistUI.val;
		scaleChange = distScaleUI.val;
		
		angle = angleeUI.val;
		angleChange = angleDeltaUI.val;
		angleChange = angleScaleUI.val;

		continuity = continuityUI.val;
		tension = tensionUI.val;

		stepSpeed = stepSpeedUI.val;
		pivot.position = parent.transform.position + Vector3.up * pivotDistanceUI.val;
		PivotSpeed = pivotAngleUI.val;
		
		float.TryParse(xOffsetUI.text, out offsetDirection.x);
		float.TryParse(yOffsetUI.text, out offsetDirection.y);
		float.TryParse(zOffsetUI.text, out offsetDirection.z);
	}
	
	IEnumerator Draw(){
		for(int i = 2; i < maxPoints; i++) {
			Step ();
			NewPoint ();

			if (PivotAroundCenter) {
				parent.transform.RotateAround (pivot.position, Vector3.forward, PivotSpeed);
			}

			yield return new WaitForSeconds(stepSpeed);
		}

		if (createSplines && closed) {

			SplinePointPair spp;

			spp = SplineUtil.ConnectPoints (curSpline, curSpline.SplinePoints[curSpline.SplinePoints.Count-1], curSpline.SplinePoints[0]);
			curSpline = spp.s;
			curPoint = spp.p;
			curPoint.transform.parent = pointsParent.transform;
			curSpline.transform.parent = parent.transform;
			editor.AddSpline(curSpline);
		}

		if (maxCrawlers < 100) {
			for (int i = 0; i < initialAmount; i++) {
				SpawnTurtle ().transform.Rotate (0, 0, transform.eulerAngles.z + Random.Range (initialAngleMin, initialAngleMax) * i);

			}
		}

		running = false;
		transform.rotation = Quaternion.identity;
	}

	void InitializeSpline(){
		
		//parent.name = name;
		if (parent.GetComponent<StellationController>() == null)
		{
			parent.AddComponent<StellationController>();	
		}

		StellationController s = parent.GetComponent<StellationController>();

		//parent.transform.position = transform.position;

		ang = angle;
		angleRandom = angleVariance;
		mxDist = maxDist;
		mDist = minDist;


		if (Raycast && SplineUtil.RaycastDownToPoint (transform.position, Mathf.Infinity, 1000f) != null) {
			curPoint = SplineUtil.RaycastDownToPoint (transform.position, Mathf.Infinity, 1000f);
			if (curPoint.HasSplines ()) {
				curSpline = curPoint._connectedSplines [0];
			} else {
				Step ();

				Point secondPoint = SpawnPointPrefab.CreatePoint (transform.position);

				if (createSplines) {
					curSpline = SplineUtil.CreateSpline (curPoint, secondPoint);
					editor.AddSpline(curSpline);
					
				}
				curPoint = secondPoint;
				curPoint.transform.parent = pointsParent.transform;
			}
			Step ();
			NewPoint ();
		} else {

			curPoint = SpawnPointPrefab.CreatePoint (transform.position);
			curPoint.transform.parent = pointsParent.transform;

			Step ();

			Point secondPoint = SpawnPointPrefab.CreatePoint (transform.position);

			if (createSplines) {
				curSpline = SplineUtil.CreateSpline (curPoint, secondPoint);
				editor.AddSpline(curSpline);
			}
			curPoint = secondPoint;
			curPoint.transform.parent = pointsParent.transform;
		}


		StartCoroutine(Draw());
	}

	public GameObject SpawnTurtle(){
		GameObject newTurtle = Instantiate (gameObject, transform.position, Quaternion.Euler(transform.eulerAngles));

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
			newPoint = SplineUtil.RaycastDownToPoint (transform.position, Mathf.Infinity, 1000f);
			if (newPoint == null) {
				newPoint = SpawnPointPrefab.CreatePoint (transform.position);
			}
		} else {
			newPoint = SpawnPointPrefab.CreatePoint (transform.position);
		}

		if (createSplines) {
			spp = SplineUtil.ConnectPoints (curSpline, curPoint, newPoint);
			curSpline = spp.s;
			curPoint = spp.p;
			curPoint.transform.parent = pointsParent.transform;
			curSpline.transform.parent = parent.transform;
		} else {
			newPoint.transform.parent = pointsParent.transform;
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
			} else {
				if (Random.Range (0f, 100f) >= 50) {
					rotation = angleRandom;
				} else {
					rotation = ang;
				}
			}
		//} else {
			rotation = Random.Range (ang -angleRandom/2f, ang + angleRandom/2f);
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

		transform.Rotate (0, 0, rotation);
	}

	void Step(){
		maxTotalPoints++;

		Rotate ();

		float moveDistance = Random.Range (mDist, mxDist);
		mDist *= scaleChange;
		mxDist *= scaleChange;
		transform.localPosition += transform.up * moveDistance + offsetDirection;
		curPoint.continuity = continuity;
		curPoint.tension = tension;
	}
}
