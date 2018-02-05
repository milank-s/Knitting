using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeSprite : MonoBehaviour {

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
		transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, Easing.BounceEaseOut(Mathf.Clamp01(time)));
		if (color.a < 0) {
			Destroy (gameObject);
		}
	}
}
