using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeInSprite : MonoBehaviour {

	bool fading = false;
	float alpha = 1;
	// Update is called once per frame
	void Update () {
		alpha = GetComponentInParent<Point> ().proximity;
		GetComponent<SpriteRenderer> ().color = new Color (1, 1, 1, alpha);
	}
}
