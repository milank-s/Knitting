using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] GameObject editorDisplay;
    [SerializeField] Image levelImage;
    [SerializeField] TMPro.TextMeshProUGUI levelTitle;
    [SerializeField] TMPro.TextMeshProUGUI levelNumber;

	[SerializeField] GameObject settings;
	[SerializeField] GameObject volumeSettings;
	[SerializeField] GameObject settingsButton;

	
	[Header("Oscilloscope")]
    public MenuKnob gameStateKnob;
    public MenuKnob levelSelectKnob;
    public MenuKnob optionSelectKnob;
    public Transform submitButton;
    public Transform escapeButton;

    public bool settingsOpen;
	bool changedSelection;

	float navDir;
	GameObject selection;
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
					optionSelectKnob.transform.Rotate(0, Mathf.Sign(navDir) * 23, 0);
				}
			}else{
				changedSelection = false;
			}
		}	
	}


    public void GameModeSelect(int i){
		MenuSelection newState = (MenuSelection)i;
        switch(newState){
			
			case MenuSelection.game:
				gameStateKnob.transform.localEulerAngles = new Vector3(0, 90, -90);
			break;

			case MenuSelection.editor:
				
				gameStateKnob.transform.localEulerAngles = new Vector3(45, 90, -90);
			break;

			case MenuSelection.oscilloscope:
				
				gameStateKnob.transform.localEulerAngles = new Vector3(90, 90, -90);
			break;
        }
    }

    public void OnSubmit(){
		// audio.PlayOneShot(submitSFX);
		PushButton(submitButton);
	}
    public void Show(bool b){

        if(b){
            OpenMenu();
        }else{
            CloseMenu();
        }

        menuRoot.SetActive(b);
		levelDisplay.SetActive(b);
        oscilloscopeModel.SetActive(b);
		oscilloscopeDisplay.SetActive(b);
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
		
		EventSystem.current.SetSelectedGameObject(SceneController.instance.levelButton.gameObject);

		SelectLevelSet(SceneController.instance.curLevelSet, true);
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
		
		ShowWord("", false);
		ShowImage(null, false);
        levelNumber.text = "";
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
			EventSystem.current.SetSelectedGameObject(SceneController.instance.levelButton.gameObject);
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
