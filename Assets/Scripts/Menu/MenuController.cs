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

	MenuSelection modeSelection;
    [SerializeField] TMPro.TextMeshPro[] gameModes;
	[SerializeField] GameObject settings;
	[SerializeField] GameObject gameStartButton;
	[SerializeField] GameObject volumeSettings;
	[SerializeField] GameObject settingsButton;
	[SerializeField] GameObject levelButton;

	[SerializeField] Transform gainMeter;
	[SerializeField] Transform leftMeter;
	[SerializeField] Transform rightMeter;
	
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

	Vector2 navDir;
	GameObject selection;

	public void Awake(){
		gameStart = Application.isEditor;
	}
	public void Start(){
		selection = EventSystem.current.currentSelectedGameObject;
	}

	void Update(){
		if(Services.main.state == GameState.menu || Services.main.state == GameState.paused){
		
			//this shouldnt play when we press enter or escape?
			float leftGain =  Mathf.Clamp01(oscilloscope.normalX/2f + AudioManager.loudness + Mathf.Abs(oscilloscope.noise.x));
			float rightGain = Mathf.Clamp01(oscilloscope.normalY/2f+ AudioManager.loudness + Mathf.Abs(oscilloscope.noise.y));
			gainMeter.localScale = Vector3.Lerp(gainMeter.localScale, new Vector3(1, AudioManager.loudness, 1), Time.deltaTime * 10);
			leftMeter.localScale = Vector3.Lerp(leftMeter.localScale, new Vector3(1, leftGain, 1), Time.deltaTime * 10);
			rightMeter.localScale = Vector3.Lerp(rightMeter.localScale, new Vector3(1, rightGain, 1), Time.deltaTime * 10);

			if(EventSystem.current.currentSelectedGameObject != selection){
				selection = EventSystem.current.currentSelectedGameObject;
				changedSelection = true;
				// audio.PlayOneShot(selectSFX);

				if(Mathf.Abs(navDir.x) > 0.1f){
					SynthController.instance.keys[0].PlayNote(35 + (int)navDir.x * 5, 0.1f, 0.5f);
					menuSelectKnob.Rotate(Mathf.Sign(navDir.x) * 23);
				}else{
					SynthController.instance.keys[3].PlayNote(40, 1f, 0.5f);
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
		
			if(gameStart){
				
				//Horizontal knob for levels

				Vector2 input = context.ReadValue<Vector2>();

				if (levelButton == EventSystem.current.currentSelectedGameObject)
				{
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
		

		gameStateKnob.Rotate((i - (int)modeSelection) * 23);

		modeSelection = (MenuSelection)i;

        switch(newState){
			
			case MenuSelection.game:
				levelDisplay.SetActive(true);
				levelImage.sprite = SceneController.instance.curLevelSet.image;
				oscilloscopeOverlay.SetActive(true);
				
			break;

			case MenuSelection.editor:
			
				levelDisplay.SetActive(false);
				oscilloscopeOverlay.SetActive(true);
			break;

			case MenuSelection.oscilloscope:
				levelDisplay.SetActive(false);
				oscilloscopeOverlay.SetActive(false);
			break;
        }
    }

	
    public void Enter(){
		
        if(!gameStart){
			
			gameStart = true;
			oscilloscope.Disable();
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
			oscilloscope.Initialize();
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;

        }else{
            Application.Quit();
        }
    }
	
    public void OnSubmit(InputAction.CallbackContext context){
		// audio.PlayOneShot(submitSFX);
		if(context.performed){
			PushButton(submitButton);
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
		modeSelection = 0;
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
			
			SynthController.instance.keys[1].PlayNote(30 + (increment ? 2 : -2), 0.25f, 0.5f);
			
			if(increment){
				
				levelSelectKnob.Rotate(-23);
			}else{
				
				levelSelectKnob.Rotate(23);
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

	public void RotateYKnob(float y){

		menuSelectKnob.Rotate(y * 23f);
	}

	public void RotateXKnob(float x){

		levelSelectKnob.Rotate(x * 23f);
	}
	
    public void TryChangeSetting(InputAction.CallbackContext context)
	{
		Vector2 input = context.ReadValue<Vector2>();
		
		if (settingsOpen)
		{

			RotateYKnob(-input.y);
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
