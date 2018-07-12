using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeTextOnPoint: MonoBehaviour {

	bool fading = false;
	float alpha = 1;
	TextMesh t;

	void Start(){
		t = GetComponent<TextMesh> ();
	}
	// Update is called once per frame
	void Update () {
		alpha -= Time.deltaTime;
		GetComponent<TextMesh> ().color = new Color (1, 1, 1, alpha);
		if (alpha <= 0) {
			Destroy (gameObject);
		}
	}
}
