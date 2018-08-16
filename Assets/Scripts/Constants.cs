using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Constants : MonoBehaviour {

	public PlayerBehaviour playerVals;

	LineRenderer l;

	public TextMesh accuracyReadout;
	public TextMesh accuracyChar;
	public TextMesh flowChar;
	public TextMesh flowReadout;

	public SpriteRenderer switching;
	public SpriteRenderer traversing;
	public SpriteRenderer flying;
	public SpriteRenderer cursorOnPoint;
	public SpriteRenderer playerAxis;
	public SpriteRenderer buttonPress;
	public SpriteRenderer canFly;
	public SpriteRenderer reset;
	public SpriteRenderer overPoint;

	Color gray = new Color(0.1f, 0.1f, 0.1f);

	void Start(){
		l = playerVals.cursor.GetComponent<LineRenderer>();
	}

	void Update () {

		//ACCURACY METER
		accuracyReadout.text = Mathf.Abs (playerVals.accuracy).ToString("F1");

		//FLOW METER
		if (playerVals.state != PlayerState.Animating) {
			flowReadout.text = Mathf.Abs (playerVals.flow).ToString ("F2");
		} else {
			flowReadout.text = (-Mathf.Abs (playerVals.flow)).ToString ("F2");
		}

		//AXIS
		playerAxis.transform.eulerAngles = Vector3.Lerp(new Vector3(0, 0, 0), new Vector3(0, 0, 180), (playerVals.accuracy + 1f)/2f);

		//PLAYER OVER POINT
		if(playerVals.state == PlayerState.Switching){
			if(Mathf.Abs(playerVals.flow) > 1){
				canFly.color = Color.white;
			}else{
				canFly.color = gray;
			}
			switching.color = Color.white;
		} else {
			switching.color = gray;
		}

		if(playerVals.state == PlayerState.Traversing){
			traversing.color = Color.white;
			overPoint.enabled = true;
		} else {
			traversing.color = gray;
			overPoint.enabled = false;
		}

		if (Input.GetButtonDown ("Button1")) {
			buttonPress.color = Color.white;
		} else {
				buttonPress.color = Color.Lerp (buttonPress.color, gray, Time.deltaTime * 3);
		}

		if (playerVals.state == PlayerState.Animating) {
			// reset.color = Color.white;
		} else {
			// reset.color = gray;
		}

		if ((playerVals.accuracy < 0.5f && playerVals.accuracy > -0.5f)) {
			accuracyChar.text = "≠";
			flowChar.text = "✴-";
		} else if (playerVals.state == PlayerState.Switching) {
			flowChar.text = "✴-";

		} else if (((playerVals.flow > 0 && playerVals.curSpeed > 0) || (playerVals.flow < 0 && playerVals.curSpeed < 0))) {
			accuracyChar.text = "≈";
			flowChar.text = "✴+";
		}

		Point p = SplineUtil.RaycastFromCamera(playerVals.cursor.transform.position, 20f);
		if (p != null && (playerVals.state == PlayerState.Switching || playerVals.state == PlayerState.Flying)) {
			if(!playerVals.curPoint.isConnectedTo(p)){
			l.positionCount = 2;
			l.SetPosition (0, p.Pos);
			l.SetPosition (1, Services.Player.transform.position);
			cursorOnPoint.enabled = true;
			traversing.color = gray;
		 }else{
			 overPoint.enabled = true;
			 cursorOnPoint.enabled = false;
 			 l.positionCount = 0;
			traversing.color = Color.white;
		 }
		} else {
			cursorOnPoint.enabled = false;
			l.positionCount = 0;
		}

	}
}
