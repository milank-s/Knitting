using UnityEngine;
using System.Collections;

public class EdgeDecorator : MonoBehaviour {

	public float fidelity = 10;
	public float speed = 2;
	public Rope_Line rope;

	private LineRenderer l;
	private BezierSpline curve;
	// Use this for initialization
	void Awake () {
		rope = GetComponent<Rope_Line> ();
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
			t += Time.deltaTime * speed;
			int index = (int)(t * fidelity);
			l.SetVertexCount (index + 1);
			l.SetPosition (index, curve.GetPoint(t) + curve.GetDirection(t));
			yield return null;
		}
	}

	IEnumerator DrawLine (){
		
		l.SetPosition (0, curve.GetPoint (0));
		float t = 1/fidelity;

		while (t <= 1) {
			t += Time.deltaTime * speed;
			int index = (int)(t * fidelity);
			l.SetVertexCount (index + 1);
			l.SetPosition (index, curve.GetPoint (t));
			yield return null;
		}
	}

	public void DestroySpline (Node toDelete, Node toAnchor){
		Destroy (curve);
		Destroy (l);


		transform.position = toAnchor.transform.position;
		GameObject ropeEnd = new GameObject ();
		ropeEnd.transform.position = toDelete.transform.position;
		rope.target = ropeEnd.transform;
		rope.enabled = true;

	}
}
