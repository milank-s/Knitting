using UnityEngine;
using System.Collections;

public class NodeAnim : MonoBehaviour {
	
	float a;
	Color c;
	SpriteRenderer s;
	// Use this for initialization
	void Start () {
		c = GetComponent<SpriteRenderer> ().color;
		s = GetComponent<SpriteRenderer> ();
	}
	
	// Update is called once per frame
	void Update () {
		//change node color
		c.a = Mathf.Abs(Mathf.Sin (Time.time));
		s.color = c;

		//change linerenderer color
	}
}
