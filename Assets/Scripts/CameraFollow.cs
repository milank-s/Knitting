using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {
	public float speed;
	public Camera uiCam;
	public Transform target;
	private Vector3 velocity = Vector2.zero;
	Vector3 nudge;
	Vector3 offset;

	public static Vector3 forward;
	float rot;
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
		//this assumes view dir is forward
		targetPos = pos;
		transform.position = pos - Main.cameraDistance * Vector3.forward;
		offset = Vector3.zero;
	}

	public IEnumerator MoveRoutine(){

		//get active stellation position
		//lerp to that position
		float t = 0;
		float startFOV = cam.fieldOfView;
		Vector3 startPos = transform.position;
		Vector3 dest = targetPos;

		if(Services.main.activeStellation._startPoints.Count > 0){
			Vector3 startPointPos = Services.main.activeStellation.start.Pos;
			if(!lockX) dest.x = startPointPos.x;
			if(!lockY) dest.y = startPointPos.y;
			if(!lockZ) dest.z = startPointPos.z;
		}
		
		Vector3 destOffset = dest - Main.cameraDistance * Vector3.forward;

		while(t < 1){
			float easedT = Easing.QuadEaseOut(t);
			cam.fieldOfView = Mathf.Lerp(startFOV, desiredFOV, t);
			transform.position = Vector3.Lerp(startPos, destOffset, easedT);
			t += Time.deltaTime;
			yield return null;
		}

		cam.fieldOfView = desiredFOV;
		WarpToPosition(dest);
	}


	// Update is called once per frame
	public void FollowPlayer()
	{
		//get cur spline
		//find its bounds
		// get around them
		
		Vector3 desiredPos = Services.PlayerBehaviour.pos;
		Vector3 playerDir = Services.PlayerBehaviour.curDirection;
		Vector3 toPlayer;

		if(Services.PlayerBehaviour.state == PlayerState.Traversing){

			//I wasn't smart enough to get this working
			//why dont you just track the players rotation delta and apply it to the camera?
			//obviously billboarding will make this a shitshow but it cant be that hard
			rot += Services.PlayerBehaviour.deltaAngle;

			if(rot > 360) rot -= 360;
			if(rot < 0) rot += 360;

			float radians = Mathf.Sin(rot * Mathf.Deg2Rad);

			// toPlayer = Quaternion.AngleAxis(rot, Vector3.up) * Vector3.forward;
			Quaternion r1 = Services.PlayerBehaviour.curPoint.transform.rotation;
			Quaternion r2 = Services.PlayerBehaviour.pointDest.transform.rotation;
			
			toPlayer = Quaternion.AngleAxis(rot, Vector3.up) * Vector3.forward;
			
			Vector3 pole = Vector3.up;

			// toPlayer = Vector3.Cross(pole, Services.PlayerBehaviour.curDirection).normalized;

			// Debug.DrawLine(pos, pos + pole, Color.green);
			// Debug.DrawLine(pos, pos + Services.PlayerBehaviour.curDirection, Color.red);
			// Debug.DrawLine(pos, pos + toPlayer, Color.blue);

			float t = Services.PlayerBehaviour.progress;
			if(!Services.PlayerBehaviour.goingForward) t = 1-t;
			transform.rotation = Quaternion.Lerp(r1, r2, t);

		}else{
			toPlayer = transform.forward;
		}
		
		if(Services.PlayerBehaviour.state != PlayerState.Flying){
			
			if(Services.PlayerBehaviour.state == PlayerState.Traversing){
			
				nudge = Vector3.Lerp(nudge, Vector3.zero, Time.deltaTime * 3);

			}else if(Services.PlayerBehaviour.state == PlayerState.Switching){
				if(Services.PlayerBehaviour.pointDest != null){
					nudge = (Services.PlayerBehaviour.pointDest.Pos - Services.PlayerBehaviour.curPoint.Pos).normalized;
				}
				
			}
			
		}else{
			nudge = transform.TransformVector(Services.PlayerBehaviour.cursorDir);
		}

		if(lockX){
			desiredPos.x = targetPos.x;
		}

		if(lockY){
			desiredPos.y = targetPos.y;
		}

		if(lockZ){
			desiredPos.z = targetPos.z;
		}
		
		//this now needs to be rotated using the current player direction pole
		desiredPos -= transform.forward * (Main.cameraDistance);

		cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, desiredFOV, Time.deltaTime * 3);

		float height;
		float yPos;
		float xPos;

		float zoom = Services.PlayerBehaviour.state == PlayerState.Flying ? 1 : 0;

		Vector3 shake = Services.PlayerBehaviour.state == 
		
		PlayerState.Traversing ? 
		
		(Vector3) Random.insideUnitCircle.normalized * Mathf.Pow(1 - Services.PlayerBehaviour.easedAccuracy, 2) *
				  (Services.PlayerBehaviour.curSpeed + 0.5f)/2f 
			: Vector3.zero;

		Vector3 lerpedPos = nudge/5f + shake - transform.forward * (zoom);
		offset = Vector3.SmoothDamp(offset, lerpedPos, ref velocity, speed);

		height = Mathf.Abs(CameraDolly.topBound - CameraDolly.bottomBound);
		yPos = Mathf.Lerp(CameraDolly.bottomBound, CameraDolly.topBound, 0.5f);
		xPos = Mathf.Lerp(CameraDolly.leftBound, CameraDolly.rightBound, 0.5f);

		transform.position = Vector3.Lerp(transform.position, desiredPos + offset, Time.deltaTime * 10);
		forward = transform.forward;
	}
}
