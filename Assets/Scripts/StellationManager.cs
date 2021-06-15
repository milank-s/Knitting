using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AudioHelm;
using UnityEngine;
using UnityEngine.Events;

public class StellationManager : MonoBehaviour
{

	public static StellationManager instance;
	[SerializeField] public List<StellationController> controllers;

	[Space(15)]
	public UnityEvent OnCompleteLap;
	public UnityEvent OnCompleteStellation;
	public UnityEvent OnLeaveStart;
	public UnityEvent OnNextStart;

	[HideInInspector]
	public int index;

	string[] stellations;
	public string fileName = "Line1";

	public void Awake()
	{
		instance = this;
	}

	void CompleteLap(){
		if(OnCompleteLap != null){
			OnCompleteLap.Invoke();
		}
	}

	void ResetToStart(){
		if(OnNextStart != null){
			OnNextStart.Invoke();
		}
	}

	void LeaveStart(){
		if(OnLeaveStart != null){
			OnLeaveStart.Invoke();
		}
	}

	public void Start()
	{
		
		
		if (controllers.Count < 1)
		{
			controllers = GetComponentsInChildren<StellationController>().ToList();
		}

		//first loop. save the file names and whether or not they have been played
		//delete all the stellations and reload their stellations from the appropriate file
		
		for(int i = controllers.Count -1; i >= 0; i--)
		{
			if(PlayerPrefs.HasKey(controllers[i].name)){
				
				Destroy(controllers[i].gameObject);
				StellationController c = MapEditor.instance.Load(controllers[i].name + "Played");
				c.transform.parent = transform;
				controllers[i] = c;
				
				//player has played the level, load from the buffer file
			}else{
				PlayerPrefs.SetInt(controllers[i].name, 1);
				//player hasn't played the level yet. Load from OG and set to 1.

			}
		}

		//second loop, load the stellations and set up any necessary unlocks

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
		SetupActiveStellation(controllers[0], true);
		
		Services.main.InitializeLevel();
		
	}
	
	public void LoadStellationFromFile(){
		//the original stellations should only be loaded the first time the player starts the level
		//after that load a diff one
		
	}

	public void SaveActiveStellation(){

	}
	
	public void ResetActiveStellation(){
		//
	}

	public void SetupActiveStellation(StellationController c, bool active)
	{
		c.isOn = active;

		if(active){
			c.OnCompleteLap += CompleteLap;
			c.OnCompleteStellation += CompleteStellation;
			c.OnLeaveStart += LeaveStart;
			c.OnNextStart += ResetToStart;

			c.Setup();
		}else{
			c.OnCompleteLap -= CompleteLap;
			c.OnCompleteStellation -= CompleteStellation;
			c.OnLeaveStart -= LeaveStart;
			c.OnNextStart -= ResetToStart;
		}
	}
	
	public void CompleteStellation()
	{

		if(OnCompleteStellation != null){
			OnCompleteStellation.Invoke();
		}

		if (Services.main.activeStellation.isComplete)
		{
			if (Services.main.activeStellation.hasUnlock)
			{
			
				Services.main.activeStellation.EnableStellation(false);	
				Services.main.activeStellation.unlock.EnableStellation(true);	
				SetupActiveStellation(Services.main.activeStellation, false);
				SetupActiveStellation(Services.main.activeStellation.unlock, true);
				Services.main.WarpPlayerToNewPoint(Services.main.activeStellation.start);

				//this only applies if we're flying

				// Services.mainCam.fieldOfView = 80;
				// CameraFollow.instance.desiredFOV = 80;
				// CameraFollow.instance.fixedCamera = false;
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
