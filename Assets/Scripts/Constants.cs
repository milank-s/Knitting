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

	public Transform cursor;
	public SpriteRenderer switching;
	public SpriteRenderer traversing;
	public SpriteRenderer flying;
	public LineRenderer cursorOnPoint;
	public LineRenderer playerAxis;
	public SpriteRenderer buttonPress;
	public SpriteRenderer canFly;
	public Renderer reset;

	Color gray = new Color(0.12f, 0.12f, 0.12f);
	Color white = new Color(1,1,1);

	private List<SpriteRenderer> UISymbols;


	void Start(){
		l = playerVals.cursor.GetComponent<LineRenderer>();
		UISymbols = new List<SpriteRenderer>();
		UISymbols.Add(switching);
		// UISymbols.Add(traversing);
		UISymbols.Add(flying);
		UISymbols.Add(buttonPress);
		UISymbols.Add(canFly);
		// UISymbols.Add(playerAxis);
	}

	void Reveal(){

	}

	void LateUpdate () {



		//FLOW METER
		if (playerVals.state != PlayerState.Animating) {
			flowReadout.text = Mathf.Abs (playerVals.flow).ToString ("F2");
		} else {
			flowChar.text = "✴--";
			accuracyChar.gameObject.SetActive(false);
			flowReadout.text = (-Mathf.Abs (playerVals.flow)).ToString ("F2");
		}

		//AXIS
		// playerAxis.transform.eulerAngles = Vector3.Lerp(new Vector3(0, 0, 90), new Vector3(0, 0, 270), (playerVals.accuracy + 1f)/2f);
		//ACCURACY METER
		if(playerVals.state == PlayerState.Animating){
			reset.enabled = true;
			accuracyChar.text = "";
			accuracyReadout.text = "";

			foreach(SpriteRenderer s in UISymbols){
				s.color = Color.Lerp(s.color, gray, Time.deltaTime * 5);
			}
		}else{
			// playerAxis.SetPosition(0, playerVals.transform.position);
			// playerAxis.SetPosition(1, playerVals.cursorPos);
			reset.enabled = false;

			accuracyReadout.text = Mathf.Abs (playerVals.accuracy).ToString("F1");

			if ((playerVals.accuracy < 0.5f && playerVals.accuracy > -0.5f)) {
				accuracyChar.text = "≠";
				flowChar.text = "✴-";
			} else if (playerVals.state == PlayerState.Switching) {
				flowChar.text = "✴-";
			} else if ((playerVals.accuracy > 0 && playerVals.flow > 0) || (playerVals.accuracy < 0 && playerVals.flow < 0)) {
				accuracyChar.text = "≈";
				flowChar.text = "✴+";
			}else if((playerVals.accuracy < 0 && playerVals.flow > 0) || (playerVals.accuracy > 0 && playerVals.flow < 0)){
				accuracyChar.text = "≠";
				flowChar.text = "✴-";
			}

		//PLAYER OVER POINT
		if(Mathf.Abs(playerVals.flow) < 1){
			flowReadout.text = "";
			flowChar.text = "";
		}

		if(playerVals.state == PlayerState.Switching){
			accuracyReadout.text = "-.-";

			if(Mathf.Abs(playerVals.flow) > 1){
				canFly.color = white;
			}else{
				canFly.color = Color.Lerp (canFly.color, gray, Time.deltaTime * 3);
			}
			switching.color = white;
		} else {
			switching.color = gray;
		}

		if(playerVals.state == PlayerState.Traversing){
			// playerAxis.color = Color.Lerp (playerAxis.color, gray, Time.deltaTime * 3);
			if(playerVals.goingForward){
				traversing.transform.localScale = new Vector3(1, 0.2f + playerVals.progress/1.5f, 1);
			}else{
				traversing.transform.localScale = new Vector3(1, 1.2f - playerVals.progress/1.5f, 1);
			}
			accuracyChar.gameObject.SetActive(true);
			traversing.color = white;
			// playerAxis.color = white;
		} else {
			traversing.color = Color.Lerp (traversing.color, gray, Time.deltaTime * 10);
		}

		if (Input.GetButton ("Button1")) {
			buttonPress.enabled = true;
			buttonPress.color = Color.white;
		} else {
			buttonPress.color = Color.Lerp (buttonPress.color, gray, Time.deltaTime * 3);
		}

		if(playerVals.state == PlayerState.Flying){
			// playerAxis.color = gray;
			flying.color = white;
		}else{
			flying.color = Color.Lerp (flying.color, gray, Time.deltaTime * 3);
		}

		Point p = SplineUtil.RaycastFromCamera(playerVals.cursor.transform.position, 20f);
		if (p != null && (playerVals.state == PlayerState.Switching || playerVals.state == PlayerState.Flying) && p != playerVals.curPoint) {

			if(!playerVals.curPoint.isConnectedTo(p)){
			l.positionCount = 2;
			cursorOnPoint.positionCount = 2;
			l.SetPosition (0, p.Pos);
			l.SetPosition (1, Services.Player.transform.position);
			cursorOnPoint.SetPosition (0, p.Pos);
			cursorOnPoint.SetPosition (1, playerVals.cursorPos);
			buttonPress.transform.position = p.Pos;
		 }else{
			 buttonPress.transform.position = cursor.position;
			 cursorOnPoint.positionCount = 0;
			 l.positionCount = 0;
		 }
		} else {
			buttonPress.transform.position = cursor.position;
			cursorOnPoint.positionCount = 0;

			l.positionCount = 0;
		}

	}
	}
}
