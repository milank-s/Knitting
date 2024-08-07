﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AudioHelm;
using UnityEngine;
using UnityEngine.Events;

public class StellationManager : MonoBehaviour
{
	public int level;
	public int checkpoint;
	public static StellationManager instance;
	
	public List<StellationGroup> stellationSets;
	List<StellationController> controllers;

	[Space(15)]
	public UnityEvent OnCompleteLap;
	public UnityEvent OnCompleteStellation;
	public UnityEvent OnLeaveStart;
	public UnityEvent OnNextStart;

	public bool save;
	string[] stellations;
	public string fileName = "";

	void Awake()
	{
		instance = this;

		foreach(StellationController c in stellationSets[level].controllers){
			c.Initialize();
		}
	}

	public void Start()
	{		
		Setup();
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

	void Update(){
		if(Input.GetKeyDown(KeyCode.S)){
			OnUnload();
		}
	}

	void LoadSavedStellations(){
		
		//I have no idea what this is about but I think its a way to save connections to file
		//depends if you want persistent connections or not

		if(save){
		for(int i = controllers.Count -1; i >= 0; i--)
		{
			if(PlayerPrefs.HasKey(controllers[i].name)){
				
				//player has played the level, load from the buffer file
				Destroy(controllers[i].gameObject);
				StellationController c = MapEditor.instance.Load(controllers[i].name + "Played");
				c.transform.parent = transform;
				controllers[i] = c;

				
			}else{
				PlayerPrefs.SetInt(controllers[i].name, 1);
				controllers[i].name = controllers[i].name + "Played";
				//player hasn't played the level yet. Set name so that it saves to new file
			}
		}
		}
	}

	void Setup(){
		//this is macro position within the game and should be selected from the main menu
		for(int i = 0; i < stellationSets.Count; i++){
			if(i != level){
				stellationSets[i].gameObject.SetActive(false);
			}
		}
		
		stellationSets[level].gameObject.SetActive(true);
		controllers = stellationSets[level].controllers;
		Services.main.activeStellation = controllers[checkpoint];

		Services.StartPoint = controllers[checkpoint].start;

		//this is pretty fucking heavy duty 
		//there should be a dedicated reset function
		Services.main.InitializeLevel();

		//for now, fuck stellation managers
		// ????????????????????????????????
		//Services.main.EnterLevelRoutine()

		//each stellation set should also have its own checkpoint to place players at the appropriate spot
		//and draw in all previous stellations based on this when resetting
		

		//second loop, load the stellations and set up any necessary unlocks

		List<StellationController> lockedStellations = new List<StellationController>();

		for(int i = 0; i < controllers.Count; i++)
		{
		
			controllers[i].Setup();

			//lock stellations and make sure they're not enabled after
			if(i >= checkpoint && controllers[i].hasUnlock){
				controllers[i].unlock.Lock();
				lockedStellations.Add(controllers[i].unlock);
			}
			
		}
		
		controllers[checkpoint].OnPlayerEnter();

		for(int i = 0; i < controllers.Count; i++){

			//show points but dont draw in splines for unaccessed stellations
			if(!lockedStellations.Contains(controllers[i])){
				controllers[i].Show(true);
			}

			//draw in splines for accessed stellations
			if(i <= checkpoint){
				controllers[i].DrawStellation();
			}
		}
	}

	public void SaveStellation(StellationController c){
		//right now the way the save function uses the point parent and active stellation might cause problems
		MapEditor.instance.Save(c);
	}
	
	public void EnterStellation(StellationController c){

		int index = controllers.IndexOf(c);
		if(index > checkpoint){
			checkpoint = index;
		}
	}

	void EnterStellationGroup(StellationGroup g){

		//gotta save this to player prefs pls

		checkpoint = 0;
		level = stellationSets.IndexOf(g);
		PlayerPrefs.SetInt("level", level);
		PlayerPrefs.SetInt("checkpoint", checkpoint);
	}

	void ExitStellationGroup(){

		DisableStellationGroup();
	}

	void EnableStellationGroup(){


	}

	void DisableStellationGroup(){

	}

	public void ResetToCheckpoint(){

		Setup();
		//warp player and camera to start point?
		//point color player is on is not being reset?

	}

	public void ResetSaves(){
		
		for(int i = controllers.Count -1; i >= 0; i--)
		{
			PlayerPrefs.DeleteKey(controllers[i].name);
		}
	}

	public void OnUnload(){
		foreach(StellationController c in controllers){
			MapEditor.instance.Save(c);
		}
	}

	// //old method for sequential levels and unlockable unicursal stellations
	// public void CompleteStellation()
	// {

	// 	if(save){
	// 		SaveStellation(Services.main.activeStellation);
	// 	}

	// 	if (Services.main.activeStellation.isComplete)
	// 	{
	// 		if (Services.main.activeStellation.hasUnlock)
	// 		{
	// 			// Services.main.activeStellation.Disable();
	// 			// Services.main.activeStellation.unlock.Enable();		
	// 			//Services.main.WarpPlayerToNewPoint(Services.main.activeStellation.start);

	// 			//this only applies if we're flying
	// 		}
			
	// 		else
	// 		{
	// 			//lets plug in the stellationRecorder here
	// 			//GetComponent<StellationRecorder>().GenerateStellation();
	// 			if(OnCompleteStellation.GetPersistentEventCount() > 0){
	// 				OnCompleteStellation.Invoke();
	// 			}else{
	// 				SceneController.instance.LoadNextStellation();
	// 			}
	// 		}
	// 		//we good
	// 	}
	// }

}
