
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Vectrosity;
using UnityEngine.InputSystem;

public enum PlayerState{Traversing, Switching, Flying, Animating};

//###################################################
//###################################################

//						TO DO


//###################################################
//###################################################


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
	public StateChange OnStartTraversing;
	public StateChange OnLeaveAnyPoint;
	public StateChange OnEnterAnyPoint;
	public StateChange OnTraversing;
	public StateChange OnFlying;
	public StateChange OnStoppedFlying;
	public StateChange OnStoppedTraversing;
	[HideInInspector] public float progress, adjustedProgress,
		accuracy,
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

	public float normalizedAccuracy => (1 + accuracy)/2f;
	public float potentialSpeed => flow + speed + boost;
	public float easedAccuracy => Mathf.Clamp01(Mathf.Pow(Mathf.Clamp01(accuracy), accuracyCoefficient));
	public float actualSpeed
	{
		get
		{
			//lets just stop using the deceleration timer
			//adjustedAccuracy = (adjustedAccuracy * (1-decelerationTimer));

		
			if(state == PlayerState.Flying){
				return flyingSpeed;
			}

			if(state == PlayerState.Traversing){
			// return (speed) * cursorDir.magnitude * easedAccuracy + flow + boost;
				if(curSpline.speed == 0){
					return speed + flow * cursorDir.magnitude * easedAccuracy + boost;
				}else{
					return Mathf.Clamp(flow + boost, 0, maxSpeed); //(curSpline.speed * (goingForward ? 1 : -1));
				}
			}

			return speed + flow * cursorDir.magnitude * easedAccuracy + boost;
		}
	}

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
	private List<Point> traversedPoints;
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
	public TrailRenderer t;
	public TrailRenderer flyingTrail;
	public TrailRenderer shortTrail;
	public ParticleSystem sparks;
	private LineRenderer l;
	public SpriteRenderer cursorSprite;
	public SpriteRenderer playerSprite;
	public SpriteRenderer boostIndicator;
	public SpriteRenderer directionIndicator;
	private int noteIndex;
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
		accuracy = 0;
		pointDest = null;
		traversedPoints = new List<Point> ();

		connectTimeCoefficient = 1;
		state = PlayerState.Switching;
		l = GetComponent<LineRenderer> ();
		t = GetComponentInChildren<TrailRenderer> ();

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
		cursorPos = transform.position;
		traversedPoints.Add (curPoint);
		curPoint.OnPlayerEnterPoint();
		flow = 0;
		speed = Services.main.activeStellation.startSpeed;
		acceleration = Services.main.activeStellation.acceleration;
		maxSpeed = Services.main.activeStellation.maxSpeed;
	
		foreach(Spline s in curPoint._connectedSplines){
			s.SetSelectedPoint(curPoint);
		}
		//CameraFollow.instance.WarpToPosition(curPoint.Pos);

		ResetFX();
		

//		Material newMat;
//		newMat = Services.Prefabs.lines[3];
//		Texture tex = newMat.mainTexture;
//		float length = newMat.mainTextureScale.x;
//		float height = newMat.mainTextureScale.y;
//
//		velocityLine = new VectorLine (name, new List<Vector3> (10), height, LineType.Discrete, Vectrosity.Joins.Weld);
//		velocityLine.color = Color.black;
//		velocityLine.smoothWidth = true;
//		velocityLine.smoothColor = true;
//
//		velocityLine.texture = tex;
//		velocityLine.textureScale = newMat.mainTextureScale.x;
//
//		newMat = Services.Prefabs.lines[3];
//		tex = newMat.mainTexture;
//		length = newMat.mainTextureScale.x;
//		height = newMat.mainTextureScale.y;
//
//		velocityLine2 = new VectorLine (name, new List<Vector3> (30), height, LineType.Discrete, Vectrosity.Joins.Weld);
//		velocityLine2.color =  Color.black;
//		velocityLine2.smoothWidth = true;
//		velocityLine2.smoothColor = true;
//
//		velocityLine2.texture = tex;
//		velocityLine2.textureScale = newMat.mainTextureScale.x;
	}

	public void Reset()
	{
		
		if (Services.main.hasGamepad)
		{
			Services.main.gamepad.ResetHaptics();
		}

		cursorDir = Vector3.zero;
		cursorDir2 = Vector3.zero;
		cursorPos = transform.position;
		cursorSprite.enabled = true;
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
		t.Clear();
		flyingTrail.Clear();
		shortTrail.Clear();
		flyingTrail.emitting = false;
		t.emitting = true;
		Services.fx.cursorTrail.Clear();
	}

	public IEnumerator RetraceTrail()
	{
		cursorSprite.sprite = null;

		Vector3[] positions = new Vector3[flyingTrail.positionCount];
		flyingTrail.GetPositions(positions);
		float f = 0;
		float lerpSpeed = 0;
		float distance;

//		Point p = SplineUtil.CreatePoint(transform.position);
//		p.pointType = PointTypes.connect;
//		p.Initialize();

		for (int i = positions.Length -1; i >= 0; i--)
		{
			float temp = 0;
			Vector3 tempPos = transform.position;
			distance = Vector3.Distance(tempPos, positions[i]);
			while (temp < 1)
			{
				transform.position = Vector3.Lerp(tempPos, positions[i], temp);
//				transform.position = positions[i];
				temp += (Time.deltaTime * lerpSpeed)/distance;
				lerpSpeed += Time.deltaTime;
				f = Mathf.Clamp01(lerpSpeed);
				//play drone music or whatever
				yield return null;
			}

			//bake the mesh out after this and copy it before clearing the trail renderer
		}

		Services.fx.BakeTrail(Services.fx.flyingTrail, Services.fx.flyingTrailMesh);

		state = PlayerState.Switching;
		curSpeed = lerpSpeed;
		StartCoroutine(Unwind());
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
			directionIndicator.enabled = false;
		}

	}

	public void Step()
	{
		pos = transform.position;
		Vector2 p1 = Services.mainCam.WorldToScreenPoint(transform.position);
		Vector2 p2 = Services.mainCam.WorldToScreenPoint(transform.position + curDirection);

		screenSpaceDir = (p2 - p1).normalized;

		Debug.DrawLine(transform.position, transform.position + Services.mainCam.transform.TransformDirection(cursorDir)/5f, Color.cyan);
		Debug.DrawLine(transform.position, transform.position + Services.mainCam.transform.TransformDirection(screenSpaceDir)/2f, Color.yellow);

		if(state == PlayerState.Traversing){
			glitching = accuracy < 0 || joystickLocked;
		}else if(state == PlayerState.Switching){
			glitching = !canTraverse;
		}else{
			glitching = false;
		}

		playerSprite.enabled = !glitching;
		glitchFX.enabled = glitching;

		if (joystickLocked)
		{
			cursorSprite.enabled = false;
			//show glitch effect if we're not on a point 

		}
		else
		{
			//hide glitch effect
			cursorSprite.enabled = true;
		}

		Point.hitColorLerp = connectTime;


		if (buttonDown)
		{
			boostIndicator.enabled = true;
			directionIndicator.enabled = true;
			boostIndicator.transform.position = transform.position + (Vector3) cursorDir2 * ((Vector3)transform.position - cursorPos).magnitude;
			boostIndicator.transform.up = cursorDir2;

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
		buttonDownTimer -= Time.deltaTime;

		float speedCoefficient;
		if(state == PlayerState.Switching || state == PlayerState.Animating){
			speedCoefficient = 0;
		}else if (state == PlayerState.Flying){
			speedCoefficient = flyingSpeed;
		}else{
			speedCoefficient = Mathf.Clamp01(Mathf.Pow(accuracy, 5) * actualSpeed + 0.25f);
		}

		if (joystickLocked)
		{
			speedCoefficient = 0;
		}

		playerSprite.transform.localScale = Vector3.Lerp(playerSprite.transform.localScale, new Vector3(Mathf.Clamp(1 - (speedCoefficient * 2), 0.15f, 0.25f), Mathf.Clamp(speedCoefficient, 0.25f, 0.5f), 0.25f), Time.deltaTime * 10);

//		if (connectTime <= 0 && PointManager._pointsHit.Count > 0) {
//			PointManager.ResetPoints ();
//			connectTime = 1;
//		}

		Effects ();

		if (state == PlayerState.Flying) {
			if(OnFlying != null){
				OnFlying.Invoke();
			}
			FreeMovement ();
			cursorSprite.sprite = canFlySprite;
			return;
		}

		boost = Mathf.Lerp(boost, 0, Time.deltaTime * 2f);
		
		if (boost < 0)
		{
			boost = 0;
		}

		if (state == PlayerState.Traversing) {
			if(curSpline != null){
				//accuracy = GetAccuracy(progress);
				float maxAcc = -100;
				// for(int i = 0; i < 1; i++){
				// 	float sign = goingForward ? 1 : -1;
				// 	float curAcc = GetAccuracy(progress + (float)i * sign * 0.05f);
				// 	if(curAcc > maxAcc){
				// 		maxAcc = curAcc;
				// 	}
				// }

				maxAcc = GetAccuracy(progress);
				//Code for placing player sprite on the line instead of the spline

				//find out the real position on the cursplines line
				int curStep = curSpline.SplinePoints.IndexOf(curPoint) * (Spline.curveFidelity) + (int)(Spline.curveFidelity * progress);
				int lastStep = goingForward ? -1 : 1;
				lastStep = curStep + lastStep;
				int upperBound = curSpline.line.points3.Count-1;
        		Vector3 spriteDest1 = curSpline.line.points3[Mathf.Clamp(lastStep, 0, upperBound)];
        		Vector3 spriteDest2 = curSpline.line.points3[Mathf.Clamp(curStep, 0, upperBound)];

       		 	float p = Spline.curveFidelity * progress;
        		float step = Mathf.Floor(p);
        		float diff = goingForward ? p - step : step - p;
				//playerSprite.transform.up = (spriteDest2 - spriteDest1);

				accuracy = maxAcc;

				transform.position = curSpline.GetPointForPlayer(progress);
        		//playerSprite.transform.position = Vector3.Lerp(spriteDest1, spriteDest2, diff);

				if(OnTraversing != null){
					OnTraversing.Invoke();
				}
				
			}
			
			PlayerMovement ();
			CheckProgress ();

		if(Mathf.Abs(flow) < 1){

			Services.fx.drawGraffiti = false;
			cursorSprite.sprite = traverseSprite;

		 }else if (Mathf.Abs(flow) < 2){

			//Services.fx.DrawLine();
			 //cursorSprite.sprite = canMoveSprite;
		 }else
			{
			 //cursorSprite.sprite = canFlySprite;
		 }


		}
		//else if? should happen all on same frame?
		if(state == PlayerState.Switching && curPoint != null)
		{
			transform.position = curPoint.Pos;
			playerSprite.transform.position = Vector3.Lerp(playerSprite.transform.position, transform.position,Time.deltaTime * 10f);
			//gravity = 0;
			//this could be fucking with
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

		if (TryLeavePoint()) // && !foundConnection)
		{
			cursorSprite.sprite = traverseSprite;
			hasPath = true;

			if (curPoint.pointType == PointTypes.ghost)
			{
				canTraverse = true;

				if (curSpline != null)
				{
					curSpline.SetSelectedPoint(curPoint);
				}
			}
			else
			{

				bool newPointSelected = prevPointDest == null || prevPointDest != pointDest;

				if (newPointSelected){
					// Services.fx.ShowSplineDirection(curSpline);
				}

				if( pointDest.pointType != PointTypes.ghost)
				{

					Services.fx.ShowNextPoint(pointDest);

					if (newPointSelected)
					{
						Services.fx.PlayAnimationAtPosition(FXManager.FXType.pulse, pointDest.transform);
					}

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

			if (CanConnectFromPoint(curPoint))
			{
				//freeCursor = true;
				
				if (FindPointToConnect())
				{		
					
			 		foundConnection = true;
					cursorSprite.sprite = traverseSprite;
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
					cursorSprite.sprite = canFlySprite;
					if (buttonWasPressed)
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

	public bool CanConnectFromPoint(Point p){
		
		if(p.pointType == PointTypes.connect){
			return true;
		}else{
			if(p.NeighbourCount() == 0 && p.pointType != PointTypes.reset && p.pointType != PointTypes.fly && p.pointType != PointTypes.ghost){
				return true;
			}
		}

		return false;
	}

	public float GetAccuracy(float prog){
		prog = Mathf.Clamp01(prog);
		
		Vector3 newDirection = curSpline.GetDirection (prog);
		if(!goingForward){newDirection = -newDirection;}
		
		deltaAngle = Vector3.SignedAngle(newDirection, curDirection, Vector3.up);
		deltaDir = newDirection - curDirection;
		curDirection = newDirection;
		
		//Debug.DrawLine(transform.position, transform.position + splineDir, Color.red);
		
		//might be best to remove the z
		//splineDir.z = 0;

		
		//Debug.DrawLine(transform.position, transform.position + curDirection, Color.yellow);

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

		if(curSpline != null && spp.s != curSpline){
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
			l.SetPosition (1, transform.position);
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

		curSpeed = 0;

		decelerationTimer = Mathf.Lerp(decelerationTimer, 0, Time.deltaTime * 2);
		timeOnPoint += Time.deltaTime;

		if(buttonDown && !freeCursor && (pointDest != null || curPoint.pointType == PointTypes.fly)){
			boostTimer += Time.deltaTime / stopTimer;
			boostIndicator.enabled = true;
		}else{
			boostTimer -= Time.deltaTime;
			boostIndicator.enabled = false;
		}

		boostTimer = Mathf.Clamp01(boostTimer);

		progress = 0;
		curPoint.PlayerOnPoint(cursorDir, flow);

		//l.SetPosition (0, Vector3.Lerp(transform.position, cursorPos, Easing.QuadEaseOut(boostTimer)));

		if(foundConnection){
			l.positionCount = 2;
			l.SetPosition(0, pointDest.Pos);
			l.SetPosition (1, transform.position);
		}else{
			
			l.positionCount = 0;
		}

		
		playerSprite.transform.localScale = Vector3.Lerp(playerSprite.transform.localScale, new Vector3(Mathf.Clamp(1 - (boostTimer), 0.1f, 0.25f), Mathf.Clamp(boostTimer, 0.25f, 0.75f), 0.25f), Time.deltaTime * 10);

		connectTime -= Time.deltaTime * connectTimeCoefficient;
//		if (connectTime < 0) {
//			if (flow > 0) {
//				flow -= decay * Time.deltaTime;
//				if (flow < 0) {
//					flow = 0;
//				}
//			} else if (flow < 0) {
//				flow += decay * Time.deltaTime;
//				if (flow > 0) {
//					flow = 0;
//				}
//			}
//		}
	}

	public IEnumerator ReturnToLastPoint(){

		state = PlayerState.Animating;
		float t = 0;
		bool moving = true;
		float flowMult = 1;


			if (drawnPoint== curSpline.Selected) {
				goingForward = false;
			} else {
				goingForward = true;
			}

			while (moving) {
				t += Time.deltaTime;
				if(Mathf.Abs(flow) > 1){
					flowMult = Mathf.Abs(flow);
				}

				if (goingForward) {
					progress += (Time.deltaTime * t * flowMult) / curSpline.segmentDistance;
				} else {
					progress -= (Time.deltaTime * t  * flowMult) / curSpline.segmentDistance;
				}

				transform.position = curSpline.GetPointForPlayer (progress);

				if (progress > 1 || progress < 0) {
					moving = false;
				}
				yield return null;
			}



// 		while(index < newPointList.Count -1){
//
// 			speed += Time.deltaTime/10;
// 			t += Time.deltaTime * ((index + 2)/2);
// //			Vector3 lastPos = transform.position;
// //			transform.position = Vector3.Lerp (newPointList[index].position, newPointList [index - 1].position, t);
// 			Transform curJoint = newPointList[newPointList.Count - 1 - index];
// 			curJoint.position = Vector3.Lerp(curJoint.position, curPoint.Pos, t);
// //			float curDistance = Vector3.Distance (newPointList [index].position, transform.position);
// 			transform.position = newPointList[0].transform.position;
// //			sprite.transform.up = transform.position - lastPos;
// 			l.SetPosition (0, transform.position);
//
// 			for(int i = 1; i <= newPointList.Count - index; i++){
// 				l.SetPosition(i, newPointList[i-1].position);
// 			}
//
// 			l.SetPosition (newPointList.Count - index + 1, curPoint.Pos);
//
//
// 			if (t >= 1) {
// 				GameObject toDestroy = newPointList [newPointList.Count - 1 - index].gameObject;
// 				Destroy (toDestroy);
//
// 				index++;
// 				l.positionCount = newPointList.Count - index + 2;
//
// 				if (index >= newPointList.Count - 1) {
// 					Destroy (newPointList [newPointList.Count - 1 - index].gameObject);
// 				} else {
//
// 					newPointList [newPointList.Count - 1 - index].GetComponent<SpringJoint> ().connectedBody = curPoint.GetComponent<Rigidbody>();
//
// 					distance = Vector3.Distance (newPointList [newPointList.Count - 1 - index].position, curPoint.Pos);
// 				}
//
//
// 				t = Mathf.Clamp01 (t - 1);
//
// 			} else {
// 				yield return null;
// 			}
// 		}
//
// 		l.positionCount = 0;
// 		newPointList.Clear ();

		state = PlayerState.Switching;
	}

	public IEnumerator FlyIntoNewPoint(Point p){

		int index = newPointList.Count - 4;
//		int index = newPointList.Count/2;

		float t = 0;

		Point curP = curPoint;
		Spline s = curSpline;

		while (index >= 0) {
			SplinePointPair spp;

//			Point newPoint = Services.PlayerBehaviour.CheckIfOverPoint (newPointList[index].position);
			Point nextp = SplineUtil.CreatePoint(newPointList[index].position);
			spp = SplineUtil.ConnectPoints (s, curP, nextp);

			//IS THIS REALLY THE ONLY CASE I CONNECT SPRINGJOINTS
			//WHY IS CONNECTING SPRING JOINTS THIS WAY BETTER THAN JUST LEAVING THEM UNCONNECTED
			// if (curP != curPoint) {
			// 	curP.GetComponent<SpringJoint> ().autoConfigureConnectedAnchor = true;
			// 	curP.GetComponent<SpringJoint> ().connectedBody = nextp.rb;
			// }

			if (newPointList [index].GetComponentInChildren<SpriteRenderer>()) {
				newPointList [index].GetComponentInChildren<SpriteRenderer> ().sprite = null;
			}

			traversedPoints.Add (curP);

			s = spp.s;
			curP = spp.p;
//			curP.transform.parent = s.transform;

			index -= 4;
		}


		//could add another point at the player's current position between curP (last in index) and p (destination) to make player position not jump
		//whats with phantom splines
		//must be an error with closed/looping splines getting created and fucking up

		SplinePointPair	sp = SplineUtil.ConnectPoints (curSpline, drawnPoint, p);
	  // drawnPoint.GetComponent<SpringJoint> ().connectedBody = p.rb;
		curSpline = sp.s;

		lastPoint = p;
		curPoint = p;
		s.SetSelectedPoint(curP);
		progress = 1;

		Vector3 pos = transform.position;

		float distance = Vector3.Distance (transform.position, p.Pos);

		float speed = 0;

		while (speed < 1) {
			transform.position = Vector3.Lerp(transform.position, p.Pos, speed);
			speed += flow  * Time.deltaTime;
			speed += Time.deltaTime;

//			for(int i = 0; i < newPointList.Count; i++){
////				newPointList [i].GetComponent<SpringJoint> ().spring = newPointList.Count / (i + 1);
////				l.SetPosition(i, Vector3.Lerp(l.GetPosition(i), newPointList[i].position, 1 -(Vector3.Distance(transform.position, p.Pos)/distance)));
////				l.SetPosition(i, newPointList[i].position);
//			}

				yield return null;
		}
		transform.position = p.Pos;

//		if (!p._connectedSplines.Contains (curSpline)) {
//			nextPoint = p;
//			SetPlayerAtEnd (nextSpline, nextPoint);
//			CheckProgress ();
//
//		} else {
//
//			SetPlayerAtEnd (curSpline, nextPoint);
//			CheckProgress ();
//		}

//		SetPlayerAtEnd (s, p);
//		CheckProgress();
		for (int i = newPointList.Count - 1; i >= 0; i--) {
			Destroy (newPointList [i].gameObject);
		}

		newPointList.Clear ();
		l.positionCount = 0;
		state = PlayerState.Switching;
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

			//& !raycastPoint.used
			Services.fx.ShowNextPoint(raycastPoint);

			//if(buttonDown){
				pointDest = raycastPoint;
				stopFlying = true;
			//}
		}
		
		if(pointDest == null && raycastPoint == null){	
			Services.fx.nextPointSprite.enabled = false;
		}

		if (stopFlying && pointDest != null)
		{
			pointDest.controller.AdjustCamera();

			flyingSpeed += Time.deltaTime;
			Vector3 toPoint = pointDest.transform.position - transform.position;
			Vector3 flyToPoint = toPoint.normalized * Time.deltaTime * (flyingSpeed);
			transform.position += Vector3.ClampMagnitude(flyToPoint, toPoint.magnitude);

			// foreach (Spline p in pointDest._connectedSplines)
			// {
			// 	p.DrawSpline(p.SplinePoints.IndexOf(pointDest));
			// }

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



	void TrackFreeMovement(){

		Vector3 inertia;

		// Make drawing points while you skate.
		//should solve the problems of jumping across new points on the same spline.

		//YOU WERE DOING TWO RAYCASTS HERE FOR NO REASON AFTER CANCREATEPOINT WAS REFACTORED
		// Point overPoint = SplineUtil.RaycastDownToPoint(cursorPos, 2f, 1f);
		// if(overPoint != null && overPoint != curPoint){
			//Getting a null ref here for some ungodly reason
			// if(Vector3.Distance (curPoint.Pos, drawnPoint.Pos) < 0.25f){
			// 	curSpline.SplinePoints.Remove(curPoint);
			// 	drawnPoint._neighbours.Remove(curPoint);
			// 	Destroy(curPoint.gameObject);
			// 	curPoint = drawnPoint;
			// 	curSpline.Selected = drawnPoint;
			// 	// SplinePointPair spp = SplineUtil.ConnectPoints(curSpline, drawnPoint, overPoint);
			// 	// curSpline = spp.s;
			// 	// drawnPoint = spp.p;
			// 	// traversedPoints.Add(drawnPoint);
			// 	//
			// 	// SplinePointPair sppp = SplineUtil.ConnectPoints(curSpline, drawnPoint, curPoint);
			// 	// curSpline = sppp.s;
			// 	// curSpline.Selected = drawnPoint;
			// 	// curSpline.OnSplineEnter (true, drawnPoint, curPoint, false);
			// }

			if(CanConnect()){
				//remove current point from curspline and connect drawnPoint to pointDest on current spline
				// curSpline.SplinePoints.Remove(curPoint);
				// drawnPoint._neighbours.Remove(curPoint);
				// Destroy(curPoint.gameObject);
				// curPoint = drawnPoint;
				//this is bugged if the player flies right into the point without creating any on the way
				//the player is warping back to the start of the last spline for no REASON

				Connect();

				curPoint.GetComponent<Collider>().enabled = true;
				curPoint.velocity = cursorDir * Mathf.Abs(flow);
				curPoint.isKinematic = false;
				SwitchState(PlayerState.Traversing);
				return;
			}


			// if(RaycastHitObj == curPoint){
			// 	StartCoroutine (ReturnToLastPoint ());
			// }else{
			// StartCoroutine(FlyIntoNewPoint(RaycastHitObj));
			// }

		 if (flow < 0) {
//			CreateJoint (newPointList[newPointList.Count-1].GetComponent<Rigidbody>());
			// StartCoroutine (ReturnToLastPoint ());
			SwitchState(PlayerState.Animating);

		} else {
			inertia = cursorDir * flow;
			flow -= Time.deltaTime/10;
			transform.position += inertia * Time.deltaTime;
			curDrawDistance += Vector3.Distance (curPoint.Pos, transform.position);
			curPoint.transform.position = transform.position;
			creationInterval -= Time.deltaTime;
			if (creationInterval < 0 && curDrawDistance > PointDrawDistance) {
					creationInterval = creationCD;
					curDrawDistance = 0;
				// if (newPointList.Count == 0) {
					curPoint.velocity = Mathf.Abs(flow) * cursorDir;
					Point newPoint;
					newPoint = SplineUtil.CreatePoint(transform.position);
					curPoint.GetComponent<Collider>().enabled = true;
					curPoint.velocity = cursorDir * Mathf.Abs(flow);
					curPoint.isKinematic = false;
					curPoint.proximity = 0;
					newPoint.GetComponent<Collider>().enabled = false;
					newPoint.isKinematic = true;
					newPoint.proximity = 1;
					SplinePointPair spp = SplineUtil.ConnectPoints(curSpline, curPoint, newPoint);
					lastPoint = drawnPoint;
					curSpline = spp.s;
					drawnPoint = curPoint;
					curPoint = newPoint;
					curSpline.SetSelectedPoint(drawnPoint);
					traversedPoints.Add(drawnPoint);
				  curSpline.OnSplineEnter ();
				// } else {
				// 	CreateJoint (newPointList [newPointList.Count - 1].GetComponent<Rigidbody> ());
				// }
			}else{
					//Something is going on when you connect to the spline you're already drawing.
					//the new spline created on the first ConnectPoint is a new spline, then something weird is happening to it
					//almost 100% sure the SELECTED point is wrong on the new splines

				// Point overPoint = SplineUtil.RaycastDownToPoint(transform.position, 10f, 5f);
				// if(overPoint != null && overPoint != drawnPoint && overPoint != curPoint && overPoint != lastPoint){
				// 	curSpline.SplinePoints.Remove(curPoint);
				// 	drawnPoint._neighbours.Remove(curPoint);
				// 	lastPoint = drawnPoint;
				// 	SplinePointPair spp = SplineUtil.ConnectPoints(curSpline, drawnPoint, overPoint);
				// 	curSpline = spp.s;
				// 	drawnPoint = spp.p;
				// 	traversedPoints.Add(drawnPoint);
				//
				// 	SplinePointPair sppp = SplineUtil.ConnectPoints(curSpline, drawnPoint, curPoint);
				// 	curSpline = sppp.s;
				// 	curSpline.Selected = drawnPoint;
				// 	curSpline.OnSplineEnter (true, drawnPoint, curPoint, false);
					//make new point for curPoint using above code
				// }
			}
		}
	}

	void PlayerMovement(){
//		adding this value to flow
//		MAKE FLOW NON REVERSIBLE. ADJUST LINE ACCURACY WITH FLOW TO MAKE PLAYER NOT STOP AT INTERSECTIONS
//		NEGOTIATE FLOW CANCELLING OUT CURRENT SPEED

		connectTime -= Time.deltaTime * connectTimeCoefficient;
		//flow -= Vector3.Dot(Vector3.up, curSpline.GetDirection(progress))/100f;
		if (curSpeed < flyingSpeedThreshold && Services.fx.flyingParticles.isPlaying)
		{
			Services.fx.flyingParticles.Pause();
		}

		float splineSpeed = curSpline.speed;

		//time to find out what the deal with this is
		

		float speedGain = (easedAccuracy - 0.66f) * 3f;
		
		float gravityCoefficient = Mathf.Clamp01(-curDirection.y);
		float gravityPull = -curDirection.y - curDirection.z;

		// speedGain = speedGain > 0 ? speedGain * acceleration : speedGain * decay;
		
		bool onBelt = curSpline.speed > 0;
		
		//no gaining speed upstream
		if(onBelt && !goingForward) speedGain = Mathf.Clamp(speedGain, -maxSpeed, 0);
		//no losing flow upstream
		if(onBelt && goingForward) speedGain = Mathf.Clamp(speedGain, 0, maxSpeed);

		if(onBelt && goingForward && actualSpeed < curSpline.speed) flow = curSpline.speed; 
		flow += speedGain * acceleration * Time.deltaTime * accelerationCurve.Evaluate(flow/maxSpeed);// * gravityCoefficient;
		if(onBelt && !goingForward) flow -= splineSpeed * Time.deltaTime;
		

		flow = Mathf.Clamp(flow, 0, maxSpeed);

		//flow -= Mathf.Clamp01(-gravityPull) * Time.deltaTime;

		//this never works in practice because you're multiplying flow to 0 when you have 0 accuracy
		//you need to use a -1 to 1 accuracy range

		if(flow <= 0 && onBelt){
			//ok they're being pushed back

			//this is probably all kinds of fucked
			goingForward = !goingForward;
			Point p = pointDest;
			pointDest = curPoint;
			curPoint = p;	
		}

		if (!joystickLocked)
		{
			CalculateMovementDistance(actualSpeed * Time.deltaTime);
			
			//progress += goingForward ? finalSpeed : -finalSpeed;

			curSpline.completion += (curSpeed * Time.deltaTime) / curSpline.segmentDistance;
		}
		//set player position to a point along the curve

		if (curPoint == curSpline.Selected) {
			curPoint.proximity = 1 - progress;
			if (pointDest != null)
			{
				pointDest.proximity = progress;
			}
			// if (curSpline.closed && curSpline.SplinePoints.IndexOf(curPoint) >= curSpline.SplinePoints.Count-1) {
			// 	curSpline.endPoint.proximity = 1 - progress;
			// }

		} else {
			curPoint.proximity = progress;
			curSpline.Selected.proximity = 1 - progress;
		}

		// if(pointDest != null && pointDest.hasPointcloud){
		// }

		if (Services.main.hasGamepad && state == PlayerState.Traversing)
		{
			float hi = Mathf.Pow(Mathf.Clamp01(-accuracy + 1), 3) * curSpeed;
			float low = Mathf.Clamp01(-accuracy) * flow + Mathf.Clamp01(hi - 1);

			if (Services.main.useVibration)
			{
				Debug.Log("low=" + low);
				Debug.Log("high= " + hi);
				Services.main.gamepad.SetMotorSpeeds(low, hi);
			}

		}
		// GetComponent<Rigidbody> ().velocity = curSpline.GetDirection (progress) * flow;

//		transform.Rotate (0, 0, flow*5);
	}

	void CalculateMovementDistance(float distanceToTravel){
		
		int curSegment = goingForward ? (int)Mathf.Ceil((float)Spline.curveFidelity * progress) : (int)Mathf.Floor((float)Spline.curveFidelity * progress);

		Vector3 curPos = transform.position;
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
					progress = step - (spillover/diff) * (step - prevStep);
					adjustedProgress = progress;
					return;
				}else{
					prevStep = step;
				}
			}

			// progress = 1.1f; ????????????
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
					progress = step + (spillover/diff) * (prevStep - step);
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
	public IEnumerator Unwind()
	{

		cursorSprite.sprite = null;

		float t = curSpeed;
		bool moving = true;
		int pIndex = traversedPoints.Count -1;
		bool moveToLastPoint = false;

		Point nextPoint =  traversedPoints [pIndex];

		if (state != PlayerState.Switching) {

			moveToLastPoint = true;
		}

		pIndex--;

		//add case for stopping in middle of line
		//figure out why flow is always non-zero on line.
		state = PlayerState.Animating;

		if (moveToLastPoint) {
			if (curPoint == curSpline.Selected) {
				goingForward = false;
			} else {
				goingForward = true;
			}

			while (moving) {
				t += Time.deltaTime;
				flow = t;

				if (goingForward) {
					progress += Time.deltaTime * t / curSpline.segmentDistance;
				} else {
					progress -= Time.deltaTime * t / curSpline.segmentDistance;
				}

				curSpline.completion = Mathf.Lerp(curSpline.completion, 0, t);
//				curSpline.distortion = Mathf.Lerp(curSpline.distortion, 1, progress);

				transform.position = curSpline.GetPointForPlayer (progress);

				if (progress > 1 || progress < 0) {
					moving = false;
				}
				

				yield return null;
			}


		}

		for (int i = pIndex; i >= 0; i--)
		{

			curPoint = nextPoint;
			nextPoint = traversedPoints[i];
			curSpline = curPoint.GetConnectingSpline(nextPoint);

			SetPlayerAtStart(curSpline, nextPoint);
			moving = true;

			while (moving)
			{
				
				curSpline.completion = Mathf.Lerp(curSpline.completion, 0, t);
				curSpline.distortion = Mathf.Sin(t * Mathf.PI);
				t += Time.deltaTime;
				t = Mathf.Clamp(t, 0f, 9f);
				flow = t;

				
				if (goingForward)
				{
					progress += Time.deltaTime * t / curSpline.segmentDistance;
				}
				else
				{
					progress -= Time.deltaTime * t / curSpline.segmentDistance;
				}

				transform.position = curSpline.GetPointForPlayer(progress);

				if (progress > 1 || progress < 0)
				{
					transform.position = curSpline.GetPointForPlayer(Mathf.Clamp01(progress));
					traversedPoints[i].Reset();
					moving = false;
				}

				yield return null;
			}
		}

		flow = 0;
		lastPoint = curPoint;
		curPoint = nextPoint;
		traversedPoints.Clear ();
		traversedPoints.Add (curPoint);
		state = PlayerState.Switching;

		Initialize();

	}

		void CheckProgress(){

		if (progress >= 1 || progress <= 0) {

// THIS IS KINDA SHITTY. DO IT BETTER
			//accuracy = 1;

			//Point PreviousPoint = curPoint;
			progressRemainder = progress;
			curPoint.proximity = 0;

//			if (progress > 1) {
//
//				progress = 0;
//
//				if (curSpline.Selected == curSpline.EndPoint && curSpline.closed) {
//					curPoint = curSpline.StartPoint;
//				} else {
//					curPoint = curSpline.SplinePoints [curSpline.GetPointIndex(curSpline.Selected) + 1];
//				}
//
//			} else if (progress < 0) {
//
//				progress = 0;
//				curPoint = curSpline.Selected;

//			}

			SwitchState(PlayerState.Switching);
		}
	}

	public void SetPlayerAtStart(Spline s, Point p2){

		int indexdiff = s.SplinePoints.IndexOf (p2) - s.SplinePoints.IndexOf (curPoint);


		if (indexdiff == -1 || indexdiff > 1) {
			s.SetSelectedPoint(p2);
			goingForward = false;
			progress = 1 - Mathf.Epsilon;

		} else {
			// if (timeOnPoint == 0)
			// {
			// 	progress = progressRemainder;
			// }
			// else
			// {
				// progress = 0 + Mathf.Epsilon;
			// }

			progress = 0 + Mathf.Epsilon;
			goingForward = true;
			s.SetSelectedPoint(curPoint);
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
					
				
				int curIndex = s.selectedIndex;
				bool looping = false;
				bool forward = true;

				for(int i = -1; i < 2; i+=2){

					int nextIndex = curIndex + i;
					
					if(nextIndex < 0){
						if(!s.closed || s.SplinePoints.Count < 2) continue;
						looping = true;
						nextIndex = s.SplinePoints.Count - 1;
					}
					
					if(nextIndex > s.SplinePoints.Count - 1){
						if(!s.closed || s.SplinePoints.Count < 2) continue;
						looping = true;
						nextIndex = 0;
					}

					forward = i == 1;
					Point p = s.SplinePoints[nextIndex];

					int indexDifference = nextIndex - curIndex;
						
					bool isGhostPoint = curPoint.pointType == PointTypes.ghost;
					
					//need to check that we're not actually at the start of a new spline thats just facing the other direction

					bool intersection = s != curSpline;

					bool canMoveBackward = (!goingForward && isGhostPoint) || !isGhostPoint || intersection; // || s.SplinePoints.IndexOf (curPoint) == s.SplinePoints.Count -1;
					bool canMoveForward = (isGhostPoint && goingForward) || !isGhostPoint || intersection; // | s.SplinePoints.IndexOf (curPoint) == 0;
					
					
					// indexDifference > 1 means we looped backwards
					// indexDifference == -1 means we went backward one point

					Vector2 p1 = Services.mainCam.WorldToScreenPoint(curPoint.Pos);
					Vector2 p2 = Services.mainCam.WorldToScreenPoint(p.Pos);
					Vector3 toPoint = (p2 - p1).normalized;
					float directAngle = Vector2.Angle(cursorDir, toPoint);

					bool tangent = false;

					Vector3 startdir = Vector3.zero;

					if (canMoveBackward && !forward && s.bidirectional) {
						
						//don't enter conveyor belts that will instantly push you back
						//it's a little janky but better than re-entering the same point every frame
						if(!isGhostPoint && actualSpeed < s.speed) continue;
						
						curAngle = s.CompareAngleAtPoint (cursorDir, p, out startdir, true);	
						
					} else if(canMoveForward && forward){

						curAngle = s.CompareAngleAtPoint (cursorDir, curPoint, out startdir);

					}else{
						//ghost point intersections dont let you change direction
						//leave cur angle infinite
						continue;
					}
					
					if(Mathf.Abs(Vector3.Dot(startdir.normalized, Services.mainCam.transform.forward)) > 0.75f){
						tangent = true;
					}
					// adjustedAngle = (adjustedAngle + curAngle) / 2f;
					//adjustedAngle = screenAngle < curAngle ? screenAngle : curAngle;
					adjustedAngle = tangent ? directAngle : curAngle;
					
					Vector2 screenDir = Services.mainCam.WorldToScreenPoint(curPoint.Pos + startdir);
					screenDir = (screenDir - p1).normalized;

					if(tangent){
						Debug.DrawLine(transform.position, transform.position + Services.mainCam.transform.TransformDirection(toPoint)/3f, Color.blue);
					}else{
					//this is being used but is buggy
						Debug.DrawLine(transform.position, transform.position + Services.mainCam.transform.TransformDirection((Vector3)screenDir)/3f, Color.magenta);
					}
					//this isn't being used, just for my own sanity
					Debug.DrawLine(transform.position, transform.position + startdir.normalized, Color.black/5f);
					

					if (adjustedAngle < minAngle) {
						
						minAngle = adjustedAngle;
						actualAngle = curAngle;
						maybeNextSpline = s;
						maybeNextPoint = p;

						if(forward){
							facingForward = true;
						}else{
							facingForward = false;
						}
						
					}
				}
			}
		}
			
			if(actualAngle < 180){
				accuracy = (90 - actualAngle) / 90;
			}

			if ((actualAngle <= StopAngleDiff || curPoint.pointType == PointTypes.ghost) && maybeNextSpline != null)
			{
				
				splineDest = maybeNextSpline;
				pointDest = maybeNextPoint;

				splineDest.CalculateDistance();

				//  if (curSpline != splineDest)
					// {
						// curSpline.OnSplineExit();
						// splineDest.OnSplineEnter(curPoint, pointDest);
					// }
				// }
				// else
				// {
				// 	splineDest.OnSplineEnter(curPoint, pointDest);
				// }
				

				//idk if you should do this? you're not technically on the spline
				//what is reading curspline while the player is stopped on the point?

				// curSpline = splineDest;

				if(facingForward){
					progress = 0;
					curDirection = splineDest.GetInitVelocity (curPoint);
				}else{
					progress = 1;
					curDirection = splineDest.GetReversedInitVelocity (curPoint);
				}
				
				return true;
			}


			cursorSprite.sprite = brakeSprite;

			return false;

		}
		return false;
	}




//	public void OnTriggerEnter(Collider col){
//		if (col.tag == "Point") {
//			if (!col.GetComponent<Point> ().isPlaced) {
//				StartCoroutine (CollectPoint (col.GetComponent<Point> ()));
//			}
//		}
//	}

	IEnumerator CollectPoint(Point p){

		p.GetComponent<SpriteRenderer> ().enabled = false;

		if (!inventory.Contains (p)) {
			p.GetComponent<Collider> ().enabled = false;
			if (inventory.Count > 0) {
				p.GetComponent<SpringJoint> ().connectedBody = inventory [inventory.Count - 1].GetComponent<Rigidbody> ();
			} else {
				p.GetComponent<SpringJoint> ().connectedBody = GetComponent<Rigidbody> ();

			}
			inventory.Add (p);
			p.transform.parent = transform;
			float t = 0;

			Vector3 originalPos = p.transform.position;

			while (t <= 1) {
				p.transform.position = Vector3.Lerp (originalPos, transform.position, t);
				t += Time.deltaTime;
				yield return null;
			}
		}
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

			Vector3 screenPos = Services.mainCam.WorldToViewportPoint(transform.position);
			screenPos += new Vector3(cursorDir.x / Services.mainCam.aspect, cursorDir.y, 0)/CameraFollow.instance.cam.aspect;
			screenPos = new Vector3(Mathf.Clamp01(screenPos.x), Mathf.Clamp01(screenPos.y), Mathf.Abs(transform.position.z - Services.mainCam.transform.position.z));
			cursorPos = Services.mainCam.ViewportToWorldPoint(screenPos);
//			cursorDir2 = cursorPos - transform.position;
//			cursorDir2.Normalize();
		}
		else
		{
			//cursorPos = transform.position + (Vector3)cursorDir2 / (Services.mainCam.fieldOfView * 0.1f);

			Vector3 screenPos = Services.mainCam.WorldToViewportPoint(transform.position);
			screenPos += new Vector3(cursorDir.x / Services.mainCam.aspect, cursorDir.y, 0)/cursorDistance;
			screenPos = new Vector3(Mathf.Clamp01(screenPos.x), Mathf.Clamp01(screenPos.y), Main.cameraDistance);
			cursorPos = Services.mainCam.ViewportToWorldPoint(screenPos);
		}



		if (cursorDir2.magnitude <= 0.25f){
			joystickLocked = true;
			//cursorDir2 = Vector3.zero;
		}else{
			joystickLocked = false;
		}



//		if(curPoint.HasSplines() && curSpline != null){
//			cursorDir.z = curSpline.GetDirection (progress).z * Mathf.Sign(accuracy);
//		}

		// Vector3 screenPos = ((cursorDir/4f) + (Vector3.one/2f));
		// screenPos = new Vector3(screenPos.x, screenPos.y, Camera.main.nearClipPlane + 10f);
		// cursorPos = Camera.main.ViewportToWorldPoint(screenPos);
		// float screenWidth = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, Camera.main.nearClipPlane)).y - transform.position.y;
		// cursorPos = transform.position + ((Vector3)cursorDir * screenWidth);


		cursor.transform.position = cursorPos;
		cursor.transform.up = cursorPos - transform.position;


		if(buttonDown && state == PlayerState.Traversing)
		{
			//SwitchState(PlayerState.Flying);
		}
		else
		{
			//cursorDir = Vector3.Lerp (cursorDir, cursorDir2, (cursorRotateSpeed + flow) * Time.deltaTime);
			cursorDir = cursorDir2;
		}

		playerSprite.transform.up = cursorDir;

		// l.SetPosition(0, transform.position);
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

	void DrawVelocity(){

		float s = 1f/(float)Spline.curveFidelity;
		for(int i = 0; i < Spline.curveFidelity * 3; i +=3){
			int index = i/3;
			float step = (float)index/(float)Spline.curveFidelity;
			if(i >= velocityLine.points3.Count-1){
				velocityLine.points3.Add(Vector3.zero);
				velocityLine.points3.Add(Vector3.zero);
				velocityLine.points3.Add(Vector3.zero);
			}
			if(i >= velocityLine2.points3.Count-1){
				velocityLine2.points3.Add(Vector3.zero);
				velocityLine2.points3.Add(Vector3.zero);
				velocityLine2.points3.Add(Vector3.zero);
			}
			Vector3 pos =  curSpline.GetPointForPlayer(step + Mathf.Epsilon);
			velocityLine.points3[i] = pos;

			float f = (step - progress);

			velocityLine2.points3[i+1] = Vector3.Lerp(velocityLine2.points3[i + 1], velocityLine2.points3[i], Time.deltaTime);
			if(f > s){
				velocityLine.points3[i+1] = pos;
			}
			else if(f <= s && f >= 0){
				if(step == 0){
					velocityLine.points3[i+1] = pos + curSpline.GetDirection(step + 0.01f) * Mathf.Pow((1-Mathf.Abs(f)), 2) * curSpeed * curSpline.segmentDistance * (s) * Spline.curveFidelity/2;
				}else{
				velocityLine.points3[i+1] = pos + curSpline.GetDirection(step) * Mathf.Pow((1-Mathf.Abs(f)), 2) * curSpeed * curSpline.segmentDistance * (s - f) * Spline.curveFidelity/2;
				}
			}else{
				velocityLine.points3[i + 1] = Vector3.Lerp(velocityLine.points3[i + 1], velocityLine.points3[i], Time.deltaTime);
			}
			velocityLine.points3[i + 2] = pos;
		}

		velocityLine.Draw3D();
		velocityLine2.Draw3D();
		// velocityLine.positionCount = velocityLine.positionCount + 10;
		// Vector3 lpos =  curSpline.GetPoint(progress);
		// velocityLine.SetPosition(velocityLine.positionCount-11, lpos);
		// // velocityLine.SetPosition(1, pos + curSpline.GetDirection(progress) * curSpeed * curSpline.distance);
		// Vector3 dir = curSpline.GetDirection(progress) * (transform.position - cursorPos).magnitude;
		// for(int i = 1; i < 10; i ++){
		// 	Vector3 lastP = velocityLine.GetPosition(velocityLine.positionCount - 10 + i-2);
		// 	velocityLine.SetPosition(velocityLine.positionCount - 10 + i-1, Vector3.Lerp(velocityLine.GetPosition(velocityLine.positionCount -10 + i -1), Vector3.Lerp(lastP + dir/8, lastP + (cursorPos - lastP)/5, (float)i/10), Time.deltaTime * 50));
		// }
		// velocityLine.SetPosition(10, Vector3.Lerp(velocityLine.GetPosition(10), cursorPos, Time.deltaTime * 50));

	}

	void LeaveState()
	{
		switch (state)
		{
			case PlayerState.Traversing:
				Services.fx.BakeParticles(sparks, Services.fx.brakeParticleMesh);

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

				//curSpeed = flyingSpeed;

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

					if (buttonWasPressed)
					{
						buttonDownTimer = 0;
						boost += Point.boostAmount + Services.PlayerBehaviour.boostTimer;
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

				cursorSprite.enabled = true;
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


				/*
				GranularSynth.moving.TurnOn();*/

				//calculate distance here still?

				// VectorLine v = velocityLine2;
				// velocityLine2 = velocityLine;
				// velocityLine = v;

				state = PlayerState.Traversing;

				if(curSpline != null){
					if (curSpline != splineDest){
						curSpline.OnSplineExit();
					}
				}

				curSpline = splineDest;
				curSpline.OnSplineEnter();

				//this is making it impossible to get off points that are widows. wtf.
				SetPlayerAtStart (curSpline, pointDest);

				//I guess I just enter splines while I'm stopped now
				//it pisses me off but I'm sure there's some reason				
				//curSpline.OnSplineEnter (true, curPoint, pointDest, false);

				//PlayerMovement ();

				t.emitting = true;

				if (curPoint.pointType != PointTypes.ghost)
				{
					if(OnStartTraversing != null){
						OnStartTraversing.Invoke();
					}
				}
				
				if(OnLeaveAnyPoint != null){
					OnLeaveAnyPoint.Invoke();
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

				flyingSpeed = flow + speed + boost;

				//THIS MAY NOT BE NECESSARY UNLESS WE CAN FLY OFF OF SPLINES, NOT JUST POINTS
				//curPoint.OnPointExit();

				curPoint.proximity = 0;
				pointDest = null;

				state = PlayerState.Flying;
				flyingTrail.emitting = true;
				curSpeed = 0;
				
				if(curSpline != null){
					curSpline.OnSplineExit();
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

				directionIndicator.enabled = false;

				state = PlayerState.Switching;

				timeOnPoint = 0;
				

				if (curPoint == null)
				{

				}
				else if (curPoint != pointDest)
				{
					traversedPoints.Add(pointDest);

					foreach (Spline s in curPoint._connectedSplines)
					{
						s.reactToPlayer = false;
					}

					lastPoint = curPoint;

					foreach (Spline s in pointDest._connectedSplines)
					{
						s.reactToPlayer = true;
						s.SetSelectedPoint(pointDest);
					}
				}

				
				curPoint = pointDest;
				
				if(curPoint.pointType != PointTypes.ghost){
					
					if(OnStoppedTraversing != null){
						OnStoppedTraversing.Invoke();
					}
				
					if (Services.main.hasGamepad)
					{
						Services.main.gamepad.ResetHaptics();
					}
				
				}
					
					if(OnEnterAnyPoint != null){
						OnEnterAnyPoint.Invoke();
					}
				
//TODO

				
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
				cursorSprite.enabled = false;
				state = PlayerState.Animating;

//				if (state == PlayerState.Flying)
//				{
//					if (!hasFlown)
//					{
//						StartCoroutine(RetraceTrail());
//						state = PlayerState.Animating;
//					}
//					else
//					{
//						PointManager.ResetPoints ();
//						Initialize();
//					}
//				}
//				else
//				{
//					state = PlayerState.Animating;
//					if (!hasFlown)
//					{
//						StartCoroutine(Unwind());
//					}
//					else
//					{
//						PointManager.ResetPoints();
//						Initialize();
//
//					}
//				}

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
//			t.time = Mathf.Lerp(t.time, 0, Time.deltaTime);
// (accuracy < 0 && flow > 0) || accuracy > 0 && flow <
			if (state != PlayerState.Switching)
			{
				e.rateOverTimeMultiplier = (1-normalizedAccuracy) * 50 * potentialSpeed;
			}else{
				e.rateOverTimeMultiplier= 0;
			}

		}

		if (curSpline != null)
		{
//			if(flow > 0.2	5f){
//				velocityLine.color = Color.Lerp(velocityLine.color, new Color(1,1,1,0.1f), Time.deltaTime);
//				velocityLine2.color = Color.Lerp(velocityLine2.color, new Color(1,1,1,0.1f), Time.deltaTime);
//				// DrawVelocity();
//			}else{
//				velocityLine.color = new Color(1,1,1,0.1f);
//				velocityLine2.color = new Color(1,1,1,0.1f);
//			}

//			curSpline.DrawLineSegmentVelocity (progress, Mathf.Sign (accuracy), goingForward ? 0 : 1);\
			// curSpline.l.material.mainTextureOffset -= Vector2.right * Mathf.Sign (accuracy) * flow * curSpline.l.material.mainTextureScale.x * 2 * Time.deltaTime;
//			l.SetPosition(0, transform.position);
//			l.SetPosition(1, transform.position + (curSpline.GetDirection(progress) * Mathf.Sign(accuracy))/2);
//			l.SetPosition(1, transform.position + cursorDir/2);
//			GetComponentInChildren<Camera>().farClipPlane = Mathf.Lerp(GetComponentInChildren<Camera>().farClipPlane,  flow + 12, Time.deltaTime * 10);
		}



// 		switch(state){
// //		Services.PlayerBehaviour.flow / (Services.PlayerBehaviour.maxSpeed/2))
// 	  case PlayerState.Traversing:
//
// 		//fade out CURPOINT proximity
// 		//fade in POINTDEST proximity
//
// 		//assign audio clip soundwhere. Don't have the sounds switch when pointDest/curPoint change
//
//
// 		break;
//
// 		case PlayerState.Flying:
// 		brakingSound.volume = Mathf.Lerp(brakingSound.volume, 0, Time.deltaTime * 5);
// 		sound.volume = Mathf.Lerp(sound.volume, Mathf.Clamp01(Mathf.Abs(flow)), Time.deltaTime * 5);
// 		break;
//
// 		case PlayerState.Switching:
// 		brakingSound.volume = Mathf.Lerp(brakingSound.volume, 0, Time.deltaTime * 5);
// 		sound.volume = Mathf.Lerp(sound.volume, 0, Time.deltaTime * 10);
// 		break;
//
// 		case PlayerState.Animating:
// 		brakingSound.volume = Mathf.Lerp(brakingSound.volume, 1, Time.deltaTime);
// 		break;
// 	}
		//centering freq on note freq will just boost the fundamental. can shift this value to highlight diff harmonics
		//graph functions
		//normalize values before multiplying by freq
		//use note to freq script

//		pitch = dot product between the current tangent of the spline and the linear distance between points
//		Services.Sounds.master.SetFloat("FreqGain", Mathf.Abs(flow)/2 + 1f);
	}

	public Vector3 GetCursorDir(){
		return cursorDir;
	}

	public Vector3 GetCursorVelocity(){
		return cursorPos - transform.position;
	}
}
