using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class PrefabManager : MonoBehaviour {


	public List<Crawler> crawlers;
	public List<CrawlerManager> crawlerPools;
	public GameObject soundEffectObject;
	public GameObject spawnedText;
	public GameObject text;
	public GameObject point;
	public GameObject spline;
	public GameObject collectible;
	public GameObject splineTurtle;
	public GameObject joint;
	public Sprite[] symbols;
	public Material[] lines;
	public Sprite[] pointSprites;
	public Mesh[] pointMeshes;
	public Material[] fontMaterials;
	public Font[] fonts;

	public void Awake(){
		
		InitCrawlers();
	}

	public void InitCrawlers(){
		crawlerPools = new List<CrawlerManager>();
		
		foreach(CrawlerType c in Enum.GetValues(typeof(CrawlerType))){
			if(c != CrawlerType.none){
				GameObject g = new GameObject();
				g.transform.parent = transform;
				CrawlerManager m = g.AddComponent<CrawlerManager>();
				m.crawlerType = c;
				g.name = Enum.GetName(typeof(CrawlerType), (int)c);
				crawlerPools.Add(m);
				m.Initialize();
			}
		}
	}

	public void SetFont(TextMesh t, int i)
	{
		t.font = fonts[i % fonts.Length];
		t.GetComponent<MeshRenderer>().material = t.font.material;
		//do I need to also update the renderers material?
	}
	public int FindFontIndex(Font f)
	{
		for (int i = 0; i < fonts.Length; i++)
		{
			if (f == fonts[i])
			{
				return i;
			}
		}

		return 0;
	}
	public AudioSource CreateSoundEffect(AudioClip clip, Vector3 pos){
		GameObject newSound = (GameObject)Instantiate (soundEffectObject, pos, Quaternion.Euler (0, 0, 0));
		newSound.GetComponent<AudioSource> ().clip = clip;
		return newSound.GetComponent<AudioSource> ();
	}
	
}
