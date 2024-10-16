﻿using System.Collections;
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

		
			p = GetComponentInParent<Point>();
			if (p != null)
			{
				p.OnEnter.AddListener(LightUp);
				hasPoint = true;
			}
	
	}

	public void LightUp(){
		alpha = 1;
	}

	// Update is called once per frame
	void Update () {

		if (MapEditor.editing)
		{
			t.color = new Color(1, 1, 1, 1);
		}
		else
		{
			if (hasPoint)
			{
				if (stayOn)
				{
					if (p.state == Point.PointState.on)
					{
						alpha = 1;	
					}
				}else{
					// alpha = Mathf.Clamp01(alpha - Time.deltaTime * 2);
					alpha = p.proximity;
				}

				t.color = new Color(1, 1, 1, alpha);
			}
			else if (!startOn)
			{
				alpha = Mathf.Clamp01(alpha - Time.deltaTime * 2);
				t.color = new Color(1, 1, 1, alpha);
			}
		}

		if (alpha <= 0 && destroy) {
			Destroy (gameObject);
		}
	}
}
