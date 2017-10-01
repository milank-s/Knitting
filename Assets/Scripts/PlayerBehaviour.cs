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
	public float boostAmount = 0.1f;

	[Header("Max Speed")]
	public float maxSpeed;
	

	public float LineAngleDiff = 30;
	public float StopAngleDiff = 60;

	public float cursorRotateSpeed = 1;
	public AudioClip[] sounds;

	public SplineWalkerMode mode;

	//components I want to access
	private TrailRenderer t;
	private AudioSource sound;

	private float boost;
	public float flow;
	public float progress;
	public float accuracy;

	public float cursorDistance;
	private bool traversing;
	public bool goingForward = true;
	private bool controllerConnected = false;

	private Vector3 cursorPos, cursorDir;
	private LineRenderer l;
	private List<Point> inventory;
	public Point lastPoint;

	void Awake(){
		l = GetComponent<LineRenderer> ();
		t = GetComponent<TrailRenderer> ();
		sound = GetComponent<AudioSource> ();
		traversing = false;
		inventory = new List<Point>();
		int i = 0;

		while(i < 50) {
			GameObject p = (GameObject)Instantiate (PointPrefab, Vector3.zero, Quaternion.identity);
			AddPoint (p.GetComponent<Point> ());
			i++;
		}
			
		lastPoint = curPoint;
		curSpline = null;


	}
		

	void Update () {


		CirclePlayer ();
		CursorInput();

		if (traversing) {
			PlayerMovement ();
			Effects ();

		}
		CheckProgress ();

		#region
		if (Input.GetAxis ("Joy Y") != 0) {
			controllerConnected = true;
		}
		#endregion
	}

	public void Boost(){
		boost = boostAmount;
	}


	void PlayerMovement(){ 

		float alignment = Vector3.Angle(cursorDir, curSpline.GetDirection(progress));

		accuracy = (90 - alignment)/90;

		flow = Mathf.Clamp (flow, -maxSpeed, maxSpeed);

		if(flow > 0 && accuracy < 0){flow = Mathf.Lerp (flow, 0, decay * Time.deltaTime);}
		if(flow < 0 && accuracy > 0){flow = Mathf.Lerp (flow, 0, decay * Time.deltaTime);}

		sound.volume = Mathf.Abs (accuracy) - 0.5f;
		boost = Mathf.Lerp (boost, 0, Time.deltaTime * decay);

//		adding this value to flow
		flow += Mathf.Sign(accuracy) * Mathf.Pow(Mathf.Abs(accuracy), 1) * acceleration;
	
		progress += ((flow + (Mathf.Sign (accuracy) * (speed + boost))) * Time.deltaTime)/curSpline.distance;

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

//		transform.Rotate (0, 0, flow*5);
	}

	void CheckProgress(){

		if (traversing) {

			if (progress > 1 || progress < 0) {

				traversing = false;

				Point PointArrivedAt = curPoint;

				if (progress > 1) {

					progress = 1;
				
					if (curSpline.Selected == curSpline.EndPoint() && curSpline.closed) {
						curPoint = curSpline.SplinePoints [curSpline.LoopIndex];
					} else {
						curPoint = curSpline.SplinePoints [curSpline.SplinePoints.IndexOf (curSpline.Selected) + 1];
					}

				} else if (progress < 0) {

					progress = 0;		   
					curPoint = curSpline.Selected;

				}

				curPoint.GetComponent<Rigidbody> ().AddForce (cursorDir * flow);

				if (PointArrivedAt != curPoint) {
					lastPoint = PointArrivedAt;
					curPoint.OnPointEnter ();
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
		float angleToSpline = Mathf.Infinity;

		if (curPoint.HasSplines ()) {

			bool forward = true;

			Spline closestSpline = null;
			Point pointDest = null;

			foreach (Spline s in curPoint.GetSplines()) {

				foreach (Point p in curPoint.GetNeighbours()) {

					if (!p._connectedSplines.Contains(s)) {
						//do nothing if the point is in another spline
					} else {

						float curAngle;

						int indexDifference = s.SplinePoints.IndexOf (p) - s.SplinePoints.IndexOf (curPoint);

						if ( indexDifference == -1 || indexDifference > 1) {
							curAngle = s.CompareAngleAtPoint (cursorDir, p, true);
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
				
			int indexdiff = closestSpline.SplinePoints.IndexOf (pointDest) - closestSpline.SplinePoints.IndexOf (curPoint) ;

			if (indexdiff == -1 || indexdiff > 1) {
				closestSpline.Selected = pointDest;
				forward = false;
				Services.Player.GetComponent<PlayerBehaviour> ().SetProgress (1f);

			} else {
				Services.Player.GetComponent<PlayerBehaviour> ().SetProgress (0f);
				forward = true;
				closestSpline.Selected = curPoint;
			}

			if (angleToSpline <= StopAngleDiff) {
				nextSpline = closestSpline;
			}
		}

		if (Input.GetButton ("x") && nextSpline == null && angleToSpline > LineAngleDiff) {

			Ray ray = Camera.main.ScreenPointToRay (Camera.main.WorldToScreenPoint (cursor.transform.position));
			RaycastHit hit;

			if (Physics.Raycast (ray, out hit)) {
				if (hit.collider.tag == "Point") {
					Point hitPoint = hit.collider.GetComponent<Point> ();

					if (hitPoint == curPoint) {
						//you clicked on the node you are currently on

					} else{
						newPoint = true;
						nextPoint = hitPoint;
					}
				}
			} else if(inventory.Count > 0) {
				//DOUBLE POINT ERROR IS ONLY HAPPENING IN THE MIDDLE OF LINES? 

				newPoint = true;
				nextPoint = CreatePoint (cursor.transform.position); 
			}
				
			if (newPoint) {
				if (curSpline != null && (curPoint == curSpline.StartPoint () || curPoint == curSpline.EndPoint ())) {

					//EDGE CASE
					//Creating endpoint when you're on startpoint 
					//make it so that the start/midpoint get shifted down one index, insert at startpoint

					newSpline = false;

					if (nextPoint == curSpline.StartPoint () || nextPoint == curSpline.EndPoint () && !curSpline.closed) {

						curSpline.closed = true;
						curSpline.LoopIndex = curSpline.SplinePoints.IndexOf (nextPoint);

						curPoint.AddPoint (nextPoint);
						nextPoint.AddPoint (curPoint);

						if (curSpline.GetPointIndex (nextPoint) - curSpline.GetPointIndex (curPoint) > 1) {
							curSpline.Selected = nextPoint;
						}

					} else {
						curSpline.AddPoint (nextPoint);
					}

					nextSpline = curSpline;
					curSpline.name = nextSpline.SplinePoints [0].name + "—" + curSpline.EndPoint ().name;

				} else {

					newSpline = true;

					//YOU CANT JUST ADD THE NEW POINT INTO THE OLD SPLINE IF YOU'RE DOING SO IN THE MIDDLE OF THE SPLINE
					//IT WILL CREATE PROBLEMS. IT WONT KNOW THE PROPER NEIGHBOURS
					//MAYBE INSERT IT AT A POSITION
					//If at the end or start of the current line, insert the new point at the ends
					//if not, don't insert it

//					if (curSpline != null && !curSpline.SplinePoints.Contains (nextPoint)) {
//						curSpline.AddPoint (nextPoint);
//					}


					nextSpline = CreateSpline (nextPoint);
				}
			}
	
		}

		//BREAK OUT OF FUNCTION BEFORE THE NEXT SPLINE IS SET TO THE CURRENT SPLINE

		if (!newPoint && nextSpline == null) {

			flow = Mathf.Lerp (flow, 0, decay * Time.deltaTime);

//			transform.position = curPoint.transform.position;

			return;

		} else{
			
			//SET NEXT SPLINE TO CURRENT SPLINE
			curSpline = nextSpline;
			curSpline.OnSplineExit ();

			if (newPoint) {
				if (curPoint == curSpline.Selected) {
					if (!goingForward) {
//					flow = -flow;
					}
					goingForward = true;
					progress = 0;

				} else {
					if (goingForward) {
//					flow = -flow;

					}
					goingForward = false;
					progress = 1;
				}
			}
			
			traversing = true; 
		}
	}
		

	private Spline CreateSpline (Point nextP){
		
		GameObject newSpline = (GameObject)Instantiate (SplinePrefab, Vector3.zero, Quaternion.identity);

		Spline s = newSpline.GetComponent<Spline> ();


		s.name = lastPoint.name + "—" + nextP.name;
		s.Selected = curPoint;
		progress = 0;

		if (lastPoint != curPoint) {
			s.AddPoint (lastPoint);
		}
		s.AddPoint (curPoint);
		s.AddPoint (nextP);

		s.transform.position = Vector3.Lerp (curPoint.Pos, nextP.Pos, 0.5f);
			
		s.DrawMesh();

		return s;
	}

	private Point CreatePoint(Vector3 pos){
		Point newPoint = inventory [0];
		newPoint.gameObject.SetActive (true);
		inventory.Remove (newPoint);
		newPoint.transform.parent = null;
		newPoint.transform.position = pos;
		newPoint.timeOffset = Time.time;
		newPoint.GetComponent<Collider> ().enabled = true;
		newPoint.GetComponent<SpringJoint> ().connectedBody = curPoint.GetComponent<Rigidbody> ();
		return newPoint;
	}

//		Debug.Log ("Spline: " + e.name + " Angle: " + e.GetAngleAtPoint (cursorDir, curPoint, false));
//		if (e.GetAngleAtPoint (cursorDir, curPoint) > LineAngleDiff || !Input.GetButtonDown('x')) {
//
//			if (Input.GetButtonDown ("x")) {
//				GetComponent<PlayerPickups> ().CreatePoint ();
//				return;
//			} else {
//				flow = Mathf.Lerp (flow, 0, Time.deltaTime * decay);
//				traversing = false;
//				return;
//			}
//		}

//		if (goingForward) {
//			//when going backwards over the start, sets them to be at the end
//
//			if (progress > 1f) {
//				if (mode == SplineWalkerMode.Once) {
//					progress = 1f;
//				}
//				else if (mode == SplineWalkerMode.Loop) {
//					progress -= 1f;
//				}
//				else {
//					progress = 2f - progress;
//					goingForward = false;
//				}
//			}
//		}
//		else {
//			if (progress < 0f) {
//				progress = -progress;
//				goingForward = true;
//			}
//		}
//	}


	public void OnTriggerStay(Collider col){
		if (col.tag == "Point") {
			if(Input.GetButton("x")){
//				Instantiate (RipplePrefab, col.transform.position, Quaternion.identity);
				GetComponent<PlayerBehaviour> ().Boost ();
			}
		}
	}
	void CirclePlayer(){
		int i = 0;
		foreach (Point p in inventory) {
			i++;
			//g.transform.RotateAround (transform.position, Vector3.forward, (GetComponent<SplineWalker>().GetFlow()*numPickups + 1)/(i/5 + 1));
			p.transform.RotateAround (transform.position, Vector3.forward, (inventory.Count * Mathf.Abs(flow) + 10)/(i));
			Vector3 direction = (p.transform.position - transform.position).normalized;
			direction = direction* (i);
			p.transform.position = transform.position + (direction / Mathf.Clamp((Mathf.Abs(flow) * inventory.Count), 5, 1000)); 
		}
	}

	IEnumerator CollectNode(Point p){
		float t = 0;
		Vector3 originalPos = p.transform.position;

		while (t <= 1) {
			p.transform.position = Vector3.Lerp (originalPos, transform.position, t);
			t += Time.deltaTime;
			yield return null;
		}

		AddPoint (p);
	}

	void AddPoint(Point p){
		inventory.Add (p);
		p.transform.parent = transform;
		p.GetComponent<Collider> ().enabled = false;
		p.gameObject.SetActive (false);
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
			cursor.transform.RotateAround (transform.position, transform.forward, -Input.GetAxis("Horizontal") * cursorRotateSpeed);
			cursorDir = (cursor.transform.position - transform.position).normalized;
			cursor.transform.position = transform.position + (cursorDir * (flow /(maxSpeed/cursorDistance) + cursorDistance)) ;
		
			if (cursorDir.magnitude > 1) cursorDir.Normalize ();


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
//		float Absflow = Mathf.Abs (flow);
		//extend trail with more flow
//		t.time = Absflow;

		//increase volume with more flow
//		sound.volume = Absflow/10;

		//emit more particles with more flow
//		ParticleSystem.EmissionModule m = GetComponent<ParticleSystem> ().emission;
//		m.rate= (int)(Absflow * 10);

//		//set pointer for player object
		sound.volume = Mathf.Clamp01 (sound.volume);

		l.SetPosition(0, transform.position);
		l.SetPosition(1, transform.position + (curSpline.GetDirection(progress) * Mathf.Sign(accuracy))/2);
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