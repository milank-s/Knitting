using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundBank : MonoBehaviour {

	public AudioMixer master;

	public List<AudioClip> Loops;
	public List<AudioClip> Attacks;

	float distance;
	float speed;

	public bool isPlaying;

	Point p1;
	Point p2;


}
