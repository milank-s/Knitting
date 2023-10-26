using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSounds : MonoBehaviour {

	public AudioClip[] hits;
	public AudioClip[] sustains;

	public AudioClip[] flyingSound;
	public AudioClip braking; // use for dissonance
	public AudioClip moving; //use for resonance

	public AudioSource curPointSound;
	public AudioSource pointDestSound;
	public AudioSource brakingSound;
	public AudioSource moveSound;
}
