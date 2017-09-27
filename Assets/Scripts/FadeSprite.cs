using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeSprite : MonoBehaviour {

	public float time = 1; 

	Color color;
	// Use this for initialization
	void Start () {
		color = GetComponent<SpriteRenderer> ().color;
	}
	
	// Update is called once per frame
	void Update () {
		color.a -= Time.deltaTime/time;
		GetComponent<SpriteRenderer> ().color = color;
		if (color.a < 0) {
			Destroy (gameObject);
		}
	}
}
