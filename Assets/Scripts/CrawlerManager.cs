using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CrawlerType {blocker, follower, bird}
public class CrawlerManager : MonoBehaviour
{
    public int crawlerCount = 10;
    public float speed = 3;
    public float spawnFrequency = 0.5f;
    public bool forward = true;
    public Spline spline;
    public CrawlerType crawlerType = CrawlerType.blocker;
    private List<Crawler> crawlers;
    private int index;
    public bool emitting;
    float spawnTimer;
    int count = 0;

    public bool cleared = false;
    public void Initialize()
    {
        crawlers = new List<Crawler>();
        emitting = true;
        cleared = false;

        for (int i = 0; i < crawlerCount; i++)
        {
            Crawler newCrawler = Instantiate(Services.Prefabs.crawlers[(int)crawlerType], transform).GetComponent<Crawler>();
            newCrawler.baseSpeed = speed;
            crawlers.Add(newCrawler);
            newCrawler.Init(this);
        }

        //Services.main.OnReset += Reset;
    }

    public int GetCrawlerIndex(Crawler c){
        return crawlers.IndexOf(c);
    }
    public void CheckCrawlers(){
        if(crawlers.Count == 0) return;
        foreach(Crawler c in crawlers){
            if(c.running){
                return;
            }
        }

        cleared = true;
    }

    public void Step(){
        if(emitting){
            spawnTimer += Time.deltaTime;
            
            if(spawnTimer > spawnFrequency){
                count ++;
                emitting = count < crawlerCount;
                spawnTimer = 0;
                AddCrawler();
            }
        }

        foreach(Crawler c in crawlers){
            if(c.running) c.Step();
        }
    }

    public void RestartCrawlers(){
        //dumb but whatever
        if(Services.main.activeStellation.isComplete) return;
        
        for (int i = 0; i < crawlerCount; i++)
        {
            AddCrawler();
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
        emitting = true;
        count = 0;
        spawnTimer = 0;

        foreach (Crawler c in crawlers)
        {
            c.Stop();
        }
    }
}
