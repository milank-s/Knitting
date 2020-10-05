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
		time += Time.deltaTime * 3;
		color.a = 2-time;
		GetComponent<SpriteRenderer> ().color = color;
		transform.localScale = Vector3.Lerp(Vector3.one * 0.1f, Vector3.one * 1.5f, Easing.BounceEaseOut(Mathf.Clamp01(time)));
		if (color.a < 0) {
			Destroy (gameObject);
		}
	}
}
