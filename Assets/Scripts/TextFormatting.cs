using UnityEngine;
using System.Collections;

public class TextFormatting : MonoBehaviour {

	TextMesh Text;
	public float speed;
	public bool fade, fadeIn, delete, rotFixed;

	private float lerpVal;

	// Use this for initialization
	void Start () {
		Text = this.GetComponent<TextMesh>();
		if (fadeIn) {
			Color tempM = Text.color;
			tempM.a = 0;
			Text.color = tempM;
			lerpVal = 0;
		} else {
			lerpVal = 1;
		}

		Services.Prefabs.SetFont(Text, Random.Range(0, Services.Prefabs.fonts.Length));

	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (rotFixed) {
			transform.rotation = Quaternion.Euler (0, 0, 0);
		}

		if (fadeIn) {
			lerpVal += Time.deltaTime;
			FadeIn ();
		} else {
			lerpVal -= Time.deltaTime / speed;
			if (fade) Text.color = new Color (Text.color.r, Text.color.g, Text.color.b, lerpVal); 
		}
			
		if (lerpVal < 0 && delete) {
			Destroy (this.gameObject);
		}
	}

//	public void Decay(){
//		lerpVal -= Time.deltaTime / speed;
//		Text.color = new Color (Text.color.r, Text.color.g, Text.color.b, lerpVal); 
//	}

	public void SetDecay(int x){
		speed = x;
	}

	public void setText(string text){
		Text = this.GetComponent<TextMesh>();
		Text.color = new Color (Text.color.r, Text.color.g, Text.color.b, 1); 
		Text.text = text;
	}

	public void FadeIn(){
		if (Text.color.a < 1) {
			Text.color = new Color (Text.color.r, Text.color.g, Text.color.b, lerpVal);
		}
		if (Text.color.a >= 1) {
			fadeIn = false;
		}
	}
}
