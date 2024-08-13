using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeText: MonoBehaviour {

	bool fading = false;
	public float alpha;
	public float speed = 5f;
	TextMesh t;
	private bool hasPoint;
	public bool startOn;
	public bool stayOn;
	public bool destroy;
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
	
	}
	// Update is called once per frame
	void Update () {

		if (MapEditor.editing)
		{
			t.color = new Color(1, 1, 1, 1);
		}
		else
		{
			alpha = Mathf.Clamp01(alpha - Time.deltaTime  * speed);
			t.color = new Color(1, 1, 1, alpha);
			
		}

		if (alpha <= 0 && destroy) {
			Destroy (gameObject);
		}
	}
}
