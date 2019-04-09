using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSounds : MonoBehaviour {

	public AudioClip[] hits;
	public AudioClip[] sustains;

	public AudioClip background;
	public AudioClip braking; // use for dissonance
	public AudioClip accelerating; //use for resonance

	public AudioSource curPointSound;
	public AudioSource pointDestSound;
	public AudioSource brakingSound;
	public AudioSource ambientSound;
}
