using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateStellation : MonoBehaviour {


	[SerializeField] StellationController controller;


	public float offset;
	public float speed = 1;
	public bool offsetPerPoint = false;
	
	[Space(15)]
	[Header("Curve Control")]
	[Space(10)]
	public AnimationCurve tensionVal;
	public float tMultiplier = 1;
	[Space(10)]
	public AnimationCurve biasVal;
	public float bMultiplier = 1;
	[Space(10)]
	public AnimationCurve continuityVal;
	public float cMultiplier = 1;
	[Space(10)]

	[Header("Animation")]
	public float contraction;
	public float rotation;

	void Update () {
		int i = 0;
		foreach(Point p in controller._points){
			float time = (speed * Time.time + (i * offset))  % 1;
			p.tension = tensionVal.Evaluate(time) * tMultiplier;
			p.bias = biasVal.Evaluate(time) * bMultiplier;
			p.continuity = continuityVal.Evaluate(time) * cMultiplier;
			if (offsetPerPoint)
			{
				if ((transform.position - new Vector3(p.anchorPos.x, p.anchorPos.y, transform.position.z))
				    .magnitude > 0.01f)
				{
					p.transform.position -= (transform.position - p.anchorPos).normalized * Time.deltaTime *
					                        Mathf.Sin(Time.time * speed + i * offset) * contraction;
				}
			}
			else
			{
				if ((transform.position - new Vector3(p.anchorPos.x, p.anchorPos.y, transform.position.z))
				    .magnitude > 0.01f)
				{
					p.transform.position -= (transform.position - p.anchorPos).normalized * Time.deltaTime *
					                        (Mathf.Sin(Time.time * speed + offset)) * contraction;
				}
			}

			//p.anchorPos = p.transform.position;
			p.isKinematic = true;
			p.transform.RotateAround(transform.position, transform.forward, rotation * Time.deltaTime);
			i++;
		}
	}
}
