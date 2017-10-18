using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabManager : MonoBehaviour {

	public GameObject SoundEffectObject;
	public GameObject Point;
	public GameObject Spline;

	public void CreateSoundEffect(AudioClip clip, Vector3 pos){
		Instantiate (SoundEffectObject, pos, Quaternion.Euler (0, 0, 0));
		SoundEffectObject.GetComponent<AudioSource> ().clip = clip;
	}
}
