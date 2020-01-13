using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartScreen : MonoBehaviour
{
    public Mandala mandala;
    private bool goNext = false;
    public GameObject word;

    public GameObject name;
    public GameObject name2;
    public GameObject name3;
    public GameObject title;
    public GameObject by;

    private bool idling;
    private float timer;
    
    [SerializeField] private Image sprite;
    // Start is called before the first frame update

    void Awake()
    {
        sprite.color = Color.black;
    }
    IEnumerator Start()
    {
        
        yield return new WaitForSeconds(0.25f);
        
        name.SetActive(true);
        
        yield return new WaitForSeconds(0.5f);
        
        name2.SetActive(true);
        
        yield return new WaitForSeconds(0.2f);
        
        name3.SetActive(true);

        
        yield return new WaitForSeconds(0.5f);
        
        by.SetActive(true);
        
        yield return new WaitForSeconds(1f);

        goNext = true;
        
        StartCoroutine(LoadNext(0, true));
        
        name.SetActive(false);
        name2.SetActive(false);
        name3.SetActive(false);
        by.SetActive(false);
        
//        title.SetActive(true);
    }
    void Update()
    {
        if (Input.anyKeyDown && !goNext)
        {
            goNext = true;
            StartCoroutine(LoadNext(0, true));
        }

        if (Services.main.state == Main.GameState.playing)
        {
            timer += Time.deltaTime;
        }
        else
        {
            timer = 0;
        }

        if (Input.anyKey)
        {
            timer = 0;
            if (idling)
            {
                idling = false;
                mandala.shutItDown = true;
                StartCoroutine(LoadNext(0, true));
            }
        }

        if (timer > 60f && !idling && Services.main.state == Main.GameState.playing)
        {
            sprite.gameObject.SetActive(true);
            idling = true;
            mandala.shutItDown = false;
            StartCoroutine(LoadNext(0, false));
        }
    }


    IEnumerator LoadNext(float delay, bool fade)
    {
//        
//        mandala.shutItDown = true;
        yield return new WaitForSeconds(delay);
        word.SetActive(false);
        
        float f = 0;
        while (f < 1)
        {
            f += Time.deltaTime;
            if (fade)
            {
                sprite.color = Color.Lerp(Color.black, Color.clear, f);
            }
            else
            {
                sprite.color = Color.Lerp(Color.black, Color.clear, 1-f);
            }

            yield return null;
        }

        if (fade)
        {
            sprite.gameObject.SetActive(false);
        }
    }    
}
