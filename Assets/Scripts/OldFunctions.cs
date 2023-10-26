using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OldFunctions : MonoBehaviour
{
    // 		while(index < newPointList.Count -1){
//
// 			speed += Time.deltaTime/10;
// 			t += Time.deltaTime * ((index + 2)/2);
// //			Vector3 lastPos =pos;
// //			transform.position = Vector3.Lerp (newPointList[index].position, newPointList [index - 1].position, t);
// 			Transform curJoint = newPointList[newPointList.Count - 1 - index];
// 			curJoint.position = Vector3.Lerp(curJoint.position, curPoint.Pos, t);
// //			float curDistance = Vector3.Distance (newPointList [index].position,pos);
// 			transform.position = newPointList[0].transform.position;
// //			sprite.transform.up =pos - lastPos;
// 			l.SetPosition (0,pos);
//
// 			for(int i = 1; i <= newPointList.Count - index; i++){
// 				l.SetPosition(i, newPointList[i-1].position);
// 			}
//
// 			l.SetPosition (newPointList.Count - index + 1, curPoint.Pos);
//
//
// 			if (t >= 1) {
// 				GameObject toDestroy = newPointList [newPointList.Count - 1 - index].gameObject;
// 				Destroy (toDestroy);
//
// 				index++;
// 				l.positionCount = newPointList.Count - index + 2;
//
// 				if (index >= newPointList.Count - 1) {
// 					Destroy (newPointList [newPointList.Count - 1 - index].gameObject);
// 				} else {
//
// 					newPointList [newPointList.Count - 1 - index].GetComponent<SpringJoint> ().connectedBody = curPoint.GetComponent<Rigidbody>();
//
// 					distance = Vector3.Distance (newPointList [newPointList.Count - 1 - index].position, curPoint.Pos);
// 				}
//
//
// 				t = Mathf.Clamp01 (t - 1);
//
// 			} else {
// 				yield return null;
// 			}
// 		}
//
// 		l.positionCount = 0;
// 		newPointList.Clear ();

// 		state = PlayerState.Switching;
// 	}

// 	public IEnumerator FlyIntoNewPoint(Point p){

// 		int index = newPointList.Count - 4;
// //		int index = newPointList.Count/2;

// 		float t = 0;

// 		Point curP = curPoint;
// 		Spline s = curSpline;

// 		while (index >= 0) {
// 			SplinePointPair spp;

// //			Point newPoint = Services.PlayerBehaviour.CheckIfOverPoint (newPointList[index].position);
// 			Point nextp = SplineUtil.CreatePoint(newPointList[index].position);
// 			spp = SplineUtil.ConnectPoints (s, curP, nextp);

// 			//IS THIS REALLY THE ONLY CASE I CONNECT SPRINGJOINTS
// 			//WHY IS CONNECTING SPRING JOINTS THIS WAY BETTER THAN JUST LEAVING THEM UNCONNECTED
// 			// if (curP != curPoint) {
// 			// 	curP.GetComponent<SpringJoint> ().autoConfigureConnectedAnchor = true;
// 			// 	curP.GetComponent<SpringJoint> ().connectedBody = nextp.rb;
// 			// }

// 			if (newPointList [index].GetComponentInChildren<SpriteRenderer>()) {
// 				newPointList [index].GetComponentInChildren<SpriteRenderer> ().sprite = null;
// 			}

// 			traversedPoints.Add (curP);

// 			s = spp.s;
// 			curP = spp.p;
// //			curP.transform.parent = s.transform;

// 			index -= 4;
// 		}


// 		//could add another point at the player's current position between curP (last in index) and p (destination) to make player position not jump
// 		//whats with phantom splines
// 		//must be an error with closed/looping splines getting created and fucking up

// 		SplinePointPair	sp = SplineUtil.ConnectPoints (curSpline, drawnPoint, p);
// 	  // drawnPoint.GetComponent<SpringJoint> ().connectedBody = p.rb;
// 		curSpline = sp.s;

// 		lastPoint = p;
// 		curPoint = p;
// 		s.SetSelectedPoint(curP);
// 		progress = 1;

// 		Vector3 pos = transform.position;

// 		float distance = Vector3.Distance (pos, p.Pos);

// 		float speed = 0;

// 		while (speed < 1) {
// 			transform.position = Vector3.Lerp(pos, p.Pos, speed);
// 			speed += flow  * Time.deltaTime;
// 			speed += Time.deltaTime;

// //			for(int i = 0; i < newPointList.Count; i++){
// ////				newPointList [i].GetComponent<SpringJoint> ().spring = newPointList.Count / (i + 1);
// ////				l.SetPosition(i, Vector3.Lerp(l.GetPosition(i), newPointList[i].position, 1 -(Vector3.Distance(transform.position, p.Pos)/distance)));
// ////				l.SetPosition(i, newPointList[i].position);
// //			}

// 				yield return null;
// 		}
// 		transform.position = p.Pos;

// //		if (!p._connectedSplines.Contains (curSpline)) {
// //			nextPoint = p;
// //			SetPlayerAtEnd (nextSpline, nextPoint);
// //			CheckProgress ();
// //
// //		} else {
// //
// //			SetPlayerAtEnd (curSpline, nextPoint);
// //			CheckProgress ();
// //		}

// //		SetPlayerAtEnd (s, p);
// //		CheckProgress();
// 		for (int i = newPointList.Count - 1; i >= 0; i--) {
// 			Destroy (newPointList [i].gameObject);
// 		}

// 		newPointList.Clear ();
// 		l.positionCount = 0;
// 		state = PlayerState.Switching;
// 	}

// void TrackFreeMovement(){

// 		Vector3 inertia;

// 		// Make drawing points while you skate.
// 		//should solve the problems of jumping across new points on the same spline.

// 		//YOU WERE DOING TWO RAYCASTS HERE FOR NO REASON AFTER CANCREATEPOINT WAS REFACTORED
// 		// Point overPoint = SplineUtil.RaycastDownToPoint(cursorPos, 2f, 1f);
// 		// if(overPoint != null && overPoint != curPoint){
// 			//Getting a null ref here for some ungodly reason
// 			// if(Vector3.Distance (curPoint.Pos, drawnPoint.Pos) < 0.25f){
// 			// 	curSpline.SplinePoints.Remove(curPoint);
// 			// 	drawnPoint._neighbours.Remove(curPoint);
// 			// 	Destroy(curPoint.gameObject);
// 			// 	curPoint = drawnPoint;
// 			// 	curSpline.Selected = drawnPoint;
// 			// 	// SplinePointPair spp = SplineUtil.ConnectPoints(curSpline, drawnPoint, overPoint);
// 			// 	// curSpline = spp.s;
// 			// 	// drawnPoint = spp.p;
// 			// 	// traversedPoints.Add(drawnPoint);
// 			// 	//
// 			// 	// SplinePointPair sppp = SplineUtil.ConnectPoints(curSpline, drawnPoint, curPoint);
// 			// 	// curSpline = sppp.s;
// 			// 	// curSpline.Selected = drawnPoint;
// 			// 	// curSpline.OnSplineEnter (true, drawnPoint, curPoint, false);
// 			// }

// 			if(CanConnect()){
// 				//remove current point from curspline and connect drawnPoint to pointDest on current spline
// 				// curSpline.SplinePoints.Remove(curPoint);
// 				// drawnPoint._neighbours.Remove(curPoint);
// 				// Destroy(curPoint.gameObject);
// 				// curPoint = drawnPoint;
// 				//this is bugged if the player flies right into the point without creating any on the way
// 				//the player is warping back to the start of the last spline for no REASON

// 				Connect();

// 				curPoint.GetComponent<Collider>().enabled = true;
// 				curPoint.velocity = cursorDir * Mathf.Abs(flow);
// 				curPoint.isKinematic = false;
// 				SwitchState(PlayerState.Traversing);
// 				return;
// 			}


// 			// if(RaycastHitObj == curPoint){
// 			// 	StartCoroutine (ReturnToLastPoint ());
// 			// }else{
// 			// StartCoroutine(FlyIntoNewPoint(RaycastHitObj));
// 			// }

// 		 if (flow < 0) {
// //			CreateJoint (newPointList[newPointList.Count-1].GetComponent<Rigidbody>());
// 			// StartCoroutine (ReturnToLastPoint ());
// 			SwitchState(PlayerState.Animating);

// 		} else {
// 			inertia = cursorDir * flow;
// 			flow -= Time.deltaTime/10;
// 			transform.position += inertia * Time.deltaTime;
// 			curDrawDistance += Vector3.Distance (curPoint.Pos,pos);
// 			curPoint.transform.position =pos;
// 			creationInterval -= Time.deltaTime;
// 			if (creationInterval < 0 && curDrawDistance > PointDrawDistance) {
// 					creationInterval = creationCD;
// 					curDrawDistance = 0;
// 				// if (newPointList.Count == 0) {
// 					curPoint.velocity = Mathf.Abs(flow) * cursorDir;
// 					Point newPoint;
// 					newPoint = SplineUtil.CreatePoint(transform.position);
// 					curPoint.GetComponent<Collider>().enabled = true;
// 					curPoint.velocity = cursorDir * Mathf.Abs(flow);
// 					curPoint.isKinematic = false;
// 					curPoint.proximity = 0;
// 					newPoint.GetComponent<Collider>().enabled = false;
// 					newPoint.isKinematic = true;
// 					newPoint.proximity = 1;
// 					SplinePointPair spp = SplineUtil.ConnectPoints(curSpline, curPoint, newPoint);
// 					lastPoint = drawnPoint;
// 					curSpline = spp.s;
// 					drawnPoint = curPoint;
// 					curPoint = newPoint;
// 					curSpline.SetSelectedPoint(drawnPoint);
// 					traversedPoints.Add(drawnPoint);
// 				  curSpline.OnSplineEnter ();
// 				// } else {
// 				// 	CreateJoint (newPointList [newPointList.Count - 1].GetComponent<Rigidbody> ());
// 				// }
// 			}else{
// 					//Something is going on when you connect to the spline you're already drawing.
// 					//the new spline created on the first ConnectPoint is a new spline, then something weird is happening to it
// 					//almost 100% sure the SELECTED point is wrong on the new splines

// 				// Point overPoint = SplineUtil.RaycastDownToPoint(transform.position, 10f, 5f);
// 				// if(overPoint != null && overPoint != drawnPoint && overPoint != curPoint && overPoint != lastPoint){
// 				// 	curSpline.SplinePoints.Remove(curPoint);
// 				// 	drawnPoint._neighbours.Remove(curPoint);
// 				// 	lastPoint = drawnPoint;
// 				// 	SplinePointPair spp = SplineUtil.ConnectPoints(curSpline, drawnPoint, overPoint);
// 				// 	curSpline = spp.s;
// 				// 	drawnPoint = spp.p;
// 				// 	traversedPoints.Add(drawnPoint);
// 				//
// 				// 	SplinePointPair sppp = SplineUtil.ConnectPoints(curSpline, drawnPoint, curPoint);
// 				// 	curSpline = sppp.s;
// 				// 	curSpline.Selected = drawnPoint;
// 				// 	curSpline.OnSplineEnter (true, drawnPoint, curPoint, false);
// 					//make new point for curPoint using above code
// 				// }
// 			}
// 		}
// 	}

//	public void OnTriggerEnter(Collider col){
//		if (col.tag == "Point") {
//			if (!col.GetComponent<Point> ().isPlaced) {
//				StartCoroutine (CollectPoint (col.GetComponent<Point> ()));
//			}
//		}
//	}

	// IEnumerator CollectPoint(Point p){

	// 	p.GetComponent<SpriteRenderer> ().enabled = false;

	// 	if (!inventory.Contains (p)) {
	// 		p.GetComponent<Collider> ().enabled = false;
	// 		if (inventory.Count > 0) {
	// 			p.GetComponent<SpringJoint> ().connectedBody = inventory [inventory.Count - 1].GetComponent<Rigidbody> ();
	// 		} else {
	// 			p.GetComponent<SpringJoint> ().connectedBody = GetComponent<Rigidbody> ();

	// 		}
	// 		inventory.Add (p);
	// 		p.transform.parent = transform;
	// 		float t = 0;

	// 		Vector3 originalPos = p.transform.position;

	// 		while (t <= 1) {
	// 			p.transform.position = Vector3.Lerp (originalPos,pos, t);
	// 			t += Time.deltaTime;
	// 			yield return null;
	// 		}
	// 	}
	// }

// public IEnumerator Unwind()
// 	{

// 		float t = curSpeed;
// 		bool moving = true;
// 		int pIndex = traversedPoints.Count -1;
// 		bool moveToLastPoint = false;

// 		Point nextPoint =  traversedPoints [pIndex];

// 		if (state != PlayerState.Switching) {

// 			moveToLastPoint = true;
// 		}

// 		pIndex--;

// 		//add case for stopping in middle of line
// 		//figure out why flow is always non-zero on line.
// 		state = PlayerState.Animating;

// 		if (moveToLastPoint) {
// 			if (curPoint == curSpline.Selected) {
// 				goingForward = false;
// 			} else {
// 				goingForward = true;
// 			}

// 			while (moving) {
// 				t += Time.deltaTime;
// 				flow = t;

// 				if (goingForward) {
// 					progress += Time.deltaTime * t / curSpline.segmentDistance;
// 				} else {
// 					progress -= Time.deltaTime * t / curSpline.segmentDistance;
// 				}

// 				curSpline.completion = Mathf.Lerp(curSpline.completion, 0, t);
// //				curSpline.distortion = Mathf.Lerp(curSpline.distortion, 1, progress);

// 				transform.position = curSpline.GetPointForPlayer (progress);

// 				if (progress > 1 || progress < 0) {
// 					moving = false;
// 				}
				

// 				yield return null;
// 			}


// 		}

// 		for (int i = pIndex; i >= 0; i--)
// 		{

// 			curPoint = nextPoint;
// 			nextPoint = traversedPoints[i];
// 			curSpline = curPoint.GetConnectingSpline(nextPoint);

// 			SetPlayerAtStart(curSpline, nextPoint);
// 			moving = true;

// 			while (moving)
// 			{
				
// 				curSpline.completion = Mathf.Lerp(curSpline.completion, 0, t);
// 				curSpline.distortion = Mathf.Sin(t * Mathf.PI);
// 				t += Time.deltaTime;
// 				t = Mathf.Clamp(t, 0f, 9f);
// 				flow = t;

				
// 				if (goingForward)
// 				{
// 					progress += Time.deltaTime * t / curSpline.segmentDistance;
// 				}
// 				else
// 				{
// 					progress -= Time.deltaTime * t / curSpline.segmentDistance;
// 				}

// 				transform.position = curSpline.GetPointForPlayer(progress);

// 				if (progress > 1 || progress < 0)
// 				{
// 					transform.position = curSpline.GetPointForPlayer(Mathf.Clamp01(progress));
// 					traversedPoints[i].Reset();
// 					moving = false;
// 				}

// 				yield return null;
// 			}
// 		}

// 		flow = 0;
// 		lastPoint = curPoint;
// 		curPoint = nextPoint;
// 		traversedPoints.Clear ();
// 		traversedPoints.Add (curPoint);
// 		state = PlayerState.Switching;

// 		Initialize();

// 	}

// void Effects(){
//     		if (curSpline != null)
// 		{
//			if(flow > 0.2	5f){
//				velocityLine.color = Color.Lerp(velocityLine.color, new Color(1,1,1,0.1f), Time.deltaTime);
//				velocityLine2.color = Color.Lerp(velocityLine2.color, new Color(1,1,1,0.1f), Time.deltaTime);
//				// DrawVelocity();
//			}else{
//				velocityLine.color = new Color(1,1,1,0.1f);
//				velocityLine2.color = new Color(1,1,1,0.1f);
//			}

//			curSpline.DrawLineSegmentVelocity (progress, Mathf.Sign (accuracy), goingForward ? 0 : 1);\
			// curSpline.l.material.mainTextureOffset -= Vector2.right * Mathf.Sign (accuracy) * flow * curSpline.l.material.mainTextureScale.x * 2 * Time.deltaTime;
//			l.SetPosition(0,pos);
//			l.SetPosition(1,pos + (curSpline.GetDirection(progress) * Mathf.Sign(accuracy))/2);
//			l.SetPosition(1,pos + cursorDir/2);
//			GetComponentInChildren<Camera>().farClipPlane = Mathf.Lerp(GetComponentInChildren<Camera>().farClipPlane,  flow + 12, Time.deltaTime * 10);
// 		}
// }

// void DrawVelocity(){

// 		float s = 1f/(float)Spline.curveFidelity;
// 		for(int i = 0; i < Spline.curveFidelity * 3; i +=3){
// 			int index = i/3;
// 			float step = (float)index/(float)Spline.curveFidelity;
// 			if(i >= velocityLine.points3.Count-1){
// 				velocityLine.points3.Add(Vector3.zero);
// 				velocityLine.points3.Add(Vector3.zero);
// 				velocityLine.points3.Add(Vector3.zero);
// 			}
// 			if(i >= velocityLine2.points3.Count-1){
// 				velocityLine2.points3.Add(Vector3.zero);
// 				velocityLine2.points3.Add(Vector3.zero);
// 				velocityLine2.points3.Add(Vector3.zero);
// 			}
// 			Vector3 pos =  curSpline.GetPointForPlayer(step + Mathf.Epsilon);
// 			velocityLine.points3[i] = pos;

// 			float f = (step - progress);

// 			velocityLine2.points3[i+1] = Vector3.Lerp(velocityLine2.points3[i + 1], velocityLine2.points3[i], Time.deltaTime);
// 			if(f > s){
// 				velocityLine.points3[i+1] = pos;
// 			}
// 			else if(f <= s && f >= 0){
// 				if(step == 0){
// 					velocityLine.points3[i+1] = pos + curSpline.GetDirection(step + 0.01f) * Mathf.Pow((1-Mathf.Abs(f)), 2) * curSpeed * curSpline.segmentDistance * (s) * Spline.curveFidelity/2;
// 				}else{
// 					velocityLine.points3[i+1] = pos + curSpline.GetDirection(step) * Mathf.Pow((1-Mathf.Abs(f)), 2) * curSpeed * curSpline.segmentDistance * (s - f) * Spline.curveFidelity/2;
// 				}
// 			}else{
// 				velocityLine.points3[i + 1] = Vector3.Lerp(velocityLine.points3[i + 1], velocityLine.points3[i], Time.deltaTime);
// 			}
// 			velocityLine.points3[i + 2] = pos;
// 		}

// 		velocityLine.Draw3D();
// 		velocityLine2.Draw3D();
		// velocityLine.positionCount = velocityLine.positionCount + 10;
		// Vector3 lpos =  curSpline.GetPoint(progress);
		// velocityLine.SetPosition(velocityLine.positionCount-11, lpos);
		// // velocityLine.SetPosition(1, pos + curSpline.GetDirection(progress) * curSpeed * curSpline.distance);
		// Vector3 dir = curSpline.GetDirection(progress) * (transform.position - cursorPos).magnitude;
		// for(int i = 1; i < 10; i ++){
		// 	Vector3 lastP = velocityLine.GetPosition(velocityLine.positionCount - 10 + i-2);
		// 	velocityLine.SetPosition(velocityLine.positionCount - 10 + i-1, Vector3.Lerp(velocityLine.GetPosition(velocityLine.positionCount -10 + i -1), Vector3.Lerp(lastP + dir/8, lastP + (cursorPos - lastP)/5, (float)i/10), Time.deltaTime * 50));
		// }
		// velocityLine.SetPosition(10, Vector3.Lerp(velocityLine.GetPosition(10), cursorPos, Time.deltaTime * 50));

	// }

    // void Initialize(){
//		Material newMat;
//		newMat = Services.Prefabs.lines[3];
//		Texture tex = newMat.mainTexture;
//		float length = newMat.mainTextureScale.x;
//		float height = newMat.mainTextureScale.y;
//
//		velocityLine = new VectorLine (name, new List<Vector3> (10), height, LineType.Discrete, Vectrosity.Joins.Weld);
//		velocityLine.color = Color.black;
//		velocityLine.smoothWidth = true;
//		velocityLine.smoothColor = true;
//
//		velocityLine.texture = tex;
//		velocityLine.textureScale = newMat.mainTextureScale.x;
//
//		newMat = Services.Prefabs.lines[3];
//		tex = newMat.mainTexture;
//		length = newMat.mainTextureScale.x;
//		height = newMat.mainTextureScale.y;
//
//		velocityLine2 = new VectorLine (name, new List<Vector3> (30), height, LineType.Discrete, Vectrosity.Joins.Weld);
//		velocityLine2.color =  Color.black;
//		velocityLine2.smoothWidth = true;
//		velocityLine2.smoothColor = true;
//
//		velocityLine2.texture = tex;
//		velocityLine2.textureScale = newMat.mainTextureScale.x;
    // }

    // public IEnumerator ReturnToLastPoint(){

		// state = PlayerState.Animating;
		// float t = 0;
		// bool moving = true;
		// float flowMult = 1;


		// 	if (drawnPoint== curSpline.Selected) {
		// 		goingForward = false;
		// 	} else {
		// 		goingForward = true;
		// 	}

		// 	while (moving) {
		// 		t += Time.deltaTime;
		// 		if(Mathf.Abs(flow) > 1){
		// 			flowMult = Mathf.Abs(flow);
		// 		}

		// 		if (goingForward) {
		// 			progress += (Time.deltaTime * t * flowMult) / curSpline.segmentDistance;
		// 		} else {
		// 			progress -= (Time.deltaTime * t  * flowMult) / curSpline.segmentDistance;
		// 		}

		// 		transform.position = curSpline.GetPointForPlayer (progress);

		// 		if (progress > 1 || progress < 0) {
		// 			moving = false;
		// 		}
		// 		yield return null;
		// 	}
	// }

//     	public IEnumerator RetraceTrail()
// 	{

// 		Vector3[] positions = new Vector3[flyingTrail.positionCount];
// 		flyingTrail.GetPositions(positions);
// 		float f = 0;
// 		float lerpSpeed = 0;
// 		float distance;

// //		Point p = SplineUtil.CreatePoint(transform.position);
// //		p.pointType = PointTypes.connect;
// //		p.Initialize();

// 		for (int i = positions.Length -1; i >= 0; i--)
// 		{
// 			float temp = 0;
// 			Vector3 tempPos =pos;
// 			distance = Vector3.Distance(tempPos, positions[i]);
// 			while (temp < 1)
// 			{
// 				transform.position = Vector3.Lerp(tempPos, positions[i], temp);
// //				transform.position = positions[i];
// 				temp += (Time.deltaTime * lerpSpeed)/distance;
// 				lerpSpeed += Time.deltaTime;
// 				f = Mathf.Clamp01(lerpSpeed);
// 				//play drone music or whatever
// 				yield return null;
// 			}

// 			//bake the mesh out after this and copy it before clearing the trail renderer
// 		}

// 		Services.fx.BakeTrail(Services.fx.flyingTrail, Services.fx.flyingTrailMesh);

// 		state = PlayerState.Switching;
// 		curSpeed = lerpSpeed;
// 		StartCoroutine(Unwind());
// 	}
}
