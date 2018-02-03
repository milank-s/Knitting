using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeTextOnPoint: MonoBehaviour {

	bool fading = false;
	float alpha = 0;
	TextMesh t;

	void Start(){
		t = GetComponent<TextMesh> ();
	}
	// Update is called once per frame
	void Update () {
		if (GetComponentInParent<Point> ().proximity >= 1) {
			alpha = 1;
		}
		alpha = Mathf.Clamp01 (alpha - Time.deltaTime);
		GetComponent<TextMesh> ().color = new Color (1, 1, 1, alpha);
		if (alpha == 0) {
			Destroy (gameObject);
		}
	}
}
