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

	public static void AddPointHit (Point p){
		
		_pointsHit.Add (p);

		if (p.Pos.y > CameraDolly.topBound) {
			CameraDolly.topBound = p.Pos.y;
		}
		if (p.Pos.y < CameraDolly.bottomBound) {
			CameraDolly.bottomBound = p.Pos.y;
		}
		if (p.Pos.x > CameraDolly.rightBound) {
			CameraDolly.rightBound = p.Pos.x;
		}
		if (p.Pos.x < CameraDolly.leftBound) {
			CameraDolly.leftBound = p.Pos.x;
		}
	}
		
	public static void ResetPoints(){
//		CameraDolly.leftBound = Services.PlayerBehaviour.curPoint.Pos.x;
//		CameraDolly.topBound = Services.PlayerBehaviour.curPoint.Pos.y;
//		CameraDolly.rightBound = CameraDolly.leftBound;
//		CameraDolly.bottomBound = CameraDolly.topBound;

		for(int i = _pointsHit.Count-1; i >= 0; i--) {
			_pointsHit [i].hit = false;
			_pointsHit.Remove (_pointsHit [i]);
		}
	}
}
