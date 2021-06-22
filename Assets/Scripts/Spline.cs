using System.Collections;
using UnityEngine;
using System.Xml;
using System.IO;
using Vectrosity;

//###################################################
//###################################################


//						TO DO					   


//Better support for textures
//More customization for how the spline is drawn (controls for static effect, bowing effect, etc)
//finally fix that extra segment that's being drawn on the end
//support for different materials, saved to the spline

//###################################################
//###################################################



public class Spline : MonoBehaviour
{
	
	public enum SplineType{normal, moving, locked}
	public enum SplineState{locked, on, done}
	public SplineState state;
	
	public SplineType type = SplineType.normal;

	public StellationController controller;

	[HideInInspector] 
	public static float shake;
	public static float amplitude = 0.25f;
	public static float noiseSpeed = 25;
	public static float frequency = 0.025f;
	public float distortion;

	float rollingDistance;
	float magnitude;
	public static System.Collections.Generic.List<Spline> Splines = new System.Collections.Generic.List<Spline> ();
	public static float drawSpeed = 1f;
	
	Coroutine drawRoutine;

	[HideInInspector]
	public System.Collections.Generic.List<Point> SplinePoints;

	[HideInInspector]
	public Point Selected;

	[HideInInspector]
	public VectorLine line;

	[HideInInspector]
	public float completion;		

	public int numPoints => SplinePoints.Count;

	public bool bidirectional = true;

	private float _completion
	{
		get { return completion / SplinePoints.Count; }

	}
	private float accuracyCoefficient;
	
	
	public float speed;
	
	[HideInInspector]
	public float maxSpeed, boost; 

	public static Spline Select;
	[Space(15)]
	
	[HideInInspector]
	public bool closed = false;
	public int order;
	private bool _locked;
	
	
	public bool locked
	{
		get { return state == SplineState.locked; }
	}
	
	public static int curveFidelity = 10;

	[Space(20)] [HideInInspector] public bool isPlayerOn, reactToPlayer;
	
	[HideInInspector]
	public bool draw = true;
	[HideInInspector]
	public float distance = 0;
	[HideInInspector]
	public float segmentDistance = 0;
	[HideInInspector]
	public Vector2 linearDirection;

	Vector3 prevPos;
	private bool reversed;
	private float colorDecay;
	private float distanceFromPlayer;
	private float invertedDistance;
	private int drawIndex;
	public bool drawingIn = false;
	private bool drawnIn;
	private int lowHitPoint = int.MaxValue;
	private int highHitPoint = -int.MaxValue;
	
	public int lineMaterial = 0;
	public int lineWidth = 1;
	private float textureWidth = 1;
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
	private float phase;
	private float volume;

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
		reactToPlayer = false;
		//line.StopDrawing3DAuto();
		drawTimer = 0;
		
		
		// if (curSound != null) {
		// 	StopCoroutine (curSound);
		// 	StartCoroutine (FadeNote (sound));
		// }

	}

	public void CheckComplete()
	{
		
		foreach (Point p in SplinePoints)
		{
			if (p.state != Point.PointState.on)
			{
				return;
			}
		}
		
		if (controller != null)
		{
			
			controller.UnlockSpline(this);
		}
	}
	
	public void OnSplineEnter (Point p1, Point p2)
	{
		if (!line.isAutoDrawing)
		{
			//line.Draw3DAuto();
		}

	
		StartDrawRoutine();
	
		
		draw = true;
		drawIndex = SplinePoints.IndexOf(p1) * curveFidelity;
		int i = SplinePoints.IndexOf (p1);
		int j = SplinePoints.IndexOf (p2);

		Selected = p1;
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

		//CalculateDistance ();

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

	public void SetUpReferences()
	{
		
		distance = 0;
		
			for(int i = 0; i < SplinePoints.Count; i++) {
				
				SplinePoints[i].AddSpline(this);
				if(i != 0){
					SplinePoints[i].AddPoint(SplinePoints[i-1]);
					SplinePoints[i-1].AddPoint(SplinePoints[i]);
				}

				Selected = SplinePoints[i];
				CalculateDistance();
				distance += segmentDistance;
			}

			if(closed){
				StartPoint.AddPoint (EndPoint);
				EndPoint.AddPoint(StartPoint);
				Selected = EndPoint;
				CalculateDistance();
				distance += segmentDistance;
			}

			reactToPlayer = false;
	}

	public void ChangeMaterial(int i)
	{
		if (i >= Services.Prefabs.lines.Length)
		{
			i %= Services.Prefabs.lines.Length;
		}
		lineMaterial = i;
		Material newMat;
		newMat = Services.Prefabs.lines[i % Services.Prefabs.lines.Length];
		Texture tex = newMat.mainTexture;
		float length = newMat.mainTextureScale.x;
		float height = newMat.mainTextureScale.y;
		
		line.texture = tex;
		line.textureScale = length;
		textureWidth = height;
		
		SetLineWidth(lineWidth);
	}

	public void SwitchMaterial(int i)
	{
		Material newMat;
		newMat = Services.Prefabs.lines[i % Services.Prefabs.lines.Length];
		Texture tex = newMat.mainTexture;
		float length = newMat.mainTextureScale.x;
		float height = newMat.mainTextureScale.y;
		
		line.texture = tex;
		line.textureScale = length;
		textureWidth = height;
		
		SetLineWidth(lineWidth);
	}

	void Awake ()
	{
		line = new VectorLine (name, new System.Collections.Generic.List<Vector3>(), 1, LineType.Continuous, Vectrosity.Joins.Weld);
		stepSize = (1.0f / (float)curveFidelity);
		Select = this;
		Splines.Add (this);
		Material newMat;
//		newMat = Services.Prefabs.lines[3];
//		Texture tex = newMat.mainTexture;
//		float length = newMat.mainTextureScale.x;
//		float height = newMat.mainTextureScale.y;

//		line.texture = tex;
//		line.textureScale = newMat.mainTextureScale.x;
	}

	//TODO
//	I WANT THE BEST Spline DRAWING Function< OF ALL FUCKING TIME>
	public void Initialize()
	{	
		SetUpReferences();
		ResetVectorLine();
		completion = 0;
	}

	public void SetSplineType(SplineType t)
	{

		if (t == SplineType.locked)
		{
			state = SplineState.locked;
		}

		if (t == SplineType.normal)
		{
			state = SplineState.on;
		}

		type = t;
	}
	
	public void SwitchState(SplineState t)
	{

		if (t == SplineState.locked)
		{
			LockSpline();
		}

		if (t == SplineState.on)
		{
			Unlock();
		}

		state = t;
	}

	public void SetLineWidth(int i)
	{
		lineWidth = i;
		line.lineWidth = lineWidth * textureWidth;
	}
	public void ResetVectorLine()
	{
		DestroyVectorLine();

		drawnIn = false;
		
		// int pointCount = SplinePoints.Count * curveFidelity;
		
		// if (!closed)
		// {
		// 	pointCount -= curveFidelity;
		// }

		System.Collections.Generic.List<Vector3> linePoints =  new System.Collections.Generic.List<Vector3> (3);
		
		line = new VectorLine (name, linePoints, 1, LineType.Continuous);
		if (MapEditor.editing)
		{
			line.color = Color.white;
		}
		else
		{
			line.color = Color.clear;
		}

		line.smoothWidth = true;
		line.smoothColor = true;
		line.lineWidth = lineWidth;
		
		ChangeMaterial(lineMaterial);
	}

	public void Reset()
	{
		SplinePoints.Clear();
		closed = false;
		drawingIn= false;
		drawnIn = false;
		ResetVectorLine();
		line.StopDrawing3DAuto();
		
	}

	public void DestroyVectorLine()
	{
		if (line != null)
		{
			VectorLine.Destroy(ref line);
		}
	}
	
	public void LockSpline()
	{	
		foreach (Point p in SplinePoints)
		{

			if (p != null && p._connectedSplines.Count <= 1)
			{
				p.SwitchState(Point.PointState.locked);
			}
		}

		state = SplineState.locked;
	}

	public void Unlock()
	{
		//fancy animation bullshit
		
		foreach (Point p in SplinePoints)
		{
			//why this condition ? p._connectedSplines.Count <= 1
			if (p != null && p.state == Point.PointState.locked)
			{
				p.SwitchState(Point.PointState.off);
			}
		}

		Services.fx.PlayAnimationAtPosition(FXManager.FXType.pulse, SplinePoints[0].transform);
		SplinePoints[0].SwitchState(Point.PointState.on);

		state = SplineState.on;

		StartDrawRoutine();
	}

	void SetLinePoint(Vector3 v, int index){
		if (index >= line.points3.Count) {
			line.points3.Add (v);
		} else {
			line.points3 [index] = v;
		}
	}

	public void DrawSplineOverride()
	{
		
		for (int i = 0; i < SplinePoints.Count - (closed ? 0 : 1); i++)
		{
			for (int k = 0; k < curveFidelity; k++)
			{
				int index = (i * curveFidelity) + k;
				float step = (float) k / (float) (curveFidelity-1);

//					DrawLine(i, index, step);
					Vector3 v = Vector3.zero;
					v = GetPointAtIndex(i, step);
					SetLinePoint(v, index);
			}
		}
		
		//RemoveExtraSegments();
	}

	public void Spin(float speed)
	{
		int end = closed ? SplinePoints.Count : SplinePoints.Count - 1;
		for (int i = 0; i < end; i++)
		{
			SplinePoints[i].isKinematic = true;
			SplinePoints[i].transform.position -= GetInitVelocity(SplinePoints[i])* Time.deltaTime * speed;
		}
	}

	public void StartDrawRoutine(){
		if(!drawingIn && !drawnIn){
			drawRoutine = StartCoroutine(DrawSplineIn());
		}
	}
	
	public IEnumerator DrawSplineIn()
	{
		drawingIn = true;
		
		int totalLineSegments = curveFidelity * (SplinePoints.Count - (closed ? 0 : 1)); // + (closed ? 0 : 1);
		int curDrawIndex = 0;
		prevPos = SplinePoints[0].Pos;
		rollingDistance = 0;	
		
		while (curDrawIndex < totalLineSegments)
		{

			if(Services.main.state != Main.GameState.playing) yield return null;
			
			float distanceTravelled = 0;

			for (int i = 0; i < SplinePoints.Count - (closed ? 0 : 1); i++)
			{
				for (int k = 0; k < curveFidelity; k++)
				{
					int index = (i * curveFidelity) + k;
					float step = (float) k / (float) (curveFidelity);
					
					// float step = (float) k / (float) (curveFidelity - 1);
	
					if(index >= curDrawIndex){

						//Debug.Log("are we getting here");
						float distanceDelta = rollingDistance;
						DrawLine(i, index, step);
						distanceDelta = rollingDistance - distanceDelta;
						distanceTravelled += distanceDelta;
						curDrawIndex ++;

						if(distanceTravelled > drawSpeed * Time.deltaTime){
							distanceTravelled = 0;
							i = 1000;
							k = 100;
						}
					}else{
						
						DrawLine(i, index, step);
					}

				}
			}
			
			yield return null;

			if (!bidirectional)
			{
				line.textureOffset -= Time.deltaTime * speed * 5f;
			}
		}

		drawingIn = false;
		drawnIn = true;
		drawRoutine = null;
	}

	public void DrawSpline(int pointIndex = 0)
	{
		if (drawingIn || !drawnIn) return;

		rollingDistance = 0;
		magnitude = Mathf.Clamp(Mathf.Pow(1 - Services.PlayerBehaviour.normalizedAccuracy, 2f) - shake, 0, 0.5f) * amplitude;

		if (isPlayerOn || reactToPlayer)
		{
			drawIndex = GetPlayerLineSegment(pointIndex);
		}
		
		
		prevPos = SplinePoints[0].Pos;
		
		if (!bidirectional)
		{
			line.textureOffset -= Time.deltaTime * speed * 5f;
		}

		int startIndex;

		if (isPlayerOn)
		{
			startIndex = 0;
			drawTimer += Time.deltaTime;
		}
		else
		{
			startIndex = pointIndex;
		}

		int closedOffset = (closed ? 0 : 1);
		for (int i = startIndex; i < SplinePoints.Count - closedOffset; i++)
		{
			for (int k = 0; k < curveFidelity; k++)
			{

				int index = (i * curveFidelity) + k;
				float step = (float) k / (float) (curveFidelity);

				Vector3 v = Vector3.zero;

				DrawLine(i, index, step);
			}
		}
		
		if(closed){
			DrawLine((SplinePoints.Count - 1), (SplinePoints.Count - 1) * Spline.curveFidelity + Spline.curveFidelity, 1);
		}
	}

	void DrawLine(int pointIndex, int segmentIndex, float step)
	{
		
		Vector3 v = GetPointAtIndex(pointIndex, step);
		
		//Add movement Effects of player is on the spline

		int indexDiff;

		//Find the shortest distance to the player in case of loop
		if (closed)
		{
			int dist1 = Mathf.Abs(segmentIndex - drawIndex);
			int dist2;

			if (segmentIndex < drawIndex)
			{
				dist2 = Mathf.Abs((line.GetSegmentNumber() - drawIndex) + segmentIndex);
			}
			else
			{
				dist2 = Mathf.Abs((line.GetSegmentNumber() - segmentIndex) + drawIndex);
			}

			indexDiff = Mathf.Min(dist1, dist2);

		}
		else
		{
			indexDiff = Mathf.Abs(drawIndex - segmentIndex);
		}

		//find the distance. 1 = one curve
		distanceFromPlayer = (float) indexDiff / (float) curveFidelity;

		//closeness to the player. 0 = one curve away
		invertedDistance = 1f - Mathf.Clamp01(Mathf.Abs(distanceFromPlayer));

		//float newFrequency = 1 + Mathf.Abs(Services.PlayerBehaviour.curSpeed);
		
		Vector3 direction = GetVelocityAtIndex(pointIndex, step).normalized;
		Vector3 distortionVector = new Vector3(-direction.y, direction.x, direction.z);
	
		//amplitude = Mathf.Clamp01(Services.PlayerBehaviour.potentialSpeed/5f) + shake;
		//Mathf.Sin(Time.time * frequency + phase - segmentIndex)
		
		//NewFrequency(newFrequency);		

		//(-Time.time * noiseSpeed) + 
		
		rollingDistance += (prevPos - v).magnitude;
		prevPos = v;


		distortion = (Mathf.PerlinNoise((-Time.time * noiseSpeed) + (rollingDistance * frequency), 1.321738f) * 2f - 1f);

		// if(!drawingIn){
			if (isPlayerOn)
			{
				//UnityEngine.Random.Range(- distortion, distortion)
				v += distortionVector * distortion * magnitude * Mathf.Clamp01(invertedDistance);
			}
			else if(reactToPlayer)
			{
				//I'm not even sure what this is doing
				v += distortionVector * distortion * magnitude * Mathf.Clamp01(-indexDiff + 10);
			}
		// }

		v += distortionVector * distortion * shake;

		SetLinePoint(v, segmentIndex);

		
//		if (segmentIndex < line.GetSegmentNumber())
//		{
			bool shouldDraw = true;
			
			int j = 0;
			if (pointIndex + 1 > SplinePoints.Count - 1)
			{
			
				if(!closed){
					shouldDraw = false;
					j = pointIndex;
				}
			}
			else
			{
				j = pointIndex + 1;
			}

			if ((reactToPlayer || isPlayerOn))
			{
//				if (segmentIndex <= drawIndex)
//				{
//					Color c = Color.white;
//					line.SetColor(c, segmentIndex);
//				}
//				else
				{
//					Color c = Color.Lerp(SplinePoints[pointIndex]._color, SplinePoints[j]._color, step);
//					(Color.white * (_completion -1))
					Color c;
					if (isPlayerOn)
					{
						 c = Color.Lerp(SplinePoints[pointIndex]._color, SplinePoints[j]._color,
							Mathf.Pow(distanceFromPlayer / Mathf.Clamp(SplinePoints.Count - 1, 1, 3), 2));
					}
					else
					{
						c = Color.Lerp(SplinePoints[pointIndex]._color, SplinePoints[j]._color, step);
					}

					c += (Color.white * Mathf.Clamp01(_completion - 1));
					c.a = 1;
					line.SetColor(c, segmentIndex);
				}
			}
			else
			{
				if (shouldDraw)
				{
					Color c = Color.Lerp(SplinePoints[pointIndex]._color, SplinePoints[j]._color, step);
					//c = Color.Lerp(c, Color.white, invertedDistance);
					c += (Color.white * Mathf.Clamp01(_completion - 1));
					c.a = 1;
					line.SetColor(c, segmentIndex);
				}
			}

	}

	void OnDestroy ()
	{
		if(drawRoutine != null){
			StopCoroutine(drawRoutine);
		}

		Splines.Remove (this);
		if (controller != null)
		{
			controller._splines.Remove(this);
			
			if (controller._splinesToUnlock.Contains(this))
			{
				controller._splinesToUnlock.Remove(this);
			}
			
		}
		
		DestroyVectorLine();
		Destroy(gameObject);
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

	public int GetPlayerLineSegment ( int i)
	{
		
		return (SplinePoints.IndexOf(Selected) * curveFidelity) + (int)Mathf.Floor((float)curveFidelity * (float)Services.PlayerBehaviour.progress);
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

		Point Point0 = SplinePoints [j];

		
		j = i;
		if (j > Count - 1) {
			if (closed) {
				j = 0;
			} else {
				j = i;
			}
		}

		Point Point1 = SplinePoints[j];
		
		j = i + 1;
		if (j > Count - 1) {
			if (closed) {
				j = j % Count;
			} else {
				j = i;
			}
		}

		Point Point2 = SplinePoints [j];

		j++;

		if (j > Count - 1) {
			if (closed) {
				j = j % Count;
			} else {
				j = i;
			}
		}

		Point Point3 = SplinePoints [j];

		float tension = Point1.tension;
		float continuity = Point1.continuity;
		float bias = Point1.bias;

		Vector3 r1 = 0.5f * (1 - tension) * ((1 + bias) * (1 - continuity) * (Point1.Pos - Point0.Pos) + (1 - bias) * (1 + continuity) * (Point2.Pos - Point1.Pos));

		tension = Point2.tension;
		continuity = Point2.continuity;
		bias = Point2.bias;

		Vector3 r2 = 0.5f * (1 - tension) * ((1 + bias) * (1 + continuity) * (Point2.Pos - Point1.Pos) + (1 - bias) * (1 - continuity) * (Point3.Pos - Point2.Pos));
		Vector3 v = GetPoint (t, Point1.Pos, Point2.Pos, r1, r2);

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
		//Vector2 noZ = GetVelocity (t);
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
		return GetVelocityAtIndex (GetPointIndex (p), 0.01f);
	}

	public Vector3 GetReversedInitVelocity (Point p)
	{
		return -GetVelocityAtIndex (GetPointIndex (p), 0.99f);
	}

	public float CompareAngleAtPoint (Vector3 direction, Point p, bool reversed = false)
	{
		Vector3 dir = Vector3.zero;
		if (reversed) {
			dir = GetReversedInitVelocity (p);
		} else {
			dir = GetInitVelocity (p);
		}

		return Vector2.Angle(direction, SplineUtil.GetScreenSpaceDirection(p.Pos, dir));
	}


	public void DestroySpline (Point toDelete, Point toAnchor)
	{
		Destroy (this);
	}

	public float GetDistance(int i){
		
		float step = (1.0f / (float)curveFidelity);
		float d = 0;

		for (int k = 0; k < curveFidelity; k++) {

			float t = (float)k / (float)(curveFidelity);
			d += Vector3.Distance (GetPointAtIndex (i, t), GetPointAtIndex (i, t + step));
		}
		return d;
	}
	public void CalculateDistance ()
	{
		//IDK IF THIS WORKS FORWARD/BACKWARDS

		float step = (1.0f / (float)curveFidelity);
		segmentDistance = 0;

		for (int k = 0; k < curveFidelity; k++) {

			float t = (float)k / (float)(curveFidelity);
			segmentDistance += Vector3.Distance (GetPoint (t), GetPoint (t + step));
		}
	}

	public void SetPoints (System.Collections.Generic.List<Point> points)
	{
		SplinePoints.Clear ();

		foreach (Point p in points) {
			AddPoint (null, p);
		}
	}

	public void InsertPoint(Point p, int index){
		SplinePoints.Insert(index, p);
	}

	public void ResetLineLength()
	{
		int pointCount;
		
		if (closed)
		{
			pointCount = SplinePoints.Count * curveFidelity;
		}
		else
		{
			pointCount = (SplinePoints.Count-1) * curveFidelity;
		}
		
		line.points3 = new System.Collections.Generic.List<Vector3>(pointCount);	
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

	public Point AddNewPoint(int i){
		Point newPoint;

		if(SplinePoints.Count > 1)
		{
			Vector3 newPos;
			if (i >= SplinePoints.Count - 1)
			{
				newPos = SplinePoints[i].Pos + GetInitVelocity(SplinePoints[i]).normalized / 5f;
			}
			else
			
			{
				newPos = GetPointAtIndex(i, 0.5f);
			}
			newPoint = SpawnPointPrefab.CreatePoint(newPos);
			InsertPoint(newPoint, i+1);
			
			Services.fx.PlayAnimationAtPosition(FXManager.FXType.pulse, newPoint.transform);
			
			newPoint.transform.parent = transform;
		}else
		{
			newPoint = SpawnPointPrefab.CreatePoint(transform.position);
				
			Services.fx.PlayAnimationAtPosition(FXManager.FXType.pulse, newPoint.transform);
			
			Point newPoint2 = SpawnPointPrefab.CreatePoint (transform.position + Vector3.up/5f);
			SplinePoints.Add(newPoint);
			SplinePoints.Add(newPoint2);
			newPoint.transform.parent = transform;
			newPoint2.transform.parent = transform;
		}

		return newPoint;
	}

	public void AddPoint (Point curPoint, Point p)
	{
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
		int.TryParse (input.DocumentElement.ChildNodes [3].Attributes [0].Value, out curveFidelity);

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
