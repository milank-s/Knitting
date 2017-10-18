using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeText : MonoBehaviour {

	bool fading = false;
	float alpha = 1;
	// Update is called once per frame
	void Update () {
		alpha = GetComponentInParent<Point> ().proximity;
		GetComponent<TextMesh> ().color = new Color (1, 1, 1, alpha);
	}
}
