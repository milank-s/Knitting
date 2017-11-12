using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineTurtle : MonoBehaviour {

	public static float maxTotalPoints = 100;

	public float minAngle = 10;
	public float maxAngle = 30;
	public float scaleFactor = 0;
	public float moveDistance = 1;
	public float branchFactor = 0;
	public int maxPoints = 50;
	public float continuity = 0;

	public Vector3 startDirection = Vector3.right;
	public Vector3 offsetDirection = Vector3.zero;


	Mesh mesh;
	Spline curSpline;
	Point curPoint;

	void Start () {


		Point firstPoint = Services.PlayerBehaviour.CreatePoint (transform.position);
		firstPoint.continuity = continuity;
		transform.Rotate (0, 0, Random.Range (minAngle, maxAngle));
		moveDistance += scaleFactor;
		transform.position += transform.up * moveDistance + offsetDirection;
		Point secondPoint = Services.PlayerBehaviour.CreatePoint(transform.position);
		secondPoint.continuity = continuity;

		curSpline = Services.PlayerBehaviour.CreateSpline (firstPoint, secondPoint);
		curPoint = secondPoint;

		for(int i = 2; i < maxPoints; i++) {
			moveDistance += scaleFactor;
			transform.Rotate (0, 0, Random.Range (minAngle, maxAngle));
			transform.position += transform.up * moveDistance + offsetDirection;
			Point newPoint = Services.PlayerBehaviour.CheckIfOverPoint (transform.position);
			SplinePointPair spp = Services.PlayerBehaviour.ConnectNewPoint(curSpline, curPoint, newPoint, transform.position);
			curSpline = spp.s;
			curPoint = spp.p;
			curPoint.continuity = continuity;
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
