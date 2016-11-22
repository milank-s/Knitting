using UnityEngine;
using System.Collections;

public class SplineDecorator : MonoBehaviour {
	
	public bool spawnNodes, drawLine;
	public float frequency;
	public bool lookForward;
	public Transform[] items;

	public BezierSpline spline;
	private LineRenderer l;
	private int points;
	private float stepSize;

	private void Awake () {

		//spline = gameObject.GetComponent<BezierSpline> ();;

		if (frequency <= 0 || items == null || items.Length == 0) {
			return;
		}

		l = GetComponent<LineRenderer> ();

//		UpdateSpline (spline);
	}

	void Update(){
	}

	public void UpdateSpline(BezierSpline b ){
		spline = b;
		points = (int) (frequency * spline.ControlPointCount);
		stepSize = points;
		l.SetVertexCount (points + 1);


		if (spline.Loop || stepSize == 1) {
			stepSize = 1f / stepSize;
		}
		else {
			stepSize = 1f / (stepSize - 1);
		}

		if (drawLine) {
			StartCoroutine(UpdateLine ());
		}

		if (spawnNodes) {
			StartCoroutine(UpdateNodes ());
		}
	}

	IEnumerator UpdateLine(){
		for (int i = 0; i <= points; i++) {
			Vector3 position = spline.GetPoint (i * stepSize);
			l.SetPosition (i, position);
			yield return null;
		}
	}

	IEnumerator UpdateNodes(){
		if (spawnNodes) {
			for (int i = 0; i <= points; i+=10) {
				Transform item = Instantiate (items [0]) as Transform;
				Vector3 position = spline.GetPoint (i * stepSize);
				item.transform.localPosition = position;
				if (lookForward) {
					item.transform.LookAt (position + spline.GetDirection (i * stepSize));
				}
				item.GetComponent<LineRenderer> ().SetPosition (0, position);
				item.GetComponent<LineRenderer> ().SetPosition (1, position + spline.GetDirection (i * stepSize));

				item.transform.parent = transform;

				yield return new WaitForSeconds (0.1f);
			}
		}
	}
}