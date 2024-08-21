using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CrawlerType {blocker, chaser, passive, bird, spark}
public class CrawlerManager : MonoBehaviour
{
    public int crawlerCount = 10;
    public float speed = 3;
    public float spawnFrequency = 0.5f;
    public CrawlerType crawlerType = CrawlerType.spark;
    private List<Crawler> crawlers;
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
            SpawnCrawler(crawlerType);
        }

        //Services.main.OnReset += Reset;
    }
    public void EmitSparks(Point p){
        
        Debug.Log("emitting sparks");

        StartCoroutine(EmitSparksRoutine(p));
    }

    IEnumerator EmitSparksRoutine(Point p){

        yield return null;

        foreach(Point n in p._neighbours){
            Debug.Log("spawning sparks");
            Spark newCrawler = (Spark)SpawnCrawler(CrawlerType.spark);
    
            Spline s = p.GetConnectingSpline(n);
            bool f = s.IsGoingForward(p, n);
            int i = f ? s.GetPointIndex(p) : s.GetPointIndex(n);
            AddCrawler(s, f, i);
        }
    }

    public Crawler SpawnCrawler(CrawlerType t){
        Crawler newCrawler = Instantiate(Services.Prefabs.crawlers[(int)t], transform).GetComponent<Crawler>();
        newCrawler.baseSpeed = speed;
        crawlers.Add(newCrawler);
        newCrawler.Init(this);
        return newCrawler;
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
                // AddCrawler(true);
            }
        }

        for(int i = 0; i < crawlers.Count; i++){
            if(crawlers[i].running) crawlers[i].Step();
        }
    }

    
    public void RestartCrawlers(){
        //dumb but whatever
        if(Services.main.activeStellation.isComplete) return;
        
        Spark.visited = new List<Point>();
        
        for (int i = 0; i < crawlerCount; i++)
        {
            // AddCrawler(true);
        }
    }

    public void OnDestroy(){
        
        Services.main.OnReset -= Reset;
    }

    public void AddCrawler(Spline s, bool forward, int i)
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
        toUse.Setup(s, forward, i);
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
