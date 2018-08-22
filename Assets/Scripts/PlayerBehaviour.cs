using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum PlayerState{Traversing, Switching, Flying, Animating};

public class PlayerBehaviour: MonoBehaviour {


	[Header("Current Spline")]
	public Spline curSpline;

	[Header("Current Point")]
	public Point curPoint;

	[Header("Cursor")]
	public GameObject cursor;

	[Header("Sprite")]
	public GameObject sprite;

	[Header("Speed")]
	public float speed;

	[Header("Decay")]
	public float decay;

	[Header("Acceleration")]
	public float acceleration;

	[Header("Boost")]
	public float flowAmount = 0.1f;
	public float boostAmount = 0.1f;

	[Header("Max Speed")]
	public float maxSpeed;

	private Spline drawnSpline;
	private Point drawnPoint;

	public float flow;
	public float negativeflow;
	public float progress;
	public float accuracy;
	public float accuracyCoefficient;
	public float curSpeed;
	public float creationCD = 0.25f;
	public float flyingSpeedThreshold = 3;
	public float cursorDistance;

	public float LineAngleDiff = 30;
	public float StopAngleDiff = 60;

	public float PointDrawDistance;
	public float connectTimeCoefficient;
	public float cursorRotateSpeed = 1;
	private List<Point> traversedPoints;

	public AudioSource AccelerationSound;
	public AudioSource BrakingSound;

	//components I want to access
	private TrailRenderer t;

	public PlayerState state;

	public float boost;
	Point pointDest;

	public bool goingForward = true;
	private bool controllerConnected = false;
	public float connectTime;

	public Vector3 cursorPos, cursorDir;
	private LineRenderer l;
	private List<Point> inventory;
	public Point lastPoint;

	float decayTimer;
	private float curDrawDistance = 0.1f;

	private ParticleSystem ps;
	private float creationInterval = 0.2f;
	private PlayerSounds sounds;

	float angleToSpline = Mathf.Infinity;
	private List<Transform> newPointList;
	bool canFly;

	public bool joystickLocked;

	void Awake(){
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

//		while(i < 50) {
//			GameObject p = (GameObject)Instantiate (PointPrefab, Vector3.zero, Quaternion.identity);
//			StartCoroutine(CollectPoint (p.GetComponent<Point> ()));
//			i++;
//		}
//

		lastPoint = null;


	}

	void Start(){
		curPoint.OnPointEnter ();
	}

	void Update () {

		canFly = PointManager.PointsHit ();
		connectTime -= Time.deltaTime / connectTimeCoefficient;
		Point.hitColorLerp = connectTime;

		if (connectTime < 0 && PointManager._pointsHit.Count > 0) {
			PointManager.ResetPoints ();
			connectTime = 0;
		}

		CursorInput();
		Effects ();

		if(pointDest != null){
		List<Spline> splinesToUpdate = new List<Spline>();
		splinesToUpdate = curPoint._connectedSplines.Union(pointDest._connectedSplines).ToList();

			foreach(Spline s in splinesToUpdate){
				s.Draw();
			}
		}else{
		foreach(Spline s in curPoint._connectedSplines){
			s.Draw();
		}
	}

		creationInterval-= Time.deltaTime;



		if (state == PlayerState.Traversing && curSpline != null) {

			SetCursorAlignment ();
		}

		if (state == PlayerState.Flying) {
			FreeMovement ();
			return;
		}

		if (state == PlayerState.Traversing) {
			PlayerMovement ();
			CheckProgress ();

		}else if(state == PlayerState.Switching) {

			transform.position = curPoint.Pos;
			bool canTraverse = false;

			if (CanLeavePoint ()) {
				canTraverse = true;
			} else {
				if(Input.GetButtonUp ("Button1")){
					canTraverse = CanCreatePoint();
					if (!canTraverse){
						if(TryToFly()){
							return;
					 }
					}
				}
		  }

			if(canTraverse){
				LeavePoint();
			}else{
				StayOnPoint();
			}
		}

		if (state != PlayerState.Animating && curPoint.HasSplines () && curSpline != null) {
			transform.position = curSpline.GetPoint (progress);
			if (traversedPoints.Count >= 2 && Mathf.Abs (flow) <= 0) {
				StartCoroutine (Unwind());
			}
		}


		#region
		if (Input.GetAxis ("Joy Y") != 0) {
			controllerConnected = true;
		}
		#endregion
	}

	public void SetCursorAlignment(){
		float alignment = Vector2.Angle (cursorDir, curSpline.GetDirection (progress));
		flow = Mathf.Clamp (flow, -maxSpeed, maxSpeed);

		accuracy = (90 - alignment) / 90;

		StopAngleDiff = Mathf.Lerp (20, 50, Mathf.Abs(flow));

		if (accuracy < 0) {
			goingForward = false;
		} else {
			goingForward = true;
		}
	}

	bool CanCreatePoint(){

			if (!joystickLocked) {

				pointDest = null;
				pointDest = SplineUtil.RaycastFromCamera(cursorPos, 20f);

				if (pointDest != null && pointDest != curPoint) {
					if(pointDest.pointType != PointTypes.leaf || (pointDest.pointType == PointTypes.leaf && pointDest.NeighbourCount() == 0) && curPoint.pointType != PointTypes.leaf){
					SplinePointPair spp = SplineUtil.ConnectPoints (curSpline, curPoint, pointDest);

					bool isEntering = false;

					if (curSpline != null && curSpline != spp.s) {
						isEntering = true;
						curSpline.OnSplineExit ();
					}


					curSpline = spp.s;
					pointDest = spp.p;
					connectTime = 1;
				  return true;

				}
			}
		}
		return false;
	}

	bool TryToFly(){
		if (Mathf.Abs(flow) > flyingSpeedThreshold && curPoint.pointType == PointTypes.fly) {

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
			return true;
		}
		return false;
	}

	void LeavePoint(){

		if (!goingForward) {
			if(curPoint.IsOffCooldown()){
			flow -= flowAmount;
		}
			boost = -boostAmount;
		} else {
			if(curPoint.IsOffCooldown()){
			flow += flowAmount;
		}
			boost = boostAmount;
		}

		curPoint.OnPointExit ();

		state = PlayerState.Traversing;
		decayTimer = 0.5f;
		//this is making it impossible to get off points that are widows. wtf.
		SetPlayerAtStart (curSpline, pointDest);
		curSpline.OnSplineEnter (true, curPoint, pointDest, false);

		SetCursorAlignment ();
		PlayerMovement ();
	}

	void StayOnPoint(){
		decayTimer -= Time.deltaTime;
		if (decayTimer < 0) {
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
		Debug.Log("returning to last Point");
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
			if (curP != curPoint) {
				curP.GetComponent<SpringJoint> ().autoConfigureConnectedAnchor = true;
				curP.GetComponent<SpringJoint> ().connectedBody = nextp.rb;
			}

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
	  drawnPoint.GetComponent<SpringJoint> ().connectedBody = p.rb;
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
//		RaycastHitObj.GetComponent<Point> ().isPlaced
// && RaycastHitObj != null && RaycastHitObj != curPoint)
		if (!joystickLocked && Input.GetButtonDown ("Button1")) {

			if(CanCreatePoint()){
				curPoint.GetComponent<Collider>().enabled = true;
				curPoint.velocity = cursorDir * Mathf.Abs(flow);
				curPoint.isKinematic = false;
				LeavePoint();
				return;
			}else{
				// if(pointDest == curPoint){
					// ???????????????????????
				// 	StartCoroutine (ReturnToLastPoint ());
				// 	return;
				// }
			}

			// if(RaycastHitObj == curPoint){
			// 	StartCoroutine (ReturnToLastPoint ());
			// }else{
			// StartCoroutine(FlyIntoNewPoint(RaycastHitObj));
			// }
		}else{
			if(CanCreatePoint()){
				curPoint.GetComponent<Collider>().enabled = true;
				curPoint.velocity = cursorDir * Mathf.Abs(flow);
				curPoint.isKinematic = false;
				LeavePoint();
				return;
			}
		}

		 if (flow < 0) {
//			CreateJoint (newPointList[newPointList.Count-1].GetComponent<Rigidbody>());
			// StartCoroutine (ReturnToLastPoint ());
			StartCoroutine(Unwind());

		} else {
			inertia = cursorDir * flow;
			flow -= Time.deltaTime / 2f;
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
		accuracyCoefficient = Mathf.Pow(Mathf.Abs(accuracy), 3);
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
			flow += Mathf.Sign (accuracy) * accuracyCoefficient * acceleration * Time.deltaTime;
		}
	}
		// curSpeed =  speed * Mathf.Sign (accuracy) * accuracyCoefficient;
		// if ((curSpeed > 0 && flow < 0) || (curSpeed < 0 && flow > 0)) {
		// 	curSpeed = 0;
		// }

		if ((accuracy < 0.5f && accuracy > -0.5f) || joystickLocked) {
			if (flow > 0) {
				flow -= decay * (2f - accuracy) * Time.deltaTime;
				if (flow < 0)
					flow = 0;
			} else if(flow < 0){
				flow += decay * (2f + accuracy) * Time.deltaTime;
				if (flow > 0)
					flow = 0;
			}
		}

		float adjustedAccuracy = goingForward ? Mathf.Clamp01(accuracy) : -Mathf.Clamp(accuracy, -1, 0);
		progress += ((flow * adjustedAccuracy + boost + curSpeed)/curSpline.distance) * Time.deltaTime;

		boost = Mathf.Lerp (boost, 0, Time.deltaTime * 2);
		//set player position to a point along the curve

		if (curPoint == curSpline.Selected) {
			curPoint.proximity = 1 - progress;
			pointDest.proximity = progress;

			if (curSpline.closed && curSpline.SplinePoints.IndexOf(curPoint) >= curSpline.SplinePoints.Count-1) {
				curSpline.SplinePoints [curSpline.LoopIndex].proximity = progress;
			} else {
				// ??? what the fuck am I looking at
				curSpline.SplinePoints [Mathf.Clamp(curSpline.GetPointIndex(curSpline.Selected)+1, 0, curSpline.SplinePoints.Count-1)].proximity = progress;
			}

		} else {
			curPoint.proximity = progress;
			curSpline.Selected.proximity = 1 - progress;
		}

		GetComponent<Rigidbody> ().velocity = curSpline.GetDirection (progress) * flow;

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


			Point PointArrivedAt = curPoint;
			curPoint.proximity = 0;
			if (progress > 1) {

				progress = 1;

				if (curSpline.Selected == curSpline.EndPoint() && curSpline.closed) {
					curPoint = curSpline.StartPoint();
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
//			}

			if (PointArrivedAt != curPoint) {
				traversedPoints.Add (curPoint);
				lastPoint = PointArrivedAt;
			}


			state = PlayerState.Switching;
		}
	}

	public void SetPlayerAtStart(Spline s, Point p2){
		int indexdiff = s.SplinePoints.IndexOf (p2) - s.SplinePoints.IndexOf (curPoint);

		if (indexdiff == -1 || indexdiff > 1) {
			s.Selected = p2;
			goingForward = false;
			progress = 1 - Mathf.Epsilon;
			flow = -Mathf.Abs (flow);

		} else {
			progress = 0 + Mathf.Epsilon;
			goingForward = true;
			s.Selected = curPoint;
			flow = Mathf.Abs (flow);
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

			Spline closestSpline = null;
			pointDest = null;

			foreach (Spline s in curPoint.GetSplines()) {


				foreach (Point p in curPoint.GetNeighbours()) {

					if (!p._connectedSplines.Contains (s)) {
						//do nothing if the point is in another spline
					} else {

						float curAngle = Mathf.Infinity;

						int indexDifference = s.SplinePoints.IndexOf (p) - s.SplinePoints.IndexOf (curPoint);
						if ((indexDifference > 1 || indexDifference < -1) && !s.closed) {

						} else {
							if (indexDifference == -1 || indexDifference > 1) {
								curAngle = s.CompareAngleAtPoint (cursorDir, p, true);
							} else {
								curAngle = s.CompareAngleAtPoint (cursorDir, curPoint);
							}
						}

						if (curAngle < angleToSpline) {
							angleToSpline = curAngle;
							closestSpline = s;
							pointDest = p;
						}
					}
				}
			}
// && (Input.GetButtonDown("Button1")
			if (angleToSpline <= StopAngleDiff && (Input.GetButtonUp ("Button1") || Mathf.Abs(flow) > 1) && !joystickLocked && !Input.GetButton("Button1")) {
				bool isEntering = false;

				if (curSpline != null && curSpline != closestSpline) {
					curSpline.OnSplineExit ();
					isEntering = true;
				} else if(curSpline == null){
					isEntering = true;
				}

				curSpline = closestSpline;
//				if (lastPoint != pointDest) {
//					curSpline.OnSplineEnter (isEntering, curPoint, pointDest);
//					connectTime = 1;
//				}

				connectTime = 1;
				return true;
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
			sprite.transform.up = cursorDir;
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
			cursorDir += new Vector3(Input.GetAxis ("Mouse X"), Input.GetAxis ("Mouse Y"), 0);
			if (cursorDir.magnitude > 1) {
				cursorDir.Normalize ();
			}
			cursorDir = Vector3.Lerp (lastCursorDir, cursorDir, (cursorRotateSpeed/(Mathf.Abs(flow) + 1) * Time.deltaTime));
			sprite.transform.up = cursorDir;
		}


		if (cursorDir.magnitude > 1) {
			cursorDir.Normalize ();

		}


//		if(curPoint.HasSplines() && curSpline != null){
//			cursorDir.z = curSpline.GetDirection (progress).z * Mathf.Sign(accuracy);
//		}



		cursorPos = transform.position + cursorDir * cursorDistance;
		cursor.transform.position = cursorPos;
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

//		if (canFly) {
//			t.time = 2f;
//		} else {
//			t.time = 0.25f;
//		}

		if (curSpline != null) {
//			curSpline.DrawLineSegmentVelocity (progress, Mathf.Sign (accuracy), goingForward ? 0 : 1);\
			// curSpline.l.material.mainTextureOffset -= Vector2.right * Mathf.Sign (accuracy) * flow * curSpline.l.material.mainTextureScale.x * 2 * Time.deltaTime;
//			l.SetPosition(0, transform.position);
//			l.SetPosition(1, transform.position + (curSpline.GetDirection(progress) * Mathf.Sign(accuracy))/2);
//			l.SetPosition(1, transform.position + cursorDir/2);
//			GetComponentInChildren<Camera>().farClipPlane = Mathf.Lerp(GetComponentInChildren<Camera>().farClipPlane,  flow + 12, Time.deltaTime * 10);
		}


	}


	public Vector3 GetCursorDir(){
		return cursorDir;
	}

	public Vector3 GetCursorVelocity(){
		return cursorPos - transform.position;
	}
}
