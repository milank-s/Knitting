using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AudioHelm;
using SimpleJSON;
using UnityEngine.Events;

//###################################################
//###################################################


//						TO DO					   


//Flag points as leaf so that they force creation of a new spline if connected?
//Initialize callback from player when "lastPoint" is updated
//Update sprites for points
//better control over physics for animating shaking


//###################################################
//###################################################

public enum PointTypes{normal, fly, ghost, stop, connect, reset, start, end, pickup}
public class Point : MonoBehaviour
{

	#region

	public UnityEvent OnEnter;
	public UnityEvent OnButtonDown;
	public UnityEvent OnExit;
	

	public enum PointState{locked, off, on}
	public PointState state = PointState.off;
	
	public bool canFly
	{
		get
		{
			return pointType == PointTypes.fly || NeighbourCount() == 0;
		}
	}

	public PointTypes pointType = PointTypes.normal;
	[Space(10)]

	public List<Point> _neighbours;
	public List<Spline> _connectedSplines;

	public static float hitColorLerp;
	public static int pointCount = 0;
	public static float boostAmount = 1f;
	public float distortion;
	public float distanceFromPlayer;
	float glow;

	bool initialized = false;
	
	[Space(10)]
	[Header("Curve")]
	public static List<Point> Points;

	public float tension;
	public float bias;
	public float continuity;
	private float initTension;
	private float initBias;
	private float initContinuity;
	[Space(10)] 
	public bool usedToFly;
	public bool isKinematic;
	public bool setDirection = false;
	[HideInInspector]
	
	public static float damping = 1000f;
	public static float stiffness = 1000f;
	public static float mass = 20f;
	
	[HideInInspector]
	public Vector3 anchorPos;
	public Vector3 initPos;
	[Space(10)]

	public string text;
	public TextMesh textMesh;
	public StellationController controller;
	[HideInInspector]
	public bool hasController;
	[Space(10)]

	[Header("Interaction")]
	public bool spawnCollectible;
	public bool recieveCollectible;
	public bool hasCollectible;
	public Collectible collectible;
	private float cooldown;
	[HideInInspector]
	public float timeOffset;
	[HideInInspector]
	public float proximity = 0;
	public int note = 32;
	

	public Color color;

	[HideInInspector]
	public float brightness = 0;
	public float hue = 0;
	public float accretion;
	public static Point Select;
	public MeshRenderer renderer;
	public MeshFilter meshFilter;
	Material mat;
	private float timeOnPoint;
	public int timesHit = 0;
	
	public bool isSelect
	{
		get
		{
			return this==Select;
		}
	}
	[HideInInspector]
	public Vector3 velocity;


	public Vector3 Pos => transform.position;

	public Color _color
	{
		get
		{
			return Color.HSVToRGB(hue, 0, brightness) + color;
		}
	}
	
	#endregion

	public JSONObject Save(int i)
	{
		JSONObject data = new JSONObject();
		data ["x"].AsFloat = transform.position.x;
		data ["y"].AsFloat = transform.position.y;
		data ["z"].AsFloat = transform.position.z;
		data["tension"].AsFloat = tension;
		data["bias"].AsFloat = bias;
		data["continuity"].AsFloat = continuity;
		data["index"].AsInt = i;
		data["pointType"].AsInt = (int) pointType;
		data["word"] = text;
		data["collect"].AsBool = false;
		
		JSONObject pointText = new JSONObject();
		
		if (textMesh != null)
		{
			pointText["x"] = textMesh.transform.position.x;
			pointText["y"] = textMesh.transform.position.y;
			pointText["z"] = textMesh.transform.position.z;
			pointText["font"].AsInt = Services.Prefabs.FindFontIndex(textMesh.font);
			pointText["fontSize"].AsInt = textMesh.fontSize;
		}
		else
		{
			pointText["x"].AsFloat = Pos.x + 0.5f;
			pointText["y"].AsFloat = Pos.y;
			pointText["z"].AsFloat = Pos.z;
			pointText["font"].AsInt = 0;
			pointText["fontSize"].AsInt = 48;
		}

		data["text"] = pointText;

		return data;
	}

	public void OnDestroy()
	{
		Points.Remove(this);
		
		if (hasController)
		{
			controller._points.Remove(this);
		}
	}

	public void Awake(){
		Initialize();
	}

	public void Initialize()
	{
		if(initialized) return;

		initialized = true;

		// so when does this happen 
		// map editor doesnt want you to keep spawning collectibles
		// if you flip this variable after the point is made it will not be reflected in the editor

		
		hue = Random.Range(0, 1f);
		if(Application.isPlaying){
			mat = renderer.material;
		}else{
			mat = renderer.sharedMaterial;
		}
		initPos = transform.position;
		state = PointState.off;
		mat.color = Color.clear;
		timeOffset = Point.pointCount;

		_neighbours = new List<Point> ();
		_connectedSplines = new List<Spline> ();
		
		Points.Add(this);
		Point.pointCount++;
	}

	public void AddCollectible(bool b){
		if(!b){
			if(collectible != null){
				Destroy(collectible.gameObject);
			}
		}else{
			if(collectible == null){
				collectible = Instantiate(Services.Prefabs.collectible, transform).GetComponent<Collectible>();
				collectible.SetPoint(this);
			}else{
				collectible.Reset();
			}
		}
	}

	public void AddForce(Vector3 vel){
		velocity += vel;
	}
	public void TurnOn()
	{
		StartCoroutine(LightUp());
	}
	
	public void Clear()
	{
		_neighbours.Clear();
		_connectedSplines.Clear();
	}
	public void Setup()
	{
		Initialize();
		
		initPos = transform.position;
		anchorPos = initPos;
		initContinuity = continuity;
		initTension = tension;
		initBias = bias;
	
		cooldown = 0;
		timesHit = 0;
		
		SetPointType(pointType);
		

		
		color = Color.clear;

		if (text != "" && textMesh != null)
		{
			textMesh.color = Color.black;
		}

		if(pointType == PointTypes.pickup){
			spawnCollectible = true;
			renderer.enabled = false;

			AddCollectible(true);
		}else{
			spawnCollectible = false;
			AddCollectible(false);
		}

		recieveCollectible = false;
		
		if(pointType == PointTypes.stop || pointType == PointTypes.end || pointType == PointTypes.start){
			recieveCollectible = true;
		}

	}

	public void SetForward(Vector3 dir){
		renderer.transform.rotation = Quaternion.LookRotation(CameraFollow.instance.transform.forward, dir);

	}

	public void SetPointType(PointTypes t)
	{
		renderer.enabled = true;
		setDirection = false;
		pointType = t;
		
		meshFilter.mesh= Services.Prefabs.pointMeshes[(int)t];

		switch(t){

			case PointTypes.fly:
				setDirection = true;
				break;

			case PointTypes.stop:
				
				break;

			case PointTypes.connect:
				
				break;

			case PointTypes.ghost:
				renderer.enabled = false;
				break;
			
			case PointTypes.normal:
				setDirection = true;
				break;
			
			case PointTypes.start:
				
				break;
			
			case PointTypes.end:
				mat.color = Color.white;
				color = Color.white;
				
				break;
		}
	}

	public void Step(){

		//Pos = transform.position;
		

		if (!MapEditor.editing)
		{
			SetColor();
			
			distortion = Mathf.Lerp(distortion, 0, Time.deltaTime * 2);
			glow = Mathf.Lerp(glow, 0, Time.deltaTime * 2);

			//this is probably not optimized
			renderer.transform.rotation = Quaternion.LookRotation(CameraFollow.instance.transform.forward, renderer.transform.up);
			
			if(pointType == PointTypes.start){
				if(controller.collected){
					renderer.transform.Rotate(0, 0, 200 * Time.deltaTime, Space.Self);
				}
			}

			if (!isKinematic)
			{
				Movement();
			}
		}
		else
		{
			mat.color = Color.white;
		}


		//transform.LookAt(Services.mainCam.transform.position, Vector3.up);
	}

	public void MoveInitPosition(Vector3 v){
		anchorPos += v;
		initPos += v;
	}

	public void Movement(){
		
		Vector3 stretch = transform.position - (anchorPos); // + controller.pos);
		Vector3 force = -stiffness * stretch - damping * velocity;
		Vector3 acceleration = force / mass;

		velocity += acceleration * Time.deltaTime;
		transform.position += velocity * Time.deltaTime;
	}

	public void AddSpline(Spline s){
		if (!_connectedSplines.Contains (s)) {
			_connectedSplines.Add (s);
		}
	}

	public void AddPoint(Point p){
		if (!_neighbours.Contains (p)) {
			_neighbours.Add (p);
		}else{
		}
	}

	public void Reset()
	{
		hasCollectible = false;
		usedToFly = false;
		anchorPos = initPos;
		transform.position = initPos;
		state = PointState.off;
		bias = initBias;
		tension = initTension;
		continuity = initContinuity;
		timesHit = 0;
		cooldown = 0;
		color = Color.clear;
		
	}

	public void CleanText()
	{
		if (textMesh != null)
		{
			Destroy(textMesh.gameObject);
			textMesh = null;
		}
	}

	IEnumerator LightUp()
	{
		

		float f = 0;
		Color startColor = color;
		Color endColor = new Color(0.2f, 0.2f, 0.2f);
		while (f < 1)
		{
			color = Color.Lerp(startColor, endColor, f);
			f += Time.deltaTime * 5f;
			SetColor();
			yield return null;
		}

		color = endColor;
		SetColor();
	}
	

	public int NeighbourCount(){
		return _connectedSplines.Count;
	}

	public int numActiveNeighbours(){
		int i = 0;
		foreach(Spline s in _connectedSplines){
			if(s.state == Spline.SplineState.on){
				i++;
			}
		}
		return i;
	}

	public void RemoveSpline(Spline s){
		_connectedSplines.Remove (s);
	}

	public void RemovePoint(Point p){
		_neighbours.Remove (p);
	}

	public void Lock()
	{
		state = PointState.locked;
		StartCoroutine(Fade());
	}

	IEnumerator Fade()
	{
		float f = 0;
		Color startColor = color;
		
		while (f < 1)
		{
			color = Color.Lerp(startColor, Color.white/8f, f);
			f += Time.deltaTime;
			SetColor();
			yield return null;
		}
	}

	public void SwitchState(PointState s)
	{

		PointState prevState = state;
		state = s;
		
		switch (s)
		{
			case PointState.locked:

				if (prevState != PointState.locked)
				{
					Lock();
				}

				break;

			case PointState.off:

				break;

			case PointState.on:
				if (prevState != PointState.on)
				{
					foreach(Spline sp in _connectedSplines){
						if(!sp.drawing){
							sp.StartDrawRoutine(this);
						}
					}
					StartCoroutine(LightUp());
				}
				//PointManager.AddPointHit(this);

				break;
		}

	}

	public void OnPointEnter(){

		if(pointType != PointTypes.ghost){
			if(Services.main.OnPointEnter != null){
				Services.main.OnPointEnter(this);
			}

			glow = 1;

			if(OnEnter != null){
				OnEnter.Invoke();
			}
		}
	}
	
	public void OnPlayerEnterPoint()
	{
		Pathfinding.PopulateGraphDistances(this);
		
		if(Services.main.activeStellation != controller){
			
			Services.main.activeStellation.OnPlayerExit();
			controller.OnPlayerEnter();
			//entered new stellation	
		}

		Services.main.activeStellation.HitPoint(this);

		proximity = 1;
		timeOnPoint = 0;
		timesHit++;
		
		OnPointEnter();
	
		
		SwitchState(PointState.on);

		if(pointType != PointTypes.ghost)
		{
			if(Services.main.OnPlayerEnterPoint != null){
				Services.main.OnPlayerEnterPoint(this);
			}

			if(spawnCollectible){
				if(!collectible.collected){
					collectible.Pickup();
				}
			}
		
			if(recieveCollectible && !hasCollectible){	
				if(pointType == PointTypes.start || pointType == PointTypes.end){
					controller.DepositPlayer();
				}else{
					controller.DepositCollectible(this);
				}
			}
		
			controller.TryToUnlock();
			
			if(pointType != PointTypes.start || (pointType != PointTypes.start && controller.startIndex != 0)){
				controller.NextWord();
			}
			

			switch (pointType)
			{
				case PointTypes.normal:
				
					break;
				
				case PointTypes.reset:

					if (Services.StartPoint != this)
					{
						Services.main.WarpPlayerToNewPoint(controller.GetStartPoint());
					}

					break;
				
				case PointTypes.start:
					

					if(controller.OnHitStart != null){
						controller.OnHitStart.Invoke();
					}
					
					
					break;
				
				case PointTypes.end:

					//I like this idea. I don't think it's well explained or utilized at the moment

					if(!controller.isComplete)	{
						
						if(_neighbours.Count < 2){
							Services.main.WarpPlayerToNewPoint(controller.GetStartPoint());
						}
					}
					
					break;
			}
		
		}
		
	}

	public void OnPlayerExitPoint(){

		Services.fx.SpawnCircle(transform);

	 	if(OnExit != null){
			OnExit.Invoke();
		}
		
		switch(pointType){
			case PointTypes.normal:
				SetForward(Services.PlayerBehaviour.curDirection);
			break;

			case PointTypes.stop:

			break;

			case PointTypes.fly:
				
			break;

			case PointTypes.pickup:
				if(spawnCollectible && collectible.collected){
					SetPointType(PointTypes.ghost);
				}

			break;
			
			case PointTypes.start:

				controller.LeftStartPoint();
				break;
			
			case PointTypes.end:

				break;
		}
	}

	public bool CanLeave()
	{
		bool buttonUp = Services.PlayerBehaviour.buttonUp;
		bool buttonPressed = Services.PlayerBehaviour.buttonWasPressed;

		if(buttonPressed){
			if(OnButtonDown != null){
				OnButtonDown.Invoke();
			}
		}	

		if(Services.PlayerBehaviour.buttonDown) return false;
		
		switch (pointType)
		{
			case PointTypes.start:
				if (timesHit > 1)
				{
					return true;
				}else if (buttonUp)
				
				{
					return true;
				}
				else
				{
					return false;
				}
			
			case PointTypes.end:
				
				return true;
			
			case PointTypes.stop:
				if (buttonPressed)
				{
					return true;
				}
				else
				{
					return false;
				}
				
			case PointTypes.connect:

			return true;

				// if (buttonPressed)
				// {
				// 	return true;
				// }
				// else
				// {
				// 	return false;
				// }
				// break;

			case PointTypes.fly:

				if (buttonPressed)
				{
					return true;
				}
				else
				{
					return false;
				}

			
			default:
				return true;
			
		}
	}
	
	public void Contract()
	{
		tension = Mathf.Clamp(tension + Time.deltaTime /5f, -1f, 1f);
	}

	public void Release()
	{
		anchorPos = Vector3.Lerp(anchorPos, initPos, Time.deltaTime);
		tension = Mathf.Lerp(tension, initTension, Time.deltaTime);
	}
	
	public bool HasSplines(){
		return _connectedSplines.Count > 0;
	}

	//points can have an arbitrary amount of connecting splines!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
	//this is all kinds of fucked up
	public Spline GetConnectingSpline(Point p){
		foreach (Spline s in _connectedSplines) {
			if (s.IsPointConnectedTo(p)){
				int indexDiff = s.GetPointIndex(p) - s.GetPointIndex(this);
				if(Mathf.Abs(indexDiff) == 1){
					return s;
				}else{
					if(s.closed){
						return s;
					}
				}
				
				continue;
			}
		}
		return null;
	}

	public List<Spline> GetConnectingSplines(Point p){
		List<Spline> sp = new List<Spline>();
		foreach (Spline s in _connectedSplines) {
			if (s.IsPointConnectedTo(p)){
				sp.Add(s);
			}
		}
		return sp;
	}

	public bool IsAdjacent(Point n){
		return _neighbours.Contains (n);
	}

	public List<Spline> GetSplines()
	{
		return _connectedSplines;
	}

	public float GetCooldown(){
		return cooldown;
	}

	public List<Point> GetNeighbours(){
		return _neighbours;
	}

	void SetColor(){


		// c = (Mathf.Sin (3 * (Time.time + timeOffset))/4 + 0.3f) + proximity;
//		c = proximity + Mathf.Sin(Time.time + timeOffset)/10 + 0.11f;
		// ACCRETION IS SHOWING POINTS THAT IT SHOULDNT?????

		//if you used HSB this would be the brightness param

		brightness = glow + proximity + (state == PointState.on ? (Mathf.Sin(-Time.time * 2 + timeOffset)/4f + 0.25f) : 0f) + 0.1f; // + timesHit/5f;
		
		// accretion
		
//		mat.color = Color.Lerp (color, new Color (1,1,1, c), Time.deltaTime * 5);

		if (state == PointState.locked)
		{
			mat.color = Color.clear;
			
		}else
		{
			if(HasSplines()){
				hue = _connectedSplines[_connectedSplines.Count-1].hue;
			}

			mat.color = _color; 	
		}

//		mat.color += Color.white * Mathf.Sin(3 * (Time.time + timeOffset)) / 10;
	}

	public void PlayerOnPoint(Vector3 direction, float force)
	{
		timeOnPoint += Time.deltaTime;
		if(setDirection){
			SetForward(direction);
		}
		
		bool buttonPressed = Services.PlayerBehaviour.buttonWasPressed;

		if(buttonPressed){
			if(OnButtonDown != null){
				OnButtonDown.Invoke();
			}
		}	


		//anchorPos = initPos + ((Vector3)Random.insideUnitCircle / 10f * Services.PlayerBehaviour.flow *  Mathf.Clamp01(timeOnPoint));
		//velocity += (Vector3)Random.insideUnitCircle / Mathf.Pow(1 + timeOnPoint, 2);
	}
	
	

	// public void SetDirectionalArrows(){
	// 	int index = 0;
	//
	// 	foreach (Spline s in _connectedSplines) {
	// 		foreach (Point p in _neighbours) {
	//
	// 			if (!p._connectedSplines.Contains (s)) {
	// 				//do nothing if the point is in another spline
	// 			} else {
	// 				if (index > _directionalSprites.Count - 1) {
	// 					GameObject newSprite = (GameObject)Instantiate (directionalSprite, Vector3.zero, Quaternion.identity);
	// 					newSprite.transform.parent = transform;
	// 					_directionalSprites.Add (newSprite);
	// 				}
	// 				SetPosAndVelocity (_directionalSprites [index], 0, s, p);
	// 				float cc = c + Mathf.Clamp01 (cooldown);
	// 				_directionalSprites[index].GetComponent<SpriteRenderer>().color =  new Color (cc,cc,cc);
	// 				index++;
	// 			}
	// 		}
	// 	}
	// }
}
