using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {
	public float speed;
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

				float topBound, leftBound, rightBound, bottomBound;

//				if (PointManager._pointsHit.Count < 4) {
//
//					leftBound = Services.PlayerBehaviour.curPoint.Pos.x;
//					topBound = Services.PlayerBehaviour.curPoint.Pos.y;
//					rightBound = leftBound;
//					bottomBound = topBound;
//
//					foreach (Point p in Services.PlayerBehaviour.curSpline.SplinePoints) {
//						if (p.Pos.y > topBound) {
//							topBound = p.Pos.y;
//						}
//						if (p.Pos.y < bottomBound) {
//							bottomBound = p.Pos.y;
//						}
//						if (p.Pos.x > rightBound) {
//							rightBound = p.Pos.x;
//						}
//						if (p.Pos.x < leftBound) {
//							leftBound = p.Pos.x;
//						}
//
//
//					}
//
//					if (target.position.x > rightBound) {
//						rightBound = target.position.x;
//					}
//
//					if (target.position.x < leftBound) {
//						leftBound = target.position.x;
//					}
//
//					if (target.position.y > topBound) {
//						topBound = target.position.y;
//					}
//
//					if (target.position.y < bottomBound) {
//						bottomBound = target.position.y;
//					}
//
//				} else {

					if (target.position.x > CameraDolly.rightBound) {
						rightBound = target.position.x;
					} else {
						rightBound = CameraDolly.rightBound;
					}

					if (target.position.x < CameraDolly.leftBound) {
						leftBound = target.position.x;
					} else {
						leftBound = CameraDolly.leftBound;
					}

					if (target.position.y > CameraDolly.topBound) {
						topBound = target.position.y;
					} else {
						topBound = CameraDolly.topBound;
					}

					if (target.position.y < CameraDolly.bottomBound) {
						bottomBound = target.position.y;
					} else {
						bottomBound = CameraDolly.bottomBound;
					}

				// if (Services.PlayerBehaviour.cursor.transform.position.x > CameraDolly.rightBound) {
				// 	rightBound = Services.PlayerBehaviour.cursor.transform.position.x;
				// } else {
				// 	rightBound = CameraDolly.rightBound;
				// }
				//
				// if (Services.PlayerBehaviour.cursor.transform.position.x < CameraDolly.leftBound) {
				// 	leftBound = Services.PlayerBehaviour.cursor.transform.position.x;
				// } else {
				// 	leftBound = CameraDolly.leftBound;
				// }
				//
				// if (Services.PlayerBehaviour.cursor.transform.position.y > CameraDolly.topBound) {
				// 	topBound = Services.PlayerBehaviour.cursor.transform.position.y;
				// } else {
				// 	topBound = CameraDolly.topBound;
				// }
				//
				// if (Services.PlayerBehaviour.cursor.transform.position.y < CameraDolly.bottomBound) {
				// 	bottomBound = Services.PlayerBehaviour.cursor.transform.position.y;
				// } else {
				// 	bottomBound = CameraDolly.bottomBound;
				// }

//				}

				height = topBound - bottomBound;
				yPos = Mathf.Lerp (bottomBound, topBound, 0.5f);
				xPos = Mathf.Lerp (leftBound, rightBound, 0.5f);

				if(target.GetComponent<PlayerBehaviour>().state != PlayerState.Animating){
					cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, desiredFOV, Time.deltaTime);
					transform.position = Vector3.SmoothDamp (transform.position, target.position + offset, ref velocity, speed);
				}else{
					cam.fieldOfView = Mathf.Lerp (cam.fieldOfView, CameraDolly.FOVForHeightAndDistance (height, -transform.position.z) + 20, Time.deltaTime * 5);
					transform.position = Vector3.SmoothDamp (transform.position, Vector3.down + offset, ref velocity, 1);
				}



				// Vector3 targetPos = Vector3.Lerp(new Vector3 (xPos, yPos, target.position.z + offset.z), target.position + offset, 0.5f);

//		GetComponent<Camera>().orthographicSize = 10 + Mathf.Abs(target.GetComponent<PlayerTraversal> ().GetFlow());
	}
}
