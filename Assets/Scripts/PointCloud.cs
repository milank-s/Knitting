using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointCloud : MonoBehaviour {

	public List<Point> _points;
	[Space(10)]
	[Header("Point Physics")]
	public bool isKinematic;
	public float damping = 600f;
	public float stiffness = 100f;
	public float mass = 50f;
	[Space(10)]

	[Header("Visuals")]
	public TextMesh title;
	public SpriteRenderer image;
	public float desiredFOV = 30;
	public TextAsset text;
	[Space(10)]

	[HideInInspector]
	public bool isOn;
	private string[] words;
	private int wordIndex;
	private float fade;

	public string GetWord (){
		wordIndex++;
		return words[(wordIndex-1) % (words.Length)];
	}

	public void Awake(){
		wordIndex = 0;
		if(text != null){
			words = text.text.Split (new char[] { ' ' });
		}

		foreach(Point p in GetComponentsInChildren<Point>()){
			p.hasPointcloud = true;
			p.pointCloud = this;
			p.isKinematic = isKinematic;
		}
	}

	public void UpdateVisuals(){

		if (isOn) {
			fade = Mathf.Clamp01(fade + Time.deltaTime);
		} else {
			fade = Mathf.Clamp01(fade - Time.deltaTime);
		}
		image.color = new Color (1, 1, 1, fade);
		title.color = image.color;
	}
}
