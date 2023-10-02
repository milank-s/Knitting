using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum MenuSelection{game, editor, oscilloscope}
public class MenuController : MonoBehaviour
{

    MenuSelection selection;
    [SerializeField] GameObject menuRoot;
    [SerializeField] GameObject levelDisplay;
    [SerializeField] GameObject oscilloscopeDisplay;
    [SerializeField] GameObject editorDisplay;
    [SerializeField] Image levelImage;
    [SerializeField] TMPro.TextMeshProUGUI levelTitle;
    [SerializeField] TMPro.TextMeshProUGUI levelNumber;

    bool settingsOpen;

    public void Show(bool b){

        if(b){
            OpenMenu();
        }else{
            CloseMenu();
        }

        menuRoot.SetActive(b);
    }

    void OpenMenu(){
        RenderSettings.fog = false;

		if (SceneController.instance.curSetIndex < 0)
		{
			
			SceneController.instance.curSetIndex = 0;
		}
		
		Services.Player.SetActive(false);
	
		SelectLevelSet(SceneController.instance.curLevelSet);
		
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		
		EventSystem.current.SetSelectedGameObject(SceneController.instance.levelButton.gameObject);
    }
    void CloseMenu(){
        if (Services.main.curLevel != "Editor") 
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
    }

    public void SelectLevelSet(LevelSet l)
    {
        ShowImage(l.image);
        ShowWord(l.title);    
        levelNumber.text = SceneController.instance.curSetIndex + ".";
    }

    void OpenSettings(){

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
