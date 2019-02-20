using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum PlayerState{Traversing, Switching, Flying, Animating};

public class PlayerBehaviour: MonoBehaviour {

	public PlayerState state;

	[Header("Current Spline")]
	public Spline curSpline;
	[Space(10)]

	[Header("Points")]
	public Point curPoint;
	public Point pointDest;
	public Point lastPoint;
	[Space(10)]

	[Header("Movement Tuning")]
	public float speed;
	public float maxSpeed;
	public float acceleration;
	public float decay;
	public float accuracyCoefficient;
	public float flowAmount = 0.1f;
	public float boostAmount = 0.1f;
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
	public float progress, accuracy, flow, boost, curSpeed, connectTime, connectTimeCoefficient;

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
	public Transform pointInfo;
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

	private GameObject cursor;
	[HideInInspector]
	public Vector3 cursorPos;
	[HideInInspector]
	public Vector2 cursorDir;

	void Awake(){
		playerSprite = GetComponentInChildren<SpriteRenderer>();
		sound = GetComponent<AudioSource>();
		Cursor.lockState = CursorLockMode.Locked;
		pointDest = null;
		traversedPoints = new List<Point> ();
		traversedPoints.Add (curPoint);
		curPoint.proximity = 1;

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

	void Start(){
		cursor = Services.Cursor;
		cursorSprite = Services.Cursor.GetComponent<Image>();
		curPoint.OnPointEnter ();
	}

	void Update () {

		Point.hitColorLerp = connectTime;

		float speedCoefficient;
		if(state == PlayerState.Switching || state == PlayerState.Animating){
			speedCoefficient = 0;
		}else if (state == PlayerState.Flying){
			speedCoefficient = curSpeed;
		}else{
			speedCoefficient = Mathf.Pow(accuracy, 5);
		}

		playerSprite.transform.localScale = Vector3.Lerp(playerSprite.transform.localScale, new Vector3(Mathf.Clamp(1 - (speedCoefficient * 2), 0.1f, 0.25f), Mathf.Clamp(speedCoefficient, 0.25f, 0.75f), 0.25f), Time.deltaTime * 10);

		if (connectTime <= 0 && PointManager._pointsHit.Count > 0) {
			PointManager.ResetPoints ();
			connectTime = 1;
		}

		CursorInput();
		Effects ();

		List<Spline> splinesToUpdate = new List<Spline>();
		if(curSpline != null){
			curSpline.DrawSpline();
			curSpline.UpdateSpline();
			ManageSound();
			splinesToUpdate.Add(curSpline);
		}

		foreach(Spline s in curPoint._connectedSplines){
			if(s != curSpline || !splinesToUpdate.Contains(s)){
		 	s.DrawLineSegment(s.SplinePoints.IndexOf(curPoint));
			splinesToUpdate.Add(s);
			}
		}

		if(pointDest != null){
			foreach(Spline s in pointDest._connectedSplines){
				if(s != curSpline || !splinesToUpdate.Contains(s)){
					s.DrawLineSegment(s.SplinePoints.IndexOf(pointDest));
				}
			}
		}

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

		}else if(state == PlayerState.Switching) {
			curSpeed = 0;
			transform.position = curPoint.Pos;
			PlayerOnPoint();
		}

		if (state != PlayerState.Animating && curPoint.HasSplines () && curSpline != null) {
			if(state == PlayerState.Switching){
				transform.position = curPoint.Pos;
			}

			if(state == PlayerState.Traversing){

				transform.position = curSpline.GetPoint(progress);
			}

			if (traversedPoints.Count >= 2 && Mathf.Abs (flow) <= 0) {
				// StartCoroutine (Unwind());
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

			if (CanLeavePoint ()) {

			if(Input.GetButton ("Button1")){

				canTraverse = true;
				LeaveSpline();
		 }else{
			 canTraverse = false;
			 cursorSprite.sprite = traverseSprite;
			 l.positionCount = 0;
 			cursorOnPoint.positionCount = 0;
		 }
		} else {
				if(CanCreatePoint()){
					if(Input.GetButtonDown("Button1")){
						canTraverse = true;
						CreatePoint();
						// PlayAttack(curPoint, pointDest);
					}else{
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
						if(Input.GetButtonUp("Button1")){
							Fly();
							return;
						}
				 }else{
					l.positionCount = 0;
		 			cursorOnPoint.positionCount = 0;
		 			cursorSprite.sprite = brakeSprite;
				 }
			}

		if(canTraverse){
			pointInfo.GetComponent<Text>().text = "";
			LeavePoint();
		}else{
			if(pointDest != null && pointDest.lockAmount > 0){
				pointInfo.GetComponent<Text>().text = pointDest.lockAmount + "•";
				pointInfo.position = pointDest.Pos + Vector3.right/5f;
			}else{
				pointInfo.GetComponent<Text>().text = "";
			}
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

				if (pointDest != null && pointDest != curPoint && !pointDest._connectedSplines.Contains(curSpline) && pointDest.isUnlocked() && !pointDest.IsAdjacent(curPoint)) {
				  return true;
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
		if (Mathf.Abs(flow) > flyingSpeedThreshold && curPoint.pointType == PointTypes.fly){
			l.positionCount = 2;
			l.SetPosition (0, cursorPos);
			l.SetPosition (1, transform.position);
			return true;
		}
		return false;
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

		curPoint.OnPointExit ();
		state = PlayerState.Traversing;
		connectTime = 1f;
		//this is making it impossible to get off points that are widows. wtf.
		SetPlayerAtStart (curSpline, pointDest);

		if (!goingForward) {
			//UNIDIRECTIONAL MOVEMENT
			flow = -Mathf.Abs (flow);
			if(curPoint.IsOffCooldown()){
			// flow -= flowAmount;
			}
			if(Mathf.Abs(flow) < 1){
				boost = -boostAmount;
			}
		} else {
			flow = Mathf.Abs (flow);
			if(curPoint.IsOffCooldown()){
			// flow += flowAmount;
			}
			if(Mathf.Abs(flow) < 1){
				boost = boostAmount;
			}
		}


		curSpline.OnSplineEnter (true, curPoint, pointDest, false);
		SetCursorAlignment ();
		PlayerMovement ();
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

	void FreeMovement(){

		Vector3 inertia;

		// Make drawing points while you skate.
		//should solve the problems of jumping across new points on the same spline.

		Point overPoint = SplineUtil.RaycastDownToPoint(cursorPos, 2f, 1f);
		if(overPoint != null && overPoint != curPoint){
			//Getting a null ref here for some ungodly reason
			if(Vector3.Distance (curPoint.Pos, drawnPoint.Pos) < 0.25f){
				curSpline.SplinePoints.Remove(curPoint);
				drawnPoint._neighbours.Remove(curPoint);
				Destroy(curPoint.gameObject);
				curPoint = drawnPoint;
				curSpline.Selected = drawnPoint;
				// SplinePointPair spp = SplineUtil.ConnectPoints(curSpline, drawnPoint, overPoint);
				// curSpline = spp.s;
				// drawnPoint = spp.p;
				// traversedPoints.Add(drawnPoint);
				//
				// SplinePointPair sppp = SplineUtil.ConnectPoints(curSpline, drawnPoint, curPoint);
				// curSpline = sppp.s;
				// curSpline.Selected = drawnPoint;
				// curSpline.OnSplineEnter (true, drawnPoint, curPoint, false);
			}

			if(CanCreatePoint()){
				//remove current point from curspline and connect drawnPoint to pointDest on current spline
				CreatePoint();
				curPoint.GetComponent<Collider>().enabled = true;
				curPoint.velocity = cursorDir * Mathf.Abs(flow);
				curPoint.isKinematic = false;
				LeavePoint();
				return;
			}
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
			flow -= Time.deltaTime * decay;
			transform.position += inertia * Time.deltaTime;
			curPoint.transform.position = transform.position;
			curPoint.originalPos = transform.position;
			curDrawDistance = Vector3.Distance (drawnPoint.Pos, curPoint.Pos);
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

		accuracyCoefficient = Mathf.Pow(Mathf.Abs(accuracy), 2);
		if (accuracy < -0.5f || accuracy > 0.5f) {
			if (flow > 0 && accuracy < 0) {
				flow += decay *  accuracy * Time.deltaTime;
				if (flow < 0)
					flow = 0;
			}else if(flow < 0 && accuracy > 0){
				flow += decay *  accuracy * Time.deltaTime;
				if (flow > 0)
					flow = 0;
			}else{
				//
			maxSpeed = curSpline.distance;

			if(Mathf.Abs(flow) < maxSpeed){
			flow += Mathf.Sign (accuracy) * accuracyCoefficient * acceleration * Time.deltaTime;
		}
		}
	}
		// curSpeed =  speed * Mathf.Sign (accuracy) * accuracyCoefficient;
		// if ((curSpeed > 0 && flow < 0) || (curSpeed < 0 && flow > 0)) {
		// 	curSpeed = 0;
		// }

		if ((accuracy < 0.5f && accuracy > -0.5f) || joystickLocked) {

			if (flow > 0) {
				// flow -= decay * (2f - accuracy) * Time.deltaTime;
				if (flow < 0)
					flow = 0;
			} else if(flow < 0){
				// flow += decay * (2f + accuracy) * Time.deltaTime;
				if (flow > 0)
					flow = 0;
			}
		}

		float adjustedAccuracy = goingForward ? Mathf.Clamp(accuracy, 0.5f, 1f) : -Mathf.Clamp(accuracy, -1, -0.5f);
		// (adjustedAccuracy + 0.1f)
		curSpeed = ((flow + boost + (speed * Mathf.Sign(flow)))/curSpline.distance) * adjustedAccuracy;
		progress += curSpeed * Time.deltaTime;
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

		if(pointDest != null && pointDest.hasPointcloud){
			CameraFollow.desiredFOV = Mathf.Lerp(CameraFollow.desiredFOV, pointDest.pointCloud.desiredFOV, pointDest.proximity);
		}


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

			Point PointArrivedAt = curPoint;
			curPoint.proximity = 0;
			if (progress > 1) {

				progress = 1;

				if (curSpline.Selected == curSpline.EndPoint && curSpline.closed) {
					curPoint = curSpline.StartPoint;
				} else {
					curPoint = curSpline.SplinePoints [curSpline.GetPointIndex(curSpline.Selected) + 1];
				}

			} else if (progress < 0) {

				progress = 0;
				curPoint = curSpline.Selected;

			}

			curPoint.proximity = 1;
			curPoint.velocity = cursorDir * Mathf.Abs(flow);

//			if (curPoint.IsOffCooldown ()) {
				curPoint.OnPointEnter ();
				PlayAttack(PointArrivedAt, curPoint);
//			}

			if (PointArrivedAt != curPoint) {
				traversedPoints.Add (curPoint);
				lastPoint = PointArrivedAt;
			}

			if(PointArrivedAt.pointType == PointTypes.boost){
				traversedPoints.Clear();
				traversedPoints.Add(PointArrivedAt);
			}


			state = PlayerState.Switching;
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

	void LeaveSpline(){
		bool isEntering = false;

		if (curSpline != null) {
			curSpline.OnSplineExit ();
			isEntering = true;
		} else if(curSpline == null){
			isEntering = true;
		}

//				if (lastPoint != pointDest) {
//					curSpline.OnSplineEnter (isEntering, curPoint, pointDest);
//					connectTime = 1;
//				}
		connectTime = 1;
	}

	public bool CanLeavePoint(){

		angleToSpline = Mathf.Infinity;

		if (curPoint.HasSplines ()) {

			Spline closestSpline = null;
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
								curAngle = s.CompareAngleAtPoint (cursorDir, curPoint);
							}

						if (curAngle < angleToSpline) {
							angleToSpline = curAngle;
							closestSpline = s;
							pointDest = p;
						}
					}
				}
			}
			}
		}
			//this is causing bugs

// && (Input.GetButtonDown("Button1")
			if (angleToSpline <= StopAngleDiff && pointDest.isUnlocked()) {

				curSpline = closestSpline;

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
			transform.up = cursorDir;
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
		  joystickLocked = false;
			cursorDir += new Vector2(Input.GetAxis ("Mouse X"), Input.GetAxis ("Mouse Y"));
			if (cursorDir.magnitude > 1) {
				cursorDir.Normalize ();
			}
			cursorDir = Vector3.Lerp (lastCursorDir, cursorDir, (cursorRotateSpeed/(Mathf.Abs(flow) + 1) * Time.deltaTime));
			transform.up = cursorDir;
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
		float screenWidth = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, Camera.main.nearClipPlane + 1.25f)).y - transform.position.y;
		cursorPos = transform.position + ((Vector3)cursorDir * screenWidth);
		cursor.transform.position = cursorPos;
		cursor.transform.rotation = Quaternion.Euler(0, 0, (float)(Mathf.Atan2(-cursorDir.x, cursorDir.y) / Mathf.PI) * 180f);

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

	public void Effects(){

		ParticleSystem.EmissionModule e = ps.emission;

		float Absflow = Mathf.Abs (flow);

		if (state == PlayerState.Flying) {
			e.rateOverTimeMultiplier = Mathf.Lerp(0, 50, Mathf.Abs(flow));
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
		switch(state){
//		Services.PlayerBehaviour.flow / (Services.PlayerBehaviour.maxSpeed/2))
	  case PlayerState.Traversing:
		// brakingSound.volume = 1 - accuracyCoefficient;
		// sound.volume = Mathf.Clamp01(curSpeed/2);
		sound.volume = 0.05f;
		float dot = Vector2.Dot(curSpline.GetDirection (progress), pointDest.Pos - curPoint.Pos);
		float curFreqGain;

		Services.Sounds.master.GetFloat ("CenterFreq", out curFreqGain);
		float lerpAmount = Services.PlayerBehaviour.goingForward ? Services.PlayerBehaviour.progress : 1 - Services.PlayerBehaviour.progress;

		Services.Sounds.master.SetFloat("FreqGain", Mathf.Abs(Services.PlayerBehaviour.flow)/2 + 1f);
		// Services.Sounds.master.SetFloat("CenterFreq", Mathf.Lerp(curFreqGain, ((dot/2f + 0.5f) + Mathf.Clamp01(1f/Mathf.Pow(curSpline.segmentDistance, 5))) * (16000f / curFreqGain), lerpAmount));

		break;

		case PlayerState.Flying:
		brakingSound.volume = Mathf.Lerp(brakingSound.volume, 0, Time.deltaTime * 5);
		sound.volume = Mathf.Lerp(sound.volume, Mathf.Clamp01(Mathf.Abs(flow)), Time.deltaTime * 5);
		break;

		case PlayerState.Switching:
		brakingSound.volume = Mathf.Lerp(brakingSound.volume, 0, Time.deltaTime * 5);
		sound.volume = Mathf.Lerp(sound.volume, 0, Time.deltaTime * 10);
		break;

		case PlayerState.Animating:
		brakingSound.volume = Mathf.Lerp(brakingSound.volume, 1, Time.deltaTime);
		break;
	}
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
