using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CycleSprites: MonoBehaviour {

	public float maxScale = 1;
	public float minScale = 0.3f;
	public float delay;
	public float fadeSpeed;
  private float timer;

	public bool createGrid;
	public float xDist, yDist, zDist;
	public string directory;
	public int height, width;
	public GameObject spritePrefab;
	public Vector3 rotation;
	public static GameObject[,] grid;

	public bool running = false;
	protected List<Sprite> sprites;
	protected List<GameObject> children;
	protected Sprite[] spriteArray;

	private int xIndex, yIndex, index;
	// Use this for initialization

	public CycleSprites(){
		sprites = new List<Sprite> ();
		children =  new List<GameObject> ();
	}

	protected void LoadSprites(){
		spriteArray =  (Resources.LoadAll<Sprite> (directory));
		sprites.AddRange (spriteArray);
	}

	void Start () {
		LoadSprites ();

		if (createGrid) {
			StartCoroutine(SpawnGrid ());
		}
	}

	// Update is called once per frame
	void Update () {
		if(!running) return;
		
		timer -= Time.deltaTime;
		if(timer <= 0){
			Spawn();
			timer = delay;
		}
	}

	public virtual void Spawn (){

		Vector3 newObjectPos = transform.position;
		newObjectPos.x += (xIndex * xDist) - (width/2 * xDist);
		newObjectPos.z += (yIndex * zDist);
		newObjectPos.y += (yIndex * yDist);

		GameObject newObject = new GameObject();
		newObject.transform.position = newObjectPos;
		newObject.transform.rotation = Quaternion.Euler (rotation);
		newObject.transform.Rotate(0, 0, Random.Range(0, 361));
		newObject.transform.parent = transform;
		newObject.transform.localScale *= Random.Range(minScale, maxScale);
		if (newObject.GetComponent<SpriteRenderer> () == null) {
			newObject.AddComponent<SpriteRenderer>();
		}
		newObject.AddComponent <FadeImage>().time = fadeSpeed;
		newObject.layer = LayerMask.NameToLayer("UI");

		SpriteRenderer r = newObject.GetComponent<SpriteRenderer> ();
		if (sprites.Count == 0) {
			sprites.AddRange (spriteArray);
		}
		Sprite s = sprites [Random.Range (0, sprites.Count)];
		r.sprite = s;
		r.sortingOrder = index;
		// newObject.transform.position += (transform.up * r.bounds.size.y)/2;
		//		newObject.transform.localScale /= r.bounds.size.y/xDist;
		sprites.Remove (s);
		children.Add (newObject);
	}

	public  IEnumerator SpawnGrid(){

		for (xIndex = 0; xIndex < width; xIndex++) {
			for (yIndex = 0; yIndex < height; yIndex++) {
				Spawn ();
				index++;
			}
			yield return new WaitForSeconds(delay);
		}
		StartCoroutine (SpawnGrid ());
	}
}
