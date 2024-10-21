using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AudioHelm;
using UnityEngine;
using UnityEngine.Events;

public class StellationManager : MonoBehaviour
{
	public int level;
	public int checkpoint;
	public int startPoint;
	public static StellationManager instance;
	
	public List<StellationController> controllers;

	[Space(15)]
	public UnityEvent OnCompleteLap;
	public UnityEvent OnCompleteStellation;
	public UnityEvent OnLeaveStart;
	public UnityEvent OnNextStart;

	public bool saveProgress = false;
	string[] stellations;
	public string fileName = "";

	void Awake()
	{
		instance = this;

		foreach(StellationController c in controllers){
			c.Initialize();
		}
	}

	void OnApplicationQuit()
    {
        SaveGame.ClearSave();
    }

	public void Start()
	{		
		if(!saveProgress){
			Setup();
		}
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

	public void Setup(){

		//this is macro position within the game and should be selected from the main menu
		// for(int i = 0; i < stellationSets.Count; i++){
		// 	if(i != level){
		// 		stellationSets[i].gameObject.SetActive(false);
		// 	}
		// }
		
		Services.main.activeStellation = controllers[checkpoint];


		//this is pretty fucking heavy duty 
		//there should be a dedicated reset function

		//second loop, load the stellations and set up any necessary unlocks

		List<StellationController> lockedStellations = new List<StellationController>();

		for(int i = 0; i < controllers.Count; i++)
		{
		
			controllers[i].Setup();

			//lock stellations and make sure they're not enabled after
			if(i >= checkpoint &&  controllers[i].hasUnlock){
				controllers[i].unlock.LockStellation();
				lockedStellations.Add(controllers[i].unlock);
			}
			
		}
		
		// controllers[checkpoint].OnPlayerEnter();

		for(int i = 0; i < controllers.Count; i++){

			//show points but dont draw in splines for unaccessed stellations
			if(!lockedStellations.Contains(controllers[i])){
				controllers[i].Show(true);
			}

			//draw in splines for accessed stellations
			if(i <= checkpoint && controllers[i] != Services.main.activeStellation){
				controllers[i].DrawStellation();
			}
		}

				//we need a way of overriding the start point before the call goes out to animate the splines
		Services.main.InitializeLevel();
		Services.StartPoint = Services.main.activeStellation._points[startPoint];

		//this function tells the active stellation to draw I believe
		StartCoroutine(ActivatePlayer());
	}

	public IEnumerator ActivatePlayer(){
		yield return new WaitForSeconds(0.1f);
		Services.main.ActivatePlayer();
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
