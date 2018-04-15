using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


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
	

	public float flow;
	public float negativeflow;
	public float progress;
	public float accuracy;
	public float creationCD = 0.25f;
	public float flyingSpeedThreshold = 3;
	public float cursorDistance;

	public float LineAngleDiff = 30;
	public float StopAngleDiff = 60;

	public float PointDrawDistance;
	public float connectTimeCoefficient;
	public float cursorRotateSpeed = 1;


	public AudioSource AccelerationSound;
	public AudioSource BrakingSound;

	//components I want to access
	private TrailRenderer t;

	public PlayerState state;

	private float boost;

	private bool traversing;
	public bool goingForward = true;
	private bool controllerConnected = false;
	public float connectTime;

	private Vector3 cursorPos, cursorDir;
	private LineRenderer l;
	private List<Point> inventory;
	public Point lastPoint;


	private float curDrawDistance = 0.1f;

	private ParticleSystem ps;
	private float creationInterval = 0.2f;
	private PlayerSounds sounds;

	float angleToSpline = Mathf.Infinity;
	private List<Transform> newPointList;
	bool canFly;

	void Awake(){
		
		curPoint.proximity = 1;
		state = PlayerState.Switching;
		sounds = GetComponent<PlayerSounds> ();
		l = GetComponent<LineRenderer> ();
		t = GetComponentInChildren<TrailRenderer> ();
		traversing = false;
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
			

			bool canTraverse = false;

			if (CanPlayerMove ()) {
				canTraverse = true;
			} else {

				if (Input.GetButton ("Button1") && angleToSpline > LineAngleDiff && creationInterval <= 0) {

					Point nextPoint = null;
					nextPoint = SplineUtil.RaycastFromCamera(cursorPos, 20f);

					if (nextPoint != null && nextPoint != curPoint) {
						SplinePointPair spp = SplineUtil.ConnectPoints (curSpline, curPoint, nextPoint);
							

						bool forcedraw = false;

						if (nextPoint == spp.s.StartPoint()) {

							forcedraw = true;
						}

						bool isEntering = false;
						if (curSpline != null && curSpline != spp.s) {
							isEntering = true;
							curSpline.OnSplineExit ();
						} else if (curSpline == null) {
							isEntering = true;
						}

						curSpline = spp.s;
						curSpline.OnSplineEnter (isEntering, curPoint, spp.p, forcedraw);
						connectTime = 1;
						creationInterval = creationCD;
						SetPlayerAtStart (curSpline, spp.p);
						canTraverse = true;

					} else if (!Input.GetButton ("Button2") && canFly && flow > flyingSpeedThreshold) {

						state = PlayerState.Flying;
						newPointList.Clear ();
						l.positionCount = 1;
						l.SetPosition (0, curPoint.Pos);
						curDrawDistance = 0;
						curSpline.OnSplineExit ();
						curPoint.OnPointExit ();
						boost += boostAmount;
						flow += flowAmount;
						return;
					}


				}
			}

			if (canTraverse && !Input.GetButton ("Button2")) {

				curPoint.OnPointExit ();
				boost += boostAmount;
				flow += flowAmount;
				state = PlayerState.Traversing;

				//this is making it impossible to get off points that are widows. wtf. 
				SetCursorAlignment ();
				PlayerMovement ();

			} else {
				flow -= decay * Time.deltaTime;
				flow = Mathf.Clamp (flow, 0, maxSpeed);
			}
		}

		if (state != PlayerState.Animating && curPoint.HasSplines () && curSpline != null) {
			transform.position = curSpline.GetPoint (progress); 
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
		if ((accuracy < 0.5f && accuracy > -0.5f) || Input.GetButton("Button2")) {
			flow -= decay * Time.deltaTime;
			flow = Mathf.Clamp (flow,  0, maxSpeed);
		}

		if (accuracy < 0) {
			goingForward = false;
		} else {
			goingForward = true;
		}
	}

	public IEnumerator ReturnToLastPoint(){
		
		float speed = 0.001f;
		int index = 0;
		float t = 0; 
		float distance = Vector3.Distance (newPointList [newPointList.Count -1].position, curPoint.Pos);
		
		while(index < newPointList.Count -1){

			speed += Time.deltaTime/10;
			t += Time.deltaTime * ((index + 2)/2);
//			Vector3 lastPos = transform.position;
//			transform.position = Vector3.Lerp (newPointList[index].position, newPointList [index - 1].position, t);
			Transform curJoint = newPointList[newPointList.Count - 1 - index];
			curJoint.position = Vector3.Lerp(curJoint.position, curPoint.Pos, t);
//			float curDistance = Vector3.Distance (newPointList [index].position, transform.position);
			transform.position = newPointList[0].transform.position;
//			sprite.transform.up = transform.position - lastPos;
			l.SetPosition (0, transform.position);

			for(int i = 1; i <= newPointList.Count - index; i++){
				l.SetPosition(i, newPointList[i-1].position);
			}

			l.SetPosition (newPointList.Count - index + 1, curPoint.Pos);


			if (t >= 1) {
				GameObject toDestroy = newPointList [newPointList.Count - 1 - index].gameObject;
				Destroy (toDestroy);

				index++;
				l.positionCount = newPointList.Count - index + 2;

				if (index >= newPointList.Count - 1) {
					Destroy (newPointList [newPointList.Count - 1 - index].gameObject);
				} else {

					newPointList [newPointList.Count - 1 - index].GetComponent<SpringJoint> ().connectedBody = curPoint.GetComponent<Rigidbody>();
			
					distance = Vector3.Distance (newPointList [newPointList.Count - 1 - index].position, curPoint.Pos);
				}


				t = Mathf.Clamp01 (t - 1);
			
			} else {
				yield return null;
			}
		}

		l.positionCount = 0;
		newPointList.Clear ();
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

			s = spp.s;
			curP = spp.p;
//			curP.transform.parent = s.transform;

			index -= 4;
		}


		//could add another point at the player's current position between curP (last in index) and p (destination) to make player position not jump
		//whats with phantom splines
		//must be an error with closed/looping splines getting created and fucking up

		SplinePointPair	sp = SplineUtil.ConnectPoints (s, curP, p);
		curP.GetComponent<SpringJoint> ().connectedBody = p.rb;
		curSpline = sp.s;
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

		float speed;
		// Make drawing points while you skate. 
		//should solve the problems of jumping across new points on the same spline. 
		l.positionCount = newPointList.Count + 2;
		l.SetPosition (newPointList.Count + 1, curPoint.Pos);

		for (int i = newPointList.Count; i > 0; i--) {
			l.SetPosition (i, newPointList [i-1].position);
		}
			
		l.SetPosition (0, transform.position);

		Point RaycastHitObj = SplineUtil.RaycastFromCamera (transform.position, 50f);

		if (RaycastHitObj != null && RaycastHitObj.GetComponent<Point> ().isPlaced && RaycastHitObj != curPoint) {
			state = PlayerState.Animating;
			StartCoroutine(FlyIntoNewPoint(RaycastHitObj));

		}else if (flow < 0) {
//			CreateJoint (newPointList[newPointList.Count-1].GetComponent<Rigidbody>());

			state = PlayerState.Animating;
			newPointList [newPointList.Count-1].GetComponent<SpringJoint> ().connectedBody = curPoint.GetComponent<Rigidbody> ();

			for(int i = 0; i < newPointList.Count-1; i++){
				newPointList [i].GetComponent<SpringJoint> ().connectedBody = newPointList [i+1].GetComponent<Rigidbody>();
			}

			StartCoroutine (ReturnToLastPoint ());

		} else {
			inertia = cursorDir * flow;
			flow -= Time.deltaTime;
			transform.position += inertia * Time.deltaTime;

			if (newPointList.Count == 0) {
				curDrawDistance = Vector3.Distance (transform.position, curPoint.Pos);
			} else {
				curDrawDistance = Vector3.Distance (newPointList [newPointList.Count - 1].position, curPoint.Pos);
			}

			if (curDrawDistance >= PointDrawDistance) {
				curDrawDistance = 0;
				if (newPointList.Count == 0) {
					CreateJoint (GetComponent<Rigidbody>());
				} else {
					CreateJoint (newPointList [newPointList.Count - 1].GetComponent<Rigidbody> ());
				}
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

		flow += Mathf.Pow(Mathf.Abs(accuracy), 2) * acceleration * Time.deltaTime;
//		Mathf.Abs(accuracy)
		progress += (((flow + boost + speed)) * Mathf.Sign(accuracy) * Time.deltaTime * Mathf.Pow(Mathf.Abs(accuracy), 2))/curSpline.distance;

		//set player position to a point along the curve

		if (curPoint == curSpline.Selected) {
			curPoint.proximity = 1 - progress;
			if (curSpline.closed && curSpline.SplinePoints.IndexOf(curPoint) >= curSpline.SplinePoints.Count-1) {
				curSpline.SplinePoints [curSpline.LoopIndex].proximity = progress;
			} else {
				
				curSpline.SplinePoints [Mathf.Clamp(curSpline.GetPointIndex(curSpline.Selected)+1, 0, curSpline.SplinePoints.Count-1)].proximity = progress;
			}

		} else {
			curPoint.proximity = progress;
			curSpline.Selected.proximity = 1 - progress;
		}

		GetComponent<Rigidbody> ().velocity = curSpline.GetDirection (progress) * flow * Mathf.Sign (accuracy);

//		transform.Rotate (0, 0, flow*5);
	}

	void CheckProgress(){

		if (progress > 1 || progress < 0) {

		
			Point PointArrivedAt = curPoint;

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
			curPoint.GetComponent<Rigidbody> ().AddForce (cursorDir * flow * 5);

			if (curPoint.IsOffCooldown ()) {
				curPoint.OnPointEnter ();
			}

			if (PointArrivedAt != curPoint) {
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
			progress = 1;

		} else {
			progress = 0;
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
		

	public bool CanPlayerMove(){

		angleToSpline = Mathf.Infinity;

		if (curPoint.HasSplines ()) {

			Spline closestSpline = null;
			Point pointDest = null;

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
				
			if (angleToSpline <= StopAngleDiff) {
				bool isEntering = false;
				SetPlayerAtStart (closestSpline, pointDest);
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
				curSpline.OnSplineEnter (isEntering, curPoint, pointDest);
				connectTime = 1;
				return true;
			}
		}
		return false;
	}
		



	public void OnTriggerEnter(Collider col){
		if (col.tag == "Point") {
			if (!col.GetComponent<Point> ().isPlaced) {
				StartCoroutine (CollectPoint (col.GetComponent<Point> ()));
			}
		}
	}

	public void OnTriggerStay(Collider col){
		if (col.tag == "Point") {
			if(traversing){
			}
		}
	}

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

		if (controllerConnected) {

			cursorDir = new Vector3(-Input.GetAxis ("Joy X"), Input.GetAxis ("Joy Y"), 0);
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
			sprite.transform.RotateAround (transform.position, transform.forward, -Input.GetAxis("Horizontal") * cursorRotateSpeed * Time.deltaTime);
			cursorDir = sprite.transform.up;
		}
			
			
		if (cursorDir.magnitude > 1) {
			cursorDir.Normalize ();

		}

		if(curPoint.HasSplines() && curSpline != null){
			cursorDir.z = curSpline.GetDirection (progress).z * Mathf.Sign(accuracy);
		}



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

//			t.time = Absflow;
			//do shit with particle systems for flying
		} else {
//			t.time = Mathf.Lerp(t.time, 0, Time.deltaTime);
		}

		if (canFly) {
			t.time = 2f;
		} else {
			t.time = 0.25f;
		}
			
//		e.rateOverTimeMultiplier = (int)Mathf.Lerp (0, flow * 25, Mathf.Pow (1 - Mathf.Abs (accuracy), 2));
//		BrakingSound.volume = Mathf.Clamp01(1- Mathf.Abs (accuracy))/6;
//		AccelerationSound.volume = Mathf.Clamp01(flow / (maxSpeed/5));

		if (curSpline != null) {
//			curSpline.DrawLineSegmentVelocity (progress, Mathf.Sign (accuracy), goingForward ? 0 : 1);\
			curSpline.l.material.mainTextureOffset -= Vector2.right * Mathf.Sign (accuracy) * flow * curSpline.l.material.mainTextureScale.x * 2 * Time.deltaTime;
//			l.SetPosition(0, transform.position);
//			l.SetPosition(1, transform.position + (curSpline.GetDirection(progress) * Mathf.Sign(accuracy))/2);
//			l.SetPosition(1, transform.position + cursorDir/2);
//			GetComponentInChildren<Camera>().farClipPlane = Mathf.Lerp(GetComponentInChildren<Camera>().farClipPlane,  flow + 12, Time.deltaTime * 10);
		}


	}
		

	public bool GetTraversing(){
		return traversing;
	}

	public Vector3 GetCursorDir(){
		return cursorDir;
	}

	public Vector3 GetCursorVelocity(){
		return cursorPos - transform.position;
	}
}