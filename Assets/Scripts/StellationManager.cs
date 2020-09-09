using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
		if (controllers.Count < 1)
		{
			controllers = GetComponentsInChildren<StellationController>().ToList();
		}
		
		for(int i = controllers.Count -1; i >= 0; i--){
			controllers[i].Initialize();
			controllers[i].Lock(true);
		}
	
		curController = controllers[0];	
		curController.Lock(false);
		curController.isOn = true;
		
		Services.main.InitializeLevel();
		
	}

	public void EnterStellation(StellationController c)
	{
		curController = c;
		c.Lock(false);
		EnableStellations(false);
		c.Initialize();
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

	IEnumerator ShowStartPoints(bool on)
	{
		foreach (StellationController s in controllers)
		{
			if (s != curController)
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
