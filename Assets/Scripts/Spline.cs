	using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System;
using Vectrosity;
using UnityEngine.Audio;
using System.Linq;
// [ExecuteInEditMode]
public class Spline : MonoBehaviour
{


	public static List<Spline> Splines = new List<Spline> ();

	public List<Point> SplinePoints;

	public Point Selected;

	[HideInInspector]
	public VectorLine line;

	public static Spline Select;
	bool reversed;
	public int curveFidelity = 10;
	public float drawSpeed = 6;
	public float distance = 0;
	public float segmentDistance = 0;
	public Vector2 linearDirection;
	public bool closed = false;
	public int LoopIndex;
	public bool locked = false;
	public bool isDrawing = true;
	public bool draw;

	private float colorDecay;
	float distanceFromPlayer;
	float invertedDistance;

	private int lowHitPoint = int.MaxValue;
	private int highHitPoint = -int.MaxValue;
	private float playerProgress;

	public bool isPlayerOn = false;

	public bool isSelect {
		get {
			return this == Spline.Select;
		}
	}

	private static string path;

	public static string SavePath {
		get {
			if (path == null) {
				path = System.Environment.GetFolderPath (System.Environment.SpecialFolder.MyDocuments) + @"\SplinesSave\";
			}

			return path;
		}
		set {
			path = value;
		}
	}

	public int CurveCount {
		get {
			return (SplinePoints.Count - 1) / 3;
		}
	}

	public Point EndPoint{
		get {
			return SplinePoints[SplinePoints.Count - 1];
		}
	}

	public Point StartPoint{
		get {
			return SplinePoints[0];
		}
	}

	private float drawCooldown = 1f;
	private float drawTimer = 0;
	private float pitch;
	private float frequency;
	private float volume;
	private AudioSource sound;
	private Coroutine curSound;

	//FX FUNCTIONS

	public void OnSplineExit ()
	{
		draw = false;
		isPlayerOn = false;

		// if (curSound != null) {
		// 	StopCoroutine (curSound);
		// 	StartCoroutine (FadeNote (sound));
		// }

	}

	public void OnSplineEnter (bool enter, Point p1, Point p2, bool forceDraw = false)
	{
		draw = false;

		int i = SplinePoints.IndexOf (p1);
		int j = SplinePoints.IndexOf (p2);

		if (forceDraw) {
			draw = true;
		}


		if (i < lowHitPoint) {
			lowHitPoint = i;
			draw = true;
		} else if (i > highHitPoint) {
			highHitPoint = i;
			draw = true;
		}

		if (j > highHitPoint) {
			highHitPoint = j;
			draw = true;
		} else if (j < lowHitPoint) {
			lowHitPoint = j;
			draw = true;
		}


		if (draw) {

			int indexdiff = j - i;

			if (indexdiff == -1 || indexdiff > 1) {
				playerProgress = 0;
//				StartCoroutine (DrawMeshOverTime (i, j, true));
				reversed = true;

			} else {
				playerProgress = 1;
//				StartCoroutine (DrawMeshOverTime (i, j));
				reversed = false;
			}
		}

		CalculateDistance ();

//		if (enter) {
		// if (curSound != null && sound != null) {
		// 	StopCoroutine (curSound);
		// 	StartCoroutine (FadeNote (sound));
		// }

		// PlayAttack (p1, p2);

		isPlayerOn = true;

//		}
	}

	public void PlayAttack (Point point1, Point point2)
	{

//		do some angle shit or normalize it??
		segmentDistance = Vector3.Distance (point1.Pos, point2.Pos);
		linearDirection = point2.Pos - point1.Pos;
		linearDirection = new Vector2(linearDirection.x, linearDirection.y).normalized;
		float dot = Vector2.Dot (linearDirection, Vector2.up);

		int index = (int)(((dot/2f) + 0.5f) * (Services.Sounds.sustains.Count-1));

		curSound = StartCoroutine (PlaySustain (index));

	}

	public void ManageSound (bool fade, float lerpVal)
	{
//		Services.PlayerBehaviour.flow / (Services.PlayerBehaviour.maxSpeed/2))

		if (fade) {
			sound.volume = Mathf.Lerp (Services.PlayerBehaviour.connectTime, 0, lerpVal);
		} else {
			sound.volume = Mathf.Lerp (0, Services.PlayerBehaviour.connectTime, lerpVal);
		}
		float dot = Vector2.Dot(Services.PlayerBehaviour.curSpline.GetDirection (Services.PlayerBehaviour.progress), linearDirection);
		float curFreqGain;

		Services.Sounds.master.GetFloat ("CenterFreq", out curFreqGain);
		float lerpAmount = Services.PlayerBehaviour.goingForward ? Services.PlayerBehaviour.progress : 1 - Services.PlayerBehaviour.progress;


		Services.Sounds.master.SetFloat("CenterFreq", Mathf.Lerp(curFreqGain, ((dot/2f + 0.5f) + Mathf.Clamp01(1f/Mathf.Pow(segmentDistance, 5))) * (16000f / curFreqGain), lerpAmount));

		//centering freq on note freq will just boost the fundamental. can shift this value to highlight diff harmonics
		//graph functions
		//normalize values before multiplying by freq
		//use note to freq script

//		pitch = dot product between the current tangent of the spline and the linear distance between points
		Services.Sounds.master.SetFloat("FreqGain", Mathf.Abs(Services.PlayerBehaviour.flow)/2 + 1f);
	}

	public IEnumerator PlaySustain (int index)
	{

//		AudioClip soundEffect = Services.Sounds.Loops [(int)((1 - Mathf.Clamp01 ((segmentDistance) / 10 * 2.5f)) * (Services.Sounds.Loops.Count - 1))];
		AudioClip soundEffect = Services.Sounds.sustains[index];

		sound = Services.Prefabs.CreateSoundEffect (soundEffect, Selected.Pos);
		sound.clip = soundEffect;
		sound.Play ();

		float t = 0;

		while (t < 1) {

			ManageSound (false, t);

			t += Time.deltaTime;
			yield return null;
		}

		while (true) {

//			float progressToSin = Mathf.Sin (Services.PlayerBehaviour.progress * Mathf.PI);
			ManageSound (false, 1);

			yield return null;
		}

	}

	public IEnumerator FadeNote(AudioSource s){

		GameObject toDelete = s.gameObject;
		float t = 0;

		while (t < 1) {
			s.volume = Mathf.Lerp (s.volume, 0, t);
			t += Time.deltaTime/1;
			yield return null;
		}

		Destroy (toDelete);
	}


	void OnDestroy ()
	{
		Splines.Remove (this);
	}

	public void SetupSpline(){

		if (SplinePoints.Count > 0) {
			List<Point> copyPoints = new List<Point>(SplinePoints);
			SplinePoints.Clear();
			foreach (Point p in copyPoints) {
				AddPoint (null, p);
			}

			if(closed){
				AddPoint (null, EndPoint);
			}
		}
	}

	void Awake ()
	{

		if (SplinePoints.Count > 0) {

		} else {
			SplinePoints = new List<Point> ();
		}

		Select = this;
		Splines.Add (this);


		line = new VectorLine (name, line.points3, 2, LineType.Continuous, Vectrosity.Joins.Weld);
		line.color = Color.black;
		line.smoothWidth = true;
		line.smoothColor = true;
		line.points3 = new List<Vector3> ();
		line.textureScale = 0.1f;

	}

	public void Draw(){
		DrawMesh (reversed);
		line.Draw3D();
	}

	void OldUpdate (){

		if (SplinePoints.Count > 0) {



			if (isPlayerOn) {
				DrawMesh (reversed);
				line.Draw3D();
//				DrawMesh (reversed);
//				line.Draw3D();
//					drawTimer -= Time.deltaTime;
//
//					if (drawTimer < 0) {
//						drawTimer = drawCooldown;
//						float time = UnityEngine.Random.Range (3f, 5f);
//						VectorLine l = VectorLine.SetLine3D (Color.gray, time, line.points3.ToArray ());
//						l = line;
//
//						StartCoroutine (FadeLine (l, time));
//					}

			}
		}

//		if(Select==null)
//			Select=this;
//		if(!isSelect)
//			return;
//
//		if (Input.GetMouseButtonDown (1)) {
//			Vector3 C = new Vector3 (Input.mousePosition.x, Input.mousePosition.y, Input.mousePosition.z);
//			float minDistance = float.MaxValue;
//			int minI = 0;
//			Vector3 minD = Vector3.zero;
//			bool flag = true;
//			minDistance = float.MaxValue;
//			for (int i = 0; i < SplinePoints.Count - 1; i++) {
//
//				Vector3 A = CameraControler.MainCamera.WorldToScreenPoint (SplinePoints [i].Pos);
//				Vector3 B = CameraControler.MainCamera.WorldToScreenPoint (SplinePoints [i + 1].Pos);
//
//				Vector3 D = A + Vector3.Project (C - A, B - A);
//				Vector3 Va = D - A;
//				Vector3 Vb = D - B;
//
//				if ((Mathf.Sign (Va.x) != Mathf.Sign (Vb.x) || Va.x == 0 && Vb.x == 0) &&
//				   (Mathf.Sign (Va.y) != Mathf.Sign (Vb.y) || Va.y == 0 && Vb.y == 0) &&
//				   (Mathf.Sign (Va.z) != Mathf.Sign (Vb.z) || Va.z == 0 && Vb.z == 0) &&
//				   Vector3.Distance (D, C) < minDistance) {
//					minI = i;
//					minD = D;
//					minDistance = Vector3.Distance (D, C);
//					flag = false;
//				}
//			}
//
//			if (closed) {
//				Vector3 A = CameraControler.MainCamera.WorldToScreenPoint (SplinePoints [0].Pos);
//				Vector3 B = CameraControler.MainCamera.WorldToScreenPoint (SplinePoints [SplinePoints.Count - 1].Pos);
//
//				Vector3 D = A + Vector3.Project (C - A, B - A);
//				Vector3 Va = D - A;
//				Vector3 Vb = D - B;
//
//				if ((Mathf.Sign (Va.x) != Mathf.Sign (Vb.x) || Va.x == 0 && Vb.x == 0) &&
//				   (Mathf.Sign (Va.y) != Mathf.Sign (Vb.y) || Va.y == 0 && Vb.y == 0) &&
//				   (Mathf.Sign (Va.z) != Mathf.Sign (Vb.z) || Va.z == 0 && Vb.z == 0) &&
//				   Vector3.Distance (D, C) < minDistance) {
//					minI = SplinePoints.Count - 1;
//					minD = D;
//					minDistance = Vector3.Distance (D, C);
//					flag = false;
//				}
//			}
//
//
//			if (flag) {
//				return;
//			}
//
//			Point point = GameObject.Instantiate (Services.Prefabs.Point).GetComponent<Point>();
//			point.transform.parent = transform;
//			Vector3 curentPos = CameraControler.MainCamera.ScreenToWorldPoint (minD);
//			point.transform.position = curentPos;
//			AddPoint(point);
//		}
	}

	#region

	public bool IsPointConnectedTo (Point p)
	{
		return SplinePoints.Contains (p);
	}

	public int GetLineSegment (int pointIndex, float progress)
	{
		return (pointIndex * curveFidelity) + (int)((float)curveFidelity * (float)progress);
	}

	public int GetPlayerLineSegment ()
	{
		return (SplinePoints.IndexOf (Selected) * curveFidelity) + (int)((float)curveFidelity * (float)Services.PlayerBehaviour.progress);
	}

	public Vector3 GetPointAtIndex (int i, float t)
	{

		//ADD SUPPORT FOR BACKWARDS/FORWARDS
		//IF FORWARDS, INCREMENT, IF BACKWARDS, DECREMENT ?

		//MAKE THIS SHIT WORK WHEN THERE'S ONLY TWO POINTS
		//Maybe you need to decrement the index by one to force it to be between both splines
		//Obviously you need to set the progress correctly when you know you're facing backwards (start at 1)

		int Count = SplinePoints.Count;

		int j = i - 1;

		if (j < 0) {
//			&& StartPoint._neighbours.Contains (SplinePoints [LoopIndex])
			if (closed) {
				j = Count - 1;
			} else {
				j = i;
			}
		}

		Point Point1 = SplinePoints [j];

		j = i + 1;
		if (j > Count - 1) {
			if (closed) {
				j = 0;
			} else {
				j = i;
			}
		}

		Point Point2 = SplinePoints [j];

		j++;

		if (j > Count - 1) {
//			&& SplinePoints [Count - 1]._neighbours.Contains (SplinePoints [LoopIndex])
			if (closed) {
				j = 0;
			} else {
				j = i;
			}
		}

		Point Point3 = SplinePoints [j];

		float tension = SplinePoints [i].tension;
		float continuity = SplinePoints [i].continuity;
		float bias = SplinePoints [i].bias;

		Vector3 r1 = 0.5f * (1 - tension) * ((1 + bias) * (1 - continuity) * (SplinePoints [i].Pos - Point1.Pos) + (1 - bias) * (1 + continuity) * (Point2.Pos - SplinePoints [i].Pos));

		tension = Point2.tension;
		continuity = Point2.continuity;
		bias = Point2.bias;

		Vector3 r2 = 0.5f * (1 - tension) * ((1 + bias) * (1 + continuity) * (Point2.Pos - SplinePoints [i].Pos) + (1 - bias) * (1 - continuity) * (Point3.Pos - Point2.Pos));
		Vector3 v = GetPoint (t, SplinePoints [i].Pos, Point2.Pos, r1, r2);

		return v;

	}

	public Vector3 GetPoint (float t)
	{

		int i = SplinePoints.IndexOf (Selected);
		return GetPointAtIndex (i, t);

	}


	Vector3 GetPoint (float t, Vector3 p1, Vector3 p2, Vector3 r1, Vector3 r2)
	{
		return p1 * (2.0f * t * t * t - 3.0f * t * t + 1.0f) + r1 * (t * t * t - 2.0f * t * t + t) +
		p2 * (-2.0f * t * t * t + 3.0f * t * t) + r2 * (t * t * t - t * t);
	}

	public Vector3 GetVelocity (float t)
	{
		int i = SplinePoints.IndexOf (Selected);
		return GetVelocityAtIndex (i, t);
	}


	public Vector3 GetVelocityAtIndex (int i, float t)
	{

		int Count = SplinePoints.Count;

		int j = i - 1;

		if (j < 0) {
			//			&& StartPoint._neighbours.Contains (SplinePoints [LoopIndex])
			if (closed) {
				j = Count - 1;
			} else {
				j = i;
			}
		}

		Point Point1 = SplinePoints [j];

		j = i + 1;
		if (j > Count - 1) {
			if (closed) {
				j = 0;
			} else {
				j = i;
			}
		}

		Point Point2 = SplinePoints [j];

		j++;

		if (j > Count - 1) {
			//			&& SplinePoints [Count - 1]._neighbours.Contains (SplinePoints [LoopIndex])
			if (closed) {
				j = 0;
			} else {
				j = i;
			}
		}

		Point Point3 = SplinePoints [j];

		float tension = SplinePoints [i].tension;
		float continuity = SplinePoints [i].continuity;
		float bias = SplinePoints [i].bias;

		Vector3 r1 = 0.5f * (1 - tension) * ((1 + bias) * (1 - continuity) * (SplinePoints [i].Pos - Point1.Pos) + (1 - bias) * (1 + continuity) * (Point2.Pos - SplinePoints [i].Pos));

		tension = Point2.tension;
		continuity = Point2.continuity;
		bias = Point2.bias;

		Vector3 r2 = 0.5f * (1 - tension) * ((1 + bias) * (1 + continuity) * (Point2.Pos - SplinePoints [i].Pos) + (1 - bias) * (1 - continuity) * (Point3.Pos - Point2.Pos));

		Vector3 v = GetFirstDerivative (SplinePoints [i].Pos, Point2.Pos, r1, r2, t);
		v = transform.TransformPoint (v) - transform.position;

		if (v == Vector3.zero && t == 1) {
			v = GetVelocityAtIndex (i, 0.99f);
		}
		return v;
	}


	Vector3 GetFirstDerivative (Vector3 p1, Vector3 p2, Vector3 r1, Vector3 r2, float t)
	{

//		return r1 * (1 - 4 * t + 3 * (t *t)) + t * (6 * (p1 - p2) * (-1 + t) + r2 * (-2 + 3 * t));
		return r1 * (1 - 4 * t + 3 * (t * t)) + t * (-6 * p1 + 6 * p2 + 6 * p1 * t - 6 * p2 * t + r2 * (-2 + 3 * t));
		//		p2 (3 (t*t) - 2 (t*t*t)) + m (t - 2 t^2 + t^3) + n (-t^2 + t^3) + p1 (1 - 3 t^2 + 2 t^3)
	}

	public Vector3 GetDirection (float t)
	{
		Vector2 noZ = GetVelocity (t);
		return new Vector2 (noZ.x, noZ.y).normalized;
	}

	public int GetPointIndex (Point point)
	{
		foreach (Point p in SplinePoints) {
			if (point == p) {
				return SplinePoints.IndexOf (p);
			}
		}

		return 0;
	}

	public Vector3 GetInitVelocity (Point p)
	{

		return GetVelocityAtIndex (GetPointIndex (p), 0.01f);

	}

	public Vector3 GetReversedInitVelocity (Point p)
	{

		//JUST WHAT SHOULD BE GOING ON HERE

		return -GetVelocityAtIndex (GetPointIndex (p), 0.99f);
	}

	public float CompareAngleAtPoint (Vector3 direction, Point p, bool reversed = false)
	{

		if (reversed) {
			return Vector2.Angle (direction, GetReversedInitVelocity (p));
		} else {
			return Vector2.Angle (direction, GetInitVelocity (p));
		}
	}


	public void DestroySpline (Point toDelete, Point toAnchor)
	{
		Destroy (this);
	}

	public void CalculateDistance ()
	{
		//IDK IF THIS WORKS FORWARD/BACKWARDS
		int Count = SplinePoints.Count;
		float step = (1.0f / (float)curveFidelity);
		distance = 0;

		for (int k = 1; k < curveFidelity; k++) {

			float t = (float)k / (float)(curveFidelity);
			distance += Vector3.Distance (GetPoint (t), GetPoint (t - step));
		}

	}

	public void SetPoints (List<Point> points)
	{
		SplinePoints.Clear ();

		foreach (Point p in points) {
			AddPoint (null, p);
		}
	}

	public void AddPoint (Point curPoint, Point p)
	{

		if(SplinePoints.Contains(p)){
			Debug.Log("ADDING EXISTING POINT BACK TO SPLINE. WRONG");
		}

		p.AddSpline (this);

		int newIndex = 0;
		if (SplinePoints.Count == 0) {

			SplinePoints.Add (p);

		} else if (SplinePoints.Count >= 1 && curPoint == StartPoint) {


			SplinePoints.Insert (0, p);
			p.AddPoint (SplinePoints [1]);
			SplinePoints [1].AddPoint (p);
			highHitPoint++;

		} else if(p == StartPoint || p == EndPoint && closed){
				StartPoint.AddPoint (EndPoint);
				EndPoint.AddPoint(StartPoint);
		}else{

			newIndex = SplinePoints.Count;
			SplinePoints.Insert (newIndex, p);

			p.AddPoint (SplinePoints [newIndex-1]);
			SplinePoints [newIndex-1].AddPoint (p);

//			p.AddPoint(curPoint);
//			curPoint.AddPoint (p);
		}
	}

	public void DrawVelocities (float t, float x)
	{
		// l2.positionCount = 3;
		// int step = (int)(t * curveFidelity);
		// t = (float)step / (float)curveFidelity;
		// l2.SetPosition (l2.positionCount - 1, GetPoint (t));
		// l2.SetPosition (l2.positionCount - 2, GetPoint (t) + (GetDirection (t) * x));
		// l2.SetPosition (l2.positionCount - 3, GetPoint (t));
	}

	public void DrawLineSegmentVelocity (float t, float x, float startVal)
	{

		// l2.positionCount = 3;

		float difference = Mathf.Abs (1 - Mathf.Abs (startVal - t));
		int steps = (int)(difference * (float)curveFidelity);
		float step = 1.0f / ((float)curveFidelity);

		if (x < 0) {
			t = 0;
		} else {
			t = 1;
		}


		for (int k = 0; k < steps; k++) {

//			t = (float)k / steps;
//			t = Mathf.Abs (startVal - t);

			// if (l2.positionCount <= k * 3) {
			// 	l2.positionCount = l2.positionCount + 3;
			// }

			float length;

			if (x < 0) {
				t += step;
//				length = Mathf.Sin (((1 - t) * Mathf.PI / 2) + Mathf.PI / 2);
				length = t;
			} else {
				t -= step;
//				length = Mathf.Sin ((t * Mathf.PI / 2) + Mathf.PI / 2);
				length = 1 - t;
			}

//			length = Mathf.Pow (length, 2);
			length *= 2;

			// l2.SetPosition (l2.positionCount - 1, GetPoint (t));
			// l2.SetPosition (l2.positionCount - 2, GetPoint (t) + GetDirection (t) * x * length);
			// l2.SetPosition (l2.positionCount - 3, GetPoint (t));

		}

	}

	// IEnumerator DrawVelocities ()
	// {

		// l2.positionCount = 3;
		//
		// float step = 0;
		// int Count = SplinePoints.Count;
		//
		//
		// l2.SetPosition (l2.positionCount - 1, GetPointAtIndex (0, step));
		// l2.SetPosition (l2.positionCount - 2, GetPointAtIndex (0, step) + GetVelocityAtIndex (0, step));
		// l2.SetPosition (l2.positionCount - 3, GetPointAtIndex (0, step));

//		float t = 1/curveFidelity;
//		int lastIndex = 0;
//		int index = 0;

//		while (t <= 1){
//
//			index = (int)(t * curveFidelity);
//			step = ((float)curveFidelity * t)/ (float)curveFidelity;
//
//			if(index != lastIndex){
//				if(l.positionCount < curveFidelity * 3){
//					l2.positionCount = l2.positionCount + 3;
//				}
//
//				l2.SetPosition (l2.positionCount - 1, GetPoint (step));
//				l2.SetPosition (l2.positionCount - 2, GetPoint (step) + GetVelocity (step));
//				l2.SetPosition (l2.positionCount - 3, GetPoint (step));
//				lastIndex = index;
//			}
//
//			t += Time.deltaTime * drawSpeed;
//			yield return null;
//		}


		// l2.SetPosition (0, GetPointAtIndex (0, 0));

		// for (int i = 0; i < Count - 1; i++) {
		// 	for (int k = 0; k < curveFidelity; k++) {
		//
		// 		int index = (i * curveFidelity) + k;
		// 		float t = (float)k / (float)(curveFidelity - 1);
		//
		// 		step = ((float)curveFidelity * t) / (float)curveFidelity;
		//
		// 		Vector3 v = GetPointAtIndex (i, t);

				// if (l2.positionCount <= Count * curveFidelity * 3) {
				// 	l2.positionCount = l2.positionCount + 3;
				// }
				//
				// l2.SetPosition (l2.positionCount - 1, GetPointAtIndex (i, step));
				// l2.SetPosition (l2.positionCount - 2, GetPointAtIndex (i, step) + GetVelocityAtIndex (i, step));
				// l2.SetPosition (l2.positionCount - 3, GetPointAtIndex (i, step));

				// yield return null;
	// 		}
	// 	}
	//
	// }

	public void DrawMeshOverTime (int p1, int p2, bool reversed = false){

		isDrawing = true;

		int start;
		if (reversed) {
			for (int i = p1; i > p2; i--) {
				for (int k = curveFidelity - 1; k >= 0; k--) {

					int index = (i * curveFidelity) + k;
					float t = (float)k / (float)(curveFidelity);

					DrawLine (i, index, t);

					line.Draw3D();
				}
			}
		} else {
			for (int i = p1; i < p2; i++) {
				for (int k = 0; k < curveFidelity; k++) {

					int index = (i * curveFidelity) + k;
					float t = (float)k / (float)(curveFidelity);

					DrawLine (i, index, t);

					line.Draw3D();
				}
			}
		}
		isDrawing = false;
	}

	void DrawMesh (bool reversed = false){

		if (reversed) {
			for (int i = highHitPoint + (closed ? 0 : 1); i >= lowHitPoint; i--) {
				for (int k = curveFidelity - 1; k >= 0; k--) {

					int index = (i * curveFidelity) + k;
					float t = (float)k / (float)(curveFidelity);

					DrawLine (i, index, t);

				}
			}
		} else {
			for (int i = lowHitPoint; i < highHitPoint + (closed ? 0: 1); i++) {
				for (int k = 0; k < curveFidelity; k++) {

					int index = (i * curveFidelity) + k;
					float t = (float)k / (float)(curveFidelity);

					DrawLine (i, index, t);
				}
			}
		}

	}

	public void DrawSegment(int pointIndex){
			for (int i = Mathf.Clamp(pointIndex - 2, 0, SplinePoints.Count); i < Mathf.Clamp(pointIndex + 2, 0, SplinePoints.Count - (closed ? 0 : 1)); i++) {
				for (int k = 0; k < curveFidelity; k++) {

					int index = (i * curveFidelity) + k;
					float t = (float)k / (float)(curveFidelity);

					DrawLine (i, index, t);
				}
			}
			line.Draw3D();
		}

	void DrawLine(int i, int index, float t){

		int indexOfPlayerPos = GetPlayerLineSegment ();

		Vector3 v = GetPointAtIndex (i, t);

		float stepSize = (1.0f / (float)curveFidelity);

		if (isPlayerOn) {

			int adjustedIndex;

			//ADJUST INDEX FOR LOOPING AROUND SPLINE WITH DISTANCE-FROM-PLAYER CALCULATIONS
			if (closed) {
				int dist1 = Mathf.Abs(index - indexOfPlayerPos);
				int dist2;

				if (index < indexOfPlayerPos) {
					dist2 = Mathf.Abs ((line.GetSegmentNumber () - indexOfPlayerPos) + index);
				} else {
					dist2 = Mathf.Abs ((line.GetSegmentNumber () - index) + indexOfPlayerPos);
				}

				adjustedIndex = Mathf.Min (dist1, dist2);
//				if (index < line.GetSegmentNumber ()/2) {
//					adjustedIndex = Mathf.Abs(index - indexOfPlayerPos);
//				}else{
//					adjustedIndex = (line.GetSegmentNumber () - index) + indexOfPlayerPos;
//				}
			} else {
				adjustedIndex = Mathf.Abs(indexOfPlayerPos - index);
			}



			distanceFromPlayer = (float)adjustedIndex / (float)curveFidelity;
			invertedDistance = 1f - Mathf.Clamp01 (Mathf.Abs (distanceFromPlayer)/2);

			float flow = Mathf.Abs(Services.PlayerBehaviour.flow);

			float phase = index;
			float newFrequency = flow * 5 + 5;
//			newFrequency *= -Mathf.Sign (Services.PlayerBehaviour.accuracy);

			float distortion = Mathf.Lerp (0, Mathf.Pow (1 - Mathf.Abs (Services.PlayerBehaviour.accuracy), 3), flow/3)/5f;

			float amplitude = Mathf.Clamp01(Services.PlayerBehaviour.connectTime) / 20 + 0.001f;

			float curr = (Time.time * frequency + phase) % (2.0f * Mathf.PI);
			float next = (Time.time * newFrequency) % (2.0f * Mathf.PI);
			phase = curr - next;
			frequency = newFrequency;

			float offset = Mathf.Sin (Time.time * frequency + phase);

			offset *= amplitude;


			Vector3 direction = GetVelocityAtIndex (i, t);

			direction = new Vector3 (-direction.y, direction.x, direction.z);

			v += (direction * offset * Mathf.Clamp01(distanceFromPlayer)/2) + (direction * UnityEngine.Random.Range (-distortion, distortion) * invertedDistance);

		}

		if (index >= line.points3.Count) {
			line.points3.Add (v);
		} else {
			line.points3 [index] = v;
		}

			//CHECK ITS IN RANGE
		if (index < line.GetSegmentNumber ()) {

			//CHECK ITS NOT THE LAST POINT

//					SplinePoints [i + 1].color = Color.Lerp (SplinePoints [i + 1].color, Color.white, Mathf.Pow (invertedDistance, 2));
//					line.SetWidth (Mathf.Lerp (1, 1, Mathf.Pow (invertedDistance, 10)), index);
			if(((indexOfPlayerPos > (i) * curveFidelity) && (indexOfPlayerPos < (i + 1) * curveFidelity)) && isPlayerOn){
				if (draw) {
					if ((reversed && Services.PlayerBehaviour.progress < playerProgress) || (!reversed && Services.PlayerBehaviour.progress > playerProgress)) {
						playerProgress = t;
					}

					float difference = (Services.PlayerBehaviour.progress - t) * (float)curveFidelity;

					if (reversed) {
						line.SetColor (Color.Lerp (Color.black, Color.white, -difference), index);
					} else {
						line.SetColor (Color.Lerp (Color.black, Color.white, difference), index);
					}
				}else{
						line.SetColor (Color.white, index);
					}
				}else{
						float lerpVal;
						Color c;
						if(i < SplinePoints.Count - 1){
								c = Color.Lerp (SplinePoints [i].color, SplinePoints [i + 1].color, t);
								lerpVal = SplinePoints[i].proximity + t * (SplinePoints[i+1].proximity - SplinePoints[i].proximity);
						}else{
							if(closed){
								c = Color.Lerp (SplinePoints [i].color, StartPoint.color, t);
								lerpVal = SplinePoints[i].proximity + t * (StartPoint.proximity - SplinePoints[i].proximity);
							}else{
								c = SplinePoints [i].color;
								lerpVal = SplinePoints[i].proximity;
							}
						}

						float difference = (lerpVal);
						line.SetColor (Color.Lerp (c, Color.white, difference), index);
					}
						//do coloring a certain way
				// } else {
				// 	if (i < SplinePoints.Count - 1) {
				// 		//
				// 		line.SetColor (Color.Lerp(new Color(0.1f, 0.1f, 0.1f), Color.white, SplinePoints [i].proximity), index);
				// 	//				line.SetWidth (Mathf.Lerp ((SplinePoints [i].NeighbourCount () - 1) + 1, (SplinePoints [i + 1].NeighbourCount () - 1) + 1, t), index);
				//
				// 	} else if (closed) {
				//
				// 	//IF IT IS THE LAST POINT, ONLY DRAW THE CONNECTION IF ITS A LOOP
				//
				// 		// line.SetColor (Color.Lerp (SplinePoints [i].color, SplinePoints [SplinePoints.Count - 1].color, t), index);
				// 		line.SetColor (Color.Lerp (new Color(0.1f, 0.1f, 0.1f), Color.white, SplinePoints [i-1].proximity), index);
				// 	//				line.SetWidth (Mathf.Lerp ((SplinePoints [i].NeighbourCount () - 1) + 1, (SplinePoints [SplinePoints.Count - 1].NeighbourCount () - 1) + 1, t), index);
				// }
				// //			line.SetWidth (1f, index);

			// }
		}
	}

	//IO FUNCTIONS
	public void Save ()
	{
		if (!Directory.Exists (Spline.SavePath))
			Directory.CreateDirectory (Spline.SavePath);
		string FileName = SavePath + name + ".xml";
		XmlWriterSettings settings = new XmlWriterSettings ();
		settings.Indent = true;
		settings.IndentChars = "    ";
		settings.NewLineChars = "\n";



		XmlWriter output = XmlWriter.Create (FileName, settings);
		output.WriteStartElement ("Spline");


		output.WriteStartElement ("SplinePoints");
		for (int i = 0; i < SplinePoints.Count; i++) {
			output.WriteStartElement ("Point");

			output.WriteAttributeString ("PositionX", SplinePoints [i].Pos.x.ToString ());
			output.WriteAttributeString ("PositionY", SplinePoints [i].Pos.y.ToString ());
			output.WriteAttributeString ("PositionZ", SplinePoints [i].Pos.z.ToString ());
			output.WriteAttributeString ("Tension", SplinePoints [i].tension.ToString ());
			output.WriteAttributeString ("Bias", SplinePoints [i].bias.ToString ());
			output.WriteAttributeString ("Continuity", SplinePoints [i].continuity.ToString ());
			output.WriteEndElement ();
		}
		output.WriteEndElement ();
		output.Flush ();

		output.WriteStartElement ("Closed");
		output.WriteAttributeString ("bool", closed.ToString ());
		output.WriteEndElement ();
		output.Flush ();

		output.WriteStartElement ("MaxVerticesCurve");
		output.WriteAttributeString ("int", curveFidelity.ToString ());
		output.WriteEndElement ();
		output.Flush ();


		output.WriteEndElement ();
		output.Flush ();

		output.Close ();
	}

	public static void Load (string name, GameObject SplinePrefab)
	{
		GameObject goSpline = GameObject.Instantiate (SplinePrefab)as GameObject;
		goSpline.name = name.Replace (Spline.SavePath, "");
		goSpline.name = goSpline.name.Remove (goSpline.name.Length - 4, 4);

		Spline spline = goSpline.GetComponent<Spline> ();

		XmlDocument input = new XmlDocument ();
		input.Load (name);

		bool.TryParse (input.DocumentElement.ChildNodes [2].Attributes [0].Value, out spline.closed);
		int.TryParse (input.DocumentElement.ChildNodes [3].Attributes [0].Value, out spline.curveFidelity);

		XmlNodeList points = input.DocumentElement.ChildNodes [0].ChildNodes;
		foreach (XmlNode point in points) {
			Vector3 pos = new Vector3 ();
			float.TryParse (point.Attributes [0].Value, out pos.x);
			float.TryParse (point.Attributes [1].Value, out pos.y);
			float.TryParse (point.Attributes [2].Value, out pos.z);


			float tension;
			float bias;
			float continuity;
			float.TryParse (point.Attributes [3].Value, out tension);
			float.TryParse (point.Attributes [4].Value, out bias);
			float.TryParse (point.Attributes [5].Value, out continuity);
//			spline.AddPoint(pos,tension,bias,continuity);
		}
	}

	void Insert ()
	{
		//		if(Input.GetMouseButtonDown(1))
		//		{
		//			Vector3 C=new Vector3(Input.mousePosition.x,Input.mousePosition.y,Input.mousePosition.z);
		//			float minDistance=float.MaxValue;
		//			int minI=0;
		//			Vector3 minD=Vector3.zero;
		//			bool flag=true;
		//			minDistance=float.MaxValue;
		//			for(int i=0;i<SplinePoints.Count-1;i++)
		//			{
		//
		//				Vector3 A=CameraControler.MainCamera.WorldToScreenPoint(SplinePoints[i].Pos);
		//				Vector3 B=CameraControler.MainCamera.WorldToScreenPoint(SplinePoints[i+1].Pos);
		//
		//				Vector3 D=A+Vector3.Project(C-A,B-A);
		//				Vector3 Va=D-A;
		//				Vector3 Vb=D-B;
		//
		//				if((Mathf.Sign(Va.x)!=Mathf.Sign(Vb.x)||Va.x==0&&Vb.x==0)&&
		//					(Mathf.Sign(Va.y)!=Mathf.Sign(Vb.y)||Va.y==0&&Vb.y==0)&&
		//					(Mathf.Sign(Va.z)!=Mathf.Sign(Vb.z)||Va.z==0&&Vb.z==0)&&
		//					Vector3.Distance(D,C)<minDistance)
		//				{
		//					minI=i;
		//					minD=D;
		//					minDistance=Vector3.Distance(D,C);
		//					flag=false;
		//				}
		//			}
		//
		//			if(closed)
		//			{
		//				Vector3 A=CameraControler.MainCamera.WorldToScreenPoint(SplinePoints[0].Pos);
		//				Vector3 B=CameraControler.MainCamera.WorldToScreenPoint(SplinePoints[SplinePoints.Count-1].Pos);
		//
		//				Vector3 D=A+Vector3.Project(C-A,B-A);
		//				Vector3 Va=D-A;
		//				Vector3 Vb=D-B;
		//
		//				if((Mathf.Sign(Va.x)!=Mathf.Sign(Vb.x)||Va.x==0&&Vb.x==0)&&
		//					(Mathf.Sign(Va.y)!=Mathf.Sign(Vb.y)||Va.y==0&&Vb.y==0)&&
		//					(Mathf.Sign(Va.z)!=Mathf.Sign(Vb.z)||Va.z==0&&Vb.z==0)&&
		//					Vector3.Distance(D,C)<minDistance)
		//				{
		//					minI=SplinePoints.Count-1;
		//					minD=D;
		//					minDistance=Vector3.Distance(D,C);
		//					flag=false;
		//				}
		//			}
		//
		//			if(flag)
		//			{
		//				return;
		//			}
		//			Point point=GameObject.Instantiate(PointPrefab) as Point;
		//			point.transform.parent=transform;
		//			Vector3 curentPos=CameraControler.MainCamera.ScreenToWorldPoint(minD);
		//			point.transform.position=curentPos;
		//			SplinePoints.Insert(minI+1,point);
		//			}
		//
	}

	#endregion
}
