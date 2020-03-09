using System.Collections;
using System.Collections.Generic;
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
		
		curController = controllers[0];	
		curController.Initialize();
		
		Services.Player.SetActive(true);
		Services.PlayerBehaviour.Initialize();
		
		
		Services.main.fx.Reset();
		Services.main.state = Main.GameState.playing;

	}

	public void EnterStellation(StellationController c)
	{
		curController = c;
		c.Lock(false);
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
