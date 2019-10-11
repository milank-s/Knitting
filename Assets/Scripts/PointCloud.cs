using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointCloud : MonoBehaviour {

	public enum UnlockType{laps, speed}
	public enum UnlockMechanism{unlockSpline, turnOnSpline, unlockPoints, switchPointTypes}
	
	public List<UnlockMechanism> unlockActions = new List<UnlockMechanism>() {UnlockMechanism.unlockSpline};
	
	List<ActivatedBehaviour>  activateOnCompletion = new List<ActivatedBehaviour>();
	
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
	public bool fixedCam = true;
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

		foreach (ActivatedBehaviour a in GetComponentsInChildren<ActivatedBehaviour>())
		{
			activateOnCompletion.Add(a);

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

foreach(Spline s in GetComponentsInChildren<Spline>()){
		foreach(Point p in s.SplinePoints){
			p.hasPointcloud = true;
			p.pointClouds.Add(this);
			_points.Add(p);
		}
	}
}
	
	public bool CheckCompleteness()
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

		return isDone;
	}

	public void TryToUnlock()
	{
		
		if (!isComplete)
		{
			bool complete = false;
			switch (unlockMethod)
			{
			case UnlockType.laps:
				complete = CheckCompleteness();
				break;
			
			case UnlockType.speed:
				complete = CheckSpeed();
				break;
			}
			
			if (complete)
			{
				if(hasUnlock)
				unlock.locked = false;

				for (int i = 0; i < activateOnCompletion.Count; i++)
				{
					activateOnCompletion[i].DoBehaviour();
				}

				isComplete = true;
			}

			
		}

		
	}
	public bool CheckSpeed()
	{
		if (Services.PlayerBehaviour.curSpeed > speedRequired)
		{
			return true;
		}
		else
		{
			return false;
		}
	}
	void Update(){
		if (isOn)
		{
			CameraFollow.fixedCamera = fixedCam;
			CameraFollow.desiredFOV = desiredFOV;
			
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
