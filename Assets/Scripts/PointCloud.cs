using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointCloud : MonoBehaviour {

	public float PointContinuity;
	public float PointAmount;

	void UpdateContinuity(float t){
		foreach(Point p in GetComponentsInChildren<Point>()){
			p.continuity = PointContinuity;
		}
	}

	void UpdatePointCount(){
		PointAmount = GetComponentsInChildren<Point> ().Length;
	}
}
