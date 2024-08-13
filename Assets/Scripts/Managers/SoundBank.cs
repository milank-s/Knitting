using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundBank : MonoBehaviour {

	public List<AudioClip> hits;
	public List<AudioClip> sustains;

	float distance;
	float speed;

	public bool isPlaying;

	Point p1;
	Point p2;

	public void PlayPointAttack (float volume)
	{
		GameObject newSound = Instantiate(Services.Prefabs.soundEffectObject, Services.Player.transform.position, Quaternion.identity);
		newSound.GetComponent<AudioSource>().clip = hits[Random.Range(0, hits.Count)];
		newSound.GetComponent<AudioSource>().Play();
		newSound.GetComponent<PlaySound>().enabled = true;
		newSound.GetComponent<AudioSource>().volume = volume;
	}
	
}
