using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineTurtle : MonoBehaviour {

	public static float maxTotalPoints = 0;
	public static float maxCrawlers = 0;

	public string parentName;

	public bool createSplines;
	public bool Randomize;

	public int initialAmount;
	public float initialAngleMax;
	public float initialAngleMin;

	public float angleChange = 0;
	public float minAngle = 10;
	public float maxAngle = 30;
	public float scaleChange = 0;
	public float maxDist = 2;
	public float minDist = 1;
	public float branchFactor = 0;
	public int maxPoints = 50;
	public float continuity = 0;
	public bool Raycast = true;
	public bool LockAngle = false;
	public bool alternateAngle = false;

	private float mAngle;
	private float mxAngle;
	private float mxDist;
	private float mDist;

	public bool PivotAroundCenter;
	public float  PivotSpeed;
	public bool closed;
	public bool childrenInherit = false;
	private bool turnleft = true;

	GameObject parent;

	public Vector3 startDirection;
	public Vector3 offsetDirection = Vector3.zero;


	Mesh mesh;
	Spline curSpline;
	Point curPoint;

	void Start () {

		if (Randomize) {

			initialAngleMax = Random.Range(-90, 45);
			initialAngleMin = Random.Range(initialAngleMax, 90);

			angleChange = Random.Range (0,3);
			minAngle = Random.Range (-90, 10);
			maxAngle = Random.Range (minAngle, 90);
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

		StartCoroutine(InitializeSpline ());

	}

	IEnumerator Draw(){
		for(int i = 2; i < maxPoints; i++) {

			Step ();
			NewPoint ();
			yield return new WaitForSeconds (0.05f);

			if (PivotAroundCenter) {
				transform.RotateAround (Vector3.zero, Vector3.forward, PivotSpeed);
			}
		}

		if (createSplines && closed) {

			SplinePointPair spp;

			spp = SplineUtil.ConnectPoints (curSpline, curSpline.SplinePoints[curSpline.SplinePoints.Count-1], curSpline.SplinePoints[0]);
			curSpline = spp.s;
			curPoint = spp.p;
			curPoint.transform.parent = parent.transform;
			curSpline.transform.parent = parent.transform;
		}

		if (maxCrawlers < 100) {
			for (int i = 0; i < initialAmount; i++) {
				SpawnTurtle ().transform.Rotate (0, 0, transform.eulerAngles.z + Random.Range (initialAngleMin, initialAngleMax) * i);	

			}
			yield return new WaitForSeconds (0.1f);
		}
	}

	IEnumerator InitializeSpline(){


		parent = new GameObject ();
		parent.name = parentName;
			
		mxAngle = maxAngle;
		mAngle = minAngle;
		mxDist = maxDist;
		mDist = minDist;


		if (SplineUtil.RaycastDownToPoint (transform.position, Mathf.Infinity, 1000f) != null) {
			curPoint = SplineUtil.RaycastDownToPoint (transform.position, Mathf.Infinity, 1000f);
			if (curPoint.HasSplines ()) {
				curSpline = curPoint._connectedSplines [0];
			} else {
				Step ();

				Point secondPoint = SplineUtil.CreatePoint (transform.position);

				if (createSplines) {
					curSpline = SplineUtil.CreateSpline (curPoint, secondPoint);
				}
				curPoint = secondPoint;
				curPoint.transform.parent = parent.transform;
			}
			Step ();
			NewPoint ();
		} else {

			curPoint = SplineUtil.CreatePoint (transform.position);
			curPoint.transform.parent = parent.transform;

			yield return new WaitForSeconds (0.1f);

			Step ();

			Point secondPoint = SplineUtil.CreatePoint (transform.position);

			if (createSplines) {
				curSpline = SplineUtil.CreateSpline (curPoint, secondPoint);
			}
			curPoint = secondPoint;
			curPoint.transform.parent = parent.transform;

			yield return new WaitForSeconds (0.1f);
		}
			

		StartCoroutine (Draw ());
	}

	public GameObject SpawnTurtle(){
		GameObject newTurtle = Instantiate (gameObject, transform.position, Quaternion.Euler(transform.eulerAngles));

		SplineTurtle newTurtleScript = newTurtle.GetComponent<SplineTurtle> ();

		newTurtle.transform.Rotate (0,0,Random.Range (initialAngleMin, initialAngleMax));

		if (!childrenInherit) {
			newTurtleScript.maxAngle = maxAngle;
			newTurtleScript.minAngle = minAngle;
			newTurtleScript.maxDist = maxDist;
			newTurtleScript.minDist = minDist;
		}
		maxCrawlers++;
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
				newPoint = SplineUtil.CreatePoint (transform.position);
			}
		} else {
			newPoint = SplineUtil.CreatePoint (transform.position);
		}

		if (createSplines) {
			spp = SplineUtil.ConnectPoints (curSpline, curPoint, newPoint);
			curSpline = spp.s;
			curPoint = spp.p;
			curPoint.transform.parent = parent.transform;
			curSpline.transform.parent = parent.transform;
		} else {
			newPoint.transform.parent = parent.transform;
		}
	}

	public void Rotate(){
		float rotation;
		if (LockAngle) {
			if (alternateAngle) {
				if (turnleft) {
					rotation = mAngle;
					turnleft = !turnleft;
				} else {
					rotation = mxAngle;
					turnleft = !turnleft;
				}
			} else {
				if (Random.Range (0f, 100f) >= 50) {
					rotation = mAngle;
				} else {
					rotation = mxAngle;
				}
			}
		} else {
			rotation = Random.Range (mAngle, mxAngle);
		}

		mAngle += angleChange;
		mxAngle += angleChange;
		if (Mathf.Abs (mAngle) > minAngle) {
//			angleChange = -angleChange;
//			mAngle = mAngle % minAngle;
		}
		if (Mathf.Abs (mxAngle) > maxAngle) {
//			mxAngle = maxAngle % maxAngle;
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
	}
}
