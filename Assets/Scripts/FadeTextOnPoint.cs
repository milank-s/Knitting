using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeTextOnPoint: MonoBehaviour {

	bool fading = false;
	public float alpha = 1;
	TextMesh t;
	public Point p;

	void Start(){
		t = GetComponent<TextMesh> ();
		alpha = 0;
	}
	// Update is called once per frame
	void Update () {
		if(p != null){
			t.color = new Color(p.color.r,p.color.r, p.color.r, p.color.a);
		}else{
			t.color = new Color(1,1,1, alpha);
		}
		alpha = Mathf.Clamp01(alpha - Time.deltaTime/3);
		// if (alpha <= 0) {
		// 	Destroy (gameObject);
		// }
	}
}
