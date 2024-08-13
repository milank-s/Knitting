using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAttributes : MonoBehaviour {

	public Camera cam;
	private Camera me;

	void Start(){
		me = GetComponent<Camera> ();
	}

	void Update () {
		me.fieldOfView = cam.fieldOfView;
	}
}
