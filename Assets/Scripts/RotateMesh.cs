using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateMesh : MonoBehaviour {

	public float speed;

	// Update is called once per frame
	void Update () {
		transform.Rotate (0, 0, speed);
//		transform.localScale = Vector3.one * (Mathf.Sin (Time.time * speed) * 2 + 3);
	}
}
