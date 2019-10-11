using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeTextOnPoint: MonoBehaviour {

	bool fading = false;
	public float alpha;
	TextMesh t;
	public Point p;
	private bool hasPoint;
	public bool startOn;
	public bool stayOn;
	void Start(){
		t = GetComponent<TextMesh> ();
		
		if (startOn)
		{
			alpha = 1;
			t.color = Color.white;
		}
		else
		{
			alpha = 0;
			t.color = Color.black;
		}

		
			p = GetComponentInParent<Point>();
			if (p != null)
			{
				hasPoint = true;
			}
	
	}
	// Update is called once per frame
	void Update () {

		if(hasPoint)
		{
			if (stayOn && p.hit)
			{
			
				t.color = new Color(1,1,1, alpha);
				
			}
			else
			{
				alpha = Mathf.Clamp01(alpha - Time.deltaTime);
				t.color = new Color(1, 1, 1, alpha);
			}
		}else if(!startOn){
			alpha = Mathf.Clamp01(alpha - Time.deltaTime/3);
			t.color = new Color(1,1,1, alpha);
		}
		
		// if (alpha <= 0) {
		// 	Destroy (gameObject);
		// }
	}
}
