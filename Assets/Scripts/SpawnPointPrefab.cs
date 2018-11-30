using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SpawnPointPrefab : ScriptableObject {

	static public Point CreatePoint(Vector3 pos){

		// #if (UNITY_EDITOR)
		// GameObject p = PrefabUtility.InstantiatePrefab(Services.Prefabs.point as GameObject) as GameObject;
		// Point newPoint = p.GetComponent<Point>();
		//
		// newPoint.transform.position = pos;
		// newPoint.originalPos = pos;
		// newPoint.GetComponent<Collider> ().enabled = true;
		//
		// return newPoint;
		// #endif

		GameObject p2 =  Instantiate(Services.Prefabs.point) as GameObject;
		Point newPoint2 = p2.GetComponent<Point>();

		newPoint2.transform.position = pos;
		newPoint2.originalPos = pos;
		newPoint2.GetComponent<Collider> ().enabled = true;

		return newPoint2;

	}
}
