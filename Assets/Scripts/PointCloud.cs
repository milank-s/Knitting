using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointCloud : MonoBehaviour {

	public float PointContinuity;
	public float PointAmount;
	public List<Point> _points;
	public SpriteRenderer image;
	public TextMesh title;
	public bool isOn;
	private float fade;

	public TextAsset text;
	private string[] words;
	private int wordIndex;

	public string GetWord (){
		wordIndex++;
		return words[(wordIndex) % (words.Length)]; 
	}

	public void Start(){
		wordIndex = 0;
		words = text.text.Split (new char[] { ' ' });
		UpdateContinuity (PointContinuity);
	}
		
	void UpdateContinuity(float t){
		foreach(Point p in GetComponentsInChildren<Point>()){
			p.continuity = PointContinuity;
		}
	}

	public void UpdateState(){
		isOn = false;

		foreach (Point p in _points) {
			if (p == Services.PlayerBehaviour.curPoint) {
				isOn = true;
			}
		}
	}

	public void Update(){
		UpdateState ();

		if (isOn) {
			fade = Mathf.Clamp01(fade + Time.deltaTime);
		} else {
			fade = Mathf.Clamp01(fade - Time.deltaTime);
		}


		image.color = new Color (1, 1, 1, fade);
		title.color = image.color;
	}

	void UpdatePointCount(){
		PointAmount = GetComponentsInChildren<Point> ().Length;
	}
}
