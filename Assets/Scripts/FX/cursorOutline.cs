using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cursorOutline : MonoBehaviour {

	LineRenderer l;
	PlayerBehaviour player;
	public void Start(){
		player = GetComponentInParent<PlayerBehaviour> ();
		l = GetComponent<LineRenderer>();
	}
	// Update is called once per frame
	void Update () {
		Point p = SplineUtil.RaycastFromCamera(transform.position, 20f);
		if (p != null && (player.state == PlayerState.Switching || player.state == PlayerState.Flying)) {
			l.positionCount = 2;
			l.SetPosition (0, p.Pos);
			l.SetPosition (1, Services.Player.transform.position);
		} else {
			l.positionCount = 0;
		}
	}
}
