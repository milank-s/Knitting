using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabManager : MonoBehaviour {

	public GameObject SoundEffectObject;
	public GameObject SpawnedText;
	public GameObject Point;
	public GameObject Spline;
	public GameObject SplineTurtle;
	public GameObject Joint;
	public Sprite[] Symbols;
	public Material[] Lines;

	public void CreateSoundEffect(AudioClip clip, Vector3 pos){
		Instantiate (SoundEffectObject, pos, Quaternion.Euler (0, 0, 0));
		SoundEffectObject.GetComponent<AudioSource> ().clip = clip;
	}

	public void LoadResources(){
		Symbols = Resources.LoadAll<Sprite> ("Symbols");
		Lines = Resources.LoadAll <Material>("Lines");
	}
}
