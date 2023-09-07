
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public class StellationController : MonoBehaviour {

	public enum UnlockType{laps, speed, time}
	public enum UnlockMechanism{unlockSpline, turnOnSpline, unlockPoints, switchPointTypes}
	
	public delegate void StellationEvent();
	public StellationEvent OnCompleteLap;
	public StellationEvent OnCompleteStellation;
	public StellationEvent OnLeaveStart;
	public StellationEvent OnNextStart;

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
    public bool lockX;
	public bool lockY;
	public bool lockZ;

	int lapCount;
	public int rootKey;
	public int laps = 1;
	public float speed = 1;
	public float acceleration = 1;
	public int startIndex;
	public float time = 1;
	public float startSpeed = 1;
	public float maxSpeed = 10;
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

	public bool won = false;
	private string[] words;
	private int wordIndex;
	private float fade;
	public bool hasUnlock;
	float count = 0;
    float average = 30;
    [SerializeField] SpriteRenderer spriteRenderer;
    Vector3 startPos;
	[HideInInspector]
	public Vector3 pos;
    public float speedAverage;
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
			foreach(Spline s in _splines){
				

				if(s.gameObject.activeSelf){
					for (int i = 0; i < s.SplinePoints.Count - (s.closed ? 0 : 1); i++){
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
							Gizmos.DrawWireSphere(s.SplinePoints[i].transform.position, 0.1f);
						}

						Gizmos.color = Color.white;

						s.DrawGizmos();
						// if(i == s.SplinePoints.Count-1){
						// 	Gizmos.DrawLine(s.SplinePoints[i].transform.position, s.SplinePoints[0].transform.position);
						// }else{
						// 	Gizmos.DrawLine(s.SplinePoints[i].transform.position, s.SplinePoints[i+1].transform.position);
						// }
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

	public void Awake()
	{
		_points = new List<Point>();
		_splines = new List<Spline>();
		_splinesToUnlock = new List<Spline>();
		_startPoints = new List<Point>();
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
			//EnableStellation(false);
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

	public void AddSpline(Spline s){
		if(!_splines.Contains(s)){
			_splines.Add(s);
			s.controller = this;
			s.transform.parent = transform;
		}
	}

	public void AddPoint(Point p){
		if (!_points.Contains(p))
			{
				p.hasController = true;
				p.controller = this;
				_points.Add(p);
				p.SR.color = Color.white * 0.2f;
				p.Initialize();
			}

		if (p.pointType == PointTypes.start){
			_startPoints.Add(p);
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
		transform.position = startPos;
		GetComponents();

		//why is this here
//		Services.main.state = Main.GameState.playing;
}

	public void LeftStartPoint(){
		if(startIndex%_startPoints.Count == 0 && !isComplete){
			//start the timer bro;
			timer = 0;
			Services.main.levelText.text = "";
			Services.main.text.text = "";
			
			Services.PlayerBehaviour.LeftStartPoint();

			if(OnLeaveStart != null){
				OnLeaveStart.Invoke();
			}
		}		
		startIndex ++;
	}
	public void Draw(){
		
		if(!lockSplines){
			// foreach(Spline s in _splines){
			// 	if(!_splinesToUnlock.Contains(s)){
			// 		s.StartDrawRoutine();
			// 	}
			// }

			_splines[0].SwitchState(Spline.SplineState.on);
		}
	}
	public void Setup()
	{	
		curSplineIndex = 0;
		startIndex = 0;
		timer = 0;
		lapCount = 0;
		isComplete = false;
		won = false;

		rootKey = UnityEngine.Random.Range(36, 49);
		isOn = true;
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

	public bool CheckLapCount(){
		return lapCount >= laps;
	}
	public void UpdateLapCount()
	{

		int minLaps = 1000;

		//this was to save time but now its causing problems

		if (curSplineIndex < (_splines.Count - _splinesToUnlock.Count) - 1 && laps > 0)
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

	public void UnlockSpline(Spline spline)
	{

		curSplineIndex ++;

		
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
		if (isOn)
		{
			transform.position = Vector3.Lerp(transform.position, pos, Time.deltaTime);

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
 
					Services.fx.readout.text = (Mathf.Clamp(speedAverage, 0, 100)/(speed) * 100).ToString("F0") + "%";
				
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


		if(isComplete){
			Services.fx.readout.text = "";
		}
	}

	public void NextWord(){
		//Services.main.text.text += GetNextWord();
	}
	public bool TryToUnlock()
	{
		UpdateLapCount();
		if (!isComplete)
		{
			switch (unlockMethod)
			{
			case UnlockType.laps:
				isComplete = CheckLapCount();
				break;
			
			case UnlockType.speed:
				isComplete = CheckSpeed();
				break;
			case UnlockType.time:
				if(startIndex > 0 && startIndex % _startPoints.Count == 0 && (Services.PlayerBehaviour.curPoint.pointType == PointTypes.start || Services.PlayerBehaviour.curPoint.pointType == PointTypes.end)){
					isComplete = time - timer > 0;
				}
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
		if (speedAverage >= speed)
		{
			return true;
		}
		
			return false;
		
	}
}
