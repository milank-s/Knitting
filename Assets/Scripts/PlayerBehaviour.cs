using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Vectrosity;

public enum PlayerState{Traversing, Switching, Flying, Animating};


//###################################################
//###################################################


//						TO DO					   


//Sounds SOUNDS OUNDS OUNDSOUNDSOUNDSOUNDSUON
//Flying no longer creates points, only attaches to other flying points
//Tuning closer to dashing in hyper light. Usage of timed button presses on intersections


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
	public float maxSpeed;
	public float acceleration;
	public float decay;
	public float accuracyCoefficient;
	public float flowAmount = 0.1f;
	public float boostAmount = 0.1f;
	public float stopTimer = 2f;
	[Space(10)]

	[Header("Cursor Control")]
	public float cursorDistance;
	public float cursorRotateSpeed = 1;
	public float LineAngleDiff = 30;
	public float StopAngleDiff = 60;
	float angleToSpline = Mathf.Infinity;
	[Space(10)]

	[HideInInspector]
	public bool goingForward = true;
	[HideInInspector]
	public float progress, accuracy, flow, boost, boostTimer, curSpeed, connectTime, connectTimeCoefficient, gravity;

	[Header("Flying tuning")]
	public float flyingSpeedThreshold = 3;
	public float PointDrawDistance;
	public float creationCD = 0.25f;
	public float creationInterval = 0.25f;
	private bool canFly;
	private List<Transform> newPointList;
	[Space(10)]

	[Header("Point Creation")]
	private Spline drawnSpline;
	private Point drawnPoint;
	private List<Point> traversedPoints;
	private List<Point> inventory;
	private float curDrawDistance = 0.1f;
	[Space(10)]

	[Header("Input")]
	public bool usingJoystick;
	public bool joystickLocked;
	private bool controllerConnected = false;
	[Space(10)]

	[Header("AV")]
	public Sprite canFlySprite;
	public Sprite canMoveSprite;
	public Sprite canConnectSprite;
	public Sprite brakeSprite;
	public Sprite traverseSprite;
	public AudioSource brakingSound;
	private PlayerSounds sounds;
	private AudioSource sound;
	private TrailRenderer t;
	private ParticleSystem ps;
	private LineRenderer l;
	private Image cursorSprite;
	private SpriteRenderer playerSprite;
	private int noteIndex;
	public LineRenderer cursorOnPoint;
	private VectorLine velocityLine;
	private VectorLine velocityLine2;
	
	private bool buttonPressed;
	private float buttonPressedBuffer = 0.2f;
	private float buttonPressedTimer;
	
	private GameObject cursor;
	[HideInInspector]
	public Vector3 cursorPos;
	[HideInInspector]
	public Vector2 cursorDir;

	public void Awake(){
		joystickLocked = true;
		playerSprite = GetComponentInChildren<SpriteRenderer>();
		sound = GetComponent<AudioSource>();
		
		pointDest = null;
		traversedPoints = new List<Point> ();
		
		connectTimeCoefficient = 1;
		state = PlayerState.Switching;
		sounds = GetComponent<PlayerSounds> ();
		l = GetComponent<LineRenderer> ();
		t = GetComponentInChildren<TrailRenderer> ();

		inventory = new List<Point>();
		ps = GetComponent<ParticleSystem> ();
		newPointList = new List<Transform> ();

		int i = 0;
		lastPoint = null;
		
	}

	public void Initialize(){

		traversedPoints.Clear();
		cursor = Services.Cursor;
		curPoint = Services.StartPoint;
		transform.position = curPoint.Pos;
		cursorSprite = Services.Cursor.GetComponent<Image>();
		
		traversedPoints.Add (curPoint);
		curPoint.OnPointEnter ();
		t.Clear();
		t.time = 100;

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



	public IEnumerator Reset()
	{


		Vector3[] positions = new Vector3[t.positionCount];
		t.GetPositions(positions);
		state = PlayerState.Animating;
		float f = 0;
		float lerpSpeed = 1;
		float distance;
			
		Point p = SplineUtil.CreatePoint(transform.position);
		p.pointType = PointTypes.connect;
		p.Initialize();
		
		if (traversedPoints.Count > 0)
		{
			
			foreach (Point pi in traversedPoints)
			{
				pi.Reinitialize();
			}
		}
		
		for (int i = positions.Length -1; i >= 0; i--)
		{
			float temp = 0;
			Vector3 tempPos = transform.position;
			distance = Vector3.Distance(tempPos, positions[i]);
//			while (temp < 1)
//			{
//				transform.position = Vector3.Lerp(tempPos, positions[i], temp);
				transform.position = positions[i];
				temp += (Time.deltaTime * lerpSpeed)/distance;
				lerpSpeed += Time.deltaTime/2;
				f = Mathf.Clamp01(lerpSpeed);
				Spline.distortion = 1 - f;
				//play drone music or whatever
				yield return null;
//			}

			//bake the mesh out after this and copy it before clearing the trail renderer
		}
	
		Spline.distortion = 0;
		
		
		curSpline = null;
		Initialize();
		state = PlayerState.Switching;
	}
	public void Step () {

		Point.hitColorLerp = connectTime;

		if (Input.GetButtonDown("Button1"))
		{
			buttonPressed = true;
			buttonPressedTimer = buttonPressedBuffer;
		}

		buttonPressedTimer -= Time.deltaTime;
		
		if (buttonPressedTimer < 0)
		{
			buttonPressed = false;
		}
		
		float speedCoefficient;
		if(state == PlayerState.Switching || state == PlayerState.Animating){
			speedCoefficient = 0;
		}else if (state == PlayerState.Flying){
			speedCoefficient = curSpeed;
		}else{
			speedCoefficient = Mathf.Clamp01(Mathf.Pow(accuracy, 5) * flow + 0.25f);
		}

		if (joystickLocked)
		{
			speedCoefficient = 0;
		}
		playerSprite.transform.localScale = Vector3.Lerp(playerSprite.transform.localScale, new Vector3(Mathf.Clamp(1 - (speedCoefficient * 2), 0.1f, 0.25f), Mathf.Clamp(speedCoefficient, 0.25f, 0.75f), 0.25f), Time.deltaTime * 10);

		if (connectTime <= 0 && PointManager._pointsHit.Count > 0) {
			PointManager.ResetPoints ();
			connectTime = 1;
		}

		CursorInput();
		Effects ();

		if (state == PlayerState.Flying) {
			FreeMovement ();
			return;
		}

		if (state == PlayerState.Traversing) {
			if(curSpline != null){
				SetCursorAlignment ();
			}

			PlayerMovement ();
			CheckProgress ();

			if(Mathf.Abs(flow) < 1){
			cursorSprite.sprite = traverseSprite;
		 }else if (Mathf.Abs(flow) < 2){
			 cursorSprite.sprite = canMoveSprite;
		 }else{
			 cursorSprite.sprite = canFlySprite;
		 }
			
			transform.position = curSpline.GetPoint(progress);
			

		}else if(state == PlayerState.Switching)
		{
			transform.position = curPoint.Pos;
			gravity = 0;
//			curSpeed = 0;
			PlayerOnPoint();
		}

		if (state != PlayerState.Animating && curPoint.HasSplines () && curSpline != null) {


			curSpline.UpdateSpline();
			ManageSound();
			
			foreach(Spline s in curPoint._connectedSplines){
				//should always be drawn
				if(!s.locked)
				{
//					s.reactToPlayer = true;
//					s.DrawSpline(true, s.SplinePoints.IndexOf(curPoint), Mathf.Clamp(s.SplinePoints.IndexOf(curPoint) + 1, 0, s.SplinePoints.Count + (s.closed ? 0 : 0)));
//					s.DrawSpline( s.SplinePoints.IndexOf(curPoint));
				}
			}
			
			if(pointDest != null){
				foreach(Spline s in pointDest._connectedSplines){
					if(!s.locked)
					{
//						s.reactToPlayer = true;
//						s.DrawSpline(s.SplinePoints.IndexOf(pointDest));
					}
				}
			}
			
			
		
			
			if (traversedPoints.Count >= 2 && Input.GetButton("Button2")) {
				// && Mathf.Abs (flow) <= 0)
				StartCoroutine (Unwind());
			}
		}


		#region
		if (Input.GetAxis ("Joy Y") != 0) {
			controllerConnected = true;
		}
		#endregion
	}
	
	public void PlayerOnPoint(){
		bool canTraverse = false;

		if (CanLeavePoint())
		{

			if (curPoint.pointType == PointTypes.ghost)
			{
				canTraverse = true;

			}
			else if (!joystickLocked && (!Input.GetButton("Button1")|| boostTimer > 1))
			{

				if (curPoint.locked)
				{
					canTraverse = true;
					curPoint.locked = false;
				}
				else if (!curPoint.locked)
				{
					canTraverse = true;
				}
			}
			else
			{
				canTraverse = false;
				cursorSprite.sprite = traverseSprite;
				l.positionCount = 0;
				cursorOnPoint.positionCount = 0;
			}
		}
		else {
			// NO CONNECTING FOR NOW
				

				if(CanCreatePoint()){
					if(Input.GetButtonUp("Button1") && pointDest.pointType == PointTypes.connect){
						CreatePoint();
						canTraverse = true;
						
					}else if (pointDest.pointType == PointTypes.connect){
						l.positionCount = 2;
		  			cursorOnPoint.positionCount = 2;
						l.SetPosition (0, pointDest.Pos);
						l.SetPosition (1, transform.position);
						cursorOnPoint.SetPosition (0, pointDest.Pos);
						cursorOnPoint.SetPosition (1, cursorPos);
						canTraverse = false;
						cursorSprite.sprite = canConnectSprite;
					}
					}else if(TryToFly()){
						cursorSprite.sprite = canFlySprite;
						if(Input.GetButton("Button1") || boostTimer >= 1){
							Float();
							return;
						}
				 }else{
					l.positionCount = 0;
		 			cursorOnPoint.positionCount = 0;
		 			cursorSprite.sprite = brakeSprite;
				 }
			}


		if (!canTraverse)
		{
			if(Input.GetButton("Button1")){
				boostTimer += Time.deltaTime / stopTimer;

			}else{
				boostTimer = Mathf.Clamp01(boostTimer - Time.deltaTime);
			}
			
			l.positionCount = 2;
			l.SetPosition (0, Vector3.Lerp(transform.position, cursorPos, Easing.QuadEaseOut(boostTimer)));
			l.SetPosition (1, transform.position);
		}



		if(canTraverse){
			// pointInfo.GetComponent<Text>().text = "";
			l.positionCount = 0;
			LeavePoint();
			boostTimer = 0;
		}else{
			
			//Staying on a point is too punishing.
			StayOnPoint();
		}
	}

	public void SetCursorAlignment(){
		float alignment = Vector2.Angle (cursorDir, curSpline.GetDirection (progress));
		accuracy = (90 - alignment) / 90;
		StopAngleDiff = Mathf.Lerp (20, 50, Mathf.Abs(flow));
	}

	bool CanCreatePoint(){

			if (!joystickLocked) {

				pointDest = null;
				pointDest = SplineUtil.RaycastFromCamera(cursorPos, 1f);

				bool drawnPointNull;
				if(drawnPoint == null){
					drawnPointNull = true;
				}
				if (pointDest != null && pointDest != curPoint && !pointDest.IsAdjacent(curPoint)) {
					if(drawnPoint != null){
						if(drawnPoint != pointDest){
							return true;
					  }else{
							return false;
						}
					}else{
						return true;
					}
				}
		}

		return false;
	}

	void CreatePoint(){
		SplinePointPair spp = SplineUtil.ConnectPoints (curSpline, curPoint, pointDest);
		//Adding points multiple times to each other is happening HERE
		//Could restrict points to never try and add their immediate neighbours?
		l.positionCount = 0;
		cursorOnPoint.positionCount = 0;

		bool isEntering = false;

		if (curSpline != null && curSpline != spp.s) {
			isEntering = true;
			curSpline.OnSplineExit ();
		}


		curSpline = spp.s;
		pointDest = spp.p;
		connectTime = 1;
	}

	bool TryToFly(){
		if (Mathf.Abs(flow) >= flyingSpeedThreshold && curPoint.canFly)
		{
			l.positionCount = 2;
			l.SetPosition (0, cursorPos);
			l.SetPosition (1, transform.position);
			return true;
		}
		return false;
	}

	void Float()
	{
		curPoint.used = true;
		
		pointDest = null;
		l.positionCount = 0;
		state = PlayerState.Flying;
		if (curSpline != null)
		{
			curSpline.OnSplineExit();
		}

		curPoint.OnPointExit ();
		curPoint.proximity = 0;
		flow += 1f;
		Debug.Log("this got called right?");
	}

	public void EmitParticles()
	{
		ps.Emit(5);
	}
	
	void Fly(){
		pointDest = null;
		l.positionCount = 0;
		state = PlayerState.Flying;
		curSpline.OnSplineExit ();
		curPoint.OnPointExit ();
		curPoint.proximity = 0;
		drawnPoint = curPoint;
		curPoint = SplineUtil.CreatePoint(transform.position);
		curSpline = SplineUtil.CreateSpline(drawnPoint, curPoint);
		curDrawDistance = 0;
		curSpline.OnSplineEnter(true, drawnPoint, curPoint, false);
		curPoint.GetComponent<Collider>().enabled = false;
		boost = boostAmount;
		flow = Mathf.Abs(flow);
	}

	void LeavePoint(){
		if(curPoint.hasPointcloud){
			if(pointDest.hasPointcloud){
				foreach(PointCloud p in curPoint.pointClouds){
					p.isOn = false;
					foreach(PointCloud q in pointDest.pointClouds){

						if(p == q){
							p.isOn = true;
						}
					}
				}
			}
		}
		VectorLine v = velocityLine2;
		velocityLine2 = velocityLine;
		velocityLine = v;

		curPoint.OnPointExit ();
		connectTime = 1;
		state = PlayerState.Traversing;

		//this is making it impossible to get off points that are widows. wtf.
		SetPlayerAtStart (curSpline, pointDest);
		curSpline.OnSplineEnter (true, curPoint, pointDest, false);
		SetCursorAlignment ();
		// PlayerMovement ();
	}

	void StayOnPoint(){
		connectTime -= Time.deltaTime * connectTimeCoefficient;
		if (connectTime < 0) {
			if (flow > 0) {
				flow -= decay * Time.deltaTime;
				if (flow < 0) {
					flow = 0;
				}
			} else if (flow < 0) {
				flow += decay * Time.deltaTime;
				if (flow > 0) {
					flow = 0;
				}
			}
		}
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
					progress += (Time.deltaTime * t * flowMult) / curSpline.distance;
				} else {
					progress -= (Time.deltaTime * t  * flowMult) / curSpline.distance;
				}

				transform.position = curSpline.GetPoint (progress);

				if (progress > 1 || progress < 0) {
					moving = false;
				}
				yield return null;
			}

			curSpline.draw = false;


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
		s.Selected = curP;
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

	void FreeMovement()
	{
		Point raycastPoint = SplineUtil.RaycastFromCamera(cursorPos, 1f);
		
		if (raycastPoint != null && raycastPoint != curPoint && raycastPoint.pointType != PointTypes.ghost && !raycastPoint.used)
		{
			pointDest = raycastPoint;
		}

		if (pointDest != null)
		{
			transform.position += (pointDest.transform.position - transform.position).normalized * Time.deltaTime;

			if (Vector3.Distance(transform.position, pointDest.Pos) < 0.025f)
			{
				EnterPoint(curPoint);
			}
		}
		else
		{
			if (flow > 0)
			{
				flow -= Time.deltaTime;
				Vector3 inertia = cursorDir * flow;
				transform.position += inertia * Time.deltaTime;
			}
			else
			{
				StartCoroutine(Reset());
			}
		}
	
	}

	public void EnterPoint(Point previousPoint)
	{
		curPoint = pointDest;
		state = PlayerState.Switching;
		curPoint.proximity = 1;
		curPoint.OnPointEnter ();
		
		PlayAttack(curPoint, pointDest);
			
		curPoint.velocity = cursorDir * Mathf.Abs(flow);
		
		foreach (Point p in curPoint._neighbours)
		{
			p.velocity = Vector3.Lerp((curPoint.Pos - p.Pos).normalized, curPoint.velocity.normalized, 0.5f) * curPoint.velocity.magnitude / 3f;
		}

		if (previousPoint == null)
		{
			traversedPoints.Add (curPoint);
		}else if (previousPoint != curPoint) {
			traversedPoints.Add (curPoint);
			foreach (Spline s in previousPoint._connectedSplines)
			{
				s.reactToPlayer = false;
			}
				
			lastPoint = previousPoint;
				
		}
		
			
		//checkpoint shit
		if(curPoint.pointType == PointTypes.stop){
//				traversedPoints.Clear();
//				traversedPoints.Add(curPoint);
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
			// 	Debug.Log("I have no idea why I'm doing this");
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

			if(CanCreatePoint()){
				//remove current point from curspline and connect drawnPoint to pointDest on current spline
				// curSpline.SplinePoints.Remove(curPoint);
				// drawnPoint._neighbours.Remove(curPoint);
				// Destroy(curPoint.gameObject);
				// curPoint = drawnPoint;
				//this is bugged if the player flies right into the point without creating any on the way
				//the player is warping back to the start of the last spline for no REASON

				CreatePoint();

				curPoint.GetComponent<Collider>().enabled = true;
				curPoint.velocity = cursorDir * Mathf.Abs(flow);
				curPoint.isKinematic = false;
				LeavePoint();
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
			StartCoroutine(Unwind());

		} else {
			inertia = cursorDir * flow;
			flow -= Time.deltaTime/10;
			transform.position += inertia * Time.deltaTime;
			curDrawDistance += Vector3.Distance (curPoint.Pos, transform.position);
			curPoint.transform.position = transform.position;
			curPoint.originalPos = transform.position;
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
					curSpline.Selected = drawnPoint;
					traversedPoints.Add(drawnPoint);
				  curSpline.OnSplineEnter (true, drawnPoint, curPoint, false);
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

	GameObject CreateJoint(Rigidbody r){
		Transform newJoint = Instantiate (Services.Prefabs.joint, curPoint.transform.position, Quaternion.identity).transform;
		newJoint.GetComponent<SpringJoint> ().connectedBody = r;
		newJoint.name = newPointList.Count.ToString();
		newPointList.Add(newJoint);
		if (newPointList.Count % 3 == 0) {
			newJoint.GetComponentInChildren<SpriteRenderer> ().enabled = true;
		}
		return newJoint.gameObject;
	}

	void PlayerMovement(){
//		adding this value to flow
//		MAKE FLOW NON REVERSIBLE. ADJUST LINE ACCURACY WITH FLOW TO MAKE PLAYER NOT STOP AT INTERSECTIONS
//		NEGOTIATE FLOW CANCELLING OUT CURRENT SPEED
		connectTime -= Time.deltaTime * connectTimeCoefficient;
		//flow -= Vector3.Dot(Vector3.up, curSpline.GetDirection(progress))/100f;
		
		
		accuracyCoefficient = Mathf.Pow(Mathf.Abs(accuracy), 2);
		if (accuracy > 0.5f && !joystickLocked) {

			if(flow < 0){
				flow += decay *  accuracy * Time.deltaTime;
				if (flow > 0)
					flow = 0;
			}else{
				//
//			maxSpeed = curSpline.distance * 2;

			if(Mathf.Abs(flow) < maxSpeed){
			flow += Mathf.Sign (accuracy) * accuracyCoefficient * acceleration * Time.deltaTime * cursorDir.magnitude;
			
			}

		}
	}
		// curSpeed =  speed * Mathf.Sign (accuracy) * accuracyCoefficient;
		// if ((curSpeed > 0 && flow < 0) || (curSpeed < 0 && flow > 0)) {
		// 	curSpeed = 0;
		// }

		if ((accuracy < 0.5f) || joystickLocked) {

			if (flow > 0) {
				// flow -= decay * (0.5f - accuracy/2f) * Time.deltaTime;
				if (flow < 0)
					flow = 0;
			}
		}

		float adjustedAccuracy = goingForward ? Mathf.Clamp(accuracy, 0.1f, 1f) : -Mathf.Clamp(accuracy, -1, -0.5f);
		// (adjustedAccuracy + 0.1f)
		if (!joystickLocked)
		{
			curSpeed = (((flow + boost + speed) * adjustedAccuracy)) * cursorDir.magnitude;
			progress += (curSpeed * Time.deltaTime) / curSpline.distance;
		}

		boost = Mathf.Lerp (boost, 0, Time.deltaTime * 3f);
		//set player position to a point along the curve

		if (curPoint == curSpline.Selected) {
			curPoint.proximity = 1 - progress;
			pointDest.proximity = progress;

			// if (curSpline.closed && curSpline.SplinePoints.IndexOf(curPoint) >= curSpline.SplinePoints.Count-1) {
			// 	curSpline.endPoint.proximity = 1 - progress;
			// }

		} else {
			curPoint.proximity = progress;
			curSpline.Selected.proximity = 1 - progress;
		}

		// if(pointDest != null && pointDest.hasPointcloud){
		// 	CameraFollow.desiredFOV = Mathf.Lerp(CameraFollow.desiredFOV, pointDest.pointCloud.desiredFOV, pointDest.proximity);
		// }


		// GetComponent<Rigidbody> ().velocity = curSpline.GetDirection (progress) * flow;

//		transform.Rotate (0, 0, flow*5);
	}

	public IEnumerator Unwind(){

		float t = 0;
		bool moving = true;
		int pIndex = traversedPoints.Count -1;
		bool moveToLastPoint = false;

		Point nextPoint =  traversedPoints [pIndex];

		if (state == PlayerState.Switching) {
			pIndex--;
		} else {
			moveToLastPoint = true;
		}


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
					progress += Time.deltaTime * t / curSpline.distance;
				} else {
					progress -= Time.deltaTime * t / curSpline.distance;
				}

				transform.position = curSpline.GetPoint (progress);

				if (progress > 1 || progress < 0) {
					moving = false;
				}
				yield return null;
			}
			curSpline.draw = false;
			pIndex--;
		}

		for(int i = pIndex; i >= 0; i--) {

			curPoint = nextPoint;
			nextPoint = traversedPoints [i];
			curSpline = curPoint.GetConnectingSpline (nextPoint);
			SetPlayerAtStart (curSpline, nextPoint);
			moving = true;

			while(moving){
				t += Time.deltaTime;
				t = Mathf.Clamp(t, 0f, 9f);
				flow = t;
				if (goingForward) {
					progress += Time.deltaTime * t / curSpline.distance;
				} else {
					progress -= Time.deltaTime * t / curSpline.distance;
				}

				transform.position = curSpline.GetPoint (progress);

				if (progress > 1 || progress < 0) {
					transform.position = curSpline.GetPoint (Mathf.Clamp01(progress));
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
	}

		void CheckProgress(){

		if (progress > 1 || progress < 0) {

// THIS IS KINDA SHITTY. DO IT BETTER
			accuracy = 1;
			
			Point PreviousPoint = curPoint;
			
			curPoint.proximity = 0;
			
			if (progress > 1) {

				progress = 0;

				if (curSpline.Selected == curSpline.EndPoint && curSpline.closed) {
					curPoint = curSpline.StartPoint;
				} else {
					curPoint = curSpline.SplinePoints [curSpline.GetPointIndex(curSpline.Selected) + 1];
				}

			} else if (progress < 0) {

				progress = 0;
				curPoint = curSpline.Selected;

			}

			EnterPoint(PreviousPoint);
	
			PlayerOnPoint();

		}
	}

	public void SetPlayerAtStart(Spline s, Point p2){
		int indexdiff = s.SplinePoints.IndexOf (p2) - s.SplinePoints.IndexOf (curPoint);

		if (indexdiff == -1 || indexdiff > 1) {
			s.Selected = p2;
			goingForward = false;
			progress = 1 - Mathf.Epsilon;

		} else {
			progress = 0 + Mathf.Epsilon;
			goingForward = true;
			s.Selected = curPoint;
		}

	}

	public void SetPlayerAtEnd(Spline s, Point p2){
		int indexdiff = s.SplinePoints.IndexOf (p2) - s.SplinePoints.IndexOf (curPoint);

		if (indexdiff == -1 || indexdiff > 1) {
			s.Selected = p2;
			goingForward = true;
			progress = 0;

		} else {
			progress = 1;
			goingForward = false;
			s.Selected = curPoint;
		}

	}

	//MAKE SURE THAT YOU CAN STILL PLACE POINTS WHILE NOT FLYING OFF THE EDGE
	//DONT CONFUSE FLYING WITH

	public bool CanLeavePoint(){

		angleToSpline = Mathf.Infinity;

		if (curPoint.HasSplines ()) {

			splineDest = null;
			pointDest = null;

			foreach (Spline s in curPoint.GetSplines()) {

				if(s.locked){

				}else{

				foreach (Point p in curPoint.GetNeighbours()) {

					if (!p._connectedSplines.Contains (s)) {
						float curAngle = Mathf.Infinity;
						//do nothing if the point is in another spline
					} else {

						float curAngle = Mathf.Infinity;

						int indexDifference = s.SplinePoints.IndexOf (p) - s.SplinePoints.IndexOf (curPoint);

							//make sure that you're not making an illegal move
							bool looping = false;

							if((p == s.StartPoint && curPoint == s.EndPoint) || (p == s.EndPoint && curPoint == s.StartPoint)){
								looping = true;
							}
						if(((indexDifference > 1 || indexDifference < -1) && !s.closed) || ((indexDifference > 1 || indexDifference < -1) && !looping)){
								//this kind of movement should be illegal
						}else{
							if (indexDifference == -1 || indexDifference > 1) {

								//curAngle = s.CompareAngleAtPoint (cursorDir, p, true);
								curAngle = Mathf.Infinity;

							} else {

								if(curPoint.pointType == PointTypes.ghost){
									if(p.pointType == PointTypes.ghost){
										curAngle = 0;
									}else{
										// curAngle = s.CompareAngleAtPoint (lastPoint.GetConnectingSpline(curPoint).GetVelocity(0.99f), curPoint);
										curAngle = 0;
									}
								}else{
									curAngle = s.CompareAngleAtPoint (cursorDir, curPoint);
								}
							}

						if (curAngle < angleToSpline) {
							angleToSpline = curAngle;
							splineDest = s;
							pointDest = p;
						}
					}
				}
			}
			}
		}
			//this is causing bugs

// && (Input.GetButtonDown("Button1")
			if (angleToSpline <= StopAngleDiff) {
				if(curSpline != null){
					curSpline.OnSplineExit ();
				}
				curSpline = splineDest;
				return true;
			}else{
				return false;
			}
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

	void CursorInput (){

		Vector3 lastCursorDir = cursorDir;
		if (controllerConnected) {

			// DO TURNING SPEED HERE

			cursorDir = new Vector3(Input.GetAxis ("Joy X"), Input.GetAxis ("Joy Y"), 0);
			if(usingJoystick){
				cursorDir = Quaternion.Euler(0,0,90) * cursorDir;
			}
//			if (cursorDir.magnitude < 0.1f) {
//				cursorDir = lastCursorDir.normalized/10f;
//			}
		if (cursorDir.magnitude <= 0.1f){
		  joystickLocked = true;
			cursorDir = Vector3.zero;
		}else{
			joystickLocked = false;
		}

			cursorDir = Vector3.Lerp (lastCursorDir, cursorDir, (cursorRotateSpeed/(((Mathf.Abs(flow) * 10) + 1)) * Time.deltaTime));
			//free movement: transform.position = transform.position + new Vector3 (-Input.GetAxis ("Joy X") / 10, Input.GetAxis ("Joy Y") / 10, 0);
			//angle to joystick position
			//zAngle = Mathf.Atan2 (Input.GetAxis ("Joy X"), Input.GetAxis ("Joy Y")) * Mathf.Rad2Deg;
		}else {
//			cursor.transform.RotateAround(transform.position, transform.forward,  Input.GetAxis ("Horizontal"));
//			cursor.transform.position = (transform.position - cursor.transform.position) * Mathf.Sign(Input.GetAxis ("Vertical"));
//			Unused code for Mouse control

//			Vector3 mousePos = new Vector3 (Input.mousePosition.x, Input.mousePosition.y, transform.position.z - Camera.main.transform.position.z);
//			mousePos = Camera.main.ScreenToWorldPoint (mousePos);
//			if((mousePos - transform.position).magnitude < 1){
//				cursorDir = (mousePos - transform.position).normalized;
//			}else{
//				cursorDir = (mousePos - transform.position);
//			}

			cursorDir += new Vector2(Input.GetAxis ("Mouse X"), Input.GetAxis ("Mouse Y"));
			if (cursorDir.magnitude <= 0.01f){
			  joystickLocked = true;
				cursorDir = Vector3.zero;
			}else{
				joystickLocked = false;
			}
			cursorDir = Vector3.Lerp (lastCursorDir, cursorDir, (cursorRotateSpeed/(Mathf.Abs(flow) + 1) * Time.deltaTime));
		}


		if (cursorDir.magnitude > 1) {
			cursorDir.Normalize ();
		}



//		if(curPoint.HasSplines() && curSpline != null){
//			cursorDir.z = curSpline.GetDirection (progress).z * Mathf.Sign(accuracy);
//		}

		// Vector3 screenPos = ((cursorDir/4f) + (Vector3.one/2f));
		// screenPos = new Vector3(screenPos.x, screenPos.y, Camera.main.nearClipPlane + 10f);
		// cursorPos = Camera.main.ViewportToWorldPoint(screenPos);
		// float screenWidth = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, Camera.main.nearClipPlane)).y - transform.position.y;
		// cursorPos = transform.position + ((Vector3)cursorDir * screenWidth);
		
		cursorPos = transform.position + (Vector3)cursorDir / (Services.mainCam.fieldOfView * 0.1f);
//		Vector3 screenPos = ((Vector3)cursorDir/10f + Vector3.one/2f);
		Vector3 screenPos = Services.mainCam.WorldToViewportPoint(transform.position);
		screenPos += (Vector3) cursorDir / 25f;
		screenPos = new Vector3(Mathf.Clamp01(screenPos.x), Mathf.Clamp01(screenPos.y), Mathf.Abs(transform.position.z - Services.mainCam.transform.position.z));
		cursorPos = Services.mainCam.ViewportToWorldPoint(screenPos);
		cursor.transform.position = cursorPos;
		cursor.transform.rotation = Quaternion.Euler(0, 0, (float)(Mathf.Atan2(-cursorDir.x, cursorDir.y) / Mathf.PI) * 180f);
		playerSprite.transform.up = cursorDir;
		l.positionCount = 2;	
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

		float s = 1f/(float)curSpline.curveFidelity;
		for(int i = 0; i < curSpline.curveFidelity * 3; i +=3){
			int index = i/3;
			float step = (float)index/(float)curSpline.curveFidelity;
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
			Vector3 pos =  curSpline.GetPoint(step + Mathf.Epsilon);
			velocityLine.points3[i] = pos;

			float f = (step - progress);

			velocityLine2.points3[i+1] = Vector3.Lerp(velocityLine2.points3[i + 1], velocityLine2.points3[i], Time.deltaTime);
			if(f > s){
				velocityLine.points3[i+1] = pos;
			}
			else if(f <= s && f >= 0){
				if(step == 0){
					velocityLine.points3[i+1] = pos + curSpline.GetDirection(step + 0.01f) * Mathf.Pow((1-Mathf.Abs(f)), 2) * curSpeed * curSpline.distance * (s) * curSpline.curveFidelity/2;
				}else{
				velocityLine.points3[i+1] = pos + curSpline.GetDirection(step) * Mathf.Pow((1-Mathf.Abs(f)), 2) * curSpeed * curSpline.distance * (s - f) * curSpline.curveFidelity/2;
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

	public void Effects(){

		ParticleSystem.EmissionModule e = ps.emission;

		float Absflow = Mathf.Abs (flow);

		if (state == PlayerState.Flying) {
			e.rateOverTimeMultiplier = Mathf.Lerp(0, 50, Mathf.Abs(curSpeed));
//			t.time = Absflow;
			//do shit with particle systems for flying
		} else {
//			t.time = Mathf.Lerp(t.time, 0, Time.deltaTime);
// (accuracy < 0 && flow > 0) || accuracy > 0 && flow <
			if (state != PlayerState.Switching) {
				e.rateOverTimeMultiplier = (1 - Mathf.Abs(accuracy)) * Mathf.Abs(flow) * 25;
			} else {
				e.rateOverTimeMultiplier = Mathf.Lerp (e.rateOverTimeMultiplier, 0, Time.deltaTime * 5);
			}
		}

		if (curSpline != null) {
//			if(flow > 0.25f){
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


	}

	public void PlayAttack (Point point1, Point point2)
	{

//		do some angle shit or normalize it??
		float segmentDistance = Vector3.Distance (point1.Pos, point2.Pos);
		Vector3 linearDirection = point2.Pos - point1.Pos;
		linearDirection = new Vector2(linearDirection.x, linearDirection.y).normalized;
		float dot = Vector2.Dot (linearDirection, Vector2.up);

		// int index = (int)(((dot/2f) + 0.5f) * (sounds.hits.Length - 1));
		GameObject newSound = Instantiate(Services.Prefabs.soundEffectObject, transform.position, Quaternion.identity);
		newSound.GetComponent<AudioSource>().clip = sounds.hits[0];
		noteIndex++;
		newSound.GetComponent<AudioSource>().Play();
		newSound.GetComponent<PlaySound>().enabled = true;
	}

	public void ManageSound ()
	{

		//
		// pointdest is null some of the time
		if(state != PlayerState.Switching){
		// sounds.curPointSound.volume =  Mathf.Pow(curPoint.proximity, 3)/5f;
			// sounds.pointDestSound.volume = Mathf.Sin(pointDest.proximity * Mathf.PI)

			float dot = Vector2.Dot(curSpline.GetDirection (progress), (pointDest.Pos - curPoint.Pos).normalized);
			float curFreqGain;
	 //
			Services.Sounds.master.GetFloat ("CenterFreq", out curFreqGain);
			float lerpAmount = Services.PlayerBehaviour.goingForward ? Services.PlayerBehaviour.progress : 1 - Services.PlayerBehaviour.progress;
			sounds.moveSound.volume = Mathf.Lerp(sounds.moveSound.volume, Mathf.Clamp01(curSpeed), Time.deltaTime);
			sounds.moveSound.pitch = Mathf.Pow(accuracy / 2 + 0.5f, 0.5f);
			sounds.brakingSound.volume = Mathf.Pow(Mathf.Clamp01(0.5f - accuracy)/2, 2);
	 //
			Services.Sounds.master.SetFloat("FreqGain", Mathf.Abs(curSpeed)/2 + 1f);
			Services.Sounds.master.SetFloat("CenterFreq", Mathf.Lerp(curFreqGain, ((dot/2f + 0.5f) + Mathf.Clamp01(1f/Mathf.Pow(curSpline.segmentDistance, 5))) * (16000f / curFreqGain), lerpAmount));

			if(pointDest.pointType != PointTypes.ghost){
				//sounds.pointDestSound.volume = Mathf.Pow(pointDest.proximity, 2)/10f;
			}else{
				sounds.pointDestSound.volume = 0;
			}
		}else{
			sounds.pointDestSound.volume = 0;
			sounds.moveSound.volume = Mathf.Lerp(sounds.moveSound.volume, 0, Time.deltaTime);
			sounds.brakingSound.volume = 0;
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
		Services.Sounds.master.SetFloat("FreqGain", Mathf.Abs(flow)/2 + 1f);
	}

	public Vector3 GetCursorDir(){
		return cursorDir;
	}

	public Vector3 GetCursorVelocity(){
		return cursorPos - transform.position;
	}
}
