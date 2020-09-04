
using System.Collections.Generic;
using System;
using UnityEngine;

public class StellationController : MonoBehaviour {

	public enum UnlockType{laps, speed, time}
	public enum UnlockMechanism{unlockSpline, turnOnSpline, unlockPoints, switchPointTypes}
	
	public List<UnlockMechanism> unlockActions = new List<UnlockMechanism>() {UnlockMechanism.unlockSpline};
	
	List<ActivatedBehaviour>  activateOnCompletion = new List<ActivatedBehaviour>();
	
	public UnlockType unlockMethod = UnlockType.laps;
	[HideInInspector]
	public List<Point> _pointshit;
	[HideInInspector]
	public List<Point> _points;

	public List<Spline> _splines;
	public List<Spline> _splinesToUnlock;
	
	public StellationController unlock;

	public int laps;
	public int speed;
	public int time;
	public float startSpeed;
	
	private float timer;
	public int lapCount;
	

	public Point start;
	public Vector3 center;
	
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
	public bool fixedCam = false;
	public int desiredFOV = 30;
	public TextAsset text;
	[Space(10)]

	
	[HideInInspector]
	public bool isOn;
	public bool isComplete;
	private string[] words;
	private int wordIndex;
	private float fade;
	private bool hasUnlock;
	
	public string GetWord (){
		wordIndex++;
		return words[(wordIndex-1) % (words.Length)];
	}

	public void AdjustCamera()
	{
		CameraFollow.instance.desiredFOV = desiredFOV;
		
	}

	public void Awake()
	{
		_points = new List<Point>();
		_splines = new List<Spline>();
		_splinesToUnlock = new List<Spline>();
	}
	
	public void Won()
	{
		if (!isComplete)
		{
			return;
		}
		
		isOn = false;
						
		//We are in a scene that supports multiple controllers
		if (StellationManager.instance != null)
		{
			//enable next controller. I dont think I'm using this anymore
			if (hasUnlock)
			{
				StellationManager.instance.EnableStellation(unlock);
				
				Services.mainCam.fieldOfView = 80;
				CameraFollow.instance.desiredFOV = 80;
				CameraFollow.instance.fixedCamera = false;
			}
			else
			{
			//unload
				SceneController.instance.LoadNextStellation();
			}
		}
		else
		{
			SceneController.instance.LoadNextStellation();
		}
					
		
		for (int i = 0; i < activateOnCompletion.Count; i++)
		{
			activateOnCompletion[i].DoBehaviour();
		}

		isComplete = true;
		
		Lock(true);
		
			//show some type of image
			//lock instantly
			//turn off over time. 
		
	}

	public void SetActive(bool b)
	{
		if (b)
		{
			start.SwitchState(Point.PointState.on);
		}
		else
		{
			
			start.SwitchState(Point.PointState.locked);
		}
	}

	public void Lock(bool b)
	{
		foreach (Point p in _points)
		{
			if (b)
			{
				p.SwitchState(Point.PointState.locked);
			}
			else
			{
				p.SwitchState(Point.PointState.off);
			}
		}
	}
	
	public void Initialize()
	{
		isComplete = false;

		Services.PlayerBehaviour.flow = startSpeed;
		
		_points.Clear();
		_splines.Clear();
		_splinesToUnlock.Clear();
		
		lapCount = 0;
		
		//stupid code for old maps that didnt have scoreCount idk. 
		if (unlockMethod == UnlockType.laps && laps == 0)
		{
			//laps = 1;
		}
		
		foreach(Point p in GetComponentsInChildren<Point>()){

			if (p.pointType == PointTypes.start)
			{
				start = p;
			}

			//expensive but easy
			if (!_points.Contains(p))
			{
				p.hasController = true;
				p.controller = this;
				_points.Add(p);
				p.SR.color = Color.white * 0.2f;
				p.Initialize();
			}
		}

			Spline[] splines = GetComponentsInChildren<Spline>();
		     Array.Sort(splines, delegate(Spline x, Spline y) { return x.order.CompareTo(y.order); });

		     int index = 0;

		     if (splines.Length == 0) return;
		     
		     
		for (int i = 0; i < splines.Length; i++)
		{
			Spline s = splines[i];
			
			s.order = i;
			
			_splines.Add(s);
			s.SetSplineType(s.type);
			
			if (s.type == Spline.SplineType.locked)
			{
				_splinesToUnlock.Add(s);
			}

			if (s.order != 0)
			{
				s.SwitchState(Spline.SplineState.locked);
			}

			s.controller = this;
			index++;
		}
		

		if (_points.Count == 0)
		{
			return;
		}
		
		if(unlock){
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

		//ffs
		
		wordIndex = 0;
		if(text != null){
			words = text.text.Split (new char[] { ' ' });
		}

		CameraFollow.instance.fixedCamera = fixedCam;
		CameraFollow.instance.desiredFOV = desiredFOV;
		if (start != null)
		{
			CameraFollow.instance.WarpToPosition(start.transform.position);
		}

		if (fixedCam)
		{
			SetCameraBounds();
		}

		if (unlockMethod == UnlockType.speed)
		{
			Services.main.crawlerManager.AddCrawler(splines[0].SplinePoints, speed);
		}
		
		Services.main.state = Main.GameState.playing;
}

	public bool CheckCompleteness()
	{

		int minLaps = 1000;
		bool isDone = true;
		foreach (Point p in _points)
		{
			//we arent checking against the points we need to unlock
			if (p.state == Point.PointState.locked)
			{
				continue;
			}
			
			if (p.timesHit < laps)
			{
//				if (p.timesHit < minLaps)
//				{
//					minLaps = p.timesHit;
//				}
//				
//				if (minLaps > lapCount)
//				{
//					lapCount = minLaps;
//				}

				return false;
			}
		}
		
		return isDone;
	}

	public void Reset()
	{
		//Services.main.InitializeLevel();
		lapCount = 0;
		isComplete = false;
	}

	public void UnlockSpline(Spline spline)
	{
		
		int index = spline.order + 1;
		
		foreach (Spline s in _splines)
		{
			if (s.order == index && !_splinesToUnlock.Contains(s))
			{
				s.Unlock();
			}
		}
	}
	
	public void ReloadFromEditor()
	{
		StellationController c = Services.main.editor.Load(gameObject.name);
		StellationManager manager = GetComponentInParent<StellationManager>();
		if (manager.controllers.Contains(this))
		{
			manager.controllers[manager.controllers.IndexOf(this)] = c;
		}
		c.transform.parent = transform.parent;
		c.transform.position = transform.position;
		DestroyImmediate(gameObject);
	}

	public void Step()
	{
			if (isOn)
			{
				foreach (Point p in _points)
				{
					p.Step();
				}
				
				Services.main.fx.readout.transform.position = Services.main.Player.transform.position;

				if (!isComplete)
				{	
					if (unlockMethod == UnlockType.speed)
					{
						//Services.fx.readout.text = (Services.PlayerBehaviour.flow - speed).ToString("F2");
					
					}else if (unlockMethod == UnlockType.time)
					{
						//Services.fx.readout.text = (time - timer).ToString("F2");
						timer += Time.deltaTime;

						if (time - timer <= 0)
						{
							Reset();
						}
					
					}else if (unlockMethod == UnlockType.laps){
					
						//Services.fx.readout.text = scoreCount.ToString("F0") + "/" + score.ToString("F0");
						Services.fx.readout.text = "";
					}

				}
				
			
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
	public bool TryToUnlock()
	{
		
		if (!isComplete)
		{
			
			switch (unlockMethod)
			{
			case UnlockType.laps:
				isComplete = CheckCompleteness();
				break;
			
			case UnlockType.speed:
				isComplete = CheckSpeed();
				break;
			}

			
			
			//Unlock shit

			if(isComplete){
				Unlock();
			}

			return isComplete;

		}

		return true;

	}

	void Unlock()
	{
		foreach (Spline s in _splinesToUnlock)
		{
			s.Unlock();
		}
	}

	public void SetCameraBounds()
	{
		
			int index = 0;

			Vector3 lowerLeft = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
			Vector3 upperRight = new Vector3(-Mathf.Infinity, -Mathf.Infinity, -Mathf.Infinity);

			foreach (Point p in _points)
			{
				if (p.Pos.x > upperRight.x)
				{
					upperRight.x = p.Pos.x;
				}

				if (p.Pos.x < lowerLeft.x)
				{
					lowerLeft.x = p.Pos.x;
				}

				if (p.Pos.y > upperRight.y)
				{
					upperRight.y = p.Pos.y;
				}

				if (p.Pos.y < lowerLeft.y)
				{
					lowerLeft.y = p.Pos.y;
				}

				if (p.Pos.z > upperRight.z)
				{
					upperRight.z = p.Pos.z;
				}

				if (p.Pos.z < lowerLeft.z)
				{
					lowerLeft.z = p.Pos.z;
				}
			}

			center = Vector3.Lerp(lowerLeft, upperRight, 0.5f);

			float height = Mathf.Abs(upperRight.y - lowerLeft.y);
			float fov = CameraDolly.FOVForHeightAndDistance(height, -CameraFollow.instance.offset.z) + 10f;
		
			//CameraFollow.instance.desiredFOV = fov;
			//CameraFollow.instance.cam.fieldOfView = fov;
			CameraFollow.instance.WarpToPosition(center);
			//get center position and fov
	}
	
	public bool CheckSpeed()
	{
		if (Services.PlayerBehaviour.flow >= speed)
		{
			return true;
		}
		
			return false;
		
	}
}
