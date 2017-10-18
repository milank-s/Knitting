using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointManager : MonoBehaviour {

	public List<Point> _points;

	public void AddPoint(Point p){
		if(!_points.Contains(p)){
			_points.Add (p);
		}
	}

	public int PointsHit(){
		int num = 0;

		foreach (Point p in _points) {
			if (!p.IsOffCooldown ()) {
				num++;
			}
		}

		return num;
	}
}
