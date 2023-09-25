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
		if (controllers.Count < 1)
		{
			controllers = GetComponentsInChildren<StellationController>().ToList();
		}

		//first loop. save the file names and whether or not they have been played
		//delete all the stellations and reload their stellations from the appropriate file
		if(save){
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
				controllers[i].name = controllers[i].name + "Played";
				//player hasn't played the level yet. Set name so that it saves to new file
			}
		}
		}

		//second loop, load the stellations and set up any necessary unlocks

		for(int i = controllers.Count -1; i >= 0; i--)
		{
			// if (i < controllers.Count - 1) 
			// {
			// 	controllers[i].unlock = controllers[i + 1];
			// }

			controllers[i].Initialize();
			if(controllers[i].hasUnlock){
				controllers[i].unlock.EnableStellation(false);
			}
			// controllers[i].EnableStellation(true);
		}
		
		Services.main.activeStellation = controllers[0];
			
		//DRAWING IN IS ONLY WORKING WHEN CALLED FROM HERE
		Services.main.activeStellation.EnableStellation(true);
		SetupActiveStellation(controllers[0], true);
		
		Services.main.InitializeLevel();
	}

	public void SaveStellation(StellationController c){
			//right now the way the save function uses the point parent and active stellation might cause problems
			MapEditor.instance.Save(c);
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
	
	public void FinishLevel(){
		SceneController.instance.LoadNextStellation();
	}
	
	public void CompleteStellation()
	{

		if(save){
			SaveStellation(Services.main.activeStellation);
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
