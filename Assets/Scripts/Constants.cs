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
	public Renderer reset;
	public SpriteRenderer overPoint;

	Color gray = new Color(0.12f, 0.12f, 0.12f);
	Color white = new Color(1,1,1);

	private List<SpriteRenderer> UISymbols;


	void Start(){
		l = playerVals.cursor.GetComponent<LineRenderer>();
		UISymbols = new List<SpriteRenderer>();
		UISymbols.Add(switching);
		UISymbols.Add(traversing);
		UISymbols.Add(flying);
		UISymbols.Add(cursorOnPoint);
		UISymbols.Add(buttonPress);
		UISymbols.Add(canFly);
		UISymbols.Add(overPoint);
	}

	void Reveal(){

	}

	void Update () {

		accuracyReadout.text = Mathf.Abs (playerVals.accuracy).ToString("F1");

		//FLOW METER
		if (playerVals.state != PlayerState.Animating) {
			flowReadout.text = Mathf.Abs (playerVals.flow).ToString ("F2");
		} else {
			flowReadout.text = (-Mathf.Abs (playerVals.flow)).ToString ("F2");
		}

		//AXIS
		playerAxis.transform.eulerAngles = Vector3.Lerp(new Vector3(0, 0, 90), new Vector3(0, 0, 270), (playerVals.accuracy + 1f)/2f);
		//ACCURACY METER
		if(playerVals.state == PlayerState.Animating){
			reset.enabled = true;

			foreach(SpriteRenderer s in UISymbols){
				s.color = Color.Lerp(s.color, gray, Time.deltaTime * 5);
			}
		}else{

			reset.enabled = false;
		//PLAYER OVER POINT
		if(playerVals.state == PlayerState.Switching){
			playerAxis.color = Color.Lerp (playerAxis.color, gray, Time.deltaTime * 3);
			if(Mathf.Abs(playerVals.flow) > 1){
				canFly.color = white;
			}else{
				canFly.color = Color.Lerp (canFly.color, gray, Time.deltaTime * 3);
			}
			switching.color = white;
		} else {
			playerAxis.color = white;
			switching.color = Color.Lerp (switching.color, gray, Time.deltaTime * 3);
		}

		if(playerVals.state == PlayerState.Traversing){
			traversing.color = white;
		} else {
			traversing.color = Color.Lerp (traversing.color, gray, Time.deltaTime * 10);
		}

		if (Input.GetButtonDown ("Button1")) {
			buttonPress.enabled = true;
			buttonPress.color = Color.white;
		} else {
				buttonPress.color = Color.Lerp (buttonPress.color, gray, Time.deltaTime * 3);
		}

		if(playerVals.state == PlayerState.Flying){
			playerAxis.enabled = false;
			canFly.color = white;
			flying.color = Color.Lerp (flying.color, gray, Time.deltaTime * 3);
		}else{
			flying.color = white;
		}

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

		Point p = SplineUtil.RaycastFromCamera(playerVals.cursor.transform.position, 20f);
		if (p != null && (playerVals.state == PlayerState.Switching || playerVals.state == PlayerState.Flying) && p != playerVals.curPoint) {
			overPoint.color = white;

			if(!playerVals.curPoint.isConnectedTo(p)){
			l.positionCount = 2;
			l.SetPosition (0, p.Pos);
			l.SetPosition (1, Services.Player.transform.position);
			cursorOnPoint.color = white;
		 }else{
			 cursorOnPoint.color = Color.Lerp (cursorOnPoint.color, gray, Time.deltaTime * 3);
			 l.positionCount = 0;
		 }
		} else {
			overPoint.color = Color.Lerp (overPoint.color, gray, Time.deltaTime * 3);
			cursorOnPoint.color = Color.Lerp (cursorOnPoint.color, gray, Time.deltaTime * 3);
			l.positionCount = 0;
		}
	}
	}
}
