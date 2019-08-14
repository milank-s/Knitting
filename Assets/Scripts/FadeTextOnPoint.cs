using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeTextOnPoint: MonoBehaviour {

	bool fading = false;
	public float alpha = 1;
	TextMesh t;
	public Point p;
	public bool startOn;
	void Start(){
		t = GetComponent<TextMesh> ();
		if (startOn)
		{
			alpha = 1;
		}
	}
	// Update is called once per frame
	void Update () {
		if(p != null)
		{
			float proximity = p.proximity;
			t.color = Color.Lerp(t.color, p.color * 5, Time.deltaTime);
		}else if(!startOn){
			alpha = Mathf.Clamp01(alpha - Time.deltaTime/3);
			t.color = new Color(1,1,1, alpha);
		}
		
		// if (alpha <= 0) {
		// 	Destroy (gameObject);
		// }
	}
}
