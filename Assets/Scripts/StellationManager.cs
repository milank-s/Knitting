using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AudioHelm;
using UnityEngine;
using UnityEngine.Events;

public class StellationManager : MonoBehaviour
{

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
	public void Start()
	{
		//this is macro position within the game and should be selected from the main menu

		stellationSets[checkpoint].gameObject.SetActive(true);
		controllers = stellationSets[checkpoint].controllers;

		//each stellation set should also have its own checkpoint to place players at the appropriate spot
		//and draw in all previous stellations based on this when resetting
		

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

		//second loop, load the stellations and set up any necessary unlocks

		List<StellationController> lockedStellations = new List<StellationController>();

		for(int i = controllers.Count -1; i >= 0; i--)
		{
			// if (i < controllers.Count - 1) 
			// {
			// 	controllers[i].unlock = controllers[i + 1];
			// }

			controllers[i].Setup();
			if(controllers[i].hasUnlock){
				controllers[i].unlock.Disable();
				lockedStellations.Add(controllers[i].unlock);
			}
			// controllers[i].EnableStellation(true);
		}

		for(int i = 0; i < controllers.Count; i++){
			if(!lockedStellations.Contains(controllers[i])){
				controllers[i].ShowStellation(true);
			}
		}
		
		Services.main.activeStellation = controllers[0];
			
		//DRAWING IN IS ONLY WORKING WHEN CALLED FROM HERE
		//Services.main.activeStellation.EnableStellation(true);
		SetStellation(controllers[0]);
		
		Services.main.InitializeLevel();
	}

	public void SaveStellation(StellationController c){
			//right now the way the save function uses the point parent and active stellation might cause problems
			MapEditor.instance.Save(c);
	}
	
	public void ReachCheckpoint(){

	}

	public void ResetCheckpoint(){

	}
	public void ResetProgress(){
		
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

	//this method will be important for checkpointing the player
	//to the start in case they die/fly off
	public void SetStellation(StellationController c)
	{
		//do we actually want to disable?
		if(Services.main.activeStellation != null){
			//Services.main.activeStellation.Disable();
		}

		c.Enable();
	}
	
	//old old old
	public void FinishLevel(){
		SceneController.instance.LoadNextStellation();
	}


	//old method for sequential levels and unlockable unicursal stellations
	public void CompleteStellation()
	{

		if(save){
			SaveStellation(Services.main.activeStellation);
		}

		if (Services.main.activeStellation.isComplete)
		{
			if (Services.main.activeStellation.hasUnlock)
			{
				Services.main.activeStellation.Disable();
				Services.main.activeStellation.unlock.Enable();		
				Services.main.WarpPlayerToNewPoint(Services.main.activeStellation.start);

				//this only applies if we're flying
			}
			
			else
			{
				//lets plug in the stellationRecorder here
				//GetComponent<StellationRecorder>().GenerateStellation();
				if(OnCompleteStellation.GetPersistentEventCount() > 0){
					OnCompleteStellation.Invoke();
				}else{
					SceneController.instance.LoadNextStellation();
				}
			}
			//we good
		}
	}

}
