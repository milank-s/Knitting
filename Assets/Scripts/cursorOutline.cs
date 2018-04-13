using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cursorOutline : MonoBehaviour {

	LineRenderer l;

	public void Start(){
		l = GetComponent<LineRenderer>();
	}
	// Update is called once per frame
	void Update () {
		Point p = SplineUtil.RaycastFromCamera(transform.position, 20f);
		if (p != null) {
			l.positionCount = 2;
			l.SetPosition (0, p.Pos);
			l.SetPosition (1, Services.Player.transform.position);
		} else {
			l.positionCount = 0;
		}
	}
}
