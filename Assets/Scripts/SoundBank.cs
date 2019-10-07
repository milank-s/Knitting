using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundBank : MonoBehaviour {

	public AudioMixer master;

	public List<AudioClip> hits;
	public List<AudioClip> sustains;

	float distance;
	float speed;

	public bool isPlaying;

	Point p1;
	Point p2;

	public void PlayPointAttack ()
	{
		GameObject newSound = Instantiate(Services.Prefabs.soundEffectObject, transform.position, Quaternion.identity);
		newSound.GetComponent<AudioSource>().clip = hits[0];
		newSound.GetComponent<AudioSource>().Play();
		newSound.GetComponent<PlaySound>().enabled = true;
	}
}
