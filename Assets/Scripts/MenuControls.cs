using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuControls : MonoBehaviour {

	public CycleSprites cycleScript;
	public Image mask;
	bool loadingScene = false;

	void Start(){
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
	}
	// Update is called once per frame
	void Update () {
			if(Input.GetButtonDown("Button1") && !loadingScene){
					StartCoroutine(LoadGame());
			}
	}

	public IEnumerator LoadGame(){
		loadingScene = true;
		if (cycleScript != null)
		{
			cycleScript.enabled = false;
		}

		float t = 0;
		while (t < 1.2f){
			mask.color = Color.Lerp(new Color (0,0,0,0), new Color (0,0,0,1), Easing.QuadEaseIn(t));
			t += Time.deltaTime;
			yield return null;
		}
		SceneManager.LoadScene(1);
	}
}
