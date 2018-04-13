using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vectrosity;

public class SimpleRay : MonoBehaviour {

	VectorLine v;
	// Use this for initialization
	void Start () {
		v = VectorLine.SetLine (Color.green,  new Vector3(-1, 0, -1), new Vector3(1, 0.5f, 1), new Vector3(-1, -2, -2));
		v.MakeSpline (v.points3.ToArray());
	}
	
	// Update is called once per frame
	void Update () {
		v.Draw3DAuto ();
	}
}
