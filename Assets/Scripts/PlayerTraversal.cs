using UnityEngine;
using UnityEngine.UI;

public class PlayerTraversal : MonoBehaviour {

	[Header("Current Edge")]
	public Edge curEdge;

	[Header("Current Node")]
	public Node curNode;

	[Header("Cursor")]
	public GameObject cursor;

	[Header("Decay")]
	public float decay;

	[Header("Acceleration")]
	public float acceleration;

	[Header("Max Speed")]
	public float maxSpeed;
	
	[Header("Max space between Edges")]
	public float maxAngleBetweenEdges;
	
	public SplineWalkerMode mode;

	//components I want to access
	private TrailRenderer t;
	private LineRenderer l;
	private AudioSource sound;

	private float flow;
	private float progress;
	private float accuracy;

	private bool traversing;
	private bool goingForward = true;
	private bool controllerConnected = false;

	private Vector3 cursorPos, cursorDir;

	void Start(){

		t = GetComponent<TrailRenderer> ();
		l = GetComponent<LineRenderer> ();
		sound = GetComponent<AudioSource> ();

	}

	void Update () {

		CursorInput();


		if (curNode.HasEdges()) {
			if (!traversing) {
				AtNodeIntersection ();
			} else {
				PlayerMovement ();
				UpdateNode ();
			}
		}
			
		Effects ();
		#region
		if (Input.GetAxis ("Joy Y") != 0) {
			controllerConnected = true;
		}
		#endregion
	}

	void PlayerMovement(){ 

		//the angle between cursor and player, and slope on the curve
		float alignment = Vector3.Angle(cursorDir, curEdge.curve.GetDirection(progress));

		//adjusting the range to be between -1 and 1
		accuracy = (90 - alignment)/90;

		if (accuracy <= 0.5f && accuracy >= -0.5f) {
			flow = Mathf.Lerp (flow, 0, decay * Time.deltaTime);
		}
			
		if (Mathf.Abs(flow) > maxSpeed) {
			flow = Mathf.Lerp (flow, 0, decay * Time.deltaTime);
		}	

		if (flow > 0 && accuracy < 0) flow = Mathf.Lerp (flow, 0, decay * Time.deltaTime);
		if (flow < 0 && accuracy > 0) flow = Mathf.Lerp (flow, 0, decay * Time.deltaTime);

		//adding this value to flow
		flow += accuracy * Time.deltaTime;

		progress += (accuracy * Time.deltaTime) + flow * acceleration;

		//set player position to a point along the curve
		Vector3 position = curEdge.curve.GetPoint(progress);
		transform.localPosition = position;

//		transform.Rotate (0, 0, flow*5);
	}

	void UpdateNode(){
		if (progress >= 1) {
			curNode = curEdge.GetVert2 ();
			traversing = false;

		}else if (progress <= 0) {
			curNode = curEdge.GetVert1 ();
			traversing = false;
		}
	}

	public void AtNodeIntersection(){


		Edge e = curNode.GetClosestEdgeDirection(cursorDir);

//		Debug.Log ("Edge: " + e.name + " Angle: " + e.GetAngleAtNode (cursorDir, curNode, false));

		if (e.GetAngleAtNode (cursorDir, curNode) > maxAngleBetweenEdges){
			flow = Mathf.Lerp (flow, 0, decay * Time.deltaTime);
			return;
		}


		if (e.GetEdgeDirection(curNode)) {
			progress = 0;
			if (goingForward != true) {
				flow = -flow;
				goingForward = true;
			}

		} else {
			progress = 1;
			if (goingForward != false) {
				flow = -flow;
				goingForward = false;
			}
		}

		curEdge = e;
		traversing = true;
		GetComponent<ParticleSystem> ().Emit (1);
	}
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

//	public void OnTriggerEnter (Collider col){
//		if (col.tag == "Node") {
//			curNode = col.GetComponent<Node> ();
//		}
//		traversing = false
//	}

	void CursorInput (){

		if (controllerConnected) {

			cursorPos = new Vector3(-Input.GetAxis ("Joy X"), Input.GetAxis ("Joy Y"), 0);

			//free movement: transform.position = transform.position + new Vector3 (-Input.GetAxis ("Joy X") / 10, Input.GetAxis ("Joy Y") / 10, 0);
			//angle to joystick position
			//zAngle = Mathf.Atan2 (Input.GetAxis ("Joy X"), Input.GetAxis ("Joy Y")) * Mathf.Rad2Deg;
		}else {

			Vector3 mousePos = new Vector3 (Input.mousePosition.x, Input.mousePosition.y, transform.position.z - Camera.main.transform.position.z);

			//set cursor to mouse position in world coordinates
			cursorPos = Camera.main.ScreenToWorldPoint (mousePos);
			if (cursorPos.magnitude > 1) cursorPos.Normalize ();
			//angle to mouse position
			//zAngle = Mathf.Atan2 (worldPos.x - transform.position.y, worldPos.y - transform.position.y) * Mathf.Rad2Deg;
		}

		cursor.transform.position = transform.position + (cursorPos * Mathf.Clamp(Mathf.Abs(flow), 1, 3));

		cursorDir = (cursor.transform.position - transform.position).normalized;
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

	public void Effects(){
		float Absflow = Mathf.Abs (flow);
		//extend trail with more flow
		t.time = Absflow/5;

		//increase volume with more flow
		sound.volume = Absflow/10;

		//emit more particles with more flow
		ParticleSystem.EmissionModule m = GetComponent<ParticleSystem> ().emission;
		m.rate= (int)(Absflow * 5);

//		//set pointer for player object
//		l.SetPosition(0, transform.position);
//		l.SetPosition(1, transform.position + spline.GetDirection(progress));
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