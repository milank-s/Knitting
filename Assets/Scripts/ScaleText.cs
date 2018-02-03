using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleText : MonoBehaviour {

	TextMesh t;
	void Start () {
		t = GetComponent<TextMesh> ();
	}
	
	// Update is called once per frame
	void Update () {
//		transform.localScale = (Vector3.one/500) * Camera.main.fieldOfView;
	}
}
