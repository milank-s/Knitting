using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PrefabManager : MonoBehaviour {

	public GameObject soundEffectObject;
	public GameObject spawnedText;
	public GameObject circleEffect;
	public ParticleSystem particleEffect;
	public GameObject point;
	public GameObject spline;
	public GameObject splineTurtle;
	public GameObject joint;
	public Sprite[] symbols;
	public Material[] lines;
	public Sprite[] pointSprites;

	void Awake(){
		LoadResources ();
	}

	public AudioSource CreateSoundEffect(AudioClip clip, Vector3 pos){
		GameObject newSound = (GameObject)Instantiate (soundEffectObject, pos, Quaternion.Euler (0, 0, 0));
		newSound.GetComponent<AudioSource> ().clip = clip;
		return newSound.GetComponent<AudioSource> ();
	}

	public void LoadResources(){
		symbols = Resources.LoadAll<Sprite> ("Symbols");
		lines = Resources.LoadAll <Material>("Lines");
	}
}
