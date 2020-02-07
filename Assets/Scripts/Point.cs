using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine.Experimental.PlayerLoop;


//###################################################
//###################################################


//						TO DO					   


//Flag points as leaf so that they force creation of a new spline if connected?
//Initialize callback from player when "lastPoint" is updated
//Update sprites for points
//better control over physics for animating shaking


//###################################################
//###################################################

public enum PointTypes{normal, fly, ghost, stop, connect, start, end}
public class Point : MonoBehaviour
{

	#region

	public enum PointState{locked, off, on}
	public PointState state = PointState.off;
	
	public bool canFly
	{
		get
		{
			if (pointType == PointTypes.fly)
//				
			{
				return true;
			}
			else
			{
				return false;
			}
		}
	}

	public PointTypes pointType = PointTypes.normal;
	[Space(10)]

	[HideInInspector]
	public List<Point> _neighbours;
	[HideInInspector]
	public List<Spline> _connectedSplines;

	public static float hitColorLerp;
	public static int pointCount = 0;

	public static float boostAmount = 0.5f;
	
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
	public bool defaultToGhost = true;
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
	private float cooldown;
	[HideInInspector]
	public float timeOffset;
	[HideInInspector]
	public float proximity = 0;
	

	[HideInInspector]
	public Color color;
	[HideInInspector]
	public float c = 0;
	public float accretion;
	public static Point Select;
	private FadeSprite activationSprite;
	private SpriteRenderer SR;
	private float timeOnPoint;
	[HideInInspector] public int timesHit = 0;
	public bool isSelect
	{
		get
		{
			return this==Select;
		}
	}
	private Vector3 _velocity;

	public Vector3 velocity
	{
		set
		{
			_velocity = value;
		}

		get { return _velocity; }
	}

	public Vector3 Pos
	{
		get
		{
			return transform.position;
		}
	}

	public Color _color
	{
		get
		{
			if (state == PointState.on)
			{
				return new Color(c, c, c, 1) + Color.white * (Mathf.Sin(3 * (Time.time + timeOffset)) / 10 + 0.2f);
				
			}
			
			return new Color(c, c, c, 1) + (Color.white * 0.1f);
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

	public void Destroy()
	{
		Points.Remove(this);
		
		if (hasController)
		{
			controller._points.Remove(this);
		}
		
		Destroy(gameObject);
	}


	void Awake()
	{
		initPos = transform.position;
		SR = GetComponent<SpriteRenderer> ();

		
//		stiffness = 1600;
//		damping = 1000;
//		mass = 20;
		activationSprite = GetComponentInChildren<FadeSprite> ();
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

	public void TurnOn()
	{
		StartCoroutine(LightUp());
	}
	
	public void Clear()
	{
		_neighbours.Clear();
		_connectedSplines.Clear();
	}
	public void Initialize()
	{

		gameObject.name = text;
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
		}
		else
		{
			color = Color.clear;
		}

		if (text != "" && textMesh != null)
		{
			textMesh.color = Color.black;
		}

	}

	public void SetPointType(PointTypes t)
	{
		
		SR.enabled = true;
		pointType = t;
		SR.sprite = SR.sprite = Services.Prefabs.pointSprites[(int)t];
		switch(t){

			case PointTypes.fly:
				
				break;

			case PointTypes.stop:
				
				break;

			case PointTypes.connect:
				
				defaultToGhost = false;
				break;

			case PointTypes.ghost:
				SR.enabled = false;
				break;
			
			case PointTypes.normal:
				
				break;
			
			case PointTypes.start:
				Services.StartPoint = this;
				break;
		}
	}

	public void Update(){
		if (!MapEditor.editing)
		{
			SetColor();

			if (!isKinematic)
			{
				Movement();
			}

		}
		else
		{
			SR.color = Color.white;
		}
	}

	void Movement(){
		
		Vector3 stretch = transform.position - anchorPos;
		Vector3 force = -stiffness * stretch - damping * _velocity;
		Vector3 acceleration = force / mass;

		_velocity += acceleration * Time.deltaTime;
		transform.position += _velocity/100;
	}

	public void AddSpline(Spline s){
		if (!_connectedSplines.Contains (s)) {
			_connectedSplines.Add (s);
		}else{
			Debug.Log("trying to add a spline twice. DONT DO THAT");
		}
	}

	public void AddPoint(Point p){
		if (!_neighbours.Contains (p)) {
			_neighbours.Add (p);
		}else{
			Debug.Log("trying to add a point twice. DONT DO THAT");
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
		while (f < 1)
		{
			Color startColor = color;
			color = Color.Lerp(startColor, Color.white * 0.5f, f);
			f += Time.deltaTime;
			yield return null;
		}
	}

	public float NeighbourCount(){
		return _connectedSplines.Count;
	}

	public void RemoveSpline(Spline s){
		_connectedSplines.Remove (s);
	}

	public void RemovePoint(Point p){
		_neighbours.Remove (p);
	}

	public void TurnOff()
	{
		
	}


	public void Updatecontrollers()
	{
		controller.Step();	
	}
	
	public void TurnOnController()
	{
		if(hasController){
			
				controller.isOn = true;	
				controller.TryToUnlock();
		}
	}
	
	public void TurnOffController()
	{
		if(hasController){
			controller.isOn = false;
			Services.fx.trailParticles.Pause();
		}
	}

	public void SwitchState(PointState s)
	{
		if (s != state)
		{
			switch (s)
			{
				case PointState.locked:
					
					break;

				case PointState.off:
					break;

				case PointState.on:
					TurnOn();
					PointManager.AddPointHit(this);

					break;
			}
		}

		state = s;
	}
	
	public void OnPointEnter()
	{
		proximity = 1;
		timeOnPoint = 0;
		timesHit++;
//		stiffness = Mathf.Clamp(stiffness -100, 100, 10000);
//		damping = Mathf.Clamp(damping - 100, 100, 10000);
		
		if(textMesh != null){
			textMesh.GetComponent<FadeTextOnPoint>().alpha = 1;
		}

		SwitchState(PointState.on);

		TurnOnController();
//		SynthController.instance.noteySynth.NoteOn(24, 1, 1);

		if(pointType != PointTypes.ghost){

			switch (pointType)
			{
				case PointTypes.normal:
					Services.Sounds.PlayPointAttack(Services.PlayerBehaviour.clampedSpeed/10);
					break;
				
				case PointTypes.end:
//					SynthController.instance.bassySynth.NoteOn(29, 1, 1);
					break;
			}
				
				GameObject fx = Instantiate (Services.Prefabs.circleEffect, transform.position, Quaternion.identity);
				fx.transform.parent = transform;
				Services.fx.PlayAnimationAtPosition(FXManager.FXType.pulse, transform);
				
		}

		if (pointType == PointTypes.end)
		{
			
			if (controller.CheckCompleteness())
			{
				
				Services.fx.SpawnSprite(0, transform);
				//Services.Sounds.PlayPointAttack(0.5f);
				Services.fx.EmitRadialBurst(20,Services.PlayerBehaviour.curSpeed + 10, transform);
				Services.fx.PlayAnimationOnPlayer(FXManager.FXType.burst);
				
				if (controller.CheckCompleteness() && SceneController.instance != null && !MapEditor.editing)
				{
					SceneController.instance.LoadNextStellation( 1);	
				}
				
			}
			else
			{
				
				Services.PlayerBehaviour.SwitchState(PlayerState.Animating);
//				Services.main.WarpPlayerToNewPoint(Services.StartPoint);
				Services.fx.ShowUnfinished();
				
			}

//			Services.PlayerBehaviour.Reset();
//			SceneController.instance.LoadNextLevel();
		}
		
	}

	public void AddBoost()
	{
		Services.PlayerBehaviour.flow += Services.PlayerBehaviour.flowAmount * (Services.PlayerBehaviour.boostTimer);
		Services.PlayerBehaviour.boost += boostAmount + Services.PlayerBehaviour.boostTimer;
		Services.fx.PlayAnimationOnPlayer(FXManager.FXType.fizzle);
		Services.fx.EmitRadialBurst(20,Services.PlayerBehaviour.boostTimer + 1 * 5, transform);
		Services.fx.EmitLinearBurst(50, Services.PlayerBehaviour.boostTimer + 1, transform, Services.PlayerBehaviour.cursorDir);
	}
	
	public void OnPointExit(){

	
		TurnOffController();
		
		switch(pointType){
			case PointTypes.stop:
				
				AddBoost();
//				Services.PlayerBehaviour.flow += 0.1f;
			break;

			case PointTypes.fly:
					
				
				
			break;

			case PointTypes.normal:

				
				
				
				if (Services.PlayerBehaviour.buttonPressed)
				{
					AddBoost();
					SynthController.instance.PlayNote(0);
				}

			break;
			
			case PointTypes.start:
				AddBoost();
				Services.Sounds.PlayPointAttack(0.5f);
				break;
			
			case PointTypes.end:
				Services.PlayerBehaviour.boost += boostAmount + Services.PlayerBehaviour.boostTimer;
				break;
		}

		if(pointType != PointTypes.ghost){
			
		}

		
		/*
			if(curPoint.IsOffCooldown()){
			// flow += flowAmount;
			}
			if(Mathf.Abs(flow) < 1){
				boost = boostAmount;
			}
		*/
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
		return _connectedSplines.Count > 0 ? true : false;
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
		c = proximity;
		// accretion
		c = Mathf.Pow (c, 1);
		
//		SR.color = Color.Lerp (color, new Color (1,1,1, c), Time.deltaTime * 5);

		if (state == PointState.on)
		{
			SR.color = _color + color;
		}
		else if (state == PointState.locked)
		{
			SR.color = Color.clear;
			
		}else if (state == PointState.off)
		{
			SR.color = _color + Color.white / 8f;
		}

//		SR.color += Color.white * Mathf.Sin(3 * (Time.time + timeOffset)) / 10;
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
