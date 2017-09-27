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
	
	[Header("Max space between Splines")]
	public float maxAngleBetweenSplines;

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
	public List<Point> traverseList;

	void Awake(){
		l = GetComponent<LineRenderer> ();
		t = GetComponent<TrailRenderer> ();
		sound = GetComponent<AudioSource> ();
		traversing = false;
		traverseList = new List<Point> ();
		inventory = new List<Point>();
		int i = 0;

		while(i < 50) {
			GameObject p = (GameObject)Instantiate (PointPrefab, Vector3.zero, Quaternion.identity);
			AddPoint (p.GetComponent<Point> ());
			i++;
		}

		Point p1 = CreatePoint (Vector3.right * 2);
		Point p2 = CreatePoint (Vector3.right * 4 + Vector3.up * 3);
		curPoint = p1;
		curSpline = CreateSpline (p1, p2);
		traverseList.Add (p1);
		traverseList.Add (p2);

	}

	void Update () {

		Debug.Log (curSpline.Selected.name);

		CirclePlayer ();
		CursorInput();

		if (traversing) {
			PlayerMovement ();
			Effects ();
			curSpline.SetPointProximity (progress);
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
		Debug.DrawLine (position, position + curSpline.GetDirection (progress));
//		transform.Rotate (0, 0, flow*5);
	}

	void CheckProgress(){

		if (traversing) {

			if (progress > 1) {
				progress = 1;
				curPoint = curSpline.SplinePoints [curSpline.SplinePoints.IndexOf (curSpline.Selected) + 1];

				traversing = false;
			} else if (progress < 0) {
				progress = 0;
				curPoint = curSpline.Selected;
				traversing = false;
			}
		}
		if (!traversing) {
			Debug.Log ("Player at " + curPoint.name + " of Spline " + curSpline.name); 
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

		if (Input.GetButton ("x")) {
			


			Ray ray = Camera.main.ScreenPointToRay (Camera.main.WorldToScreenPoint (cursor.transform.position));
			RaycastHit hit;

			if (Physics.Raycast (ray, out hit)) {
				if (hit.collider.tag == "Point") {
					Point hitPoint = hit.collider.GetComponent<Point> ();

					if (hitPoint == curPoint) {
						//you clicked on the node you are currently on

					} else if (curPoint.IsAdjacent (hitPoint)) {
						//you clicked on a node already connected to the one you're on

						//	hitPoint.GetConnectingSpline (p.curPoint).Reinforce ();
						//	hitPoint.DestroySplines ();
						//	StartCoroutine(CollectPoint(hitPoint));

					} else {

						nextPoint = hitPoint;
						newSpline = true;

						Debug.Log ("Player clicked on " + nextPoint);
					}
				}
			} else if(inventory.Count > 0) {
				nextPoint = CreatePoint (cursor.transform.position); 

				newSpline = true;

				Debug.Log ("Player created " + nextPoint);
			}

			if (newSpline) {
				
				if (curSpline.SplinePoints.Count < 3) {

					//EDGE CASE
					//Creating endpoint when you're on startpoint 
					//make it so that the start/midpoint get shifted down one index, insert at startpoint

					Debug.Log ("Adding point " + nextPoint.name + " to " + curSpline.name);
					curSpline.AddPoint (nextPoint);
					nextSpline = curSpline;
					curSpline.name = nextSpline.SplinePoints [0].name + "—" + curSpline.SplinePoints [2].name;
				} else {
					curSpline.AddPoint (nextPoint);
					nextSpline = CreateSpline (curPoint, nextPoint);
					Debug.Log ("Creating new spline: " + nextSpline.name);
				}
			}

		}
			
			
		if (curPoint.HasSplines()) {

			bool forward = true;

			if (!newSpline) {

//				nextSpline = curPoint.GetNextSpline (cursorDir);
//				Debug.Log ("Player routed onto " + nextSpline);

				float minAngle = Mathf.Infinity;
				Spline closestSpline = null;
				Point pointDest = null;

				foreach (Spline s in curPoint.GetSplines()) {

					foreach (Point p in s.SplinePoints) {

						if (Mathf.Abs (s.SplinePoints.IndexOf (p) - s.SplinePoints.IndexOf (curPoint)) > 1 || p == curPoint) {
							//do nothing if the point is more than 1 away on the spline
						} else {

							float curAngle;

							if (s.SplinePoints.IndexOf (p) < s.SplinePoints.IndexOf (curPoint)) {
								curAngle = s.CompareAngleAtPoint (cursorDir, p, true);
							} else {
								curAngle = s.CompareAngleAtPoint (cursorDir, curPoint);
							}


							if (curAngle < minAngle) {
								minAngle = curAngle;
								closestSpline = s;
								pointDest = p;

							}
						}
					}
				}


				if (closestSpline.SplinePoints.IndexOf (pointDest) < closestSpline.SplinePoints.IndexOf (curPoint)) {
					closestSpline.Selected = pointDest;
					forward = false;
					Services.Player.GetComponent<PlayerBehaviour> ().SetProgress (1);

				} else {
					Services.Player.GetComponent<PlayerBehaviour> ().SetProgress (0);
					forward = true;
					closestSpline.Selected = curPoint;
				}

				Debug.Log ("New destination: " + pointDest.name);
				Debug.Log ("New selected is: " + closestSpline.Selected);

				Debug.DrawLine(closestSpline.GetVelocity(0) + curPoint.Pos, curPoint.Pos) ;
				Debug.DrawLine(-closestSpline.GetVelocity(1) + curPoint.Pos, curPoint.Pos) ;

				nextSpline = closestSpline;

				//see code in "GetNextSpline()"
				//make sure you set the next point appropriately with forward/backward
			}

			//IN THE OLD SPLINES IT MADE SENSE TO SET PROGRESS TO 1/0 
			//BASED ON WHICH END OF THE SPLINE THE POINT WAS ON
			//NOW YOU SET IT TO 1/0 BASED ON WHICH SEGMENT OF THE SPLINE ITS ON
			//AND MAKE SURE YOU SET SELECTED TO THE SPLINE THAT LIES AT 0

			//BREAK OUT OF FUNCTION BEFORE THE NEXT SPLINE IS SET TO THE CURRENT SPLINE

			
			if (!newSpline && nextSpline.CompareAngleAtPoint (cursorDir, nextSpline.Selected, !forward) > maxAngleBetweenSplines) {

				flow = Mathf.Lerp (flow, 0, decay * Time.deltaTime);
				return;
			}


			//SET NEXT SPLINE TO CURRENT SPLINE
			curSpline = nextSpline;
			curSpline.ReDraw ();
			curSpline.CalculateDistance ();

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
				
				traversing = true; 
			}
		}

	private Spline CreateSpline (Point curP, Point nextP){
		
		GameObject newSpline = (GameObject)Instantiate (SplinePrefab, Vector3.zero, Quaternion.identity);

		Spline s = newSpline.GetComponent<Spline> ();


		s.name = curP.name + "—" + nextP.name;
		s.Selected = curP;
		progress = 0;


		s.AddPoint (curP);
		s.AddPoint (nextP);

		s.ReDraw();

		return s;
	}

	private Point CreatePoint(Vector3 pos){
		Point newPoint = inventory [0];
		inventory.Remove (newPoint);
		newPoint.transform.parent = null;
		newPoint.transform.position = pos;
		newPoint.timeOffset = Time.time;
		newPoint.GetComponent<Collider> ().enabled = true;
	
		return newPoint;
	}

//		Debug.Log ("Spline: " + e.name + " Angle: " + e.GetAngleAtPoint (cursorDir, curPoint, false));
//		if (e.GetAngleAtPoint (cursorDir, curPoint) > maxAngleBetweenSplines || !Input.GetButtonDown('x')) {
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
	}

	void CursorInput (){

		if (controllerConnected) {

			cursorDir = new Vector3(-Input.GetAxis ("Joy X"), Input.GetAxis ("Joy Y"), 0);
			cursor.transform.position = transform.position + (cursorDir * (Mathf.Abs(flow) + 0.5f));
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
			cursor.transform.position = transform.position + Vector3.ClampMagnitude(cursor.transform.position - transform.position, Mathf.Abs(flow) + cursorDistance) ;
			cursorDir = cursor.transform.position - transform.position;
			if (cursorDir.magnitude > 1) cursorDir.Normalize ();
			Debug.DrawLine(transform.position, cursor.transform.position);

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