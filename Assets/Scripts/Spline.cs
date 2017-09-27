using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System;

public class Spline : MonoBehaviour {

	public static List<Spline> Splines=new List<Spline>();
	public List<Vector3> LineSegmentPoints;
	public List<Point> SplinePoints;
	public Point PointPrefab;
	public Point Selected; 

	private LineRenderer l;
	public LineRenderer l2;

	public int curveFidelity = 10;
	public float drawSpeed = 6;
	public float distance = 0; 
	public bool closed = false;

	private static string path;
	public static string SavePath
	{
		get
		{
			if(path==null)
			{
				path =System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments)+@"\SplinesSave\";
			}

			return path;
		}
		set
		{
			path = value;
		}
	}

	public int CurveCount {
		get {
			return (SplinePoints.Count - 1) / 3;
		}
	}

	//GAME LOGIC

	void Awake()
	{
		Splines.Add(this);
		l = GetComponent<LineRenderer> ();
		l.positionCount = 0;

//		if (SplinePoints != null) {
//
//			List<Point> TempList = new List<Point> ();
//
//			foreach (Point p in SplinePoints) {
//				TempList.Add (p);
//			}
//
//			SplinePoints.Clear ();
//
//			foreach (Point p in TempList) {
//				AddPoint (p);
//			}
//		}


	}
		
	void OnDestroy()
	{
		Splines.Remove(this);
	}

	void Insert(){
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

	//HELPER FUNCTIONS
	public bool IsNodeOrigin(Point p){
		return (p = SplinePoints[0]);
	}

	public Point StartPoint(){
		if (SplinePoints [0] != null) {
			return SplinePoints [0];
		}
		Debug.Log ("No Start Point on this Spline");
		return null;
	}

	public Point MiddlePoint(){
		if (SplinePoints [1] != null) {
			return SplinePoints [1];
		}
		Debug.Log ("No Middle Point on this Spline");
		return null;
	}

	public Point EndPoint(){
		if (SplinePoints [2] != null) {
			return SplinePoints [2];
		}

		Debug.Log ("No EndPoint on this Spline");
		return null;
	}

	public bool IsPointConnectedTo(Point p){
		return SplinePoints.Contains (p);
	}

	public Vector3 GetPointAtIndex(int i, float t){
		 
		//ADD SUPPORT FOR BACKWARDS/FORWARDS
		//IF FORWARDS, INCREMENT, IF BACKWARDS, DECREMENT ?

		//MAKE THIS SHIT WORK WHEN THERE'S ONLY TWO POINTS
		//Maybe you need to decrement the index by one to force it to be between both splines
		//Obviously you need to set the progress correctly when you know you're facing backwards (start at 1)

		int Count = SplinePoints.Count;

		int j = i-1;
		if (j < 0)
			j = i;

		Point Point1 = SplinePoints[j];

		j = i+1;
		if( j >Count-1)
			j=i;

		Point Point2 = SplinePoints[j];

		j++;
		if (j >Count-1) 
			j = i;

		Point Point3 = SplinePoints[j];

		float tension=SplinePoints[i].tension;
		float continuity=SplinePoints[i].continuity;
		float bias=SplinePoints[i].bias;

		Vector3 r1= 0.5f*(1-tension)*((1+bias)*(1-continuity)*(SplinePoints[i].Pos-Point1.Pos)+ (1-bias)*(1+continuity)*(Point2.Pos-SplinePoints[i].Pos));

		tension=Point2.tension;
		continuity=Point2.continuity;
		bias=Point2.bias;

		Vector3 r2 = 0.5f*(1-tension)*((1+bias)*(1+continuity)*(Point2.Pos-SplinePoints[i].Pos)+ (1-bias)*(1-continuity)*(Point3.Pos-Point2.Pos));
		Vector3 v= GetPoint(t, SplinePoints[i].Pos, Point2.Pos, r1, r2);

		return transform.TransformPoint (v) - transform.position;

	}

	public Vector3 GetPoint(float t) {
		
		int i = SplinePoints.IndexOf (Selected);
		return GetPointAtIndex (i, t);
	
	}


	Vector3 GetPoint(float t,Vector3 p1,Vector3 p2,Vector3 r1,Vector3 r2)
	{
		return p1 * (2.0f*t*t*t - 3.0f*t*t + 1.0f) + r1 * (t*t*t - 2.0f*t*t + t) +
			p2 * (-2.0f*t*t*t + 3.0f*t*t) + r2 * (t*t*t - t*t);
	}

	public Vector3 GetVelocity (float t) {
		int i = SplinePoints.IndexOf (Selected);
		return GetVelocityAtIndex (i, t);
	}


	public Vector3 GetVelocityAtIndex (int i, float t) {
		int Count = SplinePoints.Count;

		int j = i-1;
		if (j < 0)
			j = i;

		Point Point1 = SplinePoints[j];

		j = i+1;
		if( j >Count-1)
			j=i;

		Point Point2 = SplinePoints[j];

		j++;
		if (j >Count-1) 
			j = i;

		Point Point3 = SplinePoints[j];

		float tension=SplinePoints[i].tension;
		float continuity=SplinePoints[i].continuity;
		float bias=SplinePoints[i].bias;

		Vector3 r1= 0.5f*(1-tension)*((1+bias)*(1-continuity)*(SplinePoints[i].Pos-Point1.Pos)+ (1-bias)*(1+continuity)*(Point2.Pos-SplinePoints[i].Pos));

		tension=Point2.tension;
		continuity=Point2.continuity;
		bias=Point2.bias;

		Vector3 r2 = 0.5f*(1-tension)*((1+bias)*(1+continuity)*(Point2.Pos-SplinePoints[i].Pos)+ (1-bias)*(1-continuity)*(Point3.Pos-Point2.Pos));

		Vector3 v= GetFirstDerivative(SplinePoints[i].Pos, Point2.Pos, r1, r2, t);
		v = transform.TransformPoint (v) - transform.position;

		if (v == Vector3.zero && t == 1) {
			v = GetVelocity (0.99f);
		}
		return v;
	}


	Vector3 GetFirstDerivative (Vector3 p1, Vector3 p2, Vector3 r1, Vector3 r2, float t) {

//		return r1 * (1 - 4 * t + 3 * (t *t)) + t * (6 * (p1 - p2) * (-1 + t) + r2 * (-2 + 3 * t));
		return r1 * (1 - 4 * t + 3 * (t*t)) + t * (-6 * p1 + 6*p2 + 6 * p1 * t - 6 * p2 * t + r2 * (-2 + 3 * t));
		//		p2 (3 (t*t) - 2 (t*t*t)) + m (t - 2 t^2 + t^3) + n (-t^2 + t^3) + p1 (1 - 3 t^2 + 2 t^3)
	}

	public Vector3 GetDirection (float t) {
		return GetVelocity(t).normalized;
	}
		
	public int GetPointIndex(Point point){
		foreach (Point p in SplinePoints) {
			if (point == p) {
				return SplinePoints.IndexOf (p);
			}
		}

		return 0;
	}

	public Vector3 GetInitVelocity(Point p){

		return GetVelocityAtIndex(GetPointIndex(p), 0);
	
	}

	public Vector3 GetReversedInitVelocity(Point p){

		//JUST WHAT SHOULD BE GOING ON HERE

		return -GetVelocityAtIndex(GetPointIndex(p), 1);
	}

	public float CompareAngleAtPoint(Vector3 direction, Point p, bool reversed = false){

		if (reversed) {
			return Vector3.Angle (direction, GetReversedInitVelocity (p));
		} else {
			return Vector3.Angle (direction, GetInitVelocity (p));
		}
	}
		
//	public void Create(Point from, Point to){
//
//		Edge e;
//
//		Vector3 v1 = Vector3.zero;
//		Vector3 v2 = Vector3.zero;
//		Vector3 cursorPos = GameObject.Find ("Player").GetComponent<PlayerBehaviour> ().cursor.transform.position;
//
//		float distance = Vector3.Distance (to.transform.position, from.transform.position);
//
//		if(from.HasEdges()){
//			e = from.GetClosestEdgeDirection((cursorPos - from.transform.position).normalized, true);
//			v1 = e.GetReversedInitVelocity (from).normalized * (distance/2); //could times by distance
//			v1 = Vector3.Lerp (v1, (cursorPos - from.transform.position).normalized, 0.25f);
//		}
//		if(to.HasEdges()){
//			e = to.GetClosestEdgeDirection((cursorPos - to.transform.position).normalized, true);
//			v2 = e.GetReversedInitVelocity (to).normalized * (distance/2);
//		}
//
//		curve.CreateCurve (_edgeVertices[0].transform, _edgeVertices[1].transform, distance, v1, v2);
//		mesh.Decorate ();
//	}

	public void SetPointProximity(float progress){
		SplinePoints [0].proximity = 1-progress;
		SplinePoints [1].proximity = progress;
	}

	public void DestroySpline (Point toDelete, Point toAnchor){
		Destroy (this);
		Destroy (l);
		//		transform.position = toAnchor.transform.position;
		//		GameObject ropeEnd = new GameObject ();
		//		ropeEnd.transform.position = toDelete.transform.position;
		//		rope.target = ropeEnd.transform;
		//		rope.enabled = true;
	}

	public void CalculateDistance(){
		int Count = SplinePoints.Count;
		float step = (1.0f / (float)curveFidelity);
		distance = 0;

		for (int k = 1; k < curveFidelity ; k++){

			float t = (float) k/(float)(curveFidelity-1);
			distance += Vector3.Distance (GetPoint(t), GetPoint (t - step));
		}

	}

	public void SetPoints(List<Point> points){
		SplinePoints.Clear ();

		foreach (Point p in points) {
			AddPoint (p);
		}
	}

	public void AddPoint(Point p)
	{
//		p.transform.parent=transform;

		//		p.transform.position=pos;
		//		point.tension=tension;
		//		point.bias=bias;
		//		point.continuity=continuity;

		p.AddSpline (this);
			
		int newIndex = 0;

		if (SplinePoints.Count > 1 && Services.Player.GetComponent<PlayerBehaviour>().curPoint == StartPoint()) {
			SplinePoints.Insert (0, p);
			p.AddPoint (MiddlePoint ());
			MiddlePoint ().AddPoint (p);

		} else {
			newIndex = SplinePoints.Count;
			SplinePoints.Insert (newIndex, p);

			if (newIndex == 1) {
				StartPoint ().AddPoint (p);
				p.AddPoint (StartPoint ());
			} else if (newIndex == 2) {
				MiddlePoint().AddPoint (p);
				p.AddPoint (MiddlePoint ());
			}
		}
			
		Debug.Log (SplinePoints [newIndex] + " at index " + newIndex);
	}

	//FX FUNCTIONS
	public void Draw(){
		StartCoroutine (DrawMeshStart());
	}

	public void ReDraw(){
		StartCoroutine (DrawMesh());
//		StartCoroutine (DrawVelocities());
	}

	IEnumerator DrawVelocities (){
		
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
			

		l2.SetPosition(0, GetPointAtIndex(0,0));

		for (int i = 0; i < Count-1; i++){
			for (int k = 0; k < curveFidelity ; k++){

				int index = (i * curveFidelity) + k;
				float t = (float)k / (float)(curveFidelity-1);

				step = ((float)curveFidelity * t)/ (float)curveFidelity;

				Vector3 v= GetPointAtIndex(i, t);
				LineSegmentPoints.Add(v);

				if(l2.positionCount <= Count * curveFidelity * 3){
					l2.positionCount = l2.positionCount + 3;
				}

				l2.SetPosition (l2.positionCount - 1, GetPointAtIndex (i,step));
				l2.SetPosition (l2.positionCount - 2, GetPointAtIndex (i,step) + GetVelocityAtIndex (i,step));
				l2.SetPosition (l2.positionCount - 3, GetPointAtIndex (i,step));

				yield return new WaitForSeconds(Time.deltaTime * drawSpeed);
			}
		}

	}

	IEnumerator DrawMeshStart (){
		l.positionCount = 1;
		l.SetPosition (0, GetPoint (0));
		float t = 1/curveFidelity;
		int lastIndex = 0;
		int index = 0;

		while (t <= 1) {
			index = (int)(t * curveFidelity);
			float step = (float)index/ (float)curveFidelity;

//			if (index != lastIndex) {
//				lastIndex = index;
//				distance += Vector3.Distance (GetPoint (step - (1/(float)curveFidelity)), GetPoint (step));
//			}
//
			l.positionCount = index + 1;
			l.SetPosition (l.positionCount-1, GetPoint(t));
			t += Time.deltaTime * drawSpeed;
			yield return null;
		}
	}

	IEnumerator DrawMesh(){

		LineSegmentPoints.Clear ();
		if (l.positionCount <= 1) {
			l.positionCount = 1;
		}

		int Count = SplinePoints.Count;
		Vector3 lastPosition = GetPointAtIndex (0, 0);
		l.SetPosition(0, lastPosition);

		for (int i = 0; i < Count-1; i++){
			for (int k = 0; k < curveFidelity ; k++){
				 
				int index = (i * curveFidelity) + k;
				float t = (float)k / (float)(curveFidelity-1);

				Vector3 v= GetPointAtIndex(i, t);
				LineSegmentPoints.Add(v);

				if(l.positionCount < (i * curveFidelity) + k){
					l.positionCount = ((i * curveFidelity) + k);
				}

				l.SetPosition (Mathf.Clamp(index-1, 0, int.MaxValue), v);

				lastPosition = v;

				yield return new WaitForSeconds(Time.deltaTime * drawSpeed);
			}
		}
	}
		
	//IO FUNCTIONS
	public void Save()
	{
		if(!Directory.Exists( Spline.SavePath))
			Directory.CreateDirectory(Spline.SavePath);
		string FileName=SavePath+ name+ ".xml";
		XmlWriterSettings settings = new XmlWriterSettings();
		settings.Indent = true;
		settings.IndentChars = "    ";
		settings.NewLineChars = "\n"; 
		
		
			
		XmlWriter output = XmlWriter.Create(FileName, settings);
		output.WriteStartElement("Spline");
			

		output.WriteStartElement("SplinePoints");
		for (int i = 0; i < SplinePoints.Count; i++)
		{
			output.WriteStartElement("Point");
		
			output.WriteAttributeString("PositionX",SplinePoints[i].Pos.x.ToString());
			output.WriteAttributeString("PositionY",SplinePoints[i].Pos.y.ToString());
			output.WriteAttributeString("PositionZ",SplinePoints[i].Pos.z.ToString());
			output.WriteAttributeString("Tension",SplinePoints[i].tension.ToString());
			output.WriteAttributeString("Bias",SplinePoints[i].bias.ToString());
			output.WriteAttributeString("Continuity",SplinePoints[i].continuity.ToString());
			output.WriteEndElement();
		}
		output.WriteEndElement();
		output.Flush();
		
		output.WriteStartElement("Closed");
		output.WriteAttributeString("bool",closed.ToString());
		output.WriteEndElement();
		output.Flush();
		
		output.WriteStartElement("MaxVerticesCurve");
		output.WriteAttributeString("int",curveFidelity.ToString());
		output.WriteEndElement();
		output.Flush();


		output.WriteEndElement();
		output.Flush();
		
		output.Close();
	}
	
	public static void Load(string name,GameObject SplinePrefab)
	{
		GameObject goSpline=GameObject.Instantiate(SplinePrefab)as GameObject;
		goSpline.name=name.Replace(Spline.SavePath,"");
		goSpline.name=goSpline.name.Remove(goSpline.name.Length-4,4);
		
		Spline spline=goSpline.GetComponent<Spline>();
		
		XmlDocument input = new XmlDocument();
		input.Load(name);

		bool.TryParse(input.DocumentElement.ChildNodes[2].Attributes[0].Value,out spline.closed);
		int.TryParse(input.DocumentElement.ChildNodes[3].Attributes[0].Value,out spline.curveFidelity);
		
		XmlNodeList points= input.DocumentElement.ChildNodes[0].ChildNodes;
		foreach(XmlNode point in points)
		{
			Vector3 pos=new Vector3();
			float.TryParse(point.Attributes[0].Value,out pos.x);
			float.TryParse(point.Attributes[1].Value,out pos.y);
			float.TryParse(point.Attributes[2].Value,out pos.z);
			
			
			float tension;
			float bias;
			float continuity;
			float.TryParse(point.Attributes[3].Value,out tension);
			float.TryParse(point.Attributes[4].Value,out bias);
			float.TryParse(point.Attributes[5].Value,out continuity);
//			spline.AddPoint(pos,tension,bias,continuity);
		}
		
		
	}
	
}
