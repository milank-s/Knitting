using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System;
using Vectrosity;
using UnityEngine.Audio;
using System.Linq;
using SubjectNerd.Utilities;

// [ExecuteInEditMode]
public class Spline : MonoBehaviour
{
	public static List<Spline> Splines = new List<Spline> ();
	public static float drawSpeed = 0.01f;
	[Reorderable]
	public List<Point> SplinePoints;

	[HideInInspector]
	public Point Selected;

	[HideInInspector]
	public VectorLine line;

	public static Spline Select;
	[Space(15)]
	public bool closed = false;
	public bool locked = false;
	[Space(15)]
	public float unlockSpeed;

	[HideInInspector]
	public List<Spline> splinesToUnlock;

	[Space(20)]
	public int curveFidelity = 10;
	[Space(20)]

	[HideInInspector]
	public bool isPlayerOn = false;
	[HideInInspector]
	public bool draw = true;
	[HideInInspector]
	public float distance = 0;
	[HideInInspector]
	public float segmentDistance = 0;
	[HideInInspector]
	public Vector2 linearDirection;

	private bool reversed;
	private float colorDecay;
	private float distanceFromPlayer;
	private float invertedDistance;
	private int drawIndex;
	private int lowHitPoint = int.MaxValue;
	private int highHitPoint = -int.MaxValue;

	private float playerProgress{
		get{return Services.PlayerBehaviour.progress;}
	}

	private float stepSize;

	public bool isSelect {
		get {
			return this == Spline.Select;
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
	private float phase;
	private float volume;
	private AudioSource sound;
	private Coroutine curSound;

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
		drawIndex = SplinePoints.IndexOf(p1) * curveFidelity;
		int i = SplinePoints.IndexOf (p1);
		int j = SplinePoints.IndexOf (p2);

		if (forceDraw) {
			draw = true;
		}

		//find the range of indices the player has been on
		//most likely super bugged right now

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

		//draw the line segments the player has been on
		if (draw) {
			int indexdiff = j - i;

			if (indexdiff == -1 || indexdiff > 1) {
				reversed = true;

			} else {
				reversed = false;
			}
		}

		CalculateDistance ();
		isPlayerOn = true;

		/*								Old sound stuff
		if (enter) {
			if (curSound != null && sound != null) {
				StopCoroutine (curSound);
				StartCoroutine (FadeNote (sound));
		}
		PlayAttack (p1, p2);
		}
		*/
	}

	public void SetPointPosition(int index, Vector3 pos){
		SplinePoints[index].transform.position = pos;
	}

	public void SetupSpline(){
			for(int i = 0; i < SplinePoints.Count; i++) {
				SplinePoints[i].AddSpline(this);
				if(i != 0){
					SplinePoints[i].AddPoint(SplinePoints[i-1]);
					SplinePoints[i-1].AddPoint(SplinePoints[i]);
				}
			}

			if(closed){
				AddPoint (null, EndPoint);
			}
	}

	void Awake ()
	{
		stepSize = (1.0f / (float)curveFidelity);
		Select = this;
		Splines.Add (this);
		Material newMat;
//		newMat = Services.Prefabs.lines[3];
//		Texture tex = newMat.mainTexture;
//		float length = newMat.mainTextureScale.x;
//		float height = newMat.mainTextureScale.y;

		line = new VectorLine (name, line.points3, 2, LineType.Continuous, Vectrosity.Joins.Weld);
		if (MapEditor.editing)
		{
			line.color = Color.white;
		}
		else
		{
			line.color = Color.black;
		}

		line.smoothWidth = true;
		line.smoothColor = true;
		line.points3 = new List<Vector3> (SplinePoints.Count * curveFidelity);

//		line.texture = tex;
//		line.textureScale = newMat.mainTextureScale.x;


	}

	public void UpdateSpline(){
		for(int i = 0; i < splinesToUnlock.Count; i++){
			if(Services.PlayerBehaviour.flow > splinesToUnlock[i].unlockSpeed){
				// splinesToUnlock[i].locked = false;
				// splinesToUnlock.Remove(splinesToUnlock[i]);
			}
		}
	}

	void Start(){

		SetupSpline();

		for (int i = 0; i < SplinePoints.Count; i++) {
		 for (int k = 0; k <= curveFidelity; k++) {
				 int index = (i * curveFidelity) + k;
				 float step = (float)k / (float)(curveFidelity);
					SetLinePoint(GetPointAtIndex (i, step), index);
				}
		 }

		splinesToUnlock = new List<Spline>();

		foreach(Point p in SplinePoints){
			if(p._connectedSplines.Count > 1){
				foreach(Spline s in p._connectedSplines){
					if(s.locked && s != this && !splinesToUnlock.Contains(s)){
						splinesToUnlock.Add(s);
					}
				}
			}
		}
	}

	void SetLinePoint(Vector3 v, int index){
		if (index >= line.points3.Count) {
			line.points3.Add (v);
		} else {
			line.points3 [index] = v;
		}
	}

	void DrawLineSegment(){
		if(drawTimer < 0){
			drawIndex++;
			drawTimer = drawSpeed;
		}else{
			drawTimer -= Time.deltaTime;
		}
	}

	public void DrawSplineOverride()
	{
		for (int i = 0; i < SplinePoints.Count; i++)
		{
			for (int k = 0; k <= curveFidelity; k++)
			{

				int index = (i * curveFidelity) + k;
				float step = (float) k / (float) (curveFidelity);

				
					DrawLine(i, index, step);
					Vector3 v = Vector3.zero;
					v = GetPointAtIndex(i, step);
					
			}
		}

		line.Draw3D();
	}

	public void DrawSpline(bool drawn, int pointIndex, int endIndex){

				for (int i = pointIndex; i < endIndex ; i++) {
				 for (int k = 0; k <= curveFidelity; k++) {

					 int index = (i * curveFidelity) + k;
					 float step = (float)k / (float)(curveFidelity);

					 if(drawn){
							DrawLine(i, index, step);
							Vector3 v = Vector3.zero;
							if (step < playerProgress){
							 v = GetPointAtIndex (i, step);
						 }else{
							 v = GetPointAtIndex(i, playerProgress);
						 }

							if(index <= (i * curveFidelity) + (playerProgress * curveFidelity)){
								line.SetColor (SplinePoints[pointIndex].color, index);
							}else{
								line.SetColor (SplinePoints[pointIndex].color + Color.white/5, index);
							}
					}else{
						if(k <= (playerProgress * curveFidelity) + 2){
							Vector3 v = Vector3.zero;
							if (step < playerProgress){
							 v = GetPointAtIndex (i, step);
						 }else{
							 v = GetPointAtIndex(i, playerProgress);
						 }
						 SetLinePoint(v, index);

							if(index <= (i * curveFidelity) + (playerProgress * curveFidelity)){
								// line.SetColor (SplinePoints[pointIndex].color, index);
								 line.SetColor (Color.white, index);
							}

						}else{
							break;
						}
					}
				 }
			 }
			 // line.textureOffset -= (Services.PlayerBehaviour.curSpeed / line.textureScale) * Time.deltaTime * 10;
			 line.Draw3D();
	 }

	void DrawLine(int pointIndex, int segmentIndex, float step){

		
		Vector3 v = GetPointAtIndex (pointIndex, step);

		//Add movement Effects of player is on the spline
		if (isPlayerOn) {
			int playerIndex = GetPlayerLineSegment ();
			int indexDiff;

			//Find the shortest distance to the player in case of loop
			if (closed) {
				int dist1 = Mathf.Abs(segmentIndex - playerIndex);
				int dist2;

				if (segmentIndex < playerIndex) {
					dist2 = Mathf.Abs ((line.GetSegmentNumber () - playerIndex) + segmentIndex);
				} else {
					dist2 = Mathf.Abs ((line.GetSegmentNumber () - segmentIndex) + playerIndex);
				}

				indexDiff = Mathf.Min (dist1, dist2);

			} else {
				indexDiff = Mathf.Abs(playerIndex - segmentIndex);
			}

			//find the distance. 1 = one curve
			distanceFromPlayer = (float)indexDiff / (float)curveFidelity;

			//closeness to the player. 0 = one curve away
			invertedDistance = 1f - Mathf.Clamp01 (Mathf.Abs (distanceFromPlayer));

			float flow = Mathf.Abs(Services.PlayerBehaviour.flow);
			float newFrequency = Mathf.Abs(Services.PlayerBehaviour.accuracy * 10);

			//use accuracy to show static
			float distortion = Mathf.Lerp (0, 1, Mathf.Pow (0.5f - Services.PlayerBehaviour.accuracy/2, 3));
			float amplitude = Mathf.Clamp01(flow)/5;
			NewFrequency(newFrequency);

			//get value for sine wave effect
			float offset = Mathf.Sin (Time.time * frequency + phase + segmentIndex * 0.5f);

			//rotate direction 90 degrees
			Vector3 direction = GetVelocityAtIndex (pointIndex, step);
			Vector3 distortionVector = new Vector3 (-direction.y, direction.x, direction.z);

			// apply effects with distance falloff
			//(direction * offset * Mathf.Clamp01(distanceFromPlayer))
			v += ((distortionVector * UnityEngine.Random.Range (-distortion, distortion) * invertedDistance)) * amplitude;
		}


			SetLinePoint(v, segmentIndex);





		//because I was indexing out of vectrosity's line's points, just make sure its in there

		//Set the color. There are weird problems with vectrosity going out of range with color values...

		//ASFUALDFJKL:AEGJIOGWEJIOPGJIEOSIOJEVF
//		WHAT THE FUCK IS GOING ON HERE
//		if (segmentIndex < 1) {
//				line.SetColor (Color.white, segmentIndex);
//		}
//
// 			//CHECK ITS NOT THE LAST POINT
// //		SplinePoints [i + 1].color = Color.Lerp (SplinePoints [i + 1].color, Color.white, Mathf.Pow (invertedDistance, 2));
// //		line.SetWidth (Mathf.Lerp (1, 1, Mathf.Pow (invertedDistance, 10)), index);
//
// 			//if the player is on the leading edge of the line keep it black (you should be using low and hi here?)
// 				if(index > playerIndex && isPlayerOn){
//
// 					// if ((reversed && Services.PlayerBehaviour.progress < playerProgress) || (!reversed && Services.PlayerBehaviour.progress > playerProgress)) {
// 					if(index - playerIndex == 1){
// 						float difference = 1 - ((t - Services.PlayerBehaviour.progress) * curveFidelity);
// 						line.SetColor (Color.Lerp (Color.black, Color.white, difference), index);
// 					}else{
// 						line.SetColor (Color.black, index);
// 					}
//
// 				}else{
// 					if(locked){
// 						line.SetColor (Color.black, index);
// 					}else{
// 					//why not use Tim's code for indexDiff
// 						float lerpVal;
// 						Color c;
// 						if(i < SplinePoints.Count - 1){
// 								c = Color.Lerp (SplinePoints [i].color, SplinePoints [i + 1].color, t);
// 								lerpVal = (SplinePoints[i].proximity * curveFidelity) - (index/curveFidelity);
// 						}else{
// 							if(closed){
// 								c = Color.Lerp (SplinePoints [i].color, StartPoint.color, t);
// 								lerpVal = (SplinePoints[i].proximity * curveFidelity) - (index/curveFidelity);
// 								// lerpVal = SplinePoints[i].proximity + t * (StartPoint.proximity - SplinePoints[i].proximity);
// 							}else{
// 								c = SplinePoints [i].color;
// 								lerpVal = SplinePoints[i].proximity;
// 							}
// 						}
//
// 						float difference = (lerpVal);
// 						line.SetColor (Color.Lerp (c, Color.white, difference), index);
// 					}
// 				}
// 				/* I don't know what the fuck this is
// 						do coloring a certain way
// 				} else {
// 					if (i < SplinePoints.Count - 1) {
// 						//
// 						line.SetColor (Color.Lerp(new Color(0.1f, 0.1f, 0.1f), Color.white, SplinePoints [i].proximity), index);
// 									// line.SetWidth (Mathf.Lerp ((SplinePoints [i].NeighbourCount () - 1) + 1, (SplinePoints [i + 1].NeighbourCount () - 1) + 1, t), index);
//
// 					} else if (closed) {
//
// 					//IF IT IS THE LAST POINT, ONLY DRAW THE CONNECTION IF ITS A LOOP
//
// 						// line.SetColor (Color.Lerp (SplinePoints [i].color, SplinePoints [SplinePoints.Count - 1].color, t), index);
// 						line.SetColor (Color.Lerp (new Color(0.1f, 0.1f, 0.1f), Color.white, SplinePoints [i-1].proximity), index);
// 					//				line.SetWidth (Mathf.Lerp ((SplinePoints [i].NeighbourCount () - 1) + 1, (SplinePoints [SplinePoints.Count - 1].NeighbourCount () - 1) + 1, t), index);
// 				}
// 				//			line.SetWidth (1f, index);
//
// 			}
// 			*/
// 		}
	}

	public void PlayAttack (Point point1, Point point2){

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
		return (SplinePoints.IndexOf (Selected) * curveFidelity) + (int)Mathf.Floor((float)curveFidelity * (float)Services.PlayerBehaviour.progress);
	}

	public Vector3 GetPointAtIndex (int i, float t)
	{
		//MAKE THIS SHIT WORK WHEN THERE'S ONLY TWO POINTS
		//Maybe you need to decrement the index by one to force it to be between both splines
		//Obviously you need to set the progress correctly when you know you're facing backwards (start at 1)

		int Count = SplinePoints.Count;

		int j = i - 1;

		if (j < 0) {
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

		//this was..... probably a hangover from finding the point in worldspace. fug
		// v = transform.TransformPoint (v) - transform.position;

		// why did I need this
		// if (v == Vector3.zero && t == 1) {
		// 	v = GetVelocityAtIndex (i, 0.99f);
		// }
		return v;
	}


	Vector3 GetFirstDerivative (Vector3 p1, Vector3 p2, Vector3 r1, Vector3 r2, float t){
		return r1 * (1 - 4 * t + 3 * (t * t)) + t * (-6 * p1 + 6 * p2 + 6 * p1 * t - 6 * p2 * t + r2 * (-2 + 3 * t));
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

		for (int k = 0; k <= curveFidelity; k++) {

			float t = (float)k / (float)(curveFidelity);
			distance += Vector3.Distance (GetPoint (t), GetPoint (t + step));
		}

	}

	public void SetPoints (List<Point> points)
	{
		SplinePoints.Clear ();

		foreach (Point p in points) {
			AddPoint (null, p);
		}
	}

	public void InsertPoint(Point p, int index){
		SplinePoints.Insert(index, p);
	}

	public void RemovePoint(int i){
		foreach(Point p in SplinePoints[i]._neighbours){
			if(p._connectedSplines.Contains(this)){
			p._neighbours.Remove(SplinePoints[i]);
			}
		}
		SplinePoints.Remove(SplinePoints[i]);
	}

	public void DeletePoint(int i){
		GameObject g = SplinePoints[i].gameObject;
		RemovePoint(i);
		DestroyImmediate(g);
	}

	public void ReverseSpline(){
		SplinePoints.Reverse();
	}

	public void AddNewPoint(int i){
		Point newPoint;

		if(SplinePoints.Count > 1){
			newPoint = SpawnPointPrefab.CreatePoint (SplinePoints[i-1].Pos + GetInitVelocity(SplinePoints[i-1]).normalized/5f);
			InsertPoint(newPoint, i);
			newPoint.transform.parent = transform;
		}else{
			newPoint = SpawnPointPrefab.CreatePoint (transform.position);
			Point newPoint2 = SpawnPointPrefab.CreatePoint (transform.position + Vector3.up/5f);
			SplinePoints.Add(newPoint);
			SplinePoints.Add(newPoint2);
			newPoint.transform.parent = transform;
			newPoint2.transform.parent = transform;
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
			SplinePoints [newIndex - 1].AddPoint (p);
		}
	}

	public void DrawVelocity (Vector3 pos, float t, Vector3 direction)
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
				length = t;
			} else {
				t -= step;
				length = 1 - t;
			}

			length *= 2;

			// l2.SetPosition (l2.positionCount - 1, GetPoint (t));
			// l2.SetPosition (l2.positionCount - 2, GetPoint (t) + GetDirection (t) * x * length);
			// l2.SetPosition (l2.positionCount - 3, GetPoint (t));
		}
	}

	void NewFrequency(float newFrequency){
		float curr = (Time.time * frequency + phase) % (2.0f * Mathf.PI);
		float next = (Time.time * (newFrequency*2)) % (2.0f * Mathf.PI);
		phase = curr - next;
		frequency = newFrequency*2;
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
			//do spawning with these values elsewhere
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
