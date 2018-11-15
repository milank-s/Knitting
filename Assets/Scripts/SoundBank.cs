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


}
