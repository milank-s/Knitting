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
	[SerializeField] AudioClip changeSettingSFX;
	[SerializeField] AudioClip oscilloscopeSwitchSFX;
	[SerializeField] AudioClip oscilloscopeSwitchOffSFX;

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
	[SerializeField] GameObject mandala;

	[SerializeField] Transform gainMeter;
	[SerializeField] Transform leftMeter;
	[SerializeField] Transform rightMeter;
	
	[Header("Oscilloscope")]
	
	public GameObject powerLight;
    public MenuKnob gameStateKnob;
    public MenuKnob levelSelectKnob;
    public MenuKnob menuSelectKnob;
    public MenuKnob optionSelectKnob;
    public Transform submitButton;
    public Transform escapeButton;

	bool oscilloscopeDrawing;
	bool oscilloscopeOn;
	public bool gameStart = false;
    public bool settingsOpen;
	bool changedSelection;

	Vector2 navDir;
	GameObject selection;

	public void Awake(){
		if(!Application.isEditor) gameStart = false;
	}
	public void Start(){
		
		levelDisplay.SetActive(false);
		selection = EventSystem.current.currentSelectedGameObject;
	}

	public void StartSequence(){
		
		StartCoroutine(StartRoutine());
	}

	public IEnumerator StartRoutine(){
		
		Services.fx.Fade(true, 1f);

		yield return new WaitForSeconds(1f);

		StartCoroutine(TurnOn());
		
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

				// if(Mathf.Abs(navDir.x) > 0.1f){
				// 	menuSelectKnob.Rotate(Mathf.Sign(navDir.x) * 23);
				// }else{
				// 	SynthController.instance.keys[3].PlayNote(40, 1f, 0.5f);
				// }
				

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

		SynthController.instance.keys[0].PlayNote(40 - i * 3, 0.25f, 0.5f);

		gameStateKnob.Rotate((i - (int)modeSelection) * 23);
		modeSelection = (MenuSelection)i;

		mandala.SetActive(false);

        switch(newState){
			
			case MenuSelection.game:
				levelDisplay.SetActive(true);
				levelTitle.gameObject.SetActive(true);
				levelImage.sprite = SceneController.instance.curLevelSet.image;
				
			break;

			case MenuSelection.editor:
				mandala.SetActive(true);
				levelImage.sprite = editorSprite;
				levelTitle.gameObject.SetActive(false);
				levelDisplay.SetActive(true);
			break;

			case MenuSelection.oscilloscope:
				OpenSettingsWithFrameDelay();
				levelDisplay.SetActive(false);

			break;
        }

		
    }

	public IEnumerator TurnOn(){
		oscilloscopeOn = true;
		audio.PlayOneShot(oscilloscopeSwitchSFX);
		yield return new WaitForSeconds(0.3f);
		powerLight.SetActive(true);

		if(gameStart){
			Show(true);
		}

	}
	public IEnumerator TurnOff(){

		StopDrawing();

		yield return new WaitForSeconds(0.3f);
		powerLight.SetActive(false);
		oscilloscopeOn = false;
	}

	void StopDrawing(){
		audio.PlayOneShot(oscilloscopeSwitchOffSFX);
		oscilloscopeDrawing = false;
		oscilloscope.Disable();
	}
	
    public void HideOscilloscope(){
		
        if(!oscilloscopeOn){
			StartCoroutine(TurnOn());

		}else{
			if(!oscilloscopeDrawing){
				
				ShowOscilloscope();
			}else{
				gameStart = true;
				levelButton.SetActive(true);
				StopDrawing();
				Show(true);
			}
			
		}
        
    }
    
    public void ShowOscilloscope(){
		PushButton(escapeButton);

		if(oscilloscopeOn){
			if(!oscilloscopeDrawing){
			
				oscilloscopeDrawing = true;
				audio.PlayOneShot(oscilloscopeSwitchSFX);

				levelDisplay.SetActive(false);
				levelButton.SetActive(false);

				EventSystem.current.SetSelectedGameObject(gameStartButton);
				CloseMenu();
				gameStart = false;
				oscilloscope.Initialize();

				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;

			}else{
				StartCoroutine(TurnOff());
			}

		}else{
			Debug.Log("quitting");

			Application.Quit();
		}
        
    }
	
    public void OnSubmit(InputAction.CallbackContext context){
		
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
				gameStart = true;
				ShowOscilloscope();
				
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

	public void OpenSettingsWithFrameDelay(){
		StartCoroutine(WaitBeforeSettings());
	}

	IEnumerator WaitBeforeSettings(){
		yield return null;
		OpenSettings();
	}

    public void OpenSettings(){

        settingsOpen = !settingsOpen;
		settings.SetActive(settingsOpen);
		
		if (settingsOpen)
		{
			oscilloscopeOverlay.SetActive(false);
			EventSystem.current.SetSelectedGameObject(volumeSettings);
		}
		else
		{
			oscilloscopeOverlay.SetActive(true);
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

			if(input.y != 0){
				RotateYKnob(-input.y);
				audio.PlayOneShot(changeSettingSFX);
			}

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
