using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {
	public float speed;
	public Camera uiCam;
	public Transform target;
	private Vector3 velocity = Vector2.zero;
	public Vector3 offset;
	private Camera cam;
	public static float desiredFOV;
	// Use this for initialization
	void Start () {
		cam = GetComponent<Camera> ();
		CameraDolly.leftBound = float.PositiveInfinity;
		CameraDolly.rightBound = float.NegativeInfinity;
		CameraDolly.topBound = float.NegativeInfinity;
		CameraDolly.bottomBound = float.PositiveInfinity;
		desiredFOV = cam.fieldOfView;
		Cursor.visible = false;
	}

	// Update is called once per frame
	void Update () {
		//get cur spline
		//find its bounds
		// get around them

//		transform.position = Vector3.SmoothDamp (transform.position, target.position + offset, ref velocity, speed);

				float height;
				float yPos;
				float xPos;

				// float CameraDolly.topBound, CameraDolly.leftBound, CameraDolly.rightBound, CameraDolly.bottomBound;


					// CameraDolly.leftBound = Services.PlayerBehaviour.transform.position.x;
					// CameraDolly.topBound = Services.PlayerBehaviour.transform.position.y;
					// CameraDolly.rightBound = CameraDolly.leftBound;
					// CameraDolly.bottomBound = CameraDolly.topBound;
				//
					foreach (Point p in Services.PlayerBehaviour.curSpline.SplinePoints) {
						if (p.Pos.y >= CameraDolly.topBound) {
							CameraDolly.topBound = p.Pos.y;
						}else{
							CameraDolly.topBound -= Time.deltaTime * speed;
						}
						if (p.Pos.y <= CameraDolly.bottomBound) {
							CameraDolly.bottomBound = p.Pos.y;
						}else{
							CameraDolly.bottomBound += Time.deltaTime * speed;
						}
						if (p.Pos.x >= CameraDolly.rightBound) {
							CameraDolly.rightBound = p.Pos.x;
						}else{
							CameraDolly.rightBound -= Time.deltaTime * speed;
						}
						if (p.Pos.x <= CameraDolly.leftBound) {
							CameraDolly.leftBound = p.Pos.x;
						}else{
							CameraDolly.leftBound += Time.deltaTime * speed;
						}

					}

					if (target.position.x > CameraDolly.rightBound) {
						CameraDolly.rightBound = target.position.x;
					}

					if (target.position.x < CameraDolly.leftBound) {
						CameraDolly.leftBound = target.position.x;
					}

					if (target.position.y > CameraDolly.topBound) {
						CameraDolly.topBound = target.position.y;
					}

					if (target.position.y < CameraDolly.bottomBound) {
						CameraDolly.bottomBound = target.position.y;
					}

				// if (Services.Cursor.transform.position.x > CameraDolly.CameraDolly.rightBound) {
				// 	CameraDolly.rightBound = Services.Cursor.transform.position.x;
				// } else {
				// 	CameraDolly.rightBound = CameraDolly.CameraDolly.rightBound;
				// }
				//
				// if (Services.Cursor.transform.position.x < CameraDolly.CameraDolly.leftBound) {
				// 	CameraDolly.leftBound = Services.Cursor.transform.position.x;
				// } else {
				// 	CameraDolly.leftBound = CameraDolly.CameraDolly.leftBound;
				// }
				//
				// if (Services.Cursor.transform.position.y > CameraDolly.CameraDolly.topBound) {
				// 	CameraDolly.topBound = Services.Cursor.transform.position.y;
				// } else {
				// 	CameraDolly.topBound = CameraDolly.CameraDolly.topBound;
				// }
				//
				// if (Services.Cursor.transform.position.y < CameraDolly.CameraDolly.bottomBound) {
				// 	CameraDolly.bottomBound = Services.Cursor.transform.position.y;
				// } else {
				// 	CameraDolly.bottomBound = CameraDolly.CameraDolly.bottomBound;
				// }

				height = CameraDolly.topBound - CameraDolly.bottomBound;
				yPos = Mathf.Lerp (CameraDolly.bottomBound, CameraDolly.topBound, 0.5f);
				xPos = Mathf.Lerp (CameraDolly.leftBound, CameraDolly.rightBound, 0.5f);

				// if(target.GetComponent<PlayerBehaviour>().state != PlayerState.Animating){
				// 	cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, desiredFOV, Time.deltaTime * speed);
				// 	transform.position = Vector3.SmoothDamp (transform.position, target.position + offset, ref velocity, speed);
				// }else{
					cam.fieldOfView = Mathf.Lerp (cam.fieldOfView, CameraDolly.FOVForHeightAndDistance (height, -transform.position.z) + 10, Time.deltaTime * speed);
					Vector3 shake = Services.PlayerBehaviour.state == PlayerState.Traversing ? Random.onUnitSphere / 5f * Mathf.Pow(1- Services.PlayerBehaviour.accuracy, 2) * Services.PlayerBehaviour.curSpeed : Vector3.zero;

					Vector3 targetPos = Vector3.SmoothDamp (transform.position, Vector3.Lerp(Services.PlayerBehaviour.transform.position, new Vector3(xPos, yPos, Services.PlayerBehaviour.transform.position.z), 0.5f) + shake, ref velocity, 0.25f);

					transform.position = new Vector3(targetPos.x, targetPos.y, Services.PlayerBehaviour.transform.position.z + offset.z);
				// }

				uiCam.fieldOfView = cam.fieldOfView;

				// Vector3 targetPos = Vector3.Lerp(new Vector3 (xPos, yPos, target.position.z + offset.z), target.position + offset, 0.5f);

//		GetComponent<Camera>().orthographicSize = 10 + Mathf.Abs(target.GetComponent<PlayerTraversal> ().GetFlow());
	}
}
