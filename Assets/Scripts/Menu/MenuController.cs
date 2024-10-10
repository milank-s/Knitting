using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public enum MenuSelection{game, editor, oscilloscope}
public class MenuController : MonoBehaviour
{

	[Header("SFX")]

	
	[SerializeField] AudioSource audio;
	[SerializeField] AudioClip selectSFX;
	[SerializeField] AudioClip submitSFX;
	[SerializeField] AudioClip changeLevelSFX;

	[Header("UI")]
    [SerializeField] GameObject menuRoot;
    [SerializeField] GameObject oscilloscopeModel;
    [SerializeField] GameObject levelDisplay;
    [SerializeField] GameObject oscilloscopeDisplay;
    [SerializeField] GameObject oscilloscopeOverlay;
    [SerializeField] Oscilloscope oscilloscope;
    [SerializeField] Image levelImage;
    [SerializeField] TMPro.TextMeshProUGUI levelTitle;
    [SerializeField] TMPro.TextMeshProUGUI levelNumber;

	public Sprite editorSprite;
	public Sprite settingsSprite;


    [SerializeField] TMPro.TextMeshPro[] gameModes;
	[SerializeField] GameObject settings;
	[SerializeField] GameObject gameStartButton;
	[SerializeField] GameObject volumeSettings;
	[SerializeField] GameObject settingsButton;
	[SerializeField] GameObject levelButton;

	
	[Header("Oscilloscope")]
	
    public MenuKnob gameStateKnob;
    public MenuKnob levelSelectKnob;
    public MenuKnob menuSelectKnob;
    public MenuKnob optionSelectKnob;
    public Transform submitButton;
    public Transform escapeButton;

	public bool gameStart = false;
    public bool settingsOpen;
	bool changedSelection;

	float navDir;
	GameObject selection;

	public void Awake(){
		gameStart = Application.isEditor;
	}
	public void Start(){
		selection = EventSystem.current.currentSelectedGameObject;
	}

	void Update(){
		if((Services.main.state == GameState.menu || Services.main.state == GameState.paused)){
			if(EventSystem.current.currentSelectedGameObject != selection){
				selection = EventSystem.current.currentSelectedGameObject;
				changedSelection = true;
				audio.PlayOneShot(selectSFX);
				if(Mathf.Abs(navDir) > 0.1f){
					menuSelectKnob.transform.Rotate(0, Mathf.Sign(navDir) * 23, 0);
				}
			}else{
				changedSelection = false;
			}
		}	
	}

	 public void OnNavigate(InputAction.CallbackContext context)
    {

        if (Services.main.state == GameState.menu && context.phase == InputActionPhase.Started)
        {
			
			RotateYKnob(context.ReadValue<Vector2>());

			if(gameStart){
			
				if (levelButton == EventSystem.current.currentSelectedGameObject)
				{
					Vector2 input = context.ReadValue<Vector2>();
					if (input.x > 0 && Mathf.Approximately(input.y, 0))
					{
						SceneController.instance.SelectNextLevel(true);
					}
					else if (input.x < 0 && Mathf.Approximately(input.y, 0))
					{

						SceneController.instance.SelectNextLevel(false);
					}
				}
				else
				{
					TryChangeSetting(context);   
				}
			}
        }
    }


    public void GameModeSelect(int i){

		if(!gameStart) return;

		MenuSelection newState = (MenuSelection)i;

		foreach(TextMeshPro t in gameModes){
			t.color = new Color(0.5f, 0.5f, 0.5f);
		}

		gameModes[i].color = Color.white;

        switch(newState){
			
			case MenuSelection.game:
				gameStateKnob.transform.localEulerAngles = new Vector3(0, 90, -90);
				levelDisplay.SetActive(true);
				levelImage.sprite = SceneController.instance.curLevelSet.image;
				oscilloscopeOverlay.SetActive(true);
				
			break;

			case MenuSelection.editor:
			
				levelDisplay.SetActive(false);
				oscilloscopeOverlay.SetActive(true);
				gameStateKnob.transform.localEulerAngles = new Vector3(45, 90, -90);
			break;

			case MenuSelection.oscilloscope:
				levelDisplay.SetActive(false);
				oscilloscopeOverlay.SetActive(false);
				gameStateKnob.transform.localEulerAngles = new Vector3(90, 90, -90);
			break;
        }
    }

	
    public void Enter(){
		
        if(!gameStart){
			
			gameStart = true;
			oscilloscopeDisplay.SetActive(false);
			levelButton.SetActive(true);
			
            Show(true);
        }
    }
    
    public void Escape(){
		PushButton(escapeButton);
		
        if(gameStart){

			levelDisplay.SetActive(false);
			levelButton.SetActive(false);

			EventSystem.current.SetSelectedGameObject(gameStartButton);
			CloseMenu();
			gameStart = false;
			oscilloscopeDisplay.SetActive(true);

        }else{
            Application.Quit();
        }
    }
	
    public void OnSubmit(InputAction.CallbackContext context){
		// audio.PlayOneShot(submitSFX);
		if(context.performed){
			PushButton(submitButton);
			if(gameStart){
				oscilloscope.Gauss();
			}
		}
	}
    public void Show(bool b){

        menuRoot.SetActive(b);
        oscilloscopeModel.SetActive(b);
		levelDisplay.SetActive(b);
 		
		if(b){
			if(!gameStart){
				//stupidity reaching outer limits

				gameStart = true;
				Escape();
				
			}else{
            	OpenMenu();
			}
        }else{
            CloseMenu();
        }
    }

    void OpenMenu(){

		GlitchEffect.Fizzle(0.2f);

        CameraFollow.instance.Reset();    
        RenderSettings.fog = false;
	
		if (SceneController.instance.curSetIndex < 0)
		{
			
			SceneController.instance.curSetIndex = 0;
		}
		
		Services.Player.SetActive(false);
		
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

		SelectLevelSet(SceneController.instance.curLevelSet, true);
		EventSystem.current.SetSelectedGameObject(levelButton);
    }
    void CloseMenu(){

        if (SceneController.curLevelName != "Editor") 
		{
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
			RenderSettings.fog = true;
		}
		
		if (settingsOpen)
		{
			OpenSettings();
		}
		
		//ShowWord("", false);
		//ShowImage(null, false);
        //levelNumber.text = "";
    }

	public void PushButton(Transform t){
		
		StartCoroutine(PushButtonRoutine(t));
	}

	public IEnumerator PushButtonRoutine(Transform trans){
		float t = 0;
		Vector3 start = trans.localPosition;

		while(t < 1){
			trans.localPosition = new Vector3(start.x, start.y, -Mathf.Cos(t * (Mathf.PI/2))/10f);
			t += Time.deltaTime * 5;
			yield return null;
		}

		trans.localPosition = new Vector3(start.x, start.y, 0);
	}
    public void SelectLevelSet(LevelSet l, bool increment, bool playSound = false)
    {
		if(playSound){
			audio.PlayOneShot(changeLevelSFX);
			
			if(increment){
				
				levelSelectKnob.transform.Rotate(new Vector3(0, -23,0 ));
			}else{
				levelSelectKnob.transform.Rotate(new Vector3(0, 23,0 ));
			}
		}


        ShowImage(l.image);
        ShowWord(l.title);    
        levelNumber.text = SceneController.instance.curSetIndex + ".";
    }

    public void OpenSettings(){
        settingsOpen = !settingsOpen;
		settings.SetActive(settingsOpen);
		
		if (settingsOpen)
		{
			EventSystem.current.SetSelectedGameObject(volumeSettings);
		}
		else
		{
			PushButton(escapeButton);
			EventSystem.current.SetSelectedGameObject(levelButton);
		}
    }

	public void RotateYKnob(Vector2 v){

		navDir = v.y;
	}

	public void RotateXKnob(float x){

		levelSelectKnob.transform.Rotate(new Vector3(0, x * -23, 0));
	}
	
    public void TryChangeSetting(InputAction.CallbackContext context)
	{
		Vector2 input = context.ReadValue<Vector2>();
		
		if (settingsOpen)
		{

			foreach (SettingValue s in GameSettings.i.settings)
			{
				if (s.gameObject == EventSystem.current.currentSelectedGameObject)
				{

					if (input.x > 0f)
					{
						s.ChangeValue(1);
					}
					else if (input.x < 0)
					{
						s.ChangeValue(-1);
					}

					optionSelectKnob.transform.Rotate(0, Mathf.Sign(input.x ) * 15, 0);
				}
			}
		}
	}
    public void ShowImage(Sprite s, bool show = true){

		levelImage.sprite = s;

		if (show)
		{
			levelImage.color = Color.white;
		}
		else
		{
			levelImage.color = Color.clear;
			
		}
	}
    public void ShowWord(string m,  bool show = true)
	{
		levelTitle.text = m;
		if (show)
		{
			levelTitle.color = Color.white;
		}
		else
		{
			levelTitle.color = Color.clear;
		}
	}
    
}
