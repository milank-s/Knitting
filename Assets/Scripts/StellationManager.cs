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
			controllers[i].Lock(true);
		}
	
		
		Services.main.activeStellation = controllers[0];	
		Services.main.activeStellation.Lock(false);
		Services.main.activeStellation.isOn = true;
		
		Services.main.InitializeLevel();
		
	}

	public void EnterStellation(StellationController c)
	{
		c.Lock(false);
		c.isOn = true;
		c.EnterStellation();
		c.start.OnPointEnter();
	}
	
	public void EnableStellations(bool on)
	{
		StartCoroutine(ShowStartPoints(on));	
	}

	public void EnableStellation(StellationController s)
	{
		s.SetActive(true);
	}

	public void CompleteStellation(StellationController c)
	{
		EnableStellation(c.unlock);
				
		Services.mainCam.fieldOfView = 80;
		CameraFollow.instance.desiredFOV = 80;
		CameraFollow.instance.fixedCamera = false;
		
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
	public void EndScene()
	{

		
	}

}
