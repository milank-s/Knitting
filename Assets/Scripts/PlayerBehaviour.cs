using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PlayerBehaviour: MonoBehaviour {

	public GameObject PointPrefab;
	public GameObject SplinePrefab;

	[Header("Current Spline")]
	public Spline curSpline;

	[Header("Current Point")]
	public Point curPoint;

	[Header("Cursor")]
	public GameObject cursor;

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

	private float boost;
	public float flow;
	public float progress;
	public float accuracy;
	public float creationCD = 0.25f;

	public float cursorDistance;
	private bool traversing;
	public bool goingForward = true;
	private bool controllerConnected = false;

	private Vector3 cursorPos, cursorDir;
	private LineRenderer l;
	private List<Point> inventory;
	public Point lastPoint;

	public AudioSource AccelerationSound;
	public AudioSource BrakingSound;

	private ParticleSystem ps;
	private float creationInterval = 2;
	private PlayerSounds sounds;

	void Awake(){
		sounds = GetComponent<PlayerSounds> ();
		l = GetComponent<LineRenderer> ();
		t = GetComponent<TrailRenderer> ();
		traversing = false;
		inventory = new List<Point>();
		ps = GetComponent<ParticleSystem> ();

		int i = 0;

		while(i < 50) {
			GameObject p = (GameObject)Instantiate (PointPrefab, Vector3.zero, Quaternion.identity);
			StartCoroutine(CollectPoint (p.GetComponent<Point> ()));
			i++;
		}
			
		lastPoint = curPoint;
		curSpline = null;


	}
		

	void Update () {

		if (curSpline != null) {
			float alignment = Vector3.Angle (cursorDir, curSpline.GetDirection (progress));
			flow = Mathf.Clamp (flow, -maxSpeed, maxSpeed);
			accuracy = (90 - alignment) / 90;
			if ((accuracy < 0.5f && accuracy > -0.5f) || Input.GetButton("Button2")) {
				flow = Mathf.Lerp (flow, 0, decay * Time.deltaTime);
			}
		}

		CursorInput();
		Effects ();

		if (traversing) {
			PlayerMovement ();

		}
		CheckProgress ();

		creationInterval-= Time.deltaTime;
		#region
		if (Input.GetAxis ("Joy Y") != 0) {
			controllerConnected = true;
		}
		#endregion
	}



	void PlayerMovement(){ 

//		adding this value to flow
//		flow += Mathf.Sign(accuracy) * Mathf.Pow(Mathf.Abs(accuracy), 1) * acceleration;
		
		progress += ((flow + boost + (speed * Mathf.Abs(accuracy))) * Mathf.Sign(accuracy) * Time.deltaTime)/curSpline.distance;

		//set player position to a point along the curve
		Vector3 position = curSpline.GetPoint(progress);

		transform.position = position;

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

		if (traversing) {

			if (progress > 1 || progress < 0) {

				traversing = false;
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
					curPoint.PutOnCooldown ();
				}

				if (PointArrivedAt != curPoint) {
					lastPoint = PointArrivedAt;

					curPoint.OnPointEnter ();
					curPoint.GetComponent<Rigidbody> ().AddForce (cursorDir * flow * 10);
				}


			}
		}
		if (!traversing) {
			if (curPoint.HasSplines ()) {
				transform.position = curSpline.GetPoint (progress); 
			}
				
			PointSwitch ();

		}
//			} else {
//				flow = Mathf.Lerp (flow, 0, Time.deltaTime * decay);
//			}
	}


	public void PointSwitch(){

		Spline nextSpline = null; 
		Point nextPoint = null;
		bool newSpline = false; 
		bool newPoint = false; 
		bool connectPoint = false;
		float angleToSpline = Mathf.Infinity;

		if (curPoint.HasSplines ()) {

			goingForward = true;

			Spline closestSpline = null;
			Point pointDest = null;

			foreach (Spline s in curPoint.GetSplines()) {

				foreach (Point p in curPoint.GetNeighbours()) {

					if (!p._connectedSplines.Contains(s)) {
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

				int indexdiff = closestSpline.SplinePoints.IndexOf (pointDest) - closestSpline.SplinePoints.IndexOf (curPoint);

				if (indexdiff == -1 || indexdiff > 1) {
					closestSpline.Selected = pointDest;
					goingForward = false;
					progress = 1;

				} else {
					progress = 0;
					goingForward = true;
					closestSpline.Selected = curPoint;
				}

				nextSpline = closestSpline;
			}
				
		}

		if (Input.GetButton ("Button1") && nextSpline == null && angleToSpline > LineAngleDiff && creationInterval <= 0) {

			Ray ray = Camera.main.ScreenPointToRay (Camera.main.WorldToScreenPoint (cursor.transform.position));
			RaycastHit hit;

			if (Physics.Raycast (ray, out hit)) {
				if (hit.collider.tag == "Point") {
					Point hitPoint = hit.collider.GetComponent<Point> ();

					if (hitPoint.isPlaced) {
						if (hitPoint == curPoint) {
							//you clicked on the node you are currently on
							connectPoint = false;
						} else {
							creationInterval = creationCD;
							connectPoint = true;
							nextPoint = hitPoint;
						}
					}
				}
			} 

			//IF THE RAYCAST DIDNT FIND A VALID NEXT POINT, THEN CREATE ONE

			if (inventory.Count > 0 && !connectPoint) {
				creationInterval = creationCD;
				newPoint = true;
				nextPoint = PlacePoint (cursor.transform.position); 
				Services.Points.AddPoint (nextPoint);
			}
			
			if (newPoint || connectPoint) {

				//ALL CASES WHERE THE CLICKED ON/CREATED POINTS ARE ADDED TO CURRENT SPLINE

				if (curSpline == null || curSpline.closed || curSpline.locked) {
					newSpline = true;
					nextSpline = CreateSpline (nextPoint);

				} else {
					
					if (curPoint == curSpline.StartPoint () || curPoint == curSpline.EndPoint ()) {

						newSpline = false;
						nextSpline = curSpline;

						if (nextPoint == curSpline.StartPoint () || nextPoint == curSpline.EndPoint ()) {

							curSpline.closed = true;
							curSpline.LoopIndex = curSpline.SplinePoints.IndexOf (nextPoint);

							curPoint.AddPoint (nextPoint);
							nextPoint.AddPoint (curPoint);

							if (curSpline.GetPointIndex (nextPoint) - curSpline.GetPointIndex (curPoint) > 1) {
								curSpline.Selected = nextPoint;
							}

						} else if (!curSpline.SplinePoints.Contains (nextPoint)) {
						
							curSpline.AddPoint (nextPoint);
							curSpline.name = curSpline.StartPoint ().name + "—" + curSpline.EndPoint ().name;

						} else {
							newSpline = true;
							nextSpline = CreateSpline (nextPoint);
						}	
					} else {
						newSpline = true;
						nextSpline = CreateSpline (nextPoint);
					}
					//EDGE CASE
					//Creating endpoint when you're on startpoint 
					//make it so that the start/midpoint get shifted down one index, insert at startpoin
				}
			}
		}
		//BREAK OUT OF FUNCTION BEFORE THE NEXT SPLINE IS SET TO THE CURRENT SPLINE

		if ((nextPoint == null && nextSpline == null)) {

			flow = Mathf.Lerp (flow, 0, decay * Time.deltaTime);
			return;

		} else{
			
			//SET NEXT SPLINE TO CURRENT SPLINE
			if (curSpline != null) {
				curSpline.OnSplineExit ();
			}


			curSpline = nextSpline;
			curSpline.OnSplineEnter();

//			if (newPoint) {
//				if (curPoint == curSpline.Selected) {
//					if (!goingForward) {
////					flow = -flow;
//					}
//					goingForward = true;
//					progress = 0;
//
//				} else {
//					if (goingForward) {
////					flow = -flow;
//
//					}
//					goingForward = false;
//					progress = 1;
//				}
//			}

			if(!Input.GetButton("Button2")){
				traversing = true; 
				flow += flowAmount * curPoint.NeighbourCount();
				boost = boostAmount * (curPoint.NeighbourCount()/2);
			}
		}
	}
		

	private Spline CreateSpline (Point nextP){
		
		GameObject newSpline = (GameObject)Instantiate (SplinePrefab, Vector3.zero, Quaternion.identity);

		Spline s = newSpline.GetComponent<Spline> ();


		s.name = lastPoint.name + "—" + nextP.name;
		s.Selected = curPoint;
		progress = 0;

//		if (lastPoint != curPoint) {
//			s.AddPoint (lastPoint);
//		}
		s.AddPoint (curPoint);
		s.AddPoint (nextP);

		s.transform.position = Vector3.Lerp (curPoint.Pos, nextP.Pos, 0.5f);
			
		s.DrawMesh();

		return s;
	}

	private Point PlacePoint(Vector3 pos){
		Point newPoint = inventory [inventory.Count-1];
		newPoint.isPlaced = true;
		inventory.Remove (newPoint);
		newPoint.bias = flow / maxSpeed;
		newPoint.transform.parent = null;
		newPoint.transform.position = pos;
		newPoint.timeOffset = Time.time;
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
				StartCoroutine(CollectPoint (col.GetComponent<Point> ()));
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
			cursor.transform.position = transform.position + (cursorDir * (flow /(maxSpeed/cursorDistance) + cursorDistance));
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
			cursor.transform.RotateAround (transform.position, transform.forward, -Input.GetAxis("Horizontal") * cursorRotateSpeed * Time.deltaTime);
			cursorDir = (cursor.transform.position - transform.position).normalized;
		}
			
		if (cursorDir.magnitude > 1) {
			cursorDir.Normalize ();
			cursor.transform.position = transform.position + (cursorDir * ((flow/2)+ cursorDistance)) ;
		}

		cursorPos = cursor.transform.position;

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
		t.time = Absflow;
		ParticleSystem.EmissionModule e = ps.emission;	
			
		e.rateOverTimeMultiplier = (int)Mathf.Lerp (0, flow * 25, Mathf.Pow (1 - Mathf.Abs (accuracy), 2));
//		BrakingSound.volume = Mathf.Clamp01(1- Mathf.Abs (accuracy))/6;
		AccelerationSound.volume = Mathf.Clamp01(flow / (maxSpeed/5));

		if (curSpline != null) {
//			curSpline.DrawLineSegmentVelocity (progress, Mathf.Sign (accuracy), goingForward ? 0 : 1);
			l.SetPosition(0, transform.position);
//			l.SetPosition(1, transform.position + (curSpline.GetDirection(progress) * Mathf.Sign(accuracy))/2);
			l.SetPosition(1, cursorPos);
			GetComponentInChildren<Camera>().orthographicSize = Mathf.Lerp(GetComponentInChildren<Camera>().orthographicSize,  flow + 4, Time.deltaTime * 10);
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