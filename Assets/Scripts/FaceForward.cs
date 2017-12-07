using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceForward : MonoBehaviour {

	SpringJoint s;

	void Start () {
		s = GetComponent<SpringJoint> ();	
		transform.up = s.connectedBody.transform.position - transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		transform.up = s.connectedBody.transform.position - transform.position;
	}
}
