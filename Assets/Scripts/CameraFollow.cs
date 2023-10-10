using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {
	public Transform rotationVis;
	public float speed;
	public Camera uiCam;
	public Transform target;
	private Vector3 velocity = Vector2.zero;
	public Vector3 offset;
	Vector3 nudge;
	Vector3 lerpedPos;


	Quaternion rot;
	public Camera cam;
	public bool fixedCamera;

	public bool lockX, lockY, lockZ;
	public float desiredFOV;
	public static Vector3 targetPos;
	
	public static CameraFollow instance;
	// Use this for initialization
	void Awake()
	{
		instance = this;
		CameraDolly.leftBound = float.PositiveInfinity;
		CameraDolly.rightBound = float.NegativeInfinity;
		CameraDolly.topBound = float.NegativeInfinity;
		CameraDolly.bottomBound = float.PositiveInfinity;
	}

	public void Reset(){
		targetPos = Vector3.zero;
		transform.position = Vector3.zero;
		desiredFOV = 40;
		cam.fieldOfView = 40;
	}
	
	public void WarpToPosition(Vector3 pos)
	{
		targetPos = pos;
		transform.position = pos + offset;	
		cam.fieldOfView = desiredFOV;
	}

	
	// Update is called once per frame
	public void FollowPlayer()
	{
		//get cur spline
		//find its bounds
		// get around them

		Vector3 pos = Services.Player.transform.position;
		Vector3 playerDir = Services.PlayerBehaviour.curDirection;
		Vector3 toPlayer;

		if(Services.PlayerBehaviour.state == PlayerState.Traversing){


			//I wasn't smart enough to get this working
			//why dont you just track the players rotation delta and apply it to the camera?
			//obviously billboarding will make this a shitshow but it cant be that hard
			Vector3 delta = Services.PlayerBehaviour.directionDelta;
			Vector3 rot = (delta.z * 360f) * Vector3.up;
			rotationVis.Rotate(rot);
			
			Vector3 pole = Vector3.up;

			toPlayer = Vector3.Cross(pole, Services.PlayerBehaviour.curDirection).normalized;

			Debug.DrawLine(pos, pos + pole, Color.green);
			Debug.DrawLine(pos, pos + Services.PlayerBehaviour.curDirection, Color.red);
			Debug.DrawLine(pos, pos + toPlayer, Color.blue);

			// transform.rotation = Quaternion.LookRotation(toPlayer, Vector3.up);

		}else{
			toPlayer = transform.forward;
		}

		//this now needs to be rotated using the current player direction pole
		// pos += toPlayer * offset.z;
		pos += offset;
		
		if(Services.PlayerBehaviour.state != PlayerState.Flying){
			
			if(Services.PlayerBehaviour.state == PlayerState.Traversing){
				//nudge wasn't working with backwards ughhhh
				
					nudge = Services.PlayerBehaviour.curDirection;

				if(!Services.PlayerBehaviour.goingForward){
					nudge = -nudge;
				}
				
			}else if(Services.PlayerBehaviour.state == PlayerState.Switching){
				if(Services.PlayerBehaviour.pointDest != null){
					nudge = (Services.PlayerBehaviour.pointDest.Pos - Services.PlayerBehaviour.curPoint.Pos).normalized;
				}
				//we could do lots of diff position hinting here using the point dest if connecting or about to traverse
				// nudge = Services.PlayerBehaviour.playerSprite.transform.up;
			}
			
		}else{
			nudge = transform.TransformVector(Services.PlayerBehaviour.cursorDir);
		}

		if(lockX){
			pos.x = targetPos.x;
		}

		if(lockY){
			pos.y = targetPos.y;
		}

		if(lockZ){
			pos.z = targetPos.z;
		}
	
		cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, desiredFOV, Time.deltaTime * 3);

		float height;
		float yPos;
		float xPos;

		Vector3 shake = Services.PlayerBehaviour.state == 
		
		PlayerState.Traversing ? 
		
		(Vector3) Random.insideUnitCircle.normalized * Mathf.Pow(1 - Services.PlayerBehaviour.easedAccuracy, 2) *
				  (Services.PlayerBehaviour.actualSpeed + 0.5f)/2f 
			: Vector3.zero;

		lerpedPos = Vector3.SmoothDamp(lerpedPos, nudge/5f + shake,
			ref velocity, speed);

		height = Mathf.Abs(CameraDolly.topBound - CameraDolly.bottomBound);
		yPos = Mathf.Lerp(CameraDolly.bottomBound, CameraDolly.topBound, 0.5f);
		xPos = Mathf.Lerp(CameraDolly.leftBound, CameraDolly.rightBound, 0.5f);

		transform.position = pos + lerpedPos;
		//		if (fixedCamera && Services.PlayerBehaviour.curSpline != null)
//		{
//			Vector3 curSplinePos = Services.PlayerBehaviour.curSpline.transform.position;
//			targetPos = new Vector3(curSplinePos.x, curSplinePos.y,
//				Services.PlayerBehaviour.transform.position.z + offset.z);
//		}
//		else
//		{
//			targetPos = new Vector3(target.position.x, target.position.y,
//				Services.PlayerBehaviour.transform.position.z + offset.z);
//			//desiredFOV = Mathf.Clamp(CameraDolly.FOVForHeightAndDistance(height, -offset.z) + 5, 15f, 100);
//		}

		//		if (target.position.x > CameraDolly.rightBound)
//		{
//			CameraDolly.rightBound = target.position.x;
//		}
//
//		if (target.position.x < CameraDolly.leftBound)
//		{
//			CameraDolly.leftBound = target.position.x;
//		}
//
//		if (target.position.y > CameraDolly.topBound)
//		{
//			CameraDolly.topBound = target.position.y;
//		}
//
//		if (target.position.y < CameraDolly.bottomBound)
//		{
//			CameraDolly.bottomBound = target.position.y;
//		}
//
//		CameraDolly.topBound -= Time.deltaTime / 5f;
//		CameraDolly.bottomBound += Time.deltaTime / 5f;
//		CameraDolly.rightBound -= Time.deltaTime / 5f;
//		CameraDolly.leftBound += Time.deltaTime / 5f;

//
//		CameraDolly.topBound =
//			Mathf.Clamp(CameraDolly.topBound, target.position.y + 0.25f, target.position.y + 100);
//		CameraDolly.bottomBound =
//			Mathf.Clamp(CameraDolly.bottomBound, target.position.y - 100, target.position.y - 0.25f);
//		CameraDolly.rightBound =
//			Mathf.Clamp(CameraDolly.rightBound, target.position.x + 0.25f, target.position.x + 100);
//		CameraDolly.leftBound =
//			Mathf.Clamp(CameraDolly.leftBound, target.position.x - 100, target.position.x - 0.25f);
		
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
		
		// if(target.GetComponent<PlayerBehaviour>().state != PlayerState.Animating){
		// 	cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, desiredFOV, Time.deltaTime * speed);
		// 	transform.position = Vector3.SmoothDamp (transform.position, target.position + offset, ref velocity, speed);
		// }else{

		//-transform.position.z
		// Debug.Log(CameraDolly.FOVForHeightAndDistance (height, offset.z));
		// this is negative
		
		
		// new Vector3(curSplinePos.x, curSplinePos.y Services.PlayerBehaviour.transform.position.z)
		//Vector3.Lerp(Services.PlayerBehaviour.transform.position, new Vector3(xPos, yPos, Services.PlayerBehaviour.transform.position.z), 1f)

//					transform.position = new Vector3(targetPos.x, targetPos.y, Services.PlayerBehaviour.transform.position.z + offset.z);
		// }

		// Vector3 targetPos = Vector3.Lerp(new Vector3 (xPos, yPos, target.position.z + offset.z), target.position + offset, 0.5f);

//		GetComponent<Camera>().orthographicSize = 10 + Mathf.Abs(target.GetComponent<PlayerTraversal> ().GetFlow());
	}
}
