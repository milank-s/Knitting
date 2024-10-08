using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Shapes{BOX, CIRCLE, POLYGON, SPIRAL, WAVE, BRAID, KNOT}
public class SplineTurtle : MonoBehaviour {

	public MapEditor editor;

	[Header("UI")] 
	
	
	[SerializeField] private ReadSliderValue numPointsUI;
	[SerializeField] private ReadSliderValue distUI;
	[SerializeField] private ReadSliderValue distDeltaUI;
	[SerializeField] private ReadSliderValue distScaleUI;
	[SerializeField] private ReadSliderValue angleeUI;
	[SerializeField] private ReadSliderValue angleDeltaUI;
	[SerializeField] private ReadSliderValue angleScaleUI;
	
	[SerializeField] private ReadSliderValue continuityUI;
	[SerializeField] private ReadSliderValue tensionUI;
	
	[SerializeField] private ReadSliderValue pivotAngleUI;
	[SerializeField] private ReadSliderValue pivotDistanceUI;
	[SerializeField] private ReadSliderValue startDirUI;

	[SerializeField] private InputField xOffsetUI;
	[SerializeField] private InputField yOffsetUI;
	[SerializeField] private InputField zOffsetUI;

	[SerializeField] private ReadToggleValue zigzagUI;
	[SerializeField] private ReadToggleValue randomlyZagUI;
	[SerializeField] private ReadToggleValue closeToggle;
	[SerializeField] private ReadToggleValue connectUI;
	[SerializeField] private ReadToggleValue ghostToggle;
	
	
    public Dropdown shapeTypes;

	public static float maxTotalPoints = 1000;
	public static float maxCrawlers = 1;

	public string name;

	int pointCount;
	public List<Point> points;
	public List<Spline> splines;

	public bool createSplines;
	public bool Randomize;

	public float startDir;
	public Vector3 startPos;
	public int initialAmount;
	public float initialAngleMax;
	public float initialAngleMin;

	public Transform pivot;
	public Transform turtle;
	
	public float stepSpeed = 0.1f;
	
	public float angleChange = 0;
	public float angleVariance = 10;
	public float angle = 30;
	public float scaleChange = 0;
	public float distDelta = 2;
	public float dist = 1;
	public float branchFactor = 0;
	public int maxPoints = 50;
	public float continuity = 0;
	public float tension;
	public bool Raycast = true;
	
	public bool alternateAngle = false;

	private float angleRandom;
	private float ang;
	private float mxDist;
	private float mDist;

	
	public float  PivotSpeed;
	public bool closed;
	public bool childrenInherit = false;
	private bool turnleft = true;

	public bool ghostPoints;
	
	private bool running;
	private bool redraw;
	public bool updatePoints;
	private bool randomlyZag;
	
	public Vector3 offsetDirection = Vector3.zero;

	private float timeSinceRedraw;
	
	
	private Coroutine drawing;
	
	
	Mesh mesh;
	Spline curSpline;
	public Point curPoint;

	public void Start(){
		foreach(Shapes c in System.Enum.GetValues(typeof(Shapes))){
            string label = System.Enum.GetName(typeof(Shapes), (int)c);
            shapeTypes.options.Add(new Dropdown.OptionData(label));
        }
		ChangeShapePreset(Shapes.BOX);
	}

	public void Clear(){
		if(points.Count > 0){
			for(int i = points.Count-1; i >= 0; i--){
				editor.DeletePoint(points[i]);
			}
		}

		Reset();
	}

	public void ChangeShapePreset(int i){
		ChangeShapePreset((Shapes)i);
		UpdateValues();
		Generate();
	}

	public void ToggleClosed(){
		closed = !closed;
		closeToggle.SetValue(closed);
		if(splines.Count > 0 && splines[0] != null){
			splines[0].closed = closed;
		}
	}
	public void ChangeShapePreset(Shapes s){
		
		//defaults
		distUI.ChangeValue(1);
		distDeltaUI.ChangeValue(0);
	 	distScaleUI.ChangeValue(1);
		angleDeltaUI.ChangeValue(0);
		angleScaleUI.ChangeValue(1);
	
		pivotAngleUI.ChangeValue(0);
		pivotDistanceUI.ChangeValue(0);
		startDirUI.ChangeValue(0);

		xOffsetUI.SetTextWithoutNotify("0");
		yOffsetUI.SetTextWithoutNotify("0");
		zOffsetUI.SetTextWithoutNotify("0");

		zigzagUI.SetValue(false);
		connectUI.SetValue(false);
		randomlyZagUI.SetValue(false);
		continuityUI.ChangeValue(0);

		switch(s){
			case Shapes.BOX:
				angleeUI.ChangeValue(90);
				numPointsUI.ChangeValue(4);
				closeToggle.SetValue(true);
				tensionUI.ChangeValue(1);

			break;

			case Shapes.CIRCLE:
				
				angleeUI.ChangeValue(90);
				numPointsUI.ChangeValue(4);
				closeToggle.SetValue(true);
				tensionUI.ChangeValue(-0.66f);

			break;

			case Shapes.SPIRAL:
				
				angleeUI.ChangeValue(90);
				numPointsUI.ChangeValue(20);
				closeToggle.SetValue(false);
				tensionUI.ChangeValue(-0.5f);
	 			distScaleUI.ChangeValue(0.95f);

			break;

			case Shapes.POLYGON:
				
				angleeUI.ChangeValue(90);
				numPointsUI.ChangeValue(20);
				closeToggle.SetValue(false);
				tensionUI.ChangeValue(-0.5f);
	 			distScaleUI.ChangeValue(0.95f);

			break;


			case Shapes.WAVE:
				
				angleeUI.ChangeValue(120);
				numPointsUI.ChangeValue(10);
				closeToggle.SetValue(false);
				tensionUI.ChangeValue(-0.66f);
				zigzagUI.SetValue(true);
				distUI.ChangeValue(0.25f);

			break;

			case Shapes.BRAID:
				
				xOffsetUI.SetTextWithoutNotify("0.066");
				angleeUI.ChangeValue(90);
				numPointsUI.ChangeValue(20);
				closeToggle.SetValue(false);
				tensionUI.ChangeValue(-0.5f);
				distUI.ChangeValue(0.25f);

			break;

			case Shapes.KNOT:
				
				xOffsetUI.SetTextWithoutNotify("0.066");
				angleeUI.ChangeValue(90);
				numPointsUI.ChangeValue(28);
				closeToggle.SetValue(true);
				tensionUI.ChangeValue(-0.5f);
				distUI.ChangeValue(0.25f);
				pivotAngleUI.ChangeValue(-13);
				

			break;

		}
	}

	//for toggling between play modes
	public void Reset()
	{
		points.Clear();
		splines.Clear();
		pointCount = 0;
		turtle.position = startPos;
		turtle.rotation = Quaternion.Euler(0, 0,startDir);
		pivot.position = turtle.position + Vector3.up * pivotDistanceUI.val;

	}

	public void SetPosition(Vector3 pos){
		startPos = pos;
	}

	public void Generate()
	{
		Clear();
		
		if (Randomize) {

			initialAngleMax = Random.Range(-90, 45);
			initialAngleMin = Random.Range(initialAngleMax, 90);

			angleChange = Random.Range (0,3);
			angleVariance = Random.Range (-90, 10);
			angle = Random.Range (angleVariance, 90);
			scaleChange = Random.Range (0.98f, 1.02f);
			if (Random.Range (0, 100) < 90) {
				distDelta = Random.Range (1f, 2f);
				dist = Random.Range (1, distDelta);
				maxPoints = Random.Range (5, 10);
				initialAmount = 1;
			} else {
				distDelta = Random.Range (3f, 5f);
				dist = Random.Range (2, distDelta);
				initialAmount = Random.Range (20,25);
				maxPoints = Random.Range (8, 10);
			}
			branchFactor = Random.Range(0,0);
			continuity = Random.Range(0,2);

			alternateAngle = Random.Range (0f, 100f) > 50 ? true : false;
			PivotSpeed = Random.Range (0f, 2f);
		}
		
		InitializeSpline();
	}

	public void UpdateTurtle()
	{
		UpdateValues();

		if (redraw)
		{
			timeSinceRedraw += Time.deltaTime;
			Debug.Log("redrawing");

			Generate();
			redraw = false;
		}

		if(updatePoints){
			ChangePointSettings();
			updatePoints = false;
		}
	}

	public void RedrawTurtle()
	{
		Debug.Log("redrawing turtle");
		redraw = true;
		timeSinceRedraw = 0;
	}

	void UpdateValues()
	{
		startDir = startDirUI.val;
		closed = closeToggle.val;
		alternateAngle = zigzagUI.val;
		randomlyZag = randomlyZagUI.val;
		Raycast = connectUI.val;
		maxPoints = (int)numPointsUI.val;
		dist = distUI.val;
		distDelta = distDeltaUI.val;
		scaleChange = distScaleUI.val;
		
		angle = angleeUI.val;
		angleVariance = angleDeltaUI.val;
		angleChange = angleScaleUI.val;

		ghostPoints = ghostToggle.val;
		continuity = continuityUI.val;
		tension = tensionUI.val;

		pivot.position = editor.splinesParent.position + Vector3.up * pivotDistanceUI.val;
		PivotSpeed = pivotAngleUI.val;
		
		float.TryParse(xOffsetUI.text, out offsetDirection.x);
		float.TryParse(yOffsetUI.text, out offsetDirection.y);
		float.TryParse(zOffsetUI.text, out offsetDirection.z);
	}
	
	public void Draw(){
		
		for(int i = pointCount; i < maxPoints; i++) {
			Step ();
			NewPoint ();
		}

		if (createSplines && closed) {

			SplinePointPair spp;

			spp = SplineUtil.ConnectPoints (curSpline, curSpline.SplinePoints[curSpline.SplinePoints.Count-1], curSpline.SplinePoints[0]);

			AddSpline(spp.s);
			
			curSpline = spp.s;
			curPoint = spp.p;
			curPoint.transform.parent = editor.pointsParent.transform;
			curSpline.transform.parent = editor.splinesParent;		
		}

		if (maxCrawlers < 100) {
			for (int i = 0; i < initialAmount; i++) {
				SpawnTurtle ().transform.Rotate (0, 0, turtle.eulerAngles.z + Random.Range (initialAngleMin, initialAngleMax) * i);
			}
		}
		
		running = false;
	}

	public void Complete(){
		//add it to selection
		foreach(Point p in points){
			editor.AddSelectedPoint(p);
		}
		
		foreach(Spline s in splines){
			editor.AddSelectedSpline(s, true);
		}

		Reset();
	}

	void AddSpline(Spline s){
		if(!splines.Contains(s)){
			splines.Add(s);
			editor.AddSpline(s);
		}
	}
	Point CreatePoint(){
		Point p = SplineUtil.CreatePoint (turtle.position);
		p.transform.parent = editor.pointsParent.transform;
		editor.controller.AddPoint(p);
		points.Add(p);
		pointCount ++;
		return p;
	}
	void InitializeSpline(){
		
		//parent.name = name;
		
		ang = angle;
		angleRandom = angleVariance;
		mxDist = distDelta;
		mDist = dist;

		curPoint = NewPoint(false);
		
		Step();
		NewPoint();


		if (ghostPoints)
		{
			curPoint.SetPointType(PointTypes.ghost);
		}

		curSpline.transform.parent = editor.splinesParent;
		Draw();
	}

	public GameObject SpawnTurtle(){
		GameObject newTurtle = Instantiate (gameObject, turtle.position, Quaternion.Euler(turtle.eulerAngles));

		SplineTurtle newTurtleScript = newTurtle.GetComponent<SplineTurtle> ();

		newTurtle.transform.Rotate (0,0,Random.Range (initialAngleMin, initialAngleMax));

		if (!childrenInherit) {
			newTurtleScript.angle = angle;
			newTurtleScript.angleVariance = angleVariance;
			newTurtleScript.distDelta = distDelta;
			newTurtleScript.dist = dist;
		}
		maxCrawlers++;
		// newTurtleScript.Generate();
		return newTurtle;
	}

	public void ChangeClosed(){
		if(curSpline != null){
			curSpline.closed = closed;
		}
	}

	public void ChangePointAmount(){
		if(!running){
			if(pointCount < maxPoints){
				Draw();
			}else if(pointCount > maxPoints){
				

				for(int i = 0; i < pointCount - maxPoints; i++){
					pointCount --;
					Point p = points[points.Count - 1 -i];
					points.Remove(p);
					Services.main.editor.DeletePoint(p);
				}

				if(points.Count > 0){
					curPoint = points[points.Count -1];
					turtle.transform.position = curPoint.Pos;
					turtle.transform.up = curPoint.transform.up;
				}else{
					curPoint = null;
				}
			}

		}
	}
	public void ChangePointSettings(){
		foreach(Point p in Point.Points){
			p.continuity = continuity;
			p.tension = tension;

			if(p._connectedSplines.Count > 1){
				p.SetPointType(PointTypes.stop);
			}else if(ghostPoints){
				p.SetPointType(PointTypes.ghost);
			}else{
				p.SetPointType(PointTypes.normal);
			}
		}
	}

	public Point NewPoint(bool makeSpline = true){

		if (Random.Range (0f, 100f) < branchFactor) {
			if (maxTotalPoints < 100) {
				SpawnTurtle ();
			}
		}

		SplinePointPair spp;

		Point newPoint = null;

		if (Raycast) {
			newPoint = SplineUtil.RaycastDownToPoint (turtle.position, Mathf.Infinity, 100f);
			//ignore points directly before us
			if(curPoint != null && newPoint == curPoint){
				newPoint = null;
			}
		}

		if(newPoint == null) {

			newPoint = CreatePoint ();
			newPoint.continuity = continuity;
			newPoint.tension = tension;
		}

		if (createSplines && makeSpline) {
			spp = SplineUtil.ConnectPoints (curSpline, curPoint, newPoint);
			
			AddSpline(spp.s);

			curSpline = spp.s;
			curPoint = spp.p;
			curPoint.transform.parent = editor.pointsParent.transform;
			curSpline.transform.parent = editor.splinesParent;

		} else {
			newPoint.transform.parent = editor.pointsParent.transform;
		}

		if (ghostPoints)
		{
			newPoint.SetPointType(PointTypes.ghost);
		}

		return newPoint;

	}

	public void Rotate(){
		float rotation;
		//if (LockAngle) {
			if (alternateAngle) {
				if (turnleft) {
					rotation = -ang;
					turnleft = !turnleft;
				} else {
					rotation = ang;
					turnleft = !turnleft;
				}
			}else if (randomlyZag)
			{
				if (Random.Range(0, 100) < 50) {
					rotation = -ang;
					turnleft = !turnleft;
				} else {
					rotation = ang;
					turnleft = !turnleft;
				}
			}
			else
			{
				rotation = ang;
			}
		//} else {
			rotation = rotation + Random.Range (-angleRandom/2f, angleRandom/2f);
		//}

		ang *= angleChange;
		if (Mathf.Abs (angleRandom) > angleVariance) {
//			angleChange = -angleChange;
//			angleRandom = angleRandom % angleVariance;
		}
		if (Mathf.Abs (ang) > angle) {
//			ang = angle % angle;
//			angleChange = -angleChange;
		}

		turtle.Rotate (0, 0, rotation);
	}

	void Step(){
		
		maxTotalPoints++;

		Rotate ();

		turtle.RotateAround (pivot.position, Vector3.forward, PivotSpeed);
		
		float moveDistance = mDist + Random.Range (-mxDist, mxDist);
		mDist *= scaleChange;
		mxDist *= scaleChange;
		turtle.localPosition += turtle.up * moveDistance + offsetDirection;
		
	}
}
