using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeTextOnPoint: MonoBehaviour {

	bool fading = false;
	public float alpha = 1;
	TextMesh t;
	public Point p;

	void Start(){
		t = GetComponent<TextMesh> ();
	}
	// Update is called once per frame
	void Update () {
		t.color = new Color(1,1,1, alpha);
		alpha = Mathf.Clamp01(alpha - Time.deltaTime/3);
		// if (alpha <= 0) {
		// 	Destroy (gameObject);
		// }
	}
}
