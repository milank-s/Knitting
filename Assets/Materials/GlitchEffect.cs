/**
This work is licensed under a Creative Commons Attribution 3.0 Unported License.
http://creativecommons.org/licenses/by/3.0/deed.en_GB

You are free:

to copy, distribute, display, and perform the work
to make derivative works
to make commercial use of the work
*/

using System.Collections;
using UnityEngine;

using System.Collections.Generic;
using UnityEngine.Rendering;

[ExecuteInEditMode]
[AddComponentMenu("Image Effects/GlitchEffect")]
[RequireComponent(typeof(Camera))]
public class GlitchEffect : MonoBehaviour
{
	public float minIntensity = 0;
	public float maxIntensity = 1;
	public float minFreq = 0;
	public float maxFreq = 1;
	public Texture2D displacementMap;
	public Shader Shader;
	[Header("Glitch Intensity")]

	[Range(0, 1)]
	public static float intensity;
	public static float frequency;
	
	private Material _material;

	public static GlitchEffect instance;
	void Awake()
	{
		instance = this;
		_material = new Material(Shader);
	}

	public void SetValues(float t)
	{
		intensity = Mathf.Lerp(minIntensity, maxIntensity, t);
		frequency = Mathf.Lerp(minFreq, maxFreq, t);
	}
	void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		float yScroll = 1; //Time.time/5f;
		_material.SetFloat("_Intensity", intensity);
		_material.SetFloat("_Noise", yScroll);
		_material.SetFloat("_Frequency", frequency);
		Graphics.Blit(source, destination, _material);
	}

	public static void Fizzle(float time)
	{
		instance.StartCoroutine(Shake(time));
	}

	public static IEnumerator Shake(float time)
	{
		float t = 0;
		while (t < 1)
		{
			instance.SetValues(Mathf.Sin(t * Mathf.PI));
			t += Time.unscaledDeltaTime / time;
			yield return null;
		}
		
		instance.SetValues(0);
	}

	// Called by camera to apply image effect
	/*void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		_material.SetFloat("_Intensity", intensity);
		_material.SetFloat("_ColorIntensity", colorIntensity);
		_material.SetTexture("_DispTex", displacementMap);

		flicker += Time.deltaTime * colorIntensity;
		if (flicker > _flickerTime)
		{
			_material.SetFloat("filterRadius", Random.Range(-3f, 3f) * colorIntensity);
			_material.SetVector("direction", Quaternion.AngleAxis(Random.Range(0, 360) * colorIntensity, Vector3.forward) * Vector4.one);
			flicker = 0;
			_flickerTime = Random.value;
		}

		if (colorIntensity == 0)
			_material.SetFloat("filterRadius", 0);

		_glitchup += Time.deltaTime * flipIntensity;
		if (_glitchup > _glitchupTime)
		{
			if (Random.value < 0.1f * flipIntensity)
				_material.SetFloat("flip_up", Random.Range(0, 1f) * flipIntensity);
			else
				_material.SetFloat("flip_up", 0);

			_glitchup = 0;
			_glitchupTime = Random.value / 10f;
		}

		if (flipIntensity == 0)
			_material.SetFloat("flip_up", 0);

		_glitchdown += Time.deltaTime * flipIntensity;
		if (_glitchdown > _glitchdownTime)
		{
			if (Random.value < 0.1f * flipIntensity)
				_material.SetFloat("flip_down", 1 - Random.Range(0, 1f) * flipIntensity);
			else
				_material.SetFloat("flip_down", 1);

			_glitchdown = 0;
			_glitchdownTime = Random.value / 10f;
		}

		if (flipIntensity == 0)
			_material.SetFloat("flip_down", 1);

		if (Random.value < 0.05 * intensity)
		{
			_material.SetFloat("displace", Random.value * intensity);
			_material.SetFloat("scale", 1 - Random.value * intensity);
		}
		else
			_material.SetFloat("displace", 0);

		Graphics.Blit(source, destination, _material);
	}*/
	
	
}
