using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Vectrosity;
using UnityEngine.InputSystem;
using UnityEditor;

public enum PlayerState{Traversing, Switching, Flying, Animating};

public class PlayerBehaviour: MonoBehaviour {

	public PlayerState state;

	[Header("Current Spline")]
	public Spline curSpline;
	private Spline splineDest;
	[Space(10)]

	[Header("Points")]
	public Point curPoint;
	public Point pointDest;
	public Point lastPoint;

	[Space(10)] [Header("Movement Tuning")]
	public float speed = 0;
	public float acceleration = 0;
	public AnimationCurve accelerationCurve;
	public float flowDecay;
	public float boostDecay = 0.2f;
	public float flyingSpeedDecay = 1;
	public float maxSpeed = 10;
	public float accuracyCoefficient;
	public float stopTimer = 2f;
	[Space(10)] [Header("Cursor Control")]
	public float cursorMoveSpeed = 1;
	public float minCursorDistance = 25;
	public float maxCursorDistance = 2;
	public float cursorDistance;
	public float cursorRotateSpeed = 1;
	public float AutoLeaveDiff = 30;
	public float StopAngleDiff = 60;

	public float clampedSpeed
	{
		get { return Mathf.Clamp01(curSpeed / 2); }
	}

	[HideInInspector]
	public float flyingSpeed;
	public bool hasCollectible;

	public List<Collectible> collectibles;
	public bool glitching;
	bool canTraverse;
	private bool hasFlown = false;
	private bool foundConnection = false;
	private bool freeCursor = false;
	
	private bool upstream = false; 

	[Space(10)]

	[HideInInspector]
	public Vector3 pos;
	
	[HideInInspector]
	public bool goingForward = true;
	public bool facingForward;

	public delegate void StateChange();
	public StateChange OnStartFlying;
	public StateChange OnExitPoint;
	public StateChange OnEnterPoint;
	public StateChange OnEnterSpline;
	public StateChange OnExitSpline;
	public StateChange OnTraversing;
	public StateChange OnFlying;
	public StateChange OnStoppedFlying;
	[HideInInspector] public float progress, adjustedProgress,
		signedAccuracy,
		flow = 0,
		boost,
		speedGain,
		boostTimer,
		curSpeed,
		connectTime,
		timeOnPoint,
		
		connectTimeCoefficient,
		gravity,
		decelerationTimer;

	[HideInInspector]
	public Vector3 curDirection;

	[HideInInspector]
	public Vector2 screenSpacePos;

	[HideInInspector]
	public Vector2 screenSpaceDir;
	[HideInInspector]
	public Vector3 deltaDir;
	[HideInInspector]
	public float deltaAngle;

	public float potentialSpeed => flow + boost;
	public float easedAccuracy;
	public float normalizedAccuracy;
	public float easedDistortion;

	[Header("Flying tuning")]
	public float flyingSpeedThreshold = 3;
	public float PointDrawDistance;
	public float creationCD = 0.25f;
	public float creationInterval = 0.25f;
	private bool noRaycast;
	private bool charging;
	private List<Transform> newPointList;

	[Space(10)] [Header("Point Creation")]

	private Spline drawnSpline;
	private Point drawnPoint;
	[SerializeField]
	public List<Point> traversedPoints;
	private List<Point> inventory;
	private float curDrawDistance = 0.1f;
	[Space(10)]

	[Header("Input")]
	public bool usingJoystick;
	public bool joystickLocked = true;
	[Space(10)]

	[Header("AV")]

	public Sprite canFlySprite;
	public Sprite canMoveSprite;
	public Sprite canConnectSprite;
	public Sprite brakeSprite;
	public Sprite traverseSprite;
	public SpriteRenderer glitchFX;
	private PlayerSounds sounds;
	public TrailRenderer flyingTrail;
	public TrailRenderer shortTrail;
	public ParticleSystem sparks;
	ParticleSystem.EmissionModule sparkEmission;
	private LineRenderer l;
	public SpriteRenderer cursorRenderer;
	public MeshRenderer renderer;
	public Transform visualRoot;
	public SpriteRenderer boostIndicator;
	public LineRenderer cursorOnPoint;
	private VectorLine velocityLine;
	private VectorLine velocityLine2;

	public bool buttonUp;
	public bool buttonDown;

	bool stopFlying;

	public bool buttonWasPressed => buttonDownTimer > 0;
	private float buttonDownBuffer = 0.5f;
	private float buttonDownTimer;
	private float progressRemainder;

	private GameObject cursor
	{
		get { return Services.Cursor; }
	}
	[HideInInspector]
	public Vector3 cursorPos;
	[HideInInspector]
	public Vector2 cursorDir;
	public Vector2 cursorDir2;
	public Vector3 cursorPos2;


	public void Awake(){
		
		sparkEmission = sparks.emission;
		pos = transform.position;

		joystickLocked = true;
		pointDest = null;
		traversedPoints = new List<Point> ();

		connectTimeCoefficient = 1;
		state = PlayerState.Switching;
		l = GetComponent<LineRenderer> ();

		inventory = new List<Point>();
		newPointList = new List<Transform> ();

		int i = 0;
		lastPoint = null;

		Services.main.OnReset += Reset;

	}

	public void LeftStartPoint(){
		Services.fx.cursorTrail.Clear();
	}

	public void ResetPlayerToStartPoint()
	{
		if(Services.main.state != GameState.playing && !MapEditor.editing){return;}

		Services.fx.PlayAnimationOnPlayer(FXManager.FXType.glitch);

		if (Services.main.state == GameState.playing)
		{
			Services.main.WarpPlayerToNewPoint(Services.main.activeStellation.start);
			Reset();
			flow = Services.main.activeStellation.startSpeed;
			flyingSpeed = 0;
		}
	}
	public void Initialize()
	{
		cursorDistance = minCursorDistance;
		curPoint = Services.StartPoint;
		transform.position = curPoint.Pos;
		pos = transform.position;
		cursorPos = pos;
		cursorDir2 = Vector3.zero;
		cursorDir = Vector3.zero;
		cursor.transform.position = cursorPos;
		cursorRenderer.enabled = false;
		traversedPoints.Add (curPoint);

		curPoint.OnPlayerEnterPoint();

		flow = Services.main.activeStellation.startSpeed;
		acceleration = Services.main.activeStellation.acceleration;
		maxSpeed = Services.main.activeStellation.maxSpeed;

		foreach(Spline s in curPoint._connectedSplines){
			s.SetSelectedPoint(curPoint);
		}
		
		ResetFX();
	}

	void Lose(){
		//some fx;
		SwitchState(PlayerState.Animating);
		StartCoroutine(DieRoutine());
	}

	public IEnumerator DieRoutine(){
	
		// particle effect on player
		// force on each point in proximity to the player
		// each point light turns off
		// all black before reset
		

		float t = 0; 

		Services.fx.PlayParticle(ParticleType.lose, pos, Vector3.forward);

		foreach(Point p in Services.main.activeStellation._points){
			Vector3 toPlayer = (p.Pos - visualRoot.position);
			p.AddForce(Random.onUnitSphere * 50);
		}
		
		AudioManager.instance.PlayerDeath();

		bool cancel = false;
        //GlitchEffect.SetValues(1-t);
        
		while(t < 1){
			
			//pretty sure this script just gets deleted and this never triggers

			if(Services.main.state == GameState.menu){
				cancel = true;
				break;
			}

			Services.fx.overlay.color = Color.Lerp(Color.clear, Color.black, Easing.QuadEaseIn(t));
			Spline.shake = Mathf.Lerp(0.33f, 0, t);

			t += Time.deltaTime;
			yield return null;
		}

		Spline.shake = 0;

		if(cancel){
			Services.fx.overlay.color = Color.clear;
		}else{
			Services.fx.Fade(true, 0.2f);
			Services.main.ResetLevel();
		}
	}
		

	public void Reset()
	{
		
		Spline.shake = 0;
		charging = false;
		boostTimer = 0;
		Services.fx.overlay.color = Color.clear;
		hasCollectible = false;
		collectibles = new List<Collectible>();

		if (Services.main.hasGamepad)
		{
			Services.main.gamepad.ResetHaptics();
		}

		cursorDir = Vector3.zero;
		cursorDir2 = Vector3.zero;
		cursorPos =pos;
		cursorRenderer.enabled = true;
		progress = 0;

		state = PlayerState.Switching;

		freeCursor = false;
		curSpline = null;
		traversedPoints.Clear();
		hasFlown = false;
		boost = 0;
		flow = 0;

		pointDest = null;
		lastPoint = null;
		buttonDown = false;
		buttonUp = false;
		
		buttonDownTimer = 0;

		ResetFX();
	}

	public void ResetFX()
	{
	
		flyingTrail.Clear();
		shortTrail.Clear();
		flyingTrail.emitting = false;
		Services.fx.playerTrail.emitting = false;
		Services.fx.playerTrail.Clear();
	}

	public void PressButton(InputAction.CallbackContext context){
		

		if(Services.main.state != GameState.playing){return;}

		bool dwn = context.ReadValueAsButton();

		if(!buttonDown && dwn){
			
			buttonDown = true;
		}else if(buttonDown && !dwn){
			// Debug.Log(context.action.)
			buttonUp = true;
			buttonDown = false;
			charging = false;
			
			boostTimer = 0;
			
			buttonDownTimer = buttonDownBuffer;
		}


	}

	public void AddCollectible(Collectible c){
		collectibles.Add(c);
		hasCollectible = true;
	}

	public void RemoveCollectible(Collectible c){
		collectibles.Remove(c);
		if(collectibles.Count == 0){
			hasCollectible = false;
		}
	}
	void OnGUI()
	{
		if (Application.isEditor)  // or check the app debug flag
		{
			// Rect r = new Rect(Screen.width - 200, Screen.height - 100, 200, 100);
			
			// GUI.Label(r, "curSpeed: " + curSpeed.ToString("F1"));
			// r.y -=20;
			// GUI.Label(r, "flow: " + flow.ToString("F1"));
			// r.y -=20;
			// GUI.Label(r, "boost: " + boost.ToString("F1"));

		}
	}
	public void OnDrawGizmos(){
		if(state == PlayerState.Traversing){

			Gizmos.color = Color.blue;
			Gizmos.DrawCube(curPoint.Pos, Vector3.one * 0.1f);

			Gizmos.color = Color.green;
			Gizmos.DrawCube(pointDest.Pos, Vector3.one * 0.1f);
			
			// if(curPoint == curSpline.SplinePoints[curSpline.selectedIndex]){
			// 	if(goingForward){
			// 		Debug.Log("going forward, curpoint = selected");
			// 	}else{
			// 		Debug.Log("going backward, curpoint = selected");
			// 	}
			// }else{
			// 	if(goingForward){
			// 		Debug.Log("going forward, curpoint X selected");
			// 	}else{
			// 		Debug.Log("going backward, curpoint X selected");
			// 	}
			// }
		}
	}
	public void Step()
	{

		//make everything look at the camera
		glitchFX.transform.LookAt(CameraFollow.instance.transform.position);

		if(curDirection.sqrMagnitude > 0){
			visualRoot.rotation = Quaternion.LookRotation(curDirection, CameraFollow.forward);
		}
		
		pos = transform.position;

		Vector2 p1 = Services.mainCam.WorldToScreenPoint(pos);
		Vector2 p2 = Services.mainCam.WorldToScreenPoint(pos + curDirection);

		screenSpacePos = p1;
		screenSpaceDir = (p2 - p1).normalized;

		if(state != PlayerState.Animating){
			
			normalizedAccuracy = signedAccuracy/2f + 0.5f;
			easedAccuracy = Mathf.Clamp01(Mathf.Pow(normalizedAccuracy, accuracyCoefficient));
			easedDistortion = Mathf.Lerp(easedDistortion, (1- easedAccuracy) + Spline.shake, Time.deltaTime * 5);
		}

		Debug.DrawLine(pos, pos + Services.mainCam.transform.TransformDirection(cursorDir)/5f, Color.cyan);
		Debug.DrawLine(pos, pos + Services.mainCam.transform.TransformDirection(screenSpaceDir)/2f, Color.yellow);

		//curspeed going to zero making movement asymptotes
		
		//set all your important floats bruh


		if(state == PlayerState.Traversing){
			glitching = signedAccuracy < 0 || joystickLocked;
		}else if(state == PlayerState.Switching){
			glitching = !canTraverse;
		}else{
			glitching = false;
		}

		renderer.enabled = !glitching;
		glitchFX.enabled = glitching;
		

		if (joystickLocked)
		{
			//cursorRenderer.enabled = false;
			//show glitch effect if we're not on a point 

		}
		else
		{
			//hide glitch effect
			cursorRenderer.enabled = true;
		}

		Point.hitColorLerp = connectTime;

		//UI boost visuals and button input
		if (buttonDown)
		{
			boostIndicator.enabled = true;
			
			// boostIndicator.transform.position =pos + (Vector3) cursorDir2 * ((Vector3)transform.position - cursorPos).magnitude;
			// boostIndicator.transform.rotation = Quaternion.LookRotation(CameraFollow.forward, cursorDir2);

			if(charging && state != PlayerState.Switching){
				boostTimer += Time.deltaTime;
				boostTimer = Mathf.Clamp01(boostTimer);
			}

			charging = true;
			buttonDownTimer = buttonDownBuffer;
		}
		else
		{
			boostIndicator.enabled = false;
		}

		boostIndicator.transform.localScale = Vector3.Lerp(Vector3.one * 2, Vector3.one, Easing.QuadEaseOut(boostTimer));

		//player sprite stretching

		float speedScale;
		if(state == PlayerState.Switching || state == PlayerState.Animating){
			speedScale = 0;
		}else if (state == PlayerState.Flying){
			speedScale = flyingSpeed;
		}else{
			//this math looks all kinds of fucked up
			speedScale = Mathf.Clamp01(Mathf.Pow(signedAccuracy, 5) * curSpeed + 0.25f);
		}

		if (joystickLocked)	speedScale = 0;

		renderer.transform.localScale = Vector3.Lerp(renderer.transform.localScale, new Vector3(Mathf.Clamp(1 - (speedScale * 2),1, 2), 1, Mathf.Clamp(speedScale, 1, 2)), Time.deltaTime * 10);

		Effects ();

		if (state == PlayerState.Flying) {
			if(OnFlying != null){
				OnFlying.Invoke();
			}
			FreeMovement ();
			
			return;
		}

		buttonDownTimer -= Time.deltaTime;
		connectTime -= Time.deltaTime * connectTimeCoefficient;

		if (state == PlayerState.Traversing) {
			//accuracy = GetAccuracy(progress);
			float maxAcc = -100;
			// for(int i = 0; i < 1; i++){
			// 	float sign = goingForward ? 1 : -1;
			// 	float curAcc = GetAccuracy(progress + (float)i * sign * 0.05f);
			// 	if(curAcc > maxAcc){
			// 		maxAcc = curAcc;
			// 	}
			// }
	
			signedAccuracy = GetAccuracy(progress);

			if(OnTraversing != null){
				OnTraversing.Invoke();
			}
			
			CalculateMoveSpeed ();
			curSpeed = GetSpeed();

			// Debug.Log("boost = " + boost.ToString("F1") + " flow = " + flow.ToString("F1") + " speedGain = " + (speedGain/Time.deltaTime).ToString("F1"));

			if((upstream && potentialSpeed <= 0) || (potentialSpeed <= 0.1f)){
				
				Lose();
				return;
			}
			
			UpdateProgress();
			UpdatePositionOnSpline();
			CheckProgress ();

		}
		
		if(state == PlayerState.Switching)
		{
			transform.position = curPoint.Pos;
			visualRoot.position = Vector3.Lerp(visualRoot.position,pos,Time.deltaTime * 10f);
			
			//gravity = 0;
			
			PlayerOnPoint();
		}

		buttonUp = false;
	}

	void UpdatePositionOnSpline(){
		
		curSpline.SetPlayerLineSegment();

		//Code for placing player sprite on the line instead of the spline
		//hiccups are being caused by double indices on points

		//holy fuck you actually use this value for their position?
		int curStep = curSpline.playerIndex;
		int nextStep = curStep + (goingForward ? 1 : -1);
		int upperBound = curSpline.line.points3.Count-1;
		Vector3 dest1 = curSpline.line.points3[Mathf.Clamp(curStep, 0, upperBound)];
		Vector3 dest2 = curSpline.line.points3[Mathf.Clamp(nextStep, 0, upperBound)];

		float p = Spline.curveFidelity * progress;
		float step = Mathf.Floor(p);
		float diff = goingForward ?  Mathf.Abs(p - step) : 1-Mathf.Abs(p - step);

		transform.position = curSpline.GetPointForPlayer(progress);
		Vector3 dest = Vector3.Lerp(dest1, dest2, diff);
		visualRoot.position = Vector3.Lerp(transform.position, dest, easedDistortion * 2);

		curSpline.completion += (curSpeed * Time.deltaTime) / curSpline.segmentDistance;

		if (goingForward) {
			curPoint.proximity = 1 - progress;
			pointDest.proximity = progress;

		} else {
			curPoint.proximity = progress;
			pointDest.proximity = 1 - progress;
		}
	}
	public float GetSpeed()
	{
		
		if(state == PlayerState.Flying){
			return flyingSpeed;
		}

		if(state == PlayerState.Traversing){

			// ok so we're not differentiating this at all?
			// if(curSpline.speed > 0 && goingForward){
			// 	return Mathf.Clamp(boost + flow, 0, maxSpeed);
			// }else{
				
				return boost + (flow * Mathf.Lerp(easedAccuracy, 1, adjustedProgress)); //* easedAccuracy + boost; //* cursorDir.magnitude;
			// }
		}
		if(state == PlayerState.Switching){
			return Mathf.Lerp(curSpeed, 0, Time.deltaTime * 2);
		}

		return curSpeed;
	}

	bool FindPointToConnect(){
		List<Point> points = Services.main.activeStellation._points;
		float minAlignment = 1000;
		Point candidate = null;
		List<Point> candidates = new List<Point>();

		 for(int i = 0; i < points.Count; i++){
			Point p = points[i];

			if(points[i].pointType == PointTypes.ghost || p.state == Point.PointState.locked || p == curPoint || curPoint._neighbours.Contains(p)) continue;

			Vector3 viewportPos = Services.mainCam.WorldToViewportPoint(p.Pos);
				
			if(viewportPos.x > 1 || viewportPos.x < 0 || viewportPos.y > 1 || viewportPos.y < 0){
				continue;
			}

			Vector3 screenPointAtStart = Services.mainCam.WorldToScreenPoint(curPoint.Pos);
			Vector3 screenPointAtEnd = Services.mainCam.WorldToScreenPoint(p.Pos);
			Vector3 screenSpaceDirection = (screenPointAtEnd - screenPointAtStart).normalized;

			float alignment = Vector2.Angle (cursorDir, screenSpaceDirection);

			if(alignment < 20){
				candidates.Add(p);
				candidate = p;
				minAlignment = alignment;
			}
		 }

		 if(candidates.Count > 0){
			 float distance = 100000;
			 foreach(Point p in candidates){

				Vector3 screenPointAtStart = Services.mainCam.WorldToScreenPoint(curPoint.Pos);
				Vector3 screenPointAtEnd = Services.mainCam.WorldToScreenPoint(p.Pos);

				float d = (screenPointAtEnd - screenPointAtStart).magnitude;
				if(d < distance){
					distance = d;
					candidate = p;
				}
			 }

			 pointDest = candidate;

			 return true;
		 }else{
			 return false;
		 }
	}

	public void PlayerOnPoint(){

		canTraverse = false;
		bool hasPath = false;
		Point prevPointDest = pointDest;
		Spline prevSplineDest = splineDest;

		if (TryLeavePoint()) // && !foundConnection)
		{
			cursorRenderer.sprite = traverseSprite;
			hasPath = true;

			//I think this is bugging if the player enters a ghost point when their angle is > 
			//that necessary to progress
			
			if (curPoint.pointType == PointTypes.ghost)
			{
				canTraverse = true;

			}
			else
			{
				
				bool newSplineSelected = prevSplineDest == null || splineDest != prevSplineDest;

				if (newSplineSelected){
					Services.fx.ShowSplineDirection(splineDest, curPoint, pointDest);
				}

				if(pointDest.pointType != PointTypes.ghost)
				{
					Services.fx.ShowNextPoint(pointDest);

				}else{
					
					Services.fx.nextPointSprite.enabled = false;
				}
				
			}

			//boostTimer >= 1 ||  if you wnna fuck with ppl
			if (!joystickLocked && curPoint.CanLeave()) {

				//something about locking was here
				canTraverse = true;

			}

			if (!canTraverse)
			{
				l.positionCount = 0;

			}

		}
		else
		{
			Services.fx.nextPointSprite.enabled = false;
		}

		if(!hasPath){

			foundConnection = false;

			if (curPoint.pointType == PointTypes.connect)
			{
				//freeCursor = true;
				
				if (FindPointToConnect())
				{		
					
			 		foundConnection = true;
					cursorRenderer.sprite = traverseSprite;
					Services.fx.ShowNextPoint(pointDest);

					if(buttonUp){
						//canTraverse = true;

						if(curPoint.pointType == PointTypes.connect){
							//curPoint.SetPointType(PointTypes.normal);
						}
						
						foundConnection = false;
						Connect();

						
					}
					//no need to do this now, it will happen via the CanLeavePoint func next frame
					//canTraverse = true;

				}

			}
			else if (TryToFly())
				{
					cursorRenderer.sprite = canFlySprite;

					//force flight off neighbourless points
					
					if (curPoint.CanLeave())
					{
						SwitchState(PlayerState.Flying);

						return;
					}
				}
				else{
					l.positionCount = 0;
				 }
			}


		if (timeOnPoint == 0)// && curPoint.pointType != PointTypes.ghost && ((buttonDown || buttonUp) || !curPoint.CanLeave()))
		{
			
			curPoint.velocity += (Vector3)cursorDir * (1-easedAccuracy) * potentialSpeed;
			
		}

		if (canTraverse)
		{
			// pointInfo.GetComponent<Text>().text = "";
			SwitchState(PlayerState.Traversing);

			Services.fx.nextPointSprite.enabled = false;
			cursorDistance = minCursorDistance;
		}
		else{
			
			//what in gods name is happening here? old level completion code

			if (curPoint.pointType == PointTypes.end && !curPoint.controller.isComplete)
			{
				StayOnPoint();
			}
			else
			{
				StayOnPoint();
			}
		}
	}

	public float GetAccuracy(float prog){
		
		prog = Mathf.Clamp(prog, 0.01f, 0.99f);
		
		Vector3 newDirection = curSpline.GetDirection (prog);
		if(!goingForward){newDirection = -newDirection;}
		
		deltaAngle = Vector3.SignedAngle(newDirection, curDirection, Vector3.up);
		deltaDir = newDirection - curDirection;
		curDirection = newDirection;
		
		//Debug.DrawLine(transform.position,pos + splineDir, Color.red);
		
		//might be best to remove the z
		//splineDir.z = 0;

		
		//Debug.DrawLine(transform.position,pos + curDirection, Color.yellow);

		//we have to find a way to flatten the cursor dir to screen space as well
		float alignment = Vector2.Angle (cursorDir, screenSpaceDir);

		return (90 - alignment) / 90;
		//StopAngleDiff = Mathf.Lerp (20, 50, Mathf.Abs(flow));
	}

	bool CanConnect(){

		//spherecast around player???
		

			if (!joystickLocked) {

				Point target = SplineUtil.RaycastFromCamera(cursorPos, 5f);

				bool drawnPointNull;

				if(drawnPoint == null){
					drawnPointNull = true;
				}
			
				if (target != null && target.state != Point.PointState.locked && target.pointType != PointTypes.ghost && target != curPoint && !target.IsAdjacent(curPoint))
				{

					foundConnection = true;
					if(drawnPoint != null){
						if(drawnPoint != target)
						{
							pointDest = target;
							return true;
					  }
							return false;

					}

					pointDest = target;
						return true;
				}
		}

			foundConnection = false;

		return false;
	}

	void Connect(){
		SplinePointPair spp = SplineUtil.ConnectPoints (curSpline, curPoint, pointDest);
		//Adding points multiple times to each other is happening HERE
		//Could restrict points to never try and add their immediate neighbours?
		l.positionCount = 0;

		// pointDest.tension = 1;

		if(curSpline == null || curSpline != null && spp.s != curSpline){
			Services.main.activeStellation.AddSpline(spp.s);
			spp.s.StartDrawRoutine(curPoint);
		}
		
		
		splineDest = spp.s;
		splineDest.SetSelectedPoint(curPoint);

		//why should you set pointdest here if you're already using it above as the target point?
		// pointDest = spp.p;

		connectTime = 1;
	}

	bool TryToFly(){
		if ((Mathf.Abs(flow) >= 0 && curPoint.canFly))
		{
			l.positionCount = 2;
			l.SetPosition (0, cursorPos);
			l.SetPosition (1,pos);
			return true;
		}
		return false;
	}


	public void EmitParticles()
	{
		sparks.Emit(5);
	}

	public void Fly(){
		
		if(Services.main.state != GameState.playing){return;}

		//SwitchState(PlayerState.Flying);
		

		//pointDest = null;
		//l.positionCount = 0;
		//curPoint.proximity = 0;
		//drawnPoint = curPoint;
		//curPoint = SplineUtil.CreatePoint(transform.position);
		//curSpline = SplineUtil.CreateSpline(drawnPoint, curPoint);
		//curDrawDistance = 0;
		 //curSpline.OnSplineEnter(drawnPoint, curPoint);
		//curPoint.GetComponent<Collider>().enabled = false;

		//flow = Mathf.Abs(flow);
	}


	void StayOnPoint(){

		decelerationTimer = Mathf.Lerp(decelerationTimer, 0, Time.deltaTime * 2);
		timeOnPoint += Time.deltaTime;

		if(buttonDown && !freeCursor){ //pointDest != null || curPoint.pointType == PointTypes.fly && state != PlayerState.Flying)){
			boostTimer += Time.deltaTime / stopTimer;
			boostIndicator.enabled = true;
		}else{
			boostTimer -= Time.deltaTime;
			boostIndicator.enabled = false;
		}

		boostTimer = Mathf.Clamp01(boostTimer);

		curPoint.PlayerOnPoint(cursorDir, flow);

		//l.SetPosition (0, Vector3.Lerp(transform.position, cursorPos, Easing.QuadEaseOut(boostTimer)));

		if(foundConnection){
			l.positionCount = 2;
			l.SetPosition(0, pointDest.Pos);
			l.SetPosition (1,pos);
		}else if(curPoint.pointType == PointTypes.connect){
			
			l.positionCount = 0;
		}

		
		renderer.transform.localScale = Vector3.Lerp(renderer.transform.localScale, new Vector3(Mathf.Clamp(1 - (boostTimer), 1, 2), 1, Mathf.Clamp(boostTimer, 1, 2)), Time.deltaTime * 10);

		connectTime -= Time.deltaTime * connectTimeCoefficient;

	}
	bool CanFlyToPoint(Point p){

		if(p != curPoint && p.pointType != PointTypes.ghost && p.state != Point.PointState.locked){

			//check that its not at the end of a bidirectional spline

			if(p._connectedSplines.Count > 0){
				foreach(Spline s in p._connectedSplines){
					if(!s.bidirectional && p == s.EndPoint && !s.closed){
						return false;
					}
				}
			}

			return true;
		}

		return false;
	}

	public void OnTriggerEnter(Collider c){
		//if free movement
		//just enter the point
		//if its not a ghost obviously


		//I think the player's trigger is quite large so this could suck?
		
		if(state == PlayerState.Flying && c.tag == "Point"){
			Point p = c.GetComponent<Point>();
			if(p.pointType != PointTypes.ghost){
				

				pointDest = p;
				stopFlying = true;
			}
		}
	}
	void FreeMovement()
	{
		Point raycastPoint = SplineUtil.RaycastFromCamera(cursorPos, 5f);

		Vector3 viewportPoint = Services.mainCam.WorldToViewportPoint(transform.position);
		if(viewportPoint.x < 0 || viewportPoint.x > 1 || viewportPoint.y > 1 || viewportPoint.y < 0){
			Lose();
		}
		//RESET IF PLAYER IS OFF SCREEN
		if (!stopFlying)
		{
			pointDest = null;
		}

		if (raycastPoint != null && CanFlyToPoint(raycastPoint))	{

			if(pointDest != null && raycastPoint != pointDest){
				pointDest.proximity = 0;
			}

			Services.fx.ShowNextPoint(raycastPoint);

			pointDest = raycastPoint;
			stopFlying = true;
		}
		
		if(pointDest == null && raycastPoint == null){	
			Services.fx.nextPointSprite.enabled = false;
		}

		if (stopFlying && pointDest != null)
		{
			pointDest.controller.AdjustCamera();

			flyingSpeed += Time.deltaTime;
			Vector3 toPoint = pointDest.transform.position -pos;
			Vector3 flyToPoint = toPoint.normalized * Time.deltaTime * (flyingSpeed);
			transform.position += Vector3.ClampMagnitude(flyToPoint, toPoint.magnitude);
			curDirection = toPoint.normalized;
			pointDest.proximity = 1;

			if (Vector3.Distance(transform.position, pointDest.Pos) < 0.025f)
			{
				
				SwitchState(PlayerState.Switching);
			}
		}
		else
		{
			// if (flyingSpeed > 0)
			// {
				flyingSpeed -= Time.deltaTime * flyingSpeedDecay;
				flyingSpeed = Mathf.Clamp(flyingSpeed, 0, 1000);
				Vector3 inertia = cursorDir * (flyingSpeed);
				transform.position += inertia * Time.deltaTime;
				curDirection = cursorDir;
				
				if(flyingSpeed == 0){
					Lose();
				}
			// }
			// else
			// {
			// 	//reset here
			// 	//play fizzle animation?

			// 	SwitchState(PlayerState.Animating);
			// }
		}

	}

	void CalculateMoveSpeed(){

		//store speed of spline 
		float splineSpeed = curSpline.speed;

		//absolute value
		float absSpeed = Mathf.Abs(splineSpeed);

		//accuracy with directional information
		float signedAcc = easedAccuracy * 2 - 1;
		
		//is the player fighting a spline
		upstream = curSpline.speed != 0 && (goingForward && splineSpeed < 0) || (!goingForward && splineSpeed > 0);
		
		//give direction info to player acceleration
		float speedDir = goingForward ? 1 : -1;

		//if we're going against the flow
		if (upstream){
			
			//disable speed gain
			signedAcc = Mathf.Clamp(signedAcc, -1, 0);
			
			//take from flow before boost
			float splineSpeedLost = absSpeed * Time.deltaTime;
			float playerSpeedLost = curSpeed * Time.deltaTime;

			if(curSpline.lineMaterial == 3){
				curSpline.speed += speedDir * (splineSpeedLost + playerSpeedLost);
			}

			if(flow == 0){
				boost -= playerSpeedLost + splineSpeedLost;
			}else{
				flow -= playerSpeedLost + splineSpeedLost;
			}
		}

		//add to player flow based on accuracy
		
		if(signedAcc >= 0){
			speedGain = signedAcc * acceleration * accelerationCurve.Evaluate(flow/maxSpeed);
		}else{
			speedGain = (signedAcc-1) * flowDecay;
		}

		float negativeSpeedGain = Mathf.Abs(Mathf.Clamp(speedGain, -1000, 0));
		speedGain *= Time.deltaTime;
		//if we're going with the flow

		if(!upstream){
			
			boost -= Time.deltaTime * (boostDecay + negativeSpeedGain);

			//give them boost
			if(curSpeed < absSpeed){
				boost = absSpeed - curSpeed;
			}

			if(curSpline.lineMaterial == 3){

				float splineMomentum = Mathf.Clamp01(speedGain);
				//do we want to make it impossible to slow down splines when you're in line with them?
				curSpline.speed += splineMomentum * speedDir;
			}
		}
		
		
		//give player speed
		flow += speedGain;

		flow = Mathf.Clamp(flow, 0, maxSpeed);
		boost = Mathf.Clamp(boost, 0, maxSpeed);
		curSpline.speed = Mathf.Clamp(curSpline.speed, -maxSpeed * 2, maxSpeed * 2);

		if(upstream && curSpeed <= Mathf.Epsilon){
			//ok they're being pushed back
			ReverseDirection();
		}
	}

	void SetGamepadRumble(){
		if (Services.main.hasGamepad && state == PlayerState.Traversing)
		{
			float hi = Mathf.Pow(Mathf.Clamp01(-signedAccuracy + 1), 3) * curSpeed;
			float low = Mathf.Clamp01(-signedAccuracy) * flow + Mathf.Clamp01(hi - 1);

			if (Services.main.useVibration)
			{
				Debug.Log("low=" + low);
				Debug.Log("high= " + hi);
				Services.main.gamepad.SetMotorSpeeds(low, hi);
			}

		}
	}

	void ReverseDirection(){
		
		flow = 0;
		goingForward = !goingForward;
		Point p = pointDest;
		pointDest = curPoint;
		curPoint = p;	

		if(pointDest == curPoint) Debug.Log("illegal");
			
		// int indexdiff = curSpline.SplinePoints.IndexOf (pointDest) - curSpline.SplinePoints.IndexOf (curPoint);

		// if (indexdiff == -1 || indexdiff > 1) {
		// 	curSpline.SetSelectedPoint(pointDest);

		// } else {
		// 	curSpline.SetSelectedPoint(curPoint);
		// }
	}

	void UpdateProgress(){
		
		int curSegment = goingForward ? (int)Mathf.Ceil((float)Spline.curveFidelity * progress) : (int)Mathf.Floor((float)Spline.curveFidelity * progress);

		Vector3 curPos = pos;
		float rollingDistance = 0;
		float prevStep = progress;
		
		float distanceToTravel = curSpeed * Time.deltaTime;
		// float splineSpeed = curSpline.speed;
		// if(upstream) splineSpeed = -splineSpeed;
		// distanceToTravel += splineSpeed;

		if(goingForward){
			for (int k = curSegment; k <= Spline.curveFidelity; k++)
			{
				float step = (float)k/Spline.curveFidelity;

				Vector3 pos = curSpline.GetPointForPlayer(step);
				float diff = (pos - curPos).magnitude;
				
				rollingDistance += diff;
				curPos = pos;
				
				float spillover = rollingDistance - distanceToTravel;
				
				if(spillover > 0){
					//set progress to current position along the line, 
					//minus the extra distance as a fraction of the distance travelled this iteration
					//scaled to the difference of the positions

					float fraction = (spillover/diff) * (step - prevStep);
					progress = step - fraction;
					adjustedProgress = progress;

					return;
				}else{
					prevStep = step;
				}
			}

			progress = 1;
			adjustedProgress = 1;
			return;

		}else{
			for (int k = curSegment; k >= 0; k--)
			{
				float step = (float)k/Spline.curveFidelity;
				Vector3 pos = curSpline.GetPointForPlayer(step);
				float diff = (pos - curPos).magnitude;
				rollingDistance += diff;
				curPos = pos;

				float spillover = rollingDistance - distanceToTravel;

				if(spillover > 0){
					
					//set progress to current position along the line, 
					//plus the extra distance as a fraction of the distance travelled this iteration
					//scaled to the difference of the positions
					float fraction = (spillover/diff) * (prevStep-step);
					progress = step + fraction;
					adjustedProgress = 1-progress;
					
					return;
				}else{
					prevStep = step;
				}
			}

			progress = 0; 
			adjustedProgress = 1;
			return;
		}
	}

	void CheckProgress(){

		if ((progress >= 1 && goingForward) || (progress <= 0 && !goingForward)) {

			progressRemainder = progress;
			curPoint.proximity = 0;

			SwitchState(PlayerState.Switching);
		}
	}

	public void SetPlayerAtStart(Spline s, Point p2){

		int indexdiff = s.SplinePoints.IndexOf (p2) - s.SplinePoints.IndexOf (curPoint);

		if (indexdiff == -1 || indexdiff > 1) {
			s.SetSelectedPoint(p2);
			goingForward = false;
			progress = 1;
			//curDirection = s.GetInitVelocity(p2, true);
		} else {
			progress = 0;
			goingForward = true;
			s.SetSelectedPoint(curPoint);
			//curDirection = s.GetInitVelocity(p2);
		}
	}

	public void SetPlayerAtEnd(Spline s, Point p2){
		int indexdiff = s.SplinePoints.IndexOf (p2) - s.SplinePoints.IndexOf (curPoint);

		if (indexdiff == -1 || indexdiff > 1) {
			s.SetSelectedPoint(p2);
			goingForward = true;
			progress = 0;

		} else {
			progress = 1;
			goingForward = false;
			s.SetSelectedPoint(curPoint);
		}

	}

	public bool TryLeavePoint()
	{
		float minAngle = Mathf.Infinity;
		float adjustedAngle = Mathf.Infinity;
		float actualAngle = Mathf.Infinity;
		
		bool isGhostPoint = curPoint.pointType == PointTypes.ghost;
		bool forward = true;

		if(!isGhostPoint && joystickLocked) return false;

		if (curPoint.HasSplines ()) {

			splineDest = null;
			pointDest = null;

			Point maybeNextPoint = null;
			Spline maybeNextSpline = null;

			foreach (Spline s in curPoint.GetSplines()) {

				float curAngle = Mathf.Infinity;

				if(s.state == Spline.SplineState.off){
					continue;
				}else{
				
				int curIndex = s.SplinePoints.IndexOf(curPoint);
				bool endPoint = curIndex == 0 || curIndex == s.SplinePoints.Count-1;
				bool intersection = curPoint.numActiveNeighbours() > 1;

				for(int i = -1; i < 2; i+=2){

					int nextIndex = curIndex + i;
					
					if(nextIndex < 0){
						if(!s.closed || s.SplinePoints.Count < 2) continue;
						nextIndex = s.SplinePoints.Count - 1;
					}
					
					if(nextIndex > s.SplinePoints.Count - 1){
						if(!s.closed || s.SplinePoints.Count < 2) continue;
						nextIndex = 0;
					}

					forward = i == 1;

					Point p = s.SplinePoints[nextIndex];

					int indexDifference = nextIndex - curIndex;
					bool diffSpline = s != curSpline;
					bool reversing = goingForward != forward && !diffSpline;
					bool canReverse = (endPoint && !s.closed && !intersection); //only double back if its a leaf
					
					//if we're not reversing, go on
					//if we're going in a diff direction on a diff spline, fine
					//if we're not a ghost point, sure
					//if we're a ghostpoint on a leaf, no way out
					bool canMove = !reversing || diffSpline || !isGhostPoint || canReverse;
					
					// indexDifference > 1 means we looped backwards
					// indexDifference == -1 means we went backward one point

					Vector2 p1 = Services.mainCam.WorldToScreenPoint(curPoint.Pos);
					Vector2 p2 = Services.mainCam.WorldToScreenPoint(p.Pos);
					Vector3 toPoint = (p2 - p1).normalized;
					float directAngle = Vector2.Angle(cursorDir, toPoint);
					bool tangent = false;

					Vector3 startdir = Vector3.zero;
					Vector3 pointOnLine = Vector3.zero;
					//how can I rewrite this to get a point some distance along the line?
					//this is screwing up when I use circles

					if(canMove){
						if (!forward) {
							
							//don't enter conveyor belts that will instantly push you back
							//if(!isGhostPoint && curSpeed < s.speed) continue;
							// p2 = s.GetPointAtIndex(nextIndex, 0.8f);
							pointOnLine = s.GetPointAlongLine(nextIndex, 0.8f, 0.2f, forward);	
							
						} else {
							
							// p2 = s.GetPointAtIndex(curIndex, 0.2f);
							pointOnLine = s.GetPointAlongLine(curIndex, 0.2f, 0.2f, forward);
						}
					}else{

						//ghost point intersections dont let you change direction
						//leave and continue
						continue;
					}

					Debug.DrawLine(transform.position, pointOnLine, Color.green);

					p2 = Services.mainCam.WorldToScreenPoint(pointOnLine);
					startdir = (p2 - p1).normalized;
					curAngle = Vector2.Angle(cursorDir, startdir);
					
					float angleToMomentum = Vector2.Angle(curDirection, startdir);

					//I really dont want players changing momentume at acute angles
					//on ghost points

					//only redirect themselves within reason...
					//this will have awful edge cases and I dont fucking care?

					// if(isGhostPoint && angleToMomentum > StopAngleDiff && intersection){
					// 	continue;
					// }

					if(Mathf.Abs(Vector3.Dot(startdir.normalized, Services.mainCam.transform.forward)) > 0.75f){
						tangent = true;
					}
					// adjustedAngle = (adjustedAngle + curAngle) / 2f;
					//adjustedAngle = screenAngle < curAngle ? screenAngle : curAngle;
					adjustedAngle = tangent ? directAngle : curAngle;
					
					if(tangent){
						Debug.DrawLine(pos, pos + Services.mainCam.transform.TransformDirection(toPoint)/3f, Color.blue);
					}else{
					//this is being used but is buggy
						Debug.DrawLine(pos, pos + Services.mainCam.transform.TransformDirection((Vector3)startdir)/3f, Color.magenta);
					}
					//this isn't being used, just for my own sanity
					Debug.DrawLine(pos ,pos + startdir.normalized, Color.black/5f);
					

					if (adjustedAngle < minAngle) {
						
						minAngle = adjustedAngle;
						actualAngle = curAngle;
						maybeNextSpline = s;
						maybeNextPoint = p;

						facingForward = forward;
						
					}
				}
			}
		}
			
			if(actualAngle < 180){
				// accuracy = (90 - actualAngle) / 90;
				//what the fuck is all this shit
				signedAccuracy = 1;
			}

			if ((actualAngle <= StopAngleDiff || isGhostPoint))// && maybeNextSpline != null) //why the fuck does it matter if it is null
			{
				
				splineDest = maybeNextSpline;
				pointDest = maybeNextPoint;


				if(facingForward){
					
					curDirection = splineDest.GetInitVelocity (curPoint, false);
				}else{
						
					curDirection = splineDest.GetInitVelocity (pointDest, true);
				}

				curDirection.Normalize();
				
				return true;
			}


			cursorRenderer.sprite = brakeSprite;

			return false;

		}
		return false;
	}

	public void CursorInput (InputAction.CallbackContext context){

		if(Services.main.state != GameState.playing){return;}
		
		Vector2 inputVector = context.ReadValue<Vector2>();

		if(context.control.name == "stick"){
				inputVector = Quaternion.Euler(0,0,-90) * inputVector;
		}

//		Vector3 lastCursorDir = cursorDir;
		if (context.control.name == "stick" || context.control.name == "leftStick") {

			//inputVector = new Vector3(Input.GetAxis ("Joy X"), Input.GetAxis ("Joy Y"), 0);
			if(inputVector.magnitude == 0) return;
			cursorDir2 = inputVector;
 
		}else {

			//inputVector = new Vector2(Input.GetAxis ("Mouse X"), Input.GetAxis ("Mouse Y"));
			
			cursorDir2 = cursorDir2 + inputVector;

		}

		if (cursorDir2.magnitude > 1) {
			cursorDir2.Normalize ();
		}

		//TODO WHEN ON CONNECTING POINT CURSOR SHOULD MOVE FROM CURRENT MAGNITUDE TO NEW MAGNITUDE

		if (freeCursor)
		{
			//TODO
//			clamp this shit

			if (context.control.name == "stick" || context.control.name == "Left Stick")
			{
				cursorPos += (Vector3)inputVector * cursorMoveSpeed * Time.deltaTime;
			}
			else
			{
				if (inputVector.magnitude > 1)
				{
					inputVector.Normalize();
				}
				cursorPos += (Vector3)inputVector;
			}

			Vector3 screenPos = Services.mainCam.WorldToViewportPoint(visualRoot.position);
			screenPos += new Vector3(cursorDir.x / Services.mainCam.aspect, cursorDir.y, 0)/CameraFollow.instance.cam.aspect;
			screenPos = new Vector3(Mathf.Clamp01(screenPos.x), Mathf.Clamp01(screenPos.y), Mathf.Abs(transform.position.z - Services.mainCam.transform.position.z));
			cursorPos = Services.mainCam.ViewportToWorldPoint(screenPos);

		}
		else
		{

			Vector3 screenPos = Services.mainCam.WorldToViewportPoint(visualRoot.position);
			screenPos += new Vector3(cursorDir.x / Services.mainCam.aspect, cursorDir.y, 0)/cursorDistance;
			screenPos = new Vector3(Mathf.Clamp01(screenPos.x), Mathf.Clamp01(screenPos.y), Main.cameraDistance);
			cursorPos = Services.mainCam.ViewportToWorldPoint(screenPos);
		}



		if (cursorDir2.magnitude < 0.2f){
			joystickLocked = true;
		}else{
			joystickLocked = false;
		}

		cursor.transform.position = cursorPos;
		cursor.transform.rotation = Quaternion.LookRotation(CameraFollow.forward, cursorDir);

		cursorDir = cursorDir2;

		if(buttonDown && state == PlayerState.Traversing)
		{
			//SwitchState(PlayerState.Flying);
		}
		else
		{
			//cursorDir = Vector3.Lerp (cursorDir, cursorDir2, (cursorRotateSpeed + flow) * Time.deltaTime);
			//cursorDir = cursorDir2;
		}
		
		// l.SetPosition(0,pos);
		// l.SetPosition(1, cursorPos);
	}


	public float GetFlow(){
		return flow;
	}

	public void AddFlow(float x){
		flow += x;
	}

	public void SetFlow(float x){
		flow = x;
	}

	public float GetProgress(){
		return progress;
	}

	public void SetProgress(float f){
		progress = f;
	}

	void LeaveState()
	{
		switch (state)
		{
			case PlayerState.Traversing:
				//this isn't accurate, its called on ghost points
				break;

			case PlayerState.Flying:

				hasFlown = true;
				Services.fx.BakeTrail(Services.fx.flyingTrail, Services.fx.flyingTrailMesh);

				if(OnStoppedFlying != null){
					OnStoppedFlying.Invoke();
				}

				//I dont really want the player to gain speed by flying
				//this is causing them to convert boost into flow by jumping between points

				//refund lost speed as boost
				if(flyingSpeed < flow){
					flow = flyingSpeed;
					boost = 0;
				}else{
					float leftoverSpeed = flyingSpeed;
					leftoverSpeed -= flow;
					boost = Mathf.Clamp(leftoverSpeed, 0, maxSpeed);
				}

				Services.fx.flyingParticles.Pause();

				//Services.fx.BakeParticleTrail(Services.fx.flyingParticles, Services.fx.flyingParticleTrailMesh);

				//Services.fx.BakeParticles(Services.fx.flyingParticles, Services.fx.flyingParticleMesh);

				break;

			case PlayerState.Switching:
				l.positionCount = 0;
				freeCursor = false;
				foundConnection = false;

				if (traversedPoints.Count >= 2 && (curPoint.pointType == PointTypes.start || curPoint.pointType == PointTypes.end || curPoint.pointType == PointTypes.fly))
				{
					List<Point> pointsToTraverse = new List<Point>();
					foreach (Point p in traversedPoints)
					{
						pointsToTraverse.Add(p);
					}

					//Services.main.crawlerManager.AddCrawler(pointsToTraverse, calculatedSpeed);

					traversedPoints.Clear();

					if (curPoint.pointType != PointTypes.fly)
					{
						traversedPoints.Add(curPoint);
					}
				}

				if(curPoint.pointType != PointTypes.ghost){

					 // * (1 + Services.PlayerBehaviour.boostTimer);
				
						
					
					if (buttonWasPressed)
					{
						if(boost < Point.boostAmount) {
							boost = (Point.boostAmount + boostTimer)/2f;
						}

						buttonDownTimer = 0;
						
						Services.fx.PlayAnimationOnPlayer(FXManager.FXType.fizzle);
						Services.fx.EmitRadialBurst(10,boostTimer + 1, curPoint.transform);
						Services.fx.EmitLinearBurst(5, boostTimer + 1, curPoint.transform, -cursorDir);
					}

					charging = false;
					boostIndicator.enabled = false;

					Services.fx.EmitLinearBurst((int)(boostTimer * 5), boostTimer * 2,transform, cursorDir2);
					boostTimer = 0;

					
					curPoint.OnPlayerExitPoint();
				}
			

				connectTime = 1;

				break;

			case PlayerState.Animating:
				glitching = false;
				cursorRenderer.enabled = true;
				break;
		}
	}

	public void SwitchState(PlayerState newState)
	{
		LeaveState();

		if (Services.main.hasGamepad)
		{
			Services.main.gamepad.ResetHaptics();
		}

		switch (newState)
		{
			case PlayerState.Traversing:

				state = PlayerState.Traversing;
				
				Services.fx.playerTrail.emitting = true;

				bool enteredNewSpline = false;

				if(curSpline != null){
					if (curSpline != splineDest){
						curSpline.OnSplineExit();

						if(OnExitSpline != null){
							OnExitSpline.Invoke();
						}

						enteredNewSpline = true;
					}
				}else{
					enteredNewSpline = true;
				}
				
				foreach (Spline s in pointDest._connectedSplines)
				{
					s.reactToPlayer = true;
				}
				
				curSpline = splineDest;
				SetPlayerAtStart (curSpline, pointDest);
				curSpline.CalculateSegmentDistance();
				curSpline.isPlayerOn = true;

				if(enteredNewSpline) {
					splineDest.OnSplineEnter();
					if(OnEnterSpline != null){
						OnEnterSpline.Invoke();
					}
				}

				if (curPoint.pointType != PointTypes.ghost)
				{
					if(OnExitPoint != null){
						OnExitPoint.Invoke();
					}
				}
				
				break;

			case PlayerState.Flying:

				if(OnStartFlying != null){
					OnStartFlying.Invoke();
				}

				stopFlying = false;
				Services.fx.BakeTrail(Services.fx.playerTrail, Services.fx.playerTrailMesh);

				flyingTrail.Clear();

				noRaycast = true;
				curPoint.usedToFly = true;
				pointDest = null;
				l.positionCount = 0;

				flyingSpeed = Mathf.Clamp(flow + boost, 0, maxSpeed);

				//THIS MAY NOT BE NECESSARY UNLESS WE CAN FLY OFF OF SPLINES, NOT JUST POINTS
				//curPoint.OnPointExit();

				curPoint.proximity = 0;
				pointDest = null;

				state = PlayerState.Flying;
				flyingTrail.emitting = true;
				
				if(OnExitSpline != null){
					OnExitSpline.Invoke();
				}
				
				if(curSpline != null){
					curSpline.OnSplineExit();
				}

				foreach(Spline s in curPoint._connectedSplines){
					s.reactToPlayer = false;
				}

				curSpline = null;
				

				// if (StellationManager.instance != null)
				// {
				// 	StellationManager.instance.CompleteStellation();
				// }

				break;

			case PlayerState.Switching:

				//stop players from popping off the line as soon as they enter a point

				decelerationTimer = 0;

				// directionIndicator.enabled = false;

				state = PlayerState.Switching;

				timeOnPoint = 0;

				if (curPoint == null)
				{
					//this should never happen 

				}

				// foreach (Spline s in curPoint._connectedSplines)
				// {
				// 	s.reactToPlayer = false;
				// }

				lastPoint = curPoint;

				//is there really any reason to do this?
				// foreach (Spline s in pointDest._connectedSplines)
				// {
				// 	s.SetSelectedPoint(pointDest);
				// }

				if (curPoint != pointDest){
					traversedPoints.Add(pointDest);
				}

				
				curPoint = pointDest;

				if(curPoint.pointType != PointTypes.ghost){
					
					Services.fx.BakeParticles(sparks, Services.fx.brakeParticleMesh);
					Services.fx.playerTrail.emitting = false;

					//store current speed
					// flow = curSpeed;
					// boost = 0;
					
					if(OnEnterPoint != null){
						OnEnterPoint.Invoke();
					}
				
					if (Services.main.hasGamepad)
					{
						Services.main.gamepad.ResetHaptics();
					}
				
				}
			
				
				curPoint.OnPlayerEnterPoint();

				//SPLINE IS NULL WHEN YOU ARE FLYING, THIS SUCKS
				if (curSpline != null)
				{
					curSpline.CheckComplete();
				}


				//can we check for completeness here please

				if (curPoint.controller.isComplete)
				{
					curPoint.controller.ShowEscape();
				}

				//checkpoint shit
				if (curPoint.pointType == PointTypes.stop)
				{
//				traversedPoints.Clear();
//				traversedPoints.Add(curPoint);
				}
				

				if(!curPoint.controller.won){
					PlayerOnPoint();
				}

				break;

			case PlayerState.Animating:

//				GranularSynth.rewinding.TurnOn();
//				//turn off particles
//				

				sparkEmission.rateOverTimeMultiplier = 0;
				boostIndicator.enabled = false;
				glitchFX.enabled = false;
				renderer.enabled = false;
				cursorRenderer.enabled = false;
				state = PlayerState.Animating;

				break;
		}
	}


	public void Effects()
	{


		float Absflow = Mathf.Abs(flow);

		if (state == PlayerState.Flying)
		{

		}
		else
		{
			
			if (state != PlayerState.Switching)
			{
				sparkEmission.rateOverTimeMultiplier = Mathf.Pow(easedDistortion, 2) * 250 * curSpeed;
			}else{
				sparkEmission.rateOverTimeMultiplier= 0;
			}

		}
	}

	public Vector3 GetCursorDir(){
		return cursorDir;
	}

	public Vector3 GetCursorVelocity(){
		return cursorPos -pos;
	}
}
