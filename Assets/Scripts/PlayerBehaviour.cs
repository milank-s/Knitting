using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public enum PlayerState{Traversing, Switching, Flying, Animating};

public class PlayerBehaviour: MonoBehaviour {

	public GameObject PointPrefab;
	public GameObject SplinePrefab;



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
	

	public float LineAngleDiff = 30;
	public float StopAngleDiff = 60;

	public float cursorRotateSpeed = 1;

	public SplineWalkerMode mode;

	//components I want to access
	private TrailRenderer t;

	private PlayerState state;

	private float boost;
	public float flow;
	public float negativeflow;
	public float progress;
	public float accuracy;
	public float creationCD = 0.25f;
	public float flyingSpeedThreshold = 3;
	public float cursorDistance;
	private bool traversing;
	public bool goingForward = true;
	private bool controllerConnected = false;

	private Vector3 cursorPos, cursorDir;
	private LineRenderer l;
	private List<Point> inventory;
	public Point lastPoint;

	public float PointDrawDistance = 0.01f;
	private float curDrawDistance = 0.1f;

	public AudioSource AccelerationSound;
	public AudioSource BrakingSound;

	private ParticleSystem ps;
	private float creationInterval = 0.2f;
	private PlayerSounds sounds;

	Spline nextSpline = null; 
	Point nextPoint = null;
	bool newSpline = false; 
	bool newPoint = false; 
	bool connectPoint = false;
	float angleToSpline = Mathf.Infinity;
	private List<Vector3> newPointList;
	Vector3 lastPos;

	void Awake(){
		state = PlayerState.Switching;
		sounds = GetComponent<PlayerSounds> ();
		l = GetComponent<LineRenderer> ();
		t = GetComponentInChildren<TrailRenderer> ();
		traversing = false;
		inventory = new List<Point>();
		ps = GetComponent<ParticleSystem> ();
		newPointList = new List<Vector3> ();

		int i = 0;

//		while(i < 50) {
//			GameObject p = (GameObject)Instantiate (PointPrefab, Vector3.zero, Quaternion.identity);
//			StartCoroutine(CollectPoint (p.GetComponent<Point> ()));
//			i++;
//		}
//			

		lastPoint = curPoint;


	}
		

	void Update () {

		CursorInput();

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
					if (!Input.GetButton ("Button2") && flow > flyingSpeedThreshold) {
						state = PlayerState.Flying;
						newPointList.Clear ();
						newPointList.Add (transform.position);
						curDrawDistance = PointDrawDistance;
					} else {
//						if (inventory.Count > 0) {
							Point nextPoint = null;
							nextPoint = CheckIfOverPoint (cursorPos);
							SplinePointPair spp = ConnectNewPoint (curSpline, curPoint, nextPoint, cursorPos);
							
							curSpline = spp.s;
							
							creationInterval = creationCD;
							canTraverse = true;
//						}
					}
				}
			}

			if (canTraverse && !Input.GetButton ("Button2")) {
				curSpline.OnSplineEnter ();
				state = PlayerState.Traversing;

				if (curPoint.IsOffCooldown ()) {
					flow += flowAmount;
					boost = boostAmount;
					curPoint.PutOnCooldown ();
				}

				//this is making it impossible to get off points that are widows. wtf. 
				SetCursorAlignment();
				PlayerMovement ();

			} else {
				flow = Mathf.Lerp (flow, 0, decay * Time.deltaTime);
			}
		}

		if (state != PlayerState.Animating && curPoint.HasSplines () && curSpline != null) {
			transform.position = curSpline.GetPoint (progress); 
		}

		Effects ();
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
			flow = Mathf.Lerp (flow, 0, decay * Time.deltaTime);
		}

		if (accuracy < 0) {
			goingForward = false;
		} else {
			goingForward = true;
		}
	}

	public IEnumerator ReturnToLastPoint(){
		
		float speed = 0.001f;
		int index = newPointList.Count - 1;
		float t = 0; 
		float distance = Vector3.Distance (newPointList [index], newPointList [index - 1]);
		Vector3 lastPos = transform.position;

		while(index > 0){

			speed += Time.deltaTime * 1/index;
			t += speed/distance;
			transform.position = Vector3.Lerp (newPointList[index], newPointList [index - 1], t);

			float curDistance = Vector3.Distance (newPointList [index], transform.position);
			sprite.transform.up = transform.position - newPointList [index - 1];

			if(t >= 1){
				index--;
				if (index < 1) {
					break;
				}
				distance = Vector3.Distance (newPointList [index], newPointList [index - 1]);
				t = 0;
			}

			yield return null;
		}

		newPointList.Clear ();
		state = PlayerState.Switching;
	}

	public void FlyIntoNewPoint(Point p){

		state = PlayerState.Animating;

		int index = 10;
		float t = 0; 

		Point curP = curPoint;
		Spline s = curSpline;

		while (index < newPointList.Count) {

			SplinePointPair spp;

			Point newPoint = Services.PlayerBehaviour.CheckIfOverPoint (newPointList[index]);
			spp = Services.PlayerBehaviour.ConnectNewPoint (s, curP, newPoint, newPointList[index]);


			s = spp.s;
			curP = spp.p;
			curP.transform.parent = s.transform;
			s.transform.parent = s.transform;

			index += 10;
		}
			
		//could add another point at the player's current position between curP (last in index) and p (destination) to make player position not jump
		//whats with phantom splines
		//must be an error with closed/looping splines getting created and fucking up

		SplinePointPair	sp = ConnectNewPoint (s, curP, p, curP.transform.position);
		curSpline = sp.s;
		curPoint = curP;
		s.Selected = curP;
		progress = 0;
		Vector3 pos = transform.position;

//		while (t < 1) {
//			transform.position = Vector3.Lerp (pos, p.Pos, t);
//			t += Time.deltaTime * flow;
//			yield return null;
//		}
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

		state = PlayerState.Switching;
	}

	void FreeMovement(){
		Vector3 inertia;
		lastPos = transform.position;


		float speed;
		// Make drawing points while you skate. 
		//should solve the problems of jumping across new points on the same spline. 


		if (flow < 0) {
			
			newPointList.Add (transform.position);
			state = PlayerState.Animating;
			StartCoroutine (ReturnToLastPoint ());

		} else {
			inertia = cursorDir * flow;
			flow -= Time.deltaTime;
			transform.position += inertia * Time.deltaTime;

			curDrawDistance -= (inertia * Time.deltaTime).magnitude;

			if (curDrawDistance <= 0) {
				curDrawDistance = PointDrawDistance;
				newPointList.Add (transform.position);
			}
		}
			
	}



	void PlayerMovement(){ 

//		adding this value to flow

		flow += Mathf.Pow(Mathf.Abs(accuracy), 2) * acceleration * Time.deltaTime;

		progress += ((flow + boost + (speed * Mathf.Abs(accuracy))) * Mathf.Sign(accuracy) * Time.deltaTime)/curSpline.distance;

		//set player position to a point along the curve

		if (curPoint == curSpline.Selected) {
			curPoint.proximity = 1 - progress;
			if (curSpline.closed && curSpline.SplinePoints.IndexOf(curPoint) >= curSpline.SplinePoints.Count-1) {
				curSpline.SplinePoints [curSpline.LoopIndex].proximity = progress;
			} else {
				
				curSpline.SplinePoints [Mathf.Clamp(curSpline.GetPointIndex(curSpline.Selected)+1, 0, curSpline.SplinePoints.Count-1)].proximity = progress;;
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


			if (!curPoint.locked) {
				curPoint.OnPointExit ();
			}

			Point PointArrivedAt = curPoint;

			if (progress > 1) {

				progress = 1;
			
				if (curSpline.Selected == curSpline.EndPoint() && curSpline.closed) {
					curPoint = curSpline.SplinePoints [curSpline.LoopIndex];
				} else {
					curPoint = curSpline.SplinePoints [curSpline.GetPointIndex(curSpline.Selected) + 1];
				}

			} else if (progress < 0) {

				progress = 0;		   
				curPoint = curSpline.Selected;

			}
				

			if (curPoint.IsOffCooldown ()) {
				Services.Prefabs.CreateSoundEffect (sounds.pointSounds[Random.Range(0, sounds.pointSounds.Length)],curPoint.Pos);
			}

			if (PointArrivedAt != curPoint) {
				lastPoint = PointArrivedAt;

				if (!curPoint.locked) {
					curPoint.OnPointEnter ();
				}

				curPoint.GetComponent<Rigidbody> ().AddForce (cursorDir * flow * 10);
			}

			curSpline.OnSplineExit ();
		 	state = PlayerState.Switching;
		}
	}


	public Point CheckIfOverPoint(Vector3 pos){
		Ray ray = new Ray (pos + -(Vector3.forward) * 100, Vector3.forward);
//		Ray ray = Camera.main.ScreenPointToRay (Camera.main.WorldToScreenPoint (pos));
//		Debug.DrawRay (ray.origin, ray.origin + ray.direction * 10);
		RaycastHit hit;

		if (Physics.Raycast (ray, out hit)) {
			if (hit.collider.tag == "Point") {
				Point hitPoint = hit.collider.GetComponent<Point> ();

				return hitPoint;
					
			} 
		}
		return null;
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


	public SplinePointPair ConnectNewPoint(Spline s, Point p1, Point p2, Vector3 atPos){

		SplinePointPair result = new SplinePointPair();

		Spline newSpline;

		if (p2 == null) {
			creationInterval = creationCD;
			newPoint = true;
			p2 = CreatePoint (atPos); 
		}else if (p2 == p1) {
			result.p = p1;
			result.s = s;
			return result;
		}

			//ALL CASES WHERE THE CLICKED ON/CREATED POINTS ARE ADDED TO CURRENT SPLINE

		if (s == null || s.closed || s.locked) {
			newSpline = CreateSpline (p1,p2);

		} else {

			if (p1 == s.StartPoint () || p1 == s.EndPoint ()) {

				newSpline = s;

				if (p2 == s.StartPoint () || p2 == s.EndPoint ()) {

					s.closed = true;
					s.LoopIndex = s.SplinePoints.IndexOf (p2);

					p1.AddPoint (p2);
					p2.AddPoint (p1);

					if (s.GetPointIndex (p2) - s.GetPointIndex (p1) > 1) {
						s.Selected = p2;
					}

				} else if (!s.SplinePoints.Contains (p2)) {

					s.AddPoint (p2);
					s.name = s.StartPoint ().name + "—" + s.EndPoint ().name;

				} else {

					newSpline = CreateSpline (p1, p2);
				}	
			} else {

			 newSpline = CreateSpline (p1,p2);
			}
			//EDGE CASE
			//Creating endpoint when you're on startpoint 
			//make it so that the start/midpoint get shifted down one index, insert at startpoin
		}
		result.p = p2;
		result.s = newSpline;

		return result;
	}
		

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
				SetPlayerAtStart (closestSpline, pointDest);
				curSpline = closestSpline;
				return true;
			}
		}
		return false;
	}


	public Spline CreateSpline (Point firstP, Point nextP){
		
		GameObject newSpline = (GameObject)Instantiate (SplinePrefab, Vector3.zero, Quaternion.identity);

		Spline s = newSpline.GetComponent<Spline> ();

		s.name = lastPoint.name + "—" + nextP.name;
		s.Selected = firstP;
		progress = 0;

//		if (lastPoint != curPoint) {
//			s.AddPoint (lastPoint);
//		}

		s.AddPoint (firstP);
		s.AddPoint (nextP);

		s.transform.position = Vector3.Lerp (firstP.Pos, nextP.Pos, 0.5f);
			
		s.DrawMesh();

		return s;
	}

	public Point CreatePoint(Vector3 pos){
//		Point newPoint = inventory [inventory.Count-1];

		Point newPoint = Instantiate(Services.Prefabs.Point, Vector3.zero, Quaternion.identity).GetComponent<Point>();
		Services.Points.AddPoint (newPoint);
//		inventory.Remove (newPoint);

		newPoint.isPlaced = true;
//		newPoint.bias = flow / maxSpeed;
		newPoint.transform.parent = null;
		newPoint.transform.position = pos;
		newPoint.timeOffset = Services.Points._points.Count * 0.1f;
		newPoint.GetComponent<Collider> ().enabled = true;
		newPoint.GetComponent<Rigidbody> ().velocity = Vector3.zero;
		newPoint.transform.GetChild (0).position = newPoint.transform.position;
		newPoint.GetComponent<SpringJoint> ().connectedBody = newPoint.transform.GetChild(0).GetComponent<Rigidbody> ();
		newPoint.GetComponent<SpringJoint> ().connectedAnchor = newPoint.transform.GetChild (0).transform.localPosition;
		newPoint.GetComponent<SpriteRenderer> ().enabled = true;
		return newPoint;
	}


	public void OnTriggerEnter(Collider col){
		if (col.tag == "Point") {
			if (!col.GetComponent<Point> ().isPlaced) {
				StartCoroutine (CollectPoint (col.GetComponent<Point> ()));
			} else if(state == PlayerState.Flying) {

				FlyIntoNewPoint(col.GetComponent<Point>());
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
		
		float Absflow = Mathf.Abs (flow);
		if (state == PlayerState.Flying) {
			t.time = Absflow;
		} else {
			t.time = Mathf.Lerp(t.time, 1, Time.deltaTime);
		}
		ParticleSystem.EmissionModule e = ps.emission;	
			
		e.rateOverTimeMultiplier = (int)Mathf.Lerp (0, flow * 25, Mathf.Pow (1 - Mathf.Abs (accuracy), 2));
//		BrakingSound.volume = Mathf.Clamp01(1- Mathf.Abs (accuracy))/6;
		AccelerationSound.volume = Mathf.Clamp01(flow / (maxSpeed/5));

		if (curSpline != null) {
			curSpline.DrawLineSegmentVelocity (progress, Mathf.Sign (accuracy), goingForward ? 0 : 1);
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