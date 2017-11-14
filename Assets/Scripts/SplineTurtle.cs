using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineTurtle : MonoBehaviour {

	public static float maxTotalPoints = 0;

	public float angleChange = 0;
	public float minAngle = 10;
	public float maxAngle = 30;
	public float scaleChange = 0;
	public float maxDistance = 2;
	public float minDistance = 1;
	public float branchFactor = 0;
	public int maxPoints = 50;
	public float continuity = 0;
	public bool Raycast = true;
	public bool LockAngle = false;

	public Vector3 startDirection = Vector3.right;
	public Vector3 offsetDirection = Vector3.zero;


	Mesh mesh;
	Spline curSpline;
	Point curPoint;

	void Start () {

		curPoint = Services.PlayerBehaviour.CreatePoint (transform.position);
		Services.PlayerBehaviour.curPoint = curPoint;

		Step ();

		Point secondPoint = Services.PlayerBehaviour.CreatePoint(transform.position);
		curSpline = Services.PlayerBehaviour.CreateSpline (curPoint, secondPoint);
		curPoint = secondPoint;

		for(int i = 2; i < maxPoints; i++) {
			
			Step ();

			if (Random.Range (0f, 100f) < branchFactor) {
//				branching code. make new SplineTurtle
				if (maxTotalPoints < 100) {
					Instantiate (Services.Prefabs.SplineTurtle, transform.position, Quaternion.LookRotation (transform.forward));
				}
			}

			SplinePointPair spp;

			if (!Raycast) {
				spp = Services.PlayerBehaviour.ConnectNewPoint (curSpline, curPoint, null, transform.position);
			} else {
				Point newPoint = Services.PlayerBehaviour.CheckIfOverPoint (transform.position);
				spp = Services.PlayerBehaviour.ConnectNewPoint (curSpline, curPoint, newPoint, transform.position);
			}
			curSpline = spp.s;
			curPoint = spp.p;

		}
	}
	
	void Step(){
		maxTotalPoints++;

		float rotation;
		if (LockAngle) {
			if (Random.Range (0f, 100f) >= 50) {
				rotation = minAngle;
			} else {
				rotation = maxAngle;
			}
		} else {
			rotation = Random.Range (minAngle, maxAngle);
		}

		transform.Rotate (0, 0, rotation);
		float moveDistance = Random.Range (minDistance, maxDistance);
		minDistance *= scaleChange;
		maxDistance *= scaleChange;
		transform.position += transform.up * moveDistance + offsetDirection;
		curPoint.continuity = continuity;
	}
}
