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

public enum PointTypes{normal, fly, ghost, stop, connect, reset, start, end}
public class Point : MonoBehaviour
{

	#region

	public UnityEvent OnEnter;
	

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
	public static float boostAmount = 0.5f;
	public float distortion;
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
	[HideInInspector]
	
	public static float damping = 1000f;
	public static float stiffness = 1000f;
	public static float mass = 50f;
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
	public Collectible collectible;
	private float cooldown;
	[HideInInspector]
	public float timeOffset;
	[HideInInspector]
	public float proximity = 0;
	public int note;
	

	public Color color;
	[HideInInspector]
	public float c = 0;
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
			return new Color(c, c, c, 1) + color;
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

	void Initialize()
	{
		if(initialized) return;
		initialized = true;

		// so when does this happen 
		// map editor doesnt want you to keep spawning collectibles
		// if you flip this variable after the point is made it will not be reflected in the editor

		mat = renderer.material;
		initPos = transform.position;
		state = PointState.off;
		//Pos = transform.position;

		if (MapEditor.editing)
		{
			mat.color = Color.white;
		}
		else
		{
			mat.color = Color.black;
		}
//		stiffness = 1600;
//		damping = 1000;
//		mass = 20;

		timeOffset = Point.pointCount;

		_neighbours = new List<Point> ();
		_connectedSplines = new List<Spline> ();

		textMesh = GetComponentInChildren<TextMesh>();
		
		if(textMesh != null){
			textMesh.GetComponent<FadeTextOnPoint>().p = this;
		}
		
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
	
		c = 0;
		cooldown = 0;
		timesHit = 0;
		
		SetPointType(pointType);
		
		if (MapEditor.editing)
		{
			color = Color.white;
			c = 1;
		}

		if (text != "" && textMesh != null)
		{
			textMesh.color = Color.black;
		}

		if(pointType == PointTypes.normal){
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

	public void SetPointType(PointTypes t)
	{
		renderer.enabled = true;
		pointType = t;
		
		meshFilter.mesh= Services.Prefabs.pointMeshes[(int)t];

		switch(t){

			case PointTypes.fly:
				
				break;

			case PointTypes.stop:
				
				break;

			case PointTypes.connect:
				
				break;

			case PointTypes.ghost:
				renderer.enabled = false;
				break;
			
			case PointTypes.normal:
				
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
			renderer.transform.rotation = Quaternion.LookRotation((CameraFollow.instance.pos - Pos).normalized, renderer.transform.up);
			
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
		}else{
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

		usedToFly = false;
		anchorPos = initPos;
		transform.position = initPos;
		state = PointState.off;
		bias = initBias;
		tension = initTension;
		continuity = initContinuity;
		timesHit = 0;
		c = 0;
		cooldown = 0;
		note = SynthController.instance.GetNote(this);
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

				if (prevState == PointState.locked)
				{
					
						// you need to add some colour, they're just black
				}

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
		
		// if (controller.CheckSpeed())
		// {
		if(textMesh != null){
			textMesh.GetComponent<FadeTextOnPoint>().alpha = 1;
		}
		// }

//		stiffness = Mathf.Clamp(stiffness -100, 100, 10000);
//		damping = Mathf.Clamp(damping - 100, 100, 10000);
		
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
		
			if(recieveCollectible && collectible == null){	
				if(pointType == PointTypes.start){
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

					if (StellationManager.instance != null &&
					    Services.main.activeStellation != controller)
					{
						//StellationManager.instance.EnterStellation(controller);
					}
					
					
					break;
				
				case PointTypes.end:

					if(!controller.isComplete)	{
						
						if(_neighbours.Count < 2){
							Services.main.WarpPlayerToNewPoint(controller.GetStartPoint());
						}
					}
					
					break;
			}
		
		}
		
	}

	
	public void OnPointExit(){

		switch(pointType){
			case PointTypes.stop:
//				Services.PlayerBehaviour.flow += 0.1f;

			break;

			case PointTypes.fly:

				
			break;

			case PointTypes.normal:
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

				break;
			
			case PointTypes.end:
				
				return true;
				
				break;
			
			case PointTypes.stop:
				if (buttonPressed)
				{
					return true;
				}
				else
				{
					return false;
				}
				break;
			case PointTypes.connect:

			return true;

			if (buttonPressed)
				{
					return true;
				}
				else
				{
					return false;
				}
				break;

			case PointTypes.fly:

				if (buttonPressed)
				{
					return true;
				}
				else
				{
					return false;
				}
				break;
			

			
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

	public Spline GetConnectingSpline(Point p){
		foreach (Spline s in _connectedSplines) {
			if (s.IsPointConnectedTo(p))
				return s;
		}
		return null;
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
		c = glow + proximity + (state == PointState.on ? (Mathf.Sin(-Time.time * 2 + timeOffset)/4f + 0.25f) : 0f); // + timesHit/5f;
		
		// accretion
		
//		mat.color = Color.Lerp (color, new Color (1,1,1, c), Time.deltaTime * 5);

		if (state == PointState.locked)
		{
			mat.color = Color.clear;
			
		}else
		{
			mat.color = _color; 	
		}

//		mat.color += Color.white * Mathf.Sin(3 * (Time.time + timeOffset)) / 10;
	}

	public void PlayerOnPoint(Vector3 direction, float force)
	{
		timeOnPoint += Time.deltaTime;
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
