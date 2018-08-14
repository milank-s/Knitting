using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Constants : MonoBehaviour {

	public PlayerBehaviour playerVals;

	LineRenderer l;

	public Text accuracyReadout;
	public Text accuracy;
	public Text cursorOnPoint;
	public Text negativeAccuracy;
	public Text speed;
	public Text horizontal;
	public Text vertical;
	public Text boost;
	public Text flow;
	public Text drag;
	public Text[] onPoint;
	public Text[] xButton;
	public Text canFly;
	public Text reset;

	Color gray = new Color(0.1f, 0.1f, 0.1f);

	void Start(){
		l = playerVals.cursor.GetComponent<LineRenderer>();
	}

	void Update () {

		//ACCURACY METER
		accuracyReadout.text = Mathf.Abs (playerVals.accuracy).ToString("F1");

		//FLOW METER
		if (playerVals.state != PlayerState.Animating) {
			speed.text = Mathf.Abs (playerVals.flow).ToString ("F2");
		} else {
			speed.text = (-Mathf.Abs (playerVals.flow)).ToString ("F2");
		}

		//HORIZONTAL AXIS
		if (Mathf.Abs(playerVals.cursorDir.x) > 0.5f) {
			horizontal.color = Color.white;
		}else {
			horizontal.color = gray;
		}

		//VERTICAL AXIS
		if (Mathf.Abs(playerVals.cursorDir.y) > 0.5f) {
			vertical.color = Color.white;
		} else {
			vertical.color = gray;
		}

		//PLAYER OVER POINT
		if(playerVals.state == PlayerState.Switching){
			for (int i = 0; i < onPoint.Length; i++) {
				onPoint[i].color = Color.white;
			}
		} else {
			for (int i = 0; i < onPoint.Length; i++) {
				onPoint [i].color = Color.Lerp (onPoint [i].color, gray, Time.deltaTime * 3);
			}
		}

		if (playerVals.flow >= 0) {
			canFly.color = Color.white;
		} else {
			canFly.color = gray;
		}

		if (Input.GetButton ("Button1")) {
			for (int i = 0; i < xButton.Length; i++) {
				xButton[i].color = Color.white;
			}
		} else {
			for (int i = 0; i < xButton.Length; i++) {
				xButton [i].color = Color.Lerp (xButton [i].color, gray, Time.deltaTime * 3);
			}
		}

		if (Mathf.Abs(playerVals.flow) >= 1) {
			canFly.color = Color.white;
		} else {
			canFly.color = gray;
		}

		if (playerVals.state == PlayerState.Animating) {
			reset.color = Color.white;
		} else {

			reset.color = gray;
		}

		if ((playerVals.accuracy < 0.5f && playerVals.accuracy > -0.5f)) {
			drag.color = Color.white;
			flow.color = gray;
			accuracy.color = gray;
			negativeAccuracy.color = Color.white;
		} else if (playerVals.state == PlayerState.Switching) {
			flow.color = gray;
			accuracy.color = gray;
			negativeAccuracy.color = gray;
			drag.color = Color.white;

		} else if (((playerVals.flow > 0 && playerVals.curSpeed > 0) || (playerVals.flow < 0 && playerVals.curSpeed < 0))) {
			accuracy.color = Color.white;
			flow.color = Color.white;
			drag.color = gray;
			negativeAccuracy.color = gray;
		}
		boost.color = Color.Lerp (gray, Color.white, playerVals.boost);

		Point p = SplineUtil.RaycastFromCamera(playerVals.transform.position, 20f);
		if (p != null && playerVals.state == PlayerState.Switching && !playerVals.curPoint.isConnectedTo(p)) {
			l.positionCount = 2;
			l.SetPosition (0, p.Pos);
			l.SetPosition (1, Services.Player.transform.position);
			cursorOnPoint.color = Color.white;
		} else {
			cursorOnPoint.color = gray;
			l.positionCount = 0;
		}

	}
}
