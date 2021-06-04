using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AudioHelm;
using UnityEngine;

public class StellationManager : MonoBehaviour
{

	public static StellationManager instance;
	[SerializeField] public List<StellationController> controllers;
	public int index;

	public string fileName = "Line1";

	public void Awake()
	{
		instance = this;
	}
	public void Start()
	{
		if (controllers.Count < 1)
		{
			controllers = GetComponentsInChildren<StellationController>().ToList();
		}
		
		for(int i = controllers.Count -1; i >= 0; i--)
		{
			if (i < controllers.Count - 1) 
			{
				controllers[i].unlock = controllers[i + 1];
			}

			controllers[i].Initialize();
			controllers[i].EnableStellation(false);
		}
	
		
		Services.main.activeStellation = controllers[0];	
		Services.main.activeStellation.EnableStellation(true);
		
		Services.main.InitializeLevel();
		
	}

	public void EnterStellation(StellationController c)
	{
		c.isOn = true;
		c.Setup();
		c.start.OnPlayerEnterPoint();
	}
	
	public void CompleteStellation()
	{

		if (Services.main.activeStellation.isComplete)
		{
			
			if (Services.main.activeStellation.hasUnlock)
			{
				Services.mainCam.fieldOfView = 80;
				CameraFollow.instance.desiredFOV = 80;
				CameraFollow.instance.fixedCamera = false;
				Services.main.activeStellation.unlock.EnableStellation(true);
			}
			
			else
			{
				//lets plug in the stellationRecorder here
				//GetComponent<StellationRecorder>().GenerateStellation();
				SceneController.instance.LoadNextStellation();
			}
			//we good
		}
	}
	
	IEnumerator ShowStartPoints(bool on)
	{
		foreach (StellationController s in controllers)
		{
			if (s != Services.main.activeStellation)
			{
				s.SetActive(on);
			}

			yield return new WaitForSeconds(0.25f);
		}		
	}
	
}
