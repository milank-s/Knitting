using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SpawnPointPrefab : ScriptableObject {

	static public Point CreatePoint(Vector3 pos){

		GameObject p2 =  Instantiate(Resources.Load<GameObject>("Prefabs/Point")) as GameObject;
		Point newPoint2 = p2.GetComponent<Point>();

		newPoint2.transform.position = pos;
		newPoint2.GetComponent<Collider> ().enabled = true;

		return newPoint2;

	}
}
