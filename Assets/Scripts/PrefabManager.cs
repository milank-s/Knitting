using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabManager : MonoBehaviour {

	public GameObject soundEffectObject;
	public GameObject spawnedText;
	public GameObject point;
	public GameObject spline;
	public GameObject splineTurtle;
	public GameObject joint;
	public Sprite[] symbols;
	public Material[] lines;


	void Start(){
		SplineTurtle.maxCrawlers = 0;
		SplineTurtle.maxTotalPoints = 0;

		Services.Prefabs = this;
		LoadResources ();
	}

	public void CreateSoundEffect(AudioClip clip, Vector3 pos){
		Instantiate (soundEffectObject, pos, Quaternion.Euler (0, 0, 0));
		soundEffectObject.GetComponent<AudioSource> ().clip = clip;
	}

	public void LoadResources(){
		symbols = Resources.LoadAll<Sprite> ("Symbols");
		lines = Resources.LoadAll <Material>("Lines");
	}
}
