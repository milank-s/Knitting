using UnityEngine;
using System.Collections;

public class DrawLineToTarget : MonoBehaviour {
	public Transform target;
	private LineRenderer l;
	// Use this for initialization
	void Start () {
		l = GetComponent<LineRenderer> ();
		l.positionCount = 2;
	}
	
	// Update is called once per frame
	void Update () {
		l.SetPosition (0, transform.position);
		l.SetPosition (1, target.position);
	}
}
