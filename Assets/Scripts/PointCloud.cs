using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointCloud : MonoBehaviour {

	public enum UnlockType{laps, speed}

	public UnlockType unlockMethod;
	[HideInInspector]
	public List<Point> _pointshit;
	[HideInInspector]
	public List<Point> _points;
	public Spline unlock;
	public int lapsRequired = 1;
	public float speedRequired = 1;
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
	bool isComplete;
	private string[] words;
	private int wordIndex;
	private float fade;
	private bool hasUnlock;
	public string GetWord (){
		wordIndex++;
		return words[(wordIndex-1) % (words.Length)];
	}

	public void Start(){
		if(unlock){
			unlock.locked = true;
			hasUnlock = true;
		}

		if(image != null){
			image.color = new Color(0,0,0,0);
		}

		if(title != null){
			title.color = new Color(0,0,0,0);
		}

		_pointshit = new List<Point>();
		_points = new List<Point>();
		wordIndex = 0;
		if(text != null){
			words = text.text.Split (new char[] { ' ' });
		}

foreach(Spline s in GetComponents<Spline>()){
		foreach(Point p in s.SplinePoints){
			p.hasPointcloud = true;
			p.pointClouds.Add(this);
			_points.Add(p);
		}
	}
}

	public void CheckCompleteness()
	{

		bool isDone = true;
		foreach (Point p in _points)
		{
			if (p.timesHit < lapsRequired)
			{
				isDone = false;
				break;
			}
		}

		if (isDone)
		{
			isComplete = true;
			if (unlock != null)
			{
				unlock.locked = false;
			}
		}
	}

	public void TryToUnlock()
	{
		if (hasUnlock && !isComplete)
		{
			switch (unlockMethod)
			{
			case UnlockType.laps:
				CheckCompleteness();
				break;
			
			case UnlockType.speed:
				CheckSpeed();
				break;
			}
		}
	}
	public void CheckSpeed()
	{
		if (Services.PlayerBehaviour.curSpeed > speedRequired)
		{
			if (unlock != null)
			{
				unlock.locked = false;
			}

			isComplete = true;
		}
	}
	void Update(){
		if (isOn) {
			if(title != null){
				title.color = Color.Lerp(title.color, new Color (1, 1, 1, 1), Time.deltaTime);
			}
			fade = Mathf.Clamp(fade + Time.deltaTime/20, 0, 0.1f);
		} else {
			if(title != null){
				title.color = Color.Lerp(title.color, new Color (1, 1, 1, 0), Time.deltaTime);
			}
			fade = Mathf.Clamp01(fade - Time.deltaTime/10);
		}

		if(image != null && isComplete){
			image.color = new Color (1, 1, 1, fade);
		}
		
	}
}
