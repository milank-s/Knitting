using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Vectrosity;
using UnityEngine.InputSystem;

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
	public float speed;
	public float acceleration;
	public AnimationCurve accelerationCurve;
	public float decay;
	public float maxSpeed = 10;
	public float flyingSpeedDecay = 1;
	public float accuracyCoefficient;
	public float flowAmount = 0.1f;
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
	public Vector2 screenSpaceDir;
	[HideInInspector]
	public Vector3 deltaDir;
	[HideInInspector]
	public float deltaAngle;

	public float potentialSpeed => flow + speed + boost;
	public float easedAccuracy;
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
	public bool joystickLocked;
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
		if(Services.main.state != Main.GameState.playing && !MapEditor.editing){return;}

		Services.fx.PlayAnimationOnPlayer(FXManager.FXType.glitch);

		if (Services.main.state == Main.GameState.playing)
		{
			Services.main.WarpPlayerToNewPoint(Services.main.activeStellation.start);
			Reset();
			speed = Services.main.activeStellation.startSpeed;
			flyingSpeed = 0;
		}
	}
	public void Initialize()
	{
		
		cursorDistance = minCursorDistance;
		curPoint = Services.StartPoint;
		transform.position = curPoint.Pos;
		cursorPos = pos;
		traversedPoints.Add (curPoint);

		curPoint.OnPlayerEnterPoint();

		speed = Services.main.activeStellation.startSpeed;
		acceleration = Services.main.activeStellation.acceleration;
		maxSpeed = Services.main.activeStellation.maxSpeed;

		foreach(Spline s in curPoint._connectedSplines){
			s.SetSelectedPoint(curPoint);
		}

		//CameraFollow.instance.WarpToPosition(curPoint.Pos);

		ResetFX();
	}

	public void Reset()
	{
		
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
		//if button down, buttonDown = true;
		//if button up, buttonDown = false

		if(Services.main.state != Main.GameState.playing){return;}

		if(context.performed){
			buttonDown = true;
		}

		if(context.canceled){
			buttonUp = true;
			buttonDown = false;
			charging = false;
			if (state == PlayerState.Traversing)
			{
				boostTimer = 0;
			}
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
		glitchFX.transform.LookAt(CameraFollow.instance.pos);

		if(curDirection.sqrMagnitude > 0){
			visualRoot.rotation = Quaternion.LookRotation(curDirection, CameraFollow.forward);
		}
		
		pos = transform.position;

		Vector2 p1 = Services.mainCam.WorldToScreenPoint(pos);
		Vector2 p2 = Services.mainCam.WorldToScreenPoint(pos + curDirection);

		screenSpaceDir = (p2 - p1).normalized;

		Debug.DrawLine(pos, pos + Services.mainCam.transform.TransformDirection(cursorDir)/5f, Color.cyan);
		Debug.DrawLine(pos, pos + Services.mainCam.transform.TransformDirection(screenSpaceDir)/2f, Color.yellow);

		//curspeed going to zero making movement asymptotes
		
		//set all your important floats bruh
		easedAccuracy = Mathf.Clamp01(Mathf.Pow(Mathf.Clamp01(signedAccuracy), accuracyCoefficient));
		easedDistortion = Mathf.Lerp(easedDistortion, Mathf.Pow(1 - easedAccuracy, 2f) + Spline.shake, Time.deltaTime * 5);

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
			cursorRenderer.enabled = false;
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
			// directionIndicator.enabled = true;
			boostIndicator.transform.position =pos + (Vector3) cursorDir2 * ((Vector3)transform.position - cursorPos).magnitude;
			boostIndicator.transform.rotation = Quaternion.LookRotation(CameraFollow.forward, cursorDir2);

			if(charging && state != PlayerState.Switching){
				boostTimer += Time.deltaTime;
				boostTimer = Mathf.Clamp01(boostTimer);
			}

			buttonDown = true;
			charging = true;
			//buttonDownTimer = buttonDownBuffer;
		}
		else
		{
			boostIndicator.enabled = false;
		}

		boostIndicator.transform.localScale = Vector3.Lerp(Vector3.one * 0.2f, Vector3.one , boostTimer);

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
		boost = Mathf.Lerp(boost, 0, Time.deltaTime * 2f);
		boost = Mathf.Clamp(boost, 0, maxSpeed);

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

			UpdateProgress(curSpeed * Time.deltaTime);
			UpdatePositionOnSpline();
			CheckProgress ();

		}
		//else if? should happen all on same frame?
		if(state == PlayerState.Switching)
		{
			transform.position = curPoint.Pos;
			visualRoot.position = Vector3.Lerp(visualRoot.position,pos,Time.deltaTime * 10f);
			
			//gravity = 0;
			
			PlayerOnPoint();
		}

		//we arent even using this anymore
		if (state != PlayerState.Animating && state != PlayerState.Flying && curPoint.HasSplines () && curSpline != null) {

			//curSpline.UpdateSpline();
//			ManageSound();

			//old reset button
			if (flow <= 0.01f && state == PlayerState.Traversing) {
				// && Mathf.Abs (flow) <= 0)
				//SwitchState(PlayerState.Animating);
			}
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
			if(curSpline.speed > 0 && goingForward){
				return Mathf.Clamp(flow + boost + speed, 0, maxSpeed);
			}else{
				return flow + boost + speed; //* easedAccuracy + boost; //* cursorDir.magnitude;
			}
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

				//why here
				// if (curSpline != null)
				// {
				// 	curSpline.SetSelectedPoint(curPoint);
				// }
			}
			else
			{
				
				bool newSplineSelected = prevSplineDest == null || splineDest != prevSplineDest;

				if (newSplineSelected){
					Services.fx.ShowSplineDirection(splineDest);
				}

				if( pointDest.pointType != PointTypes.ghost)
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
//			else if (curPoint.pointType == PointTypes.end)
//			{
//				if (buttonUp)
//				{
////					curPoint.OnPointExit();
////					SwitchState(PlayerState.Animating);
//				}
//			}
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
//				SwitchState(PlayerState.Animating);
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
		
		if(Services.main.state != Main.GameState.playing){return;}

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

		if(buttonDown && !freeCursor && (pointDest != null || curPoint.pointType == PointTypes.fly && state != PlayerState.Flying)){
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
		}else{
			
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

	void FreeMovement()
	{
		Point raycastPoint = SplineUtil.RaycastFromCamera(cursorPos, 5f);

		Vector3 viewportPoint = Services.mainCam.WorldToViewportPoint(transform.position);
		if(viewportPoint.x < 0 || viewportPoint.x > 1 || viewportPoint.y > 1 || viewportPoint.y < 0){
			Services.main.ResetLevel();
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
				hasFlown = true;
				Services.fx.BakeTrail(Services.fx.flyingTrail, Services.fx.flyingTrailMesh);
				buttonDownTimer = 0;
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
					Services.main.ResetLevel();
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

		float splineSpeed = curSpline.speed;
		float speedGain = easedAccuracy * 2 - 1;
		
		// float gravityCoefficient = Mathf.Clamp01(-curDirection.y);
		// float gravityPull = -curDirection.y - curDirection.z;
		
		bool onBelt = splineSpeed > 0;
		
		if(onBelt){
			if(goingForward){
			
				easedDistortion = 0;
				easedAccuracy = 1;

				speedGain = 0;
				
				if(boost < splineSpeed){
					boost = splineSpeed;
				}	
				
			}else{
				
				speedGain = Mathf.Clamp(speedGain, -maxSpeed, 0);
				flow -= splineSpeed/2f  * Time.deltaTime;		
			}
		}else{
			
			flow = Mathf.Clamp(flow, 0, maxSpeed);
			// speedGain = Mathf.Clamp(speedGain, -maxSpeed, 0);
		}
		
		if(speedGain >= 0){
			flow += speedGain * acceleration * Time.deltaTime * accelerationCurve.Evaluate(flow/maxSpeed);// * gravityCoefficient;
		}else{
			flow += speedGain * decay * Time.deltaTime;
		}

		
		//flow -= Mathf.Clamp01(-gravityPull) * Time.deltaTime;

		if(onBelt && !goingForward && curSpeed <= Mathf.Epsilon){
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

	void UpdateProgress(float distanceToTravel){
		
		int curSegment = goingForward ? (int)Mathf.Ceil((float)Spline.curveFidelity * progress) : (int)Mathf.Floor((float)Spline.curveFidelity * progress);

		Vector3 curPos = pos;
		float rollingDistance = 0;
		float prevStep = progress;

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

	//MAKE SURE THAT YOU CAN STILL PLACE POINTS WHILE NOT FLYING OFF THE EDGE
	//DONT CONFUSE FLYING WITH

	public bool TryLeavePoint()
	{
		float minAngle = Mathf.Infinity;
		float adjustedAngle = Mathf.Infinity;
		float actualAngle = Mathf.Infinity;
		
		bool isGhostPoint = curPoint.pointType == PointTypes.ghost;
		bool forward = true;

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

					if(canMove){
						if (!forward) {
							
							//don't enter conveyor belts that will instantly push you back
							//it's a little janky but better than re-entering the same point every frame
							if(!isGhostPoint && curSpeed < s.speed/2f) continue;
							
							p2 = s.GetPointAtIndex(nextIndex, 0.8f);	
							
						} else {
							
							p2 = s.GetPointAtIndex(curIndex, 0.2f);
						}
					}else{

						//ghost point intersections dont let you change direction
						//leave and continue
						continue;
					}
					Debug.DrawLine(transform.position, p2, Color.green);

					p2 = Services.mainCam.WorldToScreenPoint(p2);
					startdir = (p2 - p1).normalized;
					curAngle = Vector2.Angle(cursorDir, startdir);
					
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

		if(Services.main.state != Main.GameState.playing){return;}
		
		Vector2 inputVector = context.ReadValue<Vector2>();

		// if(context.control.name == "stick"){
		// 		inputVector = Quaternion.Euler(0,0,90) * inputVector;
		// }

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



		if (cursorDir2.magnitude < 0.9f){
			joystickLocked = true;
		}else{
			joystickLocked = false;
		}

		cursor.transform.position = cursorPos;
		cursor.transform.rotation = Quaternion.LookRotation(CameraFollow.forward, cursorDir);


		if(buttonDown && state == PlayerState.Traversing)
		{
			//SwitchState(PlayerState.Flying);
		}
		else
		{
			//cursorDir = Vector3.Lerp (cursorDir, cursorDir2, (cursorRotateSpeed + flow) * Time.deltaTime);
			cursorDir = cursorDir2;
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
				Services.fx.BakeParticles(sparks, Services.fx.brakeParticleMesh);
				Services.fx.playerTrail.emitting = false;

				//this isn't accurate, its called on ghost points

				//turn on sparks
				break;

			case PlayerState.Flying:

				if(OnStoppedFlying != null){
					OnStoppedFlying.Invoke();
				}

				//I dont really want the player to gain speed by flying

				if(flow > flyingSpeed - speed){			
					flow = Mathf.Clamp(flyingSpeed - speed, 0, 1000);
				}
		

				Services.fx.flyingParticles.Pause();

				boost = 0;
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

				curPoint.OnPointExit();

				if(curPoint.pointType != PointTypes.ghost){

					boost += Point.boostAmount * (1 + Services.PlayerBehaviour.boostTimer);
					Services.fx.SpawnCircle(curPoint.transform);
					
					if (buttonWasPressed)
					{
						buttonDownTimer = 0;
						
						Services.fx.PlayAnimationOnPlayer(FXManager.FXType.fizzle);
						Services.fx.EmitRadialBurst(10,boostTimer + 1, curPoint.transform);
						Services.fx.EmitLinearBurst(5, boostTimer + 1, curPoint.transform, -cursorDir);
					}

					charging = false;
					boostIndicator.enabled = false;

					Services.fx.EmitLinearBurst((int)(boostTimer * 5), boostTimer * 2,transform, cursorDir2);
					boostTimer = 0;
				}

				connectTime = 1;

				break;

			case PlayerState.Animating:

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
				curSpline.CalculateDistance();
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

				flyingSpeed = flow + speed;

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
				cursorRenderer.enabled = false;
				state = PlayerState.Animating;

				break;
		}
	}


	public void Effects()
	{

		ParticleSystem.EmissionModule e = sparks.emission;

		float Absflow = Mathf.Abs(flow);

		if (state == PlayerState.Flying)
		{

		}
		else
		{
			
			if (state != PlayerState.Switching)
			{
				e.rateOverTimeMultiplier = Mathf.Pow(1-easedAccuracy, 2) * 20 * potentialSpeed;
			}else{
				e.rateOverTimeMultiplier= 0;
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
