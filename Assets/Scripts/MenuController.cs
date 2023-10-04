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

    public bool settingsOpen;

	GameObject selection;
	public void Start(){
		selection = EventSystem.current.currentSelectedGameObject;
	}

	void Update(){
		if((Services.main.state == Main.GameState.menu || Services.main.state == Main.GameState.paused) && EventSystem.current.currentSelectedGameObject != selection){
			selection = EventSystem.current.currentSelectedGameObject;
			audio.PlayOneShot(selectSFX);
		}	
	}

    public void GameModeSelect(Main.GameState newState){
        switch(newState){
			
			case Main.GameState.editing:
				gameStateKnob.transform.localEulerAngles = new Vector3(-90, 90, 0);
			break;

			case Main.GameState.playing:
				
				gameStateKnob.transform.localEulerAngles = new Vector3(-90, 45, 0);
			break;
			
        }
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

		SelectLevelSet(SceneController.instance.curLevelSet);
    }
    void CloseMenu(){

        if (SceneController.curLevelName != "Editor") 
		{
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
		}

		if (settingsOpen)
		{
			OpenSettings();
		}
		
		ShowWord("", false);
		ShowImage(null, false);
        levelNumber.text = "";
    }

    public void SelectLevelSet(LevelSet l, bool playSound = false)
    {
		if(playSound){
			audio.PlayOneShot(changeLevelSFX);
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
			EventSystem.current.SetSelectedGameObject(settingsButton);
		}
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
