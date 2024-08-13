using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeTextOnSpline: MonoBehaviour {

	bool fading = false;
	float alpha = 1;
	Spline s;
	TextMesh t;

	void Start(){
		s = GetComponentInParent<Spline> ();
		t = GetComponent<TextMesh> ();
	}
	// Update is called once per frame
	void Update () {
//		alpha = GetComponentInParent<Point> ().proximity;
//		GetComponent<TextMesh> ().color = new Color (1, 1, 1, alpha);

		if (s.isPlayerOn) {
			t.color = Color.Lerp (t.color, Color.white, Time.deltaTime * 5);
		} else {
			t.color = Color.Lerp (t.color, new Color(0,0,0,0) , Time.deltaTime * 5);
		}
	}
}
