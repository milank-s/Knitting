using System.Collections;
using System.Collections.Generic;
using AudioHelm;
using UnityEngine;

public class StellationManager : MonoBehaviour
{

	public static StellationManager instance;
	[SerializeField] public List<StellationController> controllers;
	public StellationController curController;
	public int index;


	public void Awake()
	{
		instance = this;
	}
	public void Start()
	{
		
	

		foreach (StellationController c in controllers)
		{
			c.Initialize();
			c.Lock(true);
		}
	
		curController = controllers[0];	
		curController.Lock(false);
		Services.StartPoint = curController.start;
		curController.isOn = true;
		
		Services.main.InitializeLevel();

	}

	public void EnterStellation(StellationController c)
	{
		curController = c;
		c.Lock(false);
		EnableStellations(false);
	}
	
	public void EnableStellations(bool on)
	{
		foreach (StellationController s in controllers)
		{
			if (s != curController)
			{
				s.SetActive(on);
			}
		}
	}
	
	public void EndScene()
	{

		
	}

}
