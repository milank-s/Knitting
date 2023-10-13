using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CrawlerType {blocker, follower}
public class CrawlerManager : MonoBehaviour
{
    public int crawlerCount = 10;
    public float spawnFrequency = 0.5f;
    public bool forward = true;
    public Spline spline;
    public CrawlerType crawlerType = CrawlerType.blocker;
    private List<Crawler> crawlers;
    private int index;
    float spawnTimer;
    void Awake()
    {
        crawlers = new List<Crawler>();
        
        for (int i = 0; i < crawlerCount; i++)
        {
            Crawler newCrawler = Instantiate(Services.Prefabs.crawlers[(int)crawlerType], transform).GetComponent<Crawler>();
            newCrawler.Init();
            crawlers.Add(newCrawler);
            
        }

        Services.main.OnReset += Reset;
    }

    void Update(){
        spawnTimer += Time.deltaTime;
        if(spawnTimer > spawnFrequency){
            spawnTimer = 0;
            AddCrawler();
        }

        foreach(Crawler c in crawlers){
            if(c.running) c.Step();
        }
    }

    public void OnDestroy(){
        
        Services.main.OnReset -= Reset;
    }

    public void AddCrawler()
    {
        
        bool available = false;
        Crawler toUse = null;
        foreach (Crawler c in crawlers)
        {
            if (!c.running)
            {
                available = true;
                toUse = c;
                break;
            }
        }

        if (!available)
        {
            return;
            // toUse = crawlers[Mathf.Clamp((index % crawlers.Count) + 1, 0, crawlers.Count-1)];
            // index++;
        }
        
        
        toUse.gameObject.SetActive(true);
        toUse.Setup(spline, forward);
    }
    public void Reset()
    {
        foreach (Crawler c in crawlers)
        {
            c.Stop();
        }
    }
}
