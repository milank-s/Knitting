using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PointManager{

	public static List<Point> _pointsHit;
	public static List<Point> _connectedPoints;


//	public static void Init(){
//		_pointsHit = new List<Point> ();
//	}

	public static bool PointsHit(){
		if (_pointsHit.Count >= _connectedPoints.Count) {
			return true;
		} else {
			return false;
		}

	}

	public static void ResetPoints(){
		for(int i = _pointsHit.Count-1; i >=0; i--) {
			_pointsHit [i].hit = false;
			_pointsHit.Remove (_pointsHit [i]);
		}
	}
}
