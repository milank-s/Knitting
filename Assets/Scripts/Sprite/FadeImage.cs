using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeImage : MonoBehaviour {

	public float time = 0f;

	Color color;
	// Use this for initialization
	void Start () {
		color = GetComponent<SpriteRenderer> ().color;
	}

	// Update is called once per frame
	void Update () {
		time += Time.deltaTime;
		color.a = 2-time;
		GetComponent<SpriteRenderer> ().color = color;
		if (color.a < 0) {
			Destroy (gameObject);
		}
	}
}
