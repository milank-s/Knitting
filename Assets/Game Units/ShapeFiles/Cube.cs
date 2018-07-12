using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour {

	List<Vector3> shape;

	// Use this for initialization
	void Start () {
		shape = new List<Vector3>(){new Vector3(0.497f, -0.636f, -0.313f), new Vector3(0.809f, 0.096f, 0.293f), new Vector3(0.809f, 0.096f, 0.293f), new Vector3(0f, -0.033f, 0.865f), new Vector3(0f, -0.033f, 0.865f), new Vector3(-0.312f, -0.765f, 0.259f), new Vector3(-0.312f, -0.765f, 0.259f), new Vector3(0.497f, -0.636f, -0.313f), new Vector3(0.809f, 0.096f, 0.293f), new Vector3(0.312f, 0.765f, -0.259f), new Vector3(0.312f, 0.765f, -0.259f), new Vector3(-0.497f, 0.636f, 0.313f), new Vector3(-0.497f, 0.636f, 0.313f), new Vector3(0f, -0.033f, 0.865f), new Vector3(0.312f, 0.765f, -0.259f), new Vector3(0f, 0.033f, -0.865f), new Vector3(0f, 0.033f, -0.865f), new Vector3(-0.809f, -0.096f, -0.293f), new Vector3(-0.809f, -0.096f, -0.293f), new Vector3(-0.497f, 0.636f, 0.313f), new Vector3(0f, 0.033f, -0.865f), new Vector3(0.497f, -0.636f, -0.313f), new Vector3(-0.312f, -0.765f, 0.259f), new Vector3(-0.809f, -0.096f, -0.293f)};
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
