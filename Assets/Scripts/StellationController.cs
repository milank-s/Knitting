
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public class StellationController : MonoBehaviour {

	public enum UnlockType{none, laps, speed, time}
	
	public delegate void StellationEvent();

	[Header("Progression")]
	
	public UnlockType unlockMethod = UnlockType.laps;
	public StellationController unlock;
	public bool isPlayerOn;
	public bool lockSplines;

	List<CrawlerManager> crawlers;
	List<Collectible> collectibles;

	[HideInInspector]
	public bool isComplete;
	[HideInInspector]
	public bool won = false;

	
	[Header("Events")]

	public StellationEvent OnCompleteLap;
	public StellationEvent OnCompleteStellation;
	public StellationEvent OnHitStart;
	public StellationEvent OnLeaveStart;
	public StellationEvent OnNextStart;

	List<ActivatedBehaviour>  activateOnCompletion = new List<ActivatedBehaviour>();

	[Header("Components")]
	public Point start;
	public List<Spline> _splines;

	[HideInInspector]
	public List<Point> _pointshit;

	[HideInInspector]
	public List<Point> _points;

	[HideInInspector]
	public Vector3 lowerLeft, upperRight;
	
	[HideInInspector]
	public List<Point> _startPoints;

	[HideInInspector]
	public List<Spline> _escapeSplines;
	
	[HideInInspector]
	public int startIndex;

	[HideInInspector]
	public int curSplineIndex;

	private float timer;
	int lapCount;
	
	[Space(10)]
	[Header("Win Variables")]
	public int laps = 1;
	public float time = 1;
	public float speed = 1;

	[Space(10)]
	[Header("Tuning")]
	public float startSpeed = 1;
	public float acceleration = 0.2f;
	public float maxSpeed = 3;
	//if a player loops through a stellation with multiple starts they will be placed on a new start each time

	Vector3 center;
	
	[Space(10)]
	[Header("Physics")]
	public bool isKinematic;
	public float damping = 600f;
	public float stiffness = 100f;
	public float mass = 50f;

	[Space(10)]

	[Header("Camera")]
	
	public bool setCameraPos = false;
	public int desiredFOV = 60;
    public bool lockX = true;
	public bool lockY = true;
	public bool lockZ = false;
	public Vector3 cameraPos = Vector3.zero;

	[Space(10)]

	[Header("Visuals")]
	public string text = "";
	public string title = "";

	[HideInInspector]
	public bool hasUnlock;
	float count = 0;
    float average = 30;
    
	[HideInInspector]
	public Vector3 pos;
    float speedAverage;
	
	private string[] words;
	private int wordIndex;
	private float fade;
	
	[HideInInspector]
	public int rootKey;

	bool spawnedCrawler;

	public string GetWord (){
		if(words != null){
			string toReturn = "";
			toReturn = words[(wordIndex) % (words.Length)];
			return toReturn;
		}

		return "";
	}

	void OnDrawGizmos(){

		//foreach(StellationController c in controllers){
			if(_splines.Count == 0) return;

			foreach(Spline s in _splines){
				
				if(s.gameObject.activeSelf){
					
					s.DrawGizmos();

					for (int i = 0; i < s.SplinePoints.Count; i++){
						switch(s.SplinePoints[i].pointType){
							case PointTypes.normal:
								Gizmos.color = Color.white;
							break;

							case PointTypes.start:
								Gizmos.color = Color.green;
							break;

							case PointTypes.stop:
								Gizmos.color = Color.yellow;
							break;

							case PointTypes.end:
								Gizmos.color = Color.red;
							break;

							case PointTypes.fly:
								Gizmos.color = Color.blue;
							break;

						}

						if(s.SplinePoints[i].pointType != PointTypes.ghost){
							Gizmos.DrawWireSphere(s.SplinePoints[i].transform.position, 0.033f);
						}

						Gizmos.color = Color.white;

					}
				}
			}
		//}
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
	
	public void Unlock(){
		
		Show(true);

		foreach(Spline s in start._connectedSplines){
			s.StartDrawRoutine(start);
		}
	}

	public void HitPoint(Point p){
		_pointshit.Add(p);
	}

	public void Won()
	{
		won = true;
		
		//We are in a scene that supports multiple controllers
		if (StellationManager.instance != null)
		{
			//enable next controller. I dont think I'm using this anymore
			
			if(OnCompleteStellation != null){
				OnCompleteStellation.Invoke();
			}

			if(hasUnlock){
				unlock.Unlock();
			}else{
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
		
			//show some type of image
			//lock instantly
			//turn off over time. 
		
	}

	public void Initialize()
	{
		
		collectibles = GetComponentsInChildren<Collectible>().ToList();
		crawlers = GetComponentsInChildren<CrawlerManager>().ToList();

		_points = new List<Point>();
		_splines = new List<Spline>();
		_escapeSplines = new List<Spline>();
		_startPoints = new List<Point>();

		foreach(CrawlerManager c in crawlers){
			c.Initialize();
		}

		//stupid code for old maps that didnt have scoreCount idk. 
		if (unlockMethod == UnlockType.laps && laps == 0)
		{
			
		}
		
		foreach(Point p in GetComponentsInChildren<Point>()){
			//expensive but easy
			AddPoint(p);
		}

		if (_points.Count == 0) return;
		
		if(_startPoints.Count > 0){
			_startPoints.Sort((p1,p2)=>TryComparePoints(p1, p2));
		}
			

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
		
		if(unlock){
			hasUnlock = true;
		}

		foreach (ActivatedBehaviour a in GetComponentsInChildren<ActivatedBehaviour>())
		{
			activateOnCompletion.Add(a);

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
			
			s.controller = this;
			index++;
		}

		
		//stopgap to test chasing crawler;
		
		if(!spawnedCrawler && unlockMethod == UnlockType.speed){
			spawnedCrawler = true;
			CrawlerManager newCrawler = gameObject.AddComponent<CrawlerManager>();
			newCrawler.speed = speed;
			newCrawler.crawlerCount = (int)Mathf.Floor(speed);
			newCrawler.spawnFrequency = 0;
			// OnHitStart += newCrawler.Reset;
			newCrawler.spline = splines[0];
			crawlers.Add(newCrawler);
			newCrawler.Initialize();
		}


		Setup();
	}

	public void AddSpline(Spline s){
		if(!_splines.Contains(s)){
			_splines.Add(s);
			s.controller = this;
			s.transform.parent = transform;
			s.Initialize();
		}
	}

	public void AddPoint(Point p){
		if (!_points.Contains(p))
			{
				p.hasController = true;
				p.controller = this;
				_points.Add(p);
				p.isKinematic = isKinematic;
				
				p.Initialize();
			}

		if (p.pointType == PointTypes.start){
			_startPoints.Add(p);
		}
	}
	
	public void RemovePoint(Point p){
		if (_points.Contains(p)){
			_points.Remove(p);
		}

		if(_startPoints.Contains(p)){
			_startPoints.Remove(p);
		}
	}
	public int TryComparePoints(Point p1, Point p2){
		if(p1._connectedSplines.Count > 0 && p2._connectedSplines.Count > 0){
			return p1._connectedSplines[0].order.CompareTo(p2._connectedSplines[0].order);
		}

		return 0;
	}

	//when resetting this will need to be called?
	public void Setup()
	{
		curSplineIndex = 0;
		startIndex = 0;
		timer = 0;
		lapCount = 0;
		isComplete = false;
		won = false;
		isPlayerOn = false;

		_escapeSplines = new List<Spline>();

		//do we need to go through all our points?
		//I would hate to do that

		foreach(Collectible c in collectibles){
			c.Reset();
		}

		foreach (Point p in _points)
		{
			p.Reset();
		}

		foreach(CrawlerManager c in crawlers){
			c.Reset();
		}

		//why arent we resetting splines?
		
		foreach(Spline s in _splines){
			if (s.type == Spline.SplineType.locked)
				{
					_escapeSplines.Add(s);
				}

				if (lockSplines && s.order != 0)
				{
					s.SwitchState(Spline.SplineState.off);
				}
		}

		//why is this here
//		Services.main.state = Main.GameState.playing;
}

	public void EnterEndpoint(){
		//deposit collectible
		if(Services.PlayerBehaviour.hasCollectible){
			Services.PlayerBehaviour.collectible.Deposit();
		}

		CheckCompletion();
	}

	void CheckCompletion(){

		foreach(Collectible c in collectibles){
			if(!c.deposited) return;
		}

		isComplete = true;
	}

	public void LeftStartPoint(){
		if(startIndex%_startPoints.Count == 0 && !isComplete){
			//start the timer bro;
			timer = 0;
			
			Services.PlayerBehaviour.LeftStartPoint();

			if(OnLeaveStart != null){
				OnLeaveStart.Invoke();
			}
		}		
		startIndex ++;
	}
	
	public void OnPlayerExit(){
		isPlayerOn = false;
	}

	public void OnPlayerEnter()
	{	
		isPlayerOn = true;
		Services.main.activeStellation = this;
	
		SetCameraInfo();

		Show(true);

		if(StellationManager.instance != null){
			StellationManager.instance.EnterStellation(this);
		}
	}

	public void DrawStellation(){
		if(!lockSplines){
			foreach(Spline s in _splines){
				if(!_escapeSplines.Contains(s)){
					s.DrawEntireSpline();
				}
			}
		}
	}

	public void Show(bool b)
	{
		foreach (Point p in _points)
		{
			if (b)
			{
				p.SwitchState(Point.PointState.off);	
			}
			else
			{
				p.SwitchState(Point.PointState.locked);
			}
		}

		if (b)
		{
			
			//what if start is null
			// start.SwitchState(Point.PointState.on);
			
		}

		if(b && lockSplines){
			//dont turn everything on, just the start;
			_splines[0].SwitchState(Spline.SplineState.on);
		
		}else{
			foreach(Spline s in _splines){
				
				s.SwitchState(b ? Spline.SplineState.on : Spline.SplineState.off);
				
			}
		}
	}

	public bool CheckLapCount(){
		return lapCount >= laps;
	}
	public void UpdateLapCount()
	{

		int minLaps = 1000;

		//this was to save time but now its causing problems

		if (curSplineIndex < (_splines.Count - _escapeSplines.Count) - 1 && laps > 0)
		{
			return;
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
			}
		}
		
		if(minLaps > lapCount){
			if(OnCompleteLap != null){
				OnCompleteLap.Invoke();
			}
			
		}

		lapCount = minLaps;

		if(laps > 1){
			//Services.fx.readout.text = lapCount.ToString("F0") + "/" + laps.ToString("F0");
		}
	}

	//call this for flying off and resetting, player right clicking, and time running out


	public Point GetStartPoint(){
		if(OnNextStart != null){
			OnNextStart.Invoke();
		}
		return _startPoints[startIndex % _startPoints.Count];
	}

	public void OnCompleteSpline(Spline spline)
	{
		curSplineIndex ++;
		if(curSplineIndex < _splines.Count && lockSplines){
			_splines[curSplineIndex].SwitchState(Spline.SplineState.on);
			_splines[curSplineIndex].DrawEntireSpline();
		}
	}
	
	public void ReloadFromEditor()
	{
		Services.main.editor.LoadIntoStellation(this);
		StellationManager manager = GetComponentInParent<StellationManager>();

		//doing loading non destructively now
		
		// if (manager.controllers.Contains(this))
		// {
		// 	manager.controllers[manager.controllers.IndexOf(this)] = c;
		// }
		// c.transform.parent = transform.parent;
		// c.transform.position = transform.position;
		// DestroyImmediate(gameObject);
	}

	public void Step()
	{
		if (isPlayerOn)
		{
			foreach(CrawlerManager c in crawlers){
				c.Step();
			}
			//why the hell are you doing this
			// transform.position = Vector3.Lerp(transform.position, pos, Time.deltaTime);

			foreach (Point p in _points)
				{
					p.Step();
				}
			//Services.main.fx.readout.transform.position = Services.main.Player.transform.position;

			if (!won)
			{	

				if(Services.PlayerBehaviour.state == PlayerState.Traversing && !Services.PlayerBehaviour.joystickLocked){

					//pos -= Vector3.forward * Time.deltaTime / 60f;	
				}


				if (unlockMethod == UnlockType.speed)
				{
					
					float playerSpeed = Services.PlayerBehaviour.flow;
					
					
					if(count > average){
						speedAverage = speedAverage + (playerSpeed-speedAverage)/(average+1);
					
					}else{
						count++;
						speedAverage += playerSpeed;

						if(count == average){
							speedAverage /= count;
						}
					}
 
					//Services.fx.readout.text = (Mathf.Clamp(speedAverage, 0, 100)/(speed) * 100).ToString("F0") + "%";
				
				}else if (unlockMethod == UnlockType.time)
				{
					
					if(startIndex > 0){
						timer += Time.deltaTime;
						Services.fx.readout.text = Mathf.Clamp((time - timer), 0, 1000).ToString("F1");

						if (time - timer <= 0)
						{
							//ResetLevel();
							Services.fx.readout.text = "-.--";
						}

					}
				
				}else if (unlockMethod == UnlockType.laps){
				
					if(laps > 1){
						//Services.fx.readout.text = lapCount.ToString("F0") + "/" + laps.ToString("F0");
					}
				}

			}
		}

		if(isComplete){
			Services.fx.readout.text = "";
		}
	}

	public void NextWord(){
		//Services.main.text.text += GetNextWord();
	}
	public void TryToUnlock()
	{
		
		if(won) return;

		UpdateLapCount();
		if (!isComplete)
		{
			switch (unlockMethod)
			{
			case UnlockType.laps:
				isComplete = CheckLapCount();
				break;
			
			case UnlockType.speed:
				
				// isComplete = CheckSpeed();

				break;
			case UnlockType.time:
				if(startIndex > 0 && startIndex % _startPoints.Count == 0 && (Services.PlayerBehaviour.curPoint.pointType == PointTypes.start || Services.PlayerBehaviour.curPoint.pointType == PointTypes.end)){
					isComplete = time - timer > 0;
				}
				break;
			}
	
		}

		if(!won && isComplete){
			// Debug.Log("won " + title);
			Won();
		}
	}

	public void ShowEscape()
	{
		foreach (Spline s in _escapeSplines)
		{
			s.SwitchState(Spline.SplineState.on);
		}
	}

	public float GetNormalizedHeight(Vector3 pos){
		return Mathf.Clamp01(pos.y - lowerLeft.y / (upperRight.y - lowerLeft.y));
	}

	public float GetNormalizedWidth(Vector3 pos){
		return Mathf.Clamp01(pos.x - lowerLeft.x / (upperRight.x - lowerLeft.x));
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
	public void SetCameraInfo(bool teleport = false)
	{

		CameraFollow.instance.desiredFOV = desiredFOV;

		center = Vector3.Lerp(lowerLeft, upperRight, 0.5f);

		float height = Mathf.Abs(upperRight.y - lowerLeft.y);
		float fov = CameraDolly.FOVForHeightAndDistance(height, Main.cameraDistance) + 10f;
	
		//CameraFollow.instance.desiredFOV = fov;
		//CameraFollow.instance.cam.fieldOfView = fov;
		CameraFollow.instance.lockX = lockX;
		CameraFollow.instance.lockY = lockY;
		CameraFollow.instance.lockZ = lockZ;

		if(setCameraPos){
			if(teleport){
				CameraFollow.instance.WarpToPosition(cameraPos);
			}else{
				CameraFollow.targetPos = cameraPos;
			}
		}else{
			if(teleport){
				CameraFollow.instance.WarpToPosition(center);
			}else{
				CameraFollow.targetPos = center;
			}
		}

		//I think we need to set far clipping plane and fog here

	}
	
	public bool CheckSpeed()
	{
		if (speedAverage >= speed)
		{
			return true;
		}
		
			return false;
		
	}
}
