using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeInSpriteOnSpline : MonoBehaviour {

	SpriteRenderer sr;
	Spline s;
	// Update is called once per frame

	void Start(){
		s = GetComponentInParent<Spline> ();
		sr = GetComponent<SpriteRenderer> ();
		sr.sortingOrder = -10000;
	}

	void Update () {
		if (s.isPlayerOn) {
			sr.color = Color.Lerp (sr.color, Color.white, Time.deltaTime * 3);
		} else {
			sr.color = Color.Lerp (sr.color, new Color(0,0,0,0) , Time.deltaTime * 3);
		}
	}
}
