using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeInSpriteOnSpline : MonoBehaviour {

	bool fading = false;
	float alpha = 1;

	SpriteRenderer sr;
	Spline s;
	// Update is called once per frame

	void Start(){
		s = GetComponentInParent<Spline> ();
		sr = GetComponent<SpriteRenderer> ();
	}

	void Update () {
		if (s.isPlayerOn) {
			sr.color = Color.Lerp (sr.color, Color.white, Time.deltaTime * 2);
		} else {
			sr.color = Color.Lerp (sr.color, Color.black, Time.deltaTime * 2);
		}
	}
}
