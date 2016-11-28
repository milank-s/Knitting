using UnityEngine;
using System.Collections;

public class EdgeDecorator : MonoBehaviour {

	public float fidelity = 1;
	public float speed = 2;

	private LineRenderer l;
	private BezierSpline curve;
	// Use this for initialization
	void Awake () {
		l = GetComponent<LineRenderer> ();
		l.SetVertexCount (2);
		curve = GetComponent<BezierSpline> ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void Decorate(){
		StartCoroutine (DrawLine ());
//		StartCoroutine (DrawVelocities ());

	}

	IEnumerator DrawVelocities (){
		l.SetPosition (0, curve.GetDirection (0));
		float t = 1/fidelity;

		while (t <= 1){
			int index = (int)(t * fidelity);
			l.SetVertexCount (index + 1);
			l.SetPosition (index, curve.GetPoint(t) + curve.GetDirection(t));
			t += Time.deltaTime;
			yield return null;
		}
	}

	IEnumerator DrawLine (){
		
		l.SetPosition (0, curve.GetPoint (0));
		float t = 1/fidelity;

		while (t <= 1) {
			int index = (int)(t * fidelity);
			l.SetVertexCount (index + 1);
			l.SetPosition (index, curve.GetPoint (t));
			t += Time.deltaTime * speed;
			yield return null;
		}
	}
}
