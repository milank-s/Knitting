using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.WSA;

public class StellationController : MonoBehaviour {

	public enum UnlockType{laps, speed, time}
	public enum UnlockMechanism{unlockSpline, turnOnSpline, unlockPoints, switchPointTypes}
	
	public List<UnlockMechanism> unlockActions = new List<UnlockMechanism>() {UnlockMechanism.unlockSpline};
	
	List<ActivatedBehaviour>  activateOnCompletion = new List<ActivatedBehaviour>();
	
	public UnlockType unlockMethod;
	[HideInInspector]
	public List<Point> _pointshit;
	[HideInInspector]
	public List<Point> _points;
	
	public Spline unlock;

	public int score;
	public float scoreCount;
	
	[Space(10)]
	[Header("Point Physics")]
	public bool isKinematic;
	public float damping = 600f;
	public float stiffness = 100f;
	public float mass = 50f;
	[Space(10)]

	[Header("Visuals")]
	public TextMesh title;
	public SpriteRenderer image;
	public bool fixedCam = false;
	public int desiredFOV = 30;
	public TextAsset text;
	[Space(10)]

	
	[HideInInspector]
	public bool isOn;
	public bool isComplete;
	private string[] words;
	private int wordIndex;
	private float fade;
	private bool hasUnlock;
	public string GetWord (){
		wordIndex++;
		return words[(wordIndex-1) % (words.Length)];
	}

	public void Awake()
	{
		_points = new List<Point>();
	}
	public void Initialize()
	{
		
		scoreCount = 0;
		
		if (unlockMethod == UnlockType.laps && score == 0)
		{
			score = 1;
		}
		
		foreach(Point p in GetComponentsInChildren<Point>()){
			
			//expensive but easy
			if (!_points.Contains(p))
			{
				p.hasController = true;
				p.controller = this;
				_points.Add(p);
			}
		}

		if (_points.Count == 0)
		{
			return;
		}
		
		if(unlock){
			unlock.LockSpline(true);
			hasUnlock = true;
		}

		foreach (ActivatedBehaviour a in GetComponentsInChildren<ActivatedBehaviour>())
		{
			activateOnCompletion.Add(a);

		}
		if(image != null){
			image.color = new Color(0,0,0,0);
		}

		if(title != null){
			title.color = new Color(0,0,0,0);
		}

		_pointshit = new List<Point>();

		//ffs
		
		wordIndex = 0;
		if(text != null){
			words = text.text.Split (new char[] { ' ' });
		}

		CameraFollow.instance.fixedCamera = fixedCam;
		CameraFollow.instance.desiredFOV = desiredFOV;
		
		if (fixedCam)
		{
			SetCameraBounds();
		}
	
		

		Services.main.state = Main.GameState.playing;
}

	public bool CheckCompleteness()
	{

		int minLaps = 1000;
		bool isDone = true;
		foreach (Point p in _points)
		{

			if (p.timesHit < score)
			{
				if (p.timesHit < minLaps)
				{
					minLaps = p.timesHit;
				}
				
				if (minLaps > scoreCount)
				{
					scoreCount = minLaps;
				}
				
				isDone = false;
				break;
			}
		}
		
		return isDone;
	}

	public void Reset()
	{
		Services.main.InitializeLevel();
		scoreCount = score;
	}
	public void Step()
	{
		
			if (isOn)
			{

				Services.main.fx.readout.transform.position = Services.main.Player.transform.position;
				
				if (isComplete)
				{
					List<Vector3> positions = new List<Vector3>();
					
//					foreach (Point p in _points)
//					{
//						//p.Contract();
//					}

					//Services.fx.DrawLine();
				}
				else
				{
					if (unlockMethod == UnlockType.speed)
					{
						Services.fx.readout.text = (Services.PlayerBehaviour.flow - score).ToString("F2");
					
					}else if (unlockMethod == UnlockType.time)
					{
						Services.fx.readout.text = (score - scoreCount).ToString("F2");
						scoreCount += Time.deltaTime;

						if (score - scoreCount <= 0)
						{
							Reset();
						}
					
					}else if (unlockMethod == UnlockType.laps){
					
						//Services.fx.readout.text = scoreCount.ToString("F0") + "/" + score.ToString("F0");
						Services.fx.readout.text = "";
					}

				}
				
			
				if(title != null){
					title.color = Color.Lerp(title.color, new Color (1, 1, 1, 1), Time.deltaTime);
				}
				fade = Mathf.Clamp(fade + Time.deltaTime/20, 0, 0.1f);
			} else {
				if(title != null){
					title.color = Color.Lerp(title.color, new Color (1, 1, 1, 0), Time.deltaTime);
				}
				fade = Mathf.Clamp01(fade - Time.deltaTime/10);
			}
			
			if(image != null && isComplete){
				image.color = new Color (1, 1, 1, fade);
			}

		
	}
	public void TryToUnlock()
	{
		
		if (!isComplete)
		{
			
			bool complete = false;
			switch (unlockMethod)
			{
			case UnlockType.laps:
				complete = CheckCompleteness();
				break;
			
			case UnlockType.speed:
				complete = CheckSpeed();
				break;
			}
			
			
			if (complete)
			{
				if(hasUnlock)
				unlock.LockSpline(false);
				
				for (int i = 0; i < activateOnCompletion.Count; i++)
				{
					activateOnCompletion[i].DoBehaviour();
				}

				foreach (Point p in _points)
				{
					p.TurnOn();
				}
				isComplete = true;
			}
		}
	
	}

	public void SetCameraBounds()
	{
		
			int index = 0;

			Vector3 lowerLeft = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
			Vector3 upperRight = new Vector3(-Mathf.Infinity, -Mathf.Infinity, -Mathf.Infinity);

			foreach (Point p in _points)
			{
				if (p.Pos.x > upperRight.x)
				{
					upperRight.x = p.Pos.x;
				}

				if (p.Pos.x < lowerLeft.x)
				{
					lowerLeft.x = p.Pos.x;
				}

				if (p.Pos.y > upperRight.y)
				{
					upperRight.y = p.Pos.y;
				}

				if (p.Pos.y < lowerLeft.y)
				{
					lowerLeft.y = p.Pos.y;
				}

				if (p.Pos.z > upperRight.z)
				{
					upperRight.z = p.Pos.z;
				}

				if (p.Pos.z < lowerLeft.z)
				{
					lowerLeft.z = p.Pos.z;
				}
			}

			Vector3 center = Vector3.Lerp(lowerLeft, upperRight, 0.5f);

			float height = Mathf.Abs(upperRight.y - lowerLeft.y);
			float fov = CameraDolly.FOVForHeightAndDistance(height, -CameraFollow.instance.offset.z) + 10f;

		
			CameraFollow.instance.desiredFOV = fov;
			CameraFollow.instance.WarpToPosition(center);
			
			//get center position and fov
	}
	
	public bool CheckSpeed()
	{
		if (Services.PlayerBehaviour.flow > score)
		{
			return true;
		}
		else
		{
			return false;
		}
	}
}
