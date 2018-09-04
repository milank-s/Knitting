using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeTextOnPoint: MonoBehaviour {

	bool fading = false;
	float alpha = 1;
	TextMesh t;
	public Point p;

	void Start(){
		t = GetComponent<TextMesh> ();
	}
	// Update is called once per frame
	void Update () {
		alpha = p.c;
		Color c = GetComponent<TextMesh> ().color;
		GetComponent<TextMesh> ().color = new Color (c.r, c.g, c.b, alpha);
		// if (alpha <= 0) {
		// 	Destroy (gameObject);
		// }
	}
}
