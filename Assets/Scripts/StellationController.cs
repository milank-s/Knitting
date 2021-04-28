
using System.Collections.Generic;
using System;
using System.Linq;
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

	public Vector3 lowerLeft, upperRight;
	public List<Point> _startPoints;
	public List<Spline> _splines;
	public List<Spline> _splinesToUnlock;
	
	public StellationController unlock;
    public bool lockX, lockY, lockZ;

	int lapCount;
	public int rootKey;
	public int laps = 1;
	public int speed = 1;
	public int startIndex;
	public int time = 1;
	public float startSpeed = 1;
	
	private float timer;
	public int curSplineIndex;

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
	public TextMesh titleTextMesh;
	public SpriteRenderer image;
	public bool fixedCam = false;
	public int desiredFOV = 30;
	public string text = "";
	public string title = "";

	[Space(10)]

	public Vector3 cameraPos = Vector3.zero;
	public bool setCameraPos = false;
	public bool isOn;
	public bool lockSplines;
	public bool isComplete;

	bool won = false;
	private string[] words;
	private int wordIndex;
	private float fade;
	public bool hasUnlock;
	
	public string GetWord (){
		if(words != null){
			string toReturn = "";
			toReturn = words[(wordIndex) % (words.Length)];
			return toReturn;
		}

		return "";
	}

	public string GetNextWord (){
		if(words != null){

		string toReturn = "";
		if(wordIndex > 0){
			if(wordIndex >= words.Length){
				return "";
			}

			if(wordIndex % words.Length == 0){
				toReturn = '\n' + words[(wordIndex) % (words.Length)];
			}else{
				toReturn = " " + words[(wordIndex) % (words.Length)];
			}
		}else{
			toReturn = words[(wordIndex) % (words.Length)];
		}
		
		wordIndex ++;

		return toReturn;
		}
	
		return "";
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
		_startPoints = new List<Point>();
	}
	
	public void Won()
	{
		//We are in a scene that supports multiple controllers
		if (StellationManager.instance != null)
		{
			//enable next controller. I dont think I'm using this anymore
			
			StellationManager.instance.CompleteStellation();
			
		}
		else
		{
			SceneController.instance.LoadNextStellation();
		}
					
		
		for (int i = 0; i < activateOnCompletion.Count; i++)
		{
			activateOnCompletion[i].DoBehaviour();
		}


		won = true;
		
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

	public void LeaveStellation()
	{
		if (isComplete)
		{
			EnableStellation(false);
		}
	}
	//this method fucking sucks
	public void EnableStellation(bool b)
	{
		foreach (Point p in _points)
		{
			if (b)
			{
				//only unlock points which wont be unlocked via splines
				if (p._connectedSplines.Count == 0)
				{
					p.SwitchState(Point.PointState.off);
				}
			}
			else
			{
				p.SwitchState(Point.PointState.locked);
			}
		}

		if (b)
		{
			
			if (_splines.Count > 0)
			{
				_splines[0].SwitchState(Spline.SplineState.on);
//				foreach (Point p in _splines[0].SplinePoints)
//				{
//					p.SwitchState(Point.PointState.off);
//				}
			}
			else
			{
				Services.fx.PlayAnimationAtPosition(FXManager.FXType.pulse, start.transform);
				start.SwitchState(Point.PointState.on);
			}
			
			
			//particle effect?

			
		}
	}

	public void GetComponents()
	{
		curSplineIndex = 0;
		
		_points.Clear();
		_splines.Clear();
		_splinesToUnlock.Clear();
		_startPoints.Clear();
		//stupid code for old maps that didnt have scoreCount idk. 
		if (unlockMethod == UnlockType.laps && laps == 0)
		{
			//laps = 1;
		}
		
		foreach(Point p in GetComponentsInChildren<Point>()){

	
			//expensive but easy
			if (!_points.Contains(p))
			{
				p.hasController = true;
				p.controller = this;
				_points.Add(p);
				p.SR.color = Color.white * 0.2f;
				p.Initialize();
			}

				if (p.pointType == PointTypes.start)
			{
				_startPoints.Add(p);
			}

		}

		if (_points.Count == 0) return;
		
	
		_startPoints.Sort((p1,p2)=>TryComparePoints(p1, p2));
			

		if (start == null)
		{
			if(_startPoints.Count > 0)
			{
				start = _startPoints[0];
				
			}else{
				start = _points[0];
			}
		}

		Services.StartPoint = start;

		GetBounds();
		
		_pointshit = new List<Point>();
		
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

		if(titleTextMesh != null){
			titleTextMesh.color = new Color(0,0,0,0);
		}
		
		wordIndex = 0;
		
		if(text != null){
			string[] wordArray = text.Split (new char[] { ' ' });
			if(wordArray != null){
				words = wordArray;
			}
		}

		
		Spline[] splines = GetComponentsInChildren<Spline>();
		Array.Sort(splines, delegate(Spline x, Spline y) { return x.order.CompareTo(y.order); });

		int index = 0;

		if (splines.Length == 0)
		{
			return;
		}
	
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

			if (lockSplines && s.order != 0)
			{
				s.SwitchState(Spline.SplineState.locked);
			}

			s.controller = this;
			index++;
		}
	}

	
	public int TryComparePoints(Point p1, Point p2){
		if(p1._connectedSplines.Count > 0 && p2._connectedSplines.Count > 0){
			return p1._connectedSplines[0].order.CompareTo(p2._connectedSplines[0].order);
		}

		return 0;
	}
	public void Initialize()
	{
		curSplineIndex = 0;
		isComplete = false;

		GetComponents();

		//why is this here
//		Services.main.state = Main.GameState.playing;
}

	public void LeftStartPoint(){
		if(startIndex%_startPoints.Count == 0){
			//start the timer bro;
			timer = 0;
			Services.main.levelText.text = "";
			Services.main.text.text = "";
		}
		startIndex ++;
	}
	public void Draw(){
		
				// Services.main.crawlerManager.AddCrawler(s);

		if(lockSplines){
			if(_splines.Count > 0){
				_splines[0].DrawSplineIn();
			}
		}else{
			foreach(Spline s in _splines){
				Services.main.crawlerManager.AddCrawler(s);
				StartCoroutine(s.DrawSplineIn());
			}
		}
	}
	public void Setup()
	{	
		rootKey = UnityEngine.Random.Range(48, 61);
		isOn = true;
		isComplete = false;
		curSplineIndex = 0;
		Services.main.activeStellation = this;

		CameraFollow.instance.fixedCamera = fixedCam;
		CameraFollow.instance.desiredFOV = desiredFOV;

		if (start != null)
		{
			CameraFollow.instance.WarpToPosition(start.transform.position);
		}

		foreach (Spline s in _splinesToUnlock)
		{
			s.SwitchState(Spline.SplineState.locked);
		}
		
	
		SetCameraInfo();
	

		
		Services.main.text.text = text;
		Services.main.levelText.text = title;


	}

	public bool CheckCompleteness()
	{

		int minLaps = 1000;
		bool isDone = true;

		if (curSplineIndex < (_splines.Count - _splinesToUnlock.Count) - 1)
		{
			return false;
		}
		
		foreach (Point p in _points)
		{
			//we arent checking against the points we need to unlock
			if (p.state == Point.PointState.locked)
			{
				continue;
			}
			
			if (p.timesHit < minLaps)
			{

				minLaps = p.timesHit;
				lapCount = minLaps;
			}
		}
		
		if(laps > 1){
			Services.fx.readout.text = lapCount.ToString("F0") + "/" + laps.ToString("F0");
		}

		return lapCount >= laps;
	}

	//call this for flying off and resetting, player right clicking, and time running out
	public void ResetLevel()
	{
		Services.main.InitializeLevel();
		curSplineIndex = 0;
		startIndex = 0;
		timer = 0;
		lapCount = 0;
		isComplete = false;
		won = false;
	}

	public Point GetStartPoint(){
		return _startPoints[startIndex % _startPoints.Count];
	}
	public void UnlockSpline(Spline spline)
	{
		curSplineIndex = spline.order + 1;
		
		foreach (Spline s in _splines)
		{
			if (s.order == curSplineIndex && !_splinesToUnlock.Contains(s) && s.state == Spline.SplineState.locked) 
			{
				s.SwitchState(Spline.SplineState.on);
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
				
				//Services.main.fx.readout.transform.position = Services.main.Player.transform.position;

				if (!won)
				{	
					if (unlockMethod == UnlockType.speed)
					{
						Services.fx.readout.text = Services.PlayerBehaviour.potentialSpeed.ToString("F1") + "/" + speed.ToString("F0");
					
					}else if (unlockMethod == UnlockType.time)
					{
						

						if(startIndex > 0){
							timer += Time.deltaTime;
							Services.fx.readout.text = Mathf.Clamp((time - timer), 0, 1000).ToString("F2");

							if (time - timer <= 0)
							{
								//ResetLevel();
								Services.fx.readout.text = "";
							}

						}
					
					}else if (unlockMethod == UnlockType.laps){
					
						if(laps > 1){
							Services.fx.readout.text = lapCount.ToString("F0") + "/" + laps.ToString("F0");
						}
					}

				}
				
			
				if(titleTextMesh != null){
					titleTextMesh.color = Color.Lerp(titleTextMesh.color, new Color (1, 1, 1, 1), Time.deltaTime);
				}
				fade = Mathf.Clamp(fade + Time.deltaTime/20, 0, 0.1f);
			} else {
				if(titleTextMesh != null){
					titleTextMesh.color = Color.Lerp(titleTextMesh.color, new Color (1, 1, 1, 0), Time.deltaTime);
				}
				fade = Mathf.Clamp01(fade - Time.deltaTime/10);
			}
			
			if(image != null && isComplete){
				image.color = new Color (1, 1, 1, fade);
			}

	}

	public void NextWord(){
		//Services.main.text.text += GetNextWord();
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
			case UnlockType.time:
				isComplete = time - timer > 0;
				break;
			}
			return isComplete;

		}

		return true;

	}

	public void Unlock()
	{
		foreach (Spline s in _splinesToUnlock)
		{
			s.SwitchState(Spline.SplineState.on);
		}
	}

	public float GetNormalizedHeight(Vector3 pos){
		return Mathf.Clamp01(pos.y - lowerLeft.y / (upperRight.y - lowerLeft.y));
	}

	public float GetNormalizedDepth(Vector3 pos){
		if(Mathf.Abs(upperRight.z - lowerLeft.z) < 0.25f){
			return 0.5f;
		}
		return Mathf.Clamp01(pos.z - lowerLeft.z / (upperRight.z - lowerLeft.z));
	}

	void GetBounds(){

		lowerLeft = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
			upperRight = new Vector3(-Mathf.Infinity, -Mathf.Infinity, -Mathf.Infinity);

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

	}
	public void SetCameraInfo()
	{
		
			center = Vector3.Lerp(lowerLeft, upperRight, 0.5f);

			float height = Mathf.Abs(upperRight.y - lowerLeft.y);
			float fov = CameraDolly.FOVForHeightAndDistance(height, -CameraFollow.instance.offset.z) + 10f;
		
			//CameraFollow.instance.desiredFOV = fov;
			//CameraFollow.instance.cam.fieldOfView = fov;
			CameraFollow.instance.lockX = lockX;
			CameraFollow.instance.lockY = lockY;
			CameraFollow.instance.lockZ = lockZ;

			Vector3 targetPos = Services.Player.transform.position;
			targetPos.z += CameraFollow.instance.offset.z;


			if(setCameraPos){
				CameraFollow.instance.WarpToPosition(cameraPos);
			}else{
				CameraFollow.instance.WarpToPosition(targetPos);
			}

			
		
			//I think we need to set far clipping plane and fog here

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
