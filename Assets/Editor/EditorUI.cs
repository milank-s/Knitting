using UnityEngine;
using System.Collections;
using System.IO;


public class EditorUI : MonoBehaviour {

	public GameObject SplinePrefab;
	public GameObject NewSplinePrefab;
	public GUISkin Skin;

	public Rect windowPointRect = new Rect(10, 30, 400, 230);
	public	Rect windowSplineRect = new Rect(10, 270, 400, 160);
	public	Rect windowListSplineRect = new Rect(10, 400, 400, 200);
	public	Rect windowLoadRect = new Rect(10, 650, 400, 200);

	Vector2 scrolLoadSpline;
	Vector2 scrolListSpline;


	void WindowPoint(int windowID)
	{

		GUILayout.BeginHorizontal();
		GUILayout.Label("Bias:",GUILayout.Width(65));
		float.TryParse(GUILayout.TextField(Point.Select.bias.ToString(),GUILayout.Width(50)),out Point.Select.bias);
		Point.Select.bias =GUILayout.HorizontalSlider(Point.Select.bias,-2, 2);
		GUILayout.EndHorizontal();
		GUILayout.Space(5);

		GUILayout.BeginHorizontal();
		GUILayout.Label("Continuity:",GUILayout.Width(65));
		float.TryParse(GUILayout.TextField(Point.Select.continuity.ToString(),GUILayout.Width(50)),out Point.Select.continuity);
		Point.Select.continuity =GUILayout.HorizontalSlider(Point.Select.continuity,-2, 2);
		GUILayout.EndHorizontal();
		GUILayout.Space(5);

		GUILayout.BeginHorizontal();
		GUILayout.Label("Tension:",GUILayout.Width(65));
		float.TryParse(GUILayout.TextField(Point.Select.tension.ToString(),GUILayout.Width(50)),out Point.Select.tension);
		Point.Select.tension =GUILayout.HorizontalSlider(Point.Select.tension,-2, 2);
		GUILayout.EndHorizontal();
		GUILayout.Space(5);

		GUILayout.BeginHorizontal();
		GUILayout.Label("Position: ");
		Vector3 pos=Point.Select.transform.position;
		GUILayout.Label("X");
		float.TryParse(GUILayout.TextField(pos.x.ToString(),GUILayout.Width(windowPointRect.width/4-30)),out pos.x);

		GUILayout.Label("Y");
		float.TryParse(GUILayout.TextField(pos.y.ToString(),GUILayout.Width(windowPointRect.width/4-30)),out pos.y);

		GUILayout.Label("Z");
		float.TryParse(GUILayout.TextField(pos.z.ToString(),GUILayout.Width(windowPointRect.width/4-30)),out pos.z);
		GUILayout.EndHorizontal();

		Point.Select.transform.position=pos;
		GUILayout.Space(5);

		if(GUILayout.Button("Delete"))
		{
			if(Spline.Select.SplinePoints.Count>3)
			{
				Spline.Select.SplinePoints.Remove(Point.Select);
				Destroy( Point.Select.gameObject);
			}
		}

		if(GUILayout.Button("Reset"))
		{
			Point.Select.continuity=0;
			Point.Select.tension=0;
			Point.Select.bias=0;
		}
		GUI.DragWindow();
	}


	void WindowSpline(int windowID)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label("Name: ",GUILayout.Width(40));
		Spline.Select.name=GUILayout.TextField(Spline.Select.name);
		GUILayout.EndVertical();


		GUILayout.BeginHorizontal();
		GUILayout.Label("Max Vertices Curve:",GUILayout.Width(120));
		Spline.curveFidelity = (int)GUILayout.HorizontalSlider(Spline.curveFidelity,3, 100);
		GUILayout.EndVertical();

		Spline.Select.closed = GUILayout.Toggle( Spline.Select.closed," Closed");

//		Spline.Select.DrawLine = GUILayout.Toggle(Spline.Select.DrawLine," Draw Line");

		GUI.DragWindow();

	}


	void WindowLoad(int windowID)
	{

		//GUILayout.BeginHorizontal();
		GUILayout.Label("Path: "+Spline.SavePath);
		//Spline.SavePath=GUILayout.TextField(Spline.SavePath);
		//GUILayout.EndVertical();
		//return;
		//return;
		if(!Directory.Exists( Spline.SavePath))
			return;
		scrolLoadSpline=GUILayout.BeginScrollView(scrolLoadSpline);
		foreach(string file in  Directory.GetFiles(Spline.SavePath,"*.xml"))
		{

			GUILayout.BeginHorizontal();
			string name=file.Replace(Spline.SavePath,"");
			name=name.Remove(name.Length-4,4);
			GUILayout.Box(name);

			if(GUILayout.Button("Load"))
			{
				Spline.Load(file,SplinePrefab);
			}

			if(GUILayout.Button("Delete"))
			{
				File.Delete(file);
			}
			GUILayout.EndHorizontal();
		}
		GUILayout.EndScrollView();
		GUI.DragWindow();

	}


	void WindowListSpline(int windowID)
	{
		scrolListSpline=GUILayout.BeginScrollView(scrolListSpline);
		foreach(var spline in Spline.Splines)
		{

			GUILayout.BeginHorizontal();
			if(spline.isSelect)
				Skin.button.normal.textColor=Color.green;
			else
				Skin.button.normal.textColor=Color.white;

			//GUILayout.Box(spline.name,Skin.box);

			if(GUILayout.Button(spline.name,Skin.button))
			{
				Spline.Select=spline;
			}
			if(GUILayout.Button("Delete"))
			{
				Destroy( spline.gameObject);
			}

			if(GUILayout.Button("Save"))
			{
				spline.Save();
			}
			GUILayout.EndHorizontal();
		}
		GUILayout.EndScrollView();
		GUILayout.Space(10);
		GUI.DragWindow();

	}
	void OnGUI ()
	{
		if(Point.Select)
		{
			windowPointRect=GUI.Window(0, windowPointRect, WindowPoint, "Select Point");
		}
		if(Spline.Select)
		{
			windowSplineRect=GUI.Window(1, windowSplineRect, WindowSpline, "Select Spline: "+Spline.Select.name);
		}
		windowListSplineRect=GUI.Window(2, windowListSplineRect, WindowListSpline, "List Spline ");


		if(GUI.Button(new Rect(0,0,100,20),"New Spline"))
		{
			GameObject.Instantiate(NewSplinePrefab).name="New Spline";
		}

	}
}
