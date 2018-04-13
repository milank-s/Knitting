using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System;
using Vectrosity;

public class Spline : MonoBehaviour
{

	public static List<Spline> Splines = new List<Spline> ();
	[HideInInspector]
	public List<Point> SplinePoints;

	public Point Selected;

	public VectorLine line;

	[HideInInspector]
	public LineRenderer l;
	public LineRenderer l2;

	public static Spline Select;

	private float[] widthVals;
	private Keyframe[] widthKeys;
	public int curveFidelity = 10;
	public float drawSpeed = 6;
	public float distance = 0;
	public float segmentDistance = 0;
	public bool closed = false;
	public int LoopIndex;
	public bool locked = false;
	public bool isDrawing;

	private float colorDecay;

	private int lowHitPoint, highHitPoint;

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

		isPlayerOn = false;
		CleanUpLines ();
		if (curSound != null) {
			StopCoroutine (curSound);
			StartCoroutine (FadeNote (sound));
		}

	}

	public void OnSplineEnter (bool enter, Point p1, Point p2, bool forceDraw = false)
	{
		isPlayerOn = true;

		bool draw = false;

		int i = SplinePoints.IndexOf (p1);
		int j = SplinePoints.IndexOf (p2);

		if (forceDraw) {
			draw = true;
		} else {

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
		}

		if (draw) {

			int indexdiff = j - i;

			if (indexdiff == -1 || indexdiff > 1) {
				StartCoroutine (DrawMeshOverTime (i, j, true));

			} else {
				StartCoroutine (DrawMeshOverTime (i, j));
			}
		}
			
		CalculateDistance ();

//		if (enter) {
		if (curSound != null) {
			StopCoroutine (curSound);
			StartCoroutine (FadeNote (sound));
		}

		PlayAttack (p1, p2);
//		}
	}

	public void PlayAttack (Point point1, Point point2)
	{

		segmentDistance = Vector3.Distance (point1.Pos, point2.Pos);
		curSound = StartCoroutine (PlaySustain ());

	}

	public void ManageSound (bool fade, float lerpVal)
	{
		if (fade) {
			sound.volume = Mathf.Lerp (Mathf.Lerp (0.1f, 1, Services.PlayerBehaviour.flow / (Services.PlayerBehaviour.maxSpeed/2)), 0, lerpVal);
		} else {
			sound.volume = Mathf.Lerp (0, Mathf.Lerp (0.1f, 1, Services.PlayerBehaviour.flow / (Services.PlayerBehaviour.maxSpeed)/2), lerpVal);
		}
	}

	public IEnumerator PlaySustain ()
	{

//		AudioClip soundEffect = Services.Sounds.Loops [(int)((1 - Mathf.Clamp01 ((segmentDistance) / 10 * 2.5f)) * (Services.Sounds.Loops.Count - 1))];
		AudioClip soundEffect = Services.Sounds.Loops[UnityEngine.Random.Range(0, Services.Sounds.Loops.Count)];
			
		sound = Services.Prefabs.CreateSoundEffect (soundEffect, Selected.Pos);
		sound.clip = soundEffect;
		sound.Play ();

		float t = 0;

		while (t < 1) {

			ManageSound (false, t);

			t += Time.deltaTime * 3;
			yield return null;
		}

		while (true) {
			
//			float progressToSin = Mathf.Sin (Services.PlayerBehaviour.progress * Mathf.PI);
			ManageSound (false, 1);
		
			yield return null;
		}

	}

	public IEnumerator FadeNote(AudioSource s){

		Debug.Log ("starting Fade");
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

	void Awake ()
	{

//		widthKeys = new Keyframe[8];
//		widthKeys [0] = new Keyframe (0f, 1f);
//		widthVals = new float[8];
//
//		for (int i = 0; i < 8; i++) {
//			widthVals[i] = UnityEngine.Random.Range(0f, 1f);
//
//		}
//
//		widthKeys [7] = new Keyframe (1f, 1f);
		lowHitPoint = int.MaxValue;
		highHitPoint = -int.MaxValue;

		l = GetComponent<LineRenderer> ();
		l.positionCount = 0;

		if (SplinePoints.Count > 0) {
			foreach (Point p in SplinePoints) {
				AddPoint (null, p);
			}
		} else {
			SplinePoints = new List<Point> ();
		}
			
		SplinePoints = new List<Point> ();

		Select = this;
		Splines.Add (this);

		line = new VectorLine (name, line.points3, 1, LineType.Continuous, Vectrosity.Joins.Weld);
		line.smoothWidth = true;
		line.smoothColor = true;
		line.points3 = new List<Vector3> ();

	}

	void Update ()
	{

		if (SplinePoints.Count > 0) {


			line.Draw3DAuto ();

			if (!isDrawing) {
				DrawMesh ();

				if (isPlayerOn) {
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

	IEnumerator FadeLine (VectorLine l, float time)
	{
		float t = time;
		while (t > 0) {
			l.color = Color.Lerp (Color.black, l.color, t / time);
			t -= Time.deltaTime;
			yield return null;
		}
	}

	void CleanUpLines ()
	{
//		l2.positionCount = 0;
	}


	#region

	//HELPER FUNCTIONS

	public Point StartPoint ()
	{
		return SplinePoints [0];
	}


	public Point EndPoint ()
	{
		return SplinePoints [SplinePoints.Count - 1];

	}

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
//			&& StartPoint ()._neighbours.Contains (SplinePoints [LoopIndex])
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
			//			&& StartPoint ()._neighbours.Contains (SplinePoints [LoopIndex])
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
		return GetVelocity (t).normalized;
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

		return GetVelocityAtIndex (GetPointIndex (p), 0.1f);
	
	}

	public Vector3 GetReversedInitVelocity (Point p)
	{

		//JUST WHAT SHOULD BE GOING ON HERE

		return -GetVelocityAtIndex (GetPointIndex (p), 0.9f);
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
		Destroy (l);
	}

	public void CalculateDistance ()
	{
		int Count = SplinePoints.Count;
		float step = (1.0f / (float)curveFidelity);
		distance = 0;

		for (int k = 1; k < curveFidelity; k++) {

			float t = (float)k / (float)(curveFidelity - 1);
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
		if (SplinePoints.Contains (p)) {
			return;
		}

		p.AddSpline (this);
			
		int newIndex = 0;
	

		if (SplinePoints.Count == 0) {
			
			SplinePoints.Add (p);

		} else if (SplinePoints.Count >= 1 && curPoint == StartPoint ()) {
			SplinePoints.Insert (0, p);
			p.AddPoint (SplinePoints [1]);
			SplinePoints [1].AddPoint (p);
			highHitPoint++;

			if (closed) {
				p.AddPoint (SplinePoints [SplinePoints.Count - 1]);
				SplinePoints [SplinePoints.Count - 1].AddPoint (p);
			}

		} else {
			newIndex = SplinePoints.Count;
			SplinePoints.Insert (newIndex, p);

			p.AddPoint (SplinePoints [newIndex - 1]);
			SplinePoints [newIndex - 1].AddPoint (p);
		}

		if (GetComponent<WordBank> () != null) {
			p.text = GetComponent<WordBank> ().GetWord ();
		}
	}

	public void DrawVelocities (float t, float x)
	{
		l2.positionCount = 3;
		int step = (int)(t * curveFidelity);
		t = (float)step / (float)curveFidelity;
		l2.SetPosition (l2.positionCount - 1, GetPoint (t));
		l2.SetPosition (l2.positionCount - 2, GetPoint (t) + (GetDirection (t) * x));
		l2.SetPosition (l2.positionCount - 3, GetPoint (t));
	}

	public void DrawLineSegmentVelocity (float t, float x, float startVal)
	{

		l2.positionCount = 3;

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

			if (l2.positionCount <= k * 3) {
				l2.positionCount = l2.positionCount + 3;
			}

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

			l2.SetPosition (l2.positionCount - 1, GetPoint (t));
			l2.SetPosition (l2.positionCount - 2, GetPoint (t) + GetDirection (t) * x * length);
			l2.SetPosition (l2.positionCount - 3, GetPoint (t));

		}

	}

	IEnumerator DrawVelocities ()
	{
		
		l2.positionCount = 3;

		float step = 0;
		int Count = SplinePoints.Count;


		l2.SetPosition (l2.positionCount - 1, GetPointAtIndex (0, step));
		l2.SetPosition (l2.positionCount - 2, GetPointAtIndex (0, step) + GetVelocityAtIndex (0, step));
		l2.SetPosition (l2.positionCount - 3, GetPointAtIndex (0, step));

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
			

		l2.SetPosition (0, GetPointAtIndex (0, 0));

		for (int i = 0; i < Count - 1; i++) {
			for (int k = 0; k < curveFidelity; k++) {

				int index = (i * curveFidelity) + k;
				float t = (float)k / (float)(curveFidelity - 1);

				step = ((float)curveFidelity * t) / (float)curveFidelity;

				Vector3 v = GetPointAtIndex (i, t);

				if (l2.positionCount <= Count * curveFidelity * 3) {
					l2.positionCount = l2.positionCount + 3;
				}

				l2.SetPosition (l2.positionCount - 1, GetPointAtIndex (i, step));
				l2.SetPosition (l2.positionCount - 2, GetPointAtIndex (i, step) + GetVelocityAtIndex (i, step));
				l2.SetPosition (l2.positionCount - 3, GetPointAtIndex (i, step));

				yield return null;
			}
		}

	}

	IEnumerator DrawMeshOverTime (int p1, int p2, bool reversed = false)
	{

		isDrawing = true;

		int start;
		if (reversed) {
			for (int i = p2; i > p2 - 1; i--) {
				for (int k = curveFidelity; k > 0; k--) {

					int index = (i * curveFidelity) + k;
					float t = (float)k / (float)(curveFidelity - 1);

					Vector3 v = GetPointAtIndex (i, t);

					//				if(l.positionCount < (i * curveFidelity) + k){
					//					l.positionCount = ((i * curveFidelity) + k);
					//				}

					line.points3.Insert (0, v);
					//				l.SetPosition (Mathf.Clamp(index-1, 0, int.MaxValue), v);



					yield return null;
				}
			}
		} else {
			for (int i = p1; i < p2; i++) {
				for (int k = 0; k < curveFidelity; k++) {

					int index = (i * curveFidelity) + k;
					float t = (float)k / (float)(curveFidelity - 1);

					Vector3 v = GetPointAtIndex (i, t);

					//				if(l.positionCount < (i * curveFidelity) + k){
					//					l.positionCount = ((i * curveFidelity) + k);
					//				}

					line.points3.Add (v);
					//				l.SetPosition (Mathf.Clamp(index-1, 0, int.MaxValue), v);



					yield return null;
				}
			}
		}
		line.lineType = Vectrosity.LineType.Continuous;

		isDrawing = false;

	}


	void DrawMesh ()
	{

//		if (l.positionCount <= 1) {
//			l.positionCount = 1;
//		}

		int Count = SplinePoints.Count;
		int indexOfSelected = SplinePoints.IndexOf (Selected);
		int indexOfPlayerPos = GetPlayerLineSegment ();

		Vector3 lastPosition = GetPointAtIndex (0, 0);

		//		l.SetPosition(0, lastPosition);

		for (int i = lowHitPoint; i < highHitPoint - (closed ? -1 : 0); i++) {
			for (int k = 0; k < curveFidelity; k++) {

				int index = (i * curveFidelity) + k;
				float t = (float)k / (float)(curveFidelity - 1);

				Vector3 v = GetPointAtIndex (i, t);

				LineColors (i, index, k);

				if (isPlayerOn) {

					float distanceFromPlayer = (float)(indexOfPlayerPos - index) / (float)curveFidelity;
					float invertedDistance = 1f - Mathf.Clamp01 (Mathf.Abs (distanceFromPlayer)/5);
					float flow = Services.PlayerBehaviour.flow;

					float phase = index;
					float newFrequency = flow + 50;
					newFrequency *= -Mathf.Sign (Services.PlayerBehaviour.accuracy);

					float distortion = Mathf.Lerp (0, Mathf.Pow (1 - Mathf.Abs (Services.PlayerBehaviour.accuracy), 3), flow / 10);

					float amplitude = Mathf.Clamp01 (Services.PlayerBehaviour.flow / Services.PlayerBehaviour.maxSpeed) / 5 + 0.01f;
						
					float curr = (Time.time * frequency + phase) % (2.0f * Mathf.PI);
					float next = (Time.time * newFrequency) % (2.0f * Mathf.PI);
					phase = curr - next;
					frequency = newFrequency;

					float offset = Mathf.Sin (Time.time * frequency + phase);

					offset += UnityEngine.Random.Range (-distortion, distortion);
					offset *= amplitude;

					

					Vector3 direction = GetVelocityAtIndex (i, t);

					direction = new Vector3 (-direction.y, direction.x, direction.z);

					v += (direction * offset * invertedDistance);
				}

				if (index >= line.points3.Count) {
					line.points3.Add (v);
				} else {
					line.points3 [index] = v;
				}
					
				if (isPlayerOn) {

					float distanceFromPlayer = 1f - Mathf.Clamp01 (Mathf.Abs (((float)(indexOfPlayerPos - index) / (float)curveFidelity)));


					if (i == indexOfSelected) {
						line.SetWidth (Mathf.Lerp (1, 10, Mathf.Pow (distanceFromPlayer, 10)), index);
						line.SetColor (Color.Lerp (Selected.color, Color.white, Mathf.Pow (distanceFromPlayer, 3)), index);
					}
				}
			
					

//				l.SetPosition (Mathf.Clamp(index-1, 0, int.MaxValue), v);

				lastPosition = v;
			}
		}
		//		StartCoroutine (DrawMeshOverTime ());
	
	}

		
	public void LineColors (int pointIndex, int index, int segmentIndex)
	{
		if (pointIndex < SplinePoints.Count - 1) {

			line.SetColor (Color.Lerp (SplinePoints [pointIndex].color, SplinePoints [pointIndex + 1].color, (float)segmentIndex / (float)curveFidelity), index);
			line.SetWidth (Mathf.Lerp ((SplinePoints [pointIndex].NeighbourCount () - 1) + 1, (SplinePoints [pointIndex + 1].NeighbourCount () - 1) + 1, (float)segmentIndex / (float)curveFidelity), index);
		} else if (closed) {
			line.SetColor (Color.Lerp (SplinePoints [pointIndex].color, SplinePoints [SplinePoints.Count - 1].color, (float)segmentIndex / (float)curveFidelity), index);
			line.SetWidth (Mathf.Lerp ((SplinePoints [pointIndex].NeighbourCount () - 1) + 1, (SplinePoints [SplinePoints.Count - 1].NeighbourCount () - 1) + 1, (float)segmentIndex / (float)curveFidelity), index);
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
