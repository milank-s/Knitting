using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrawlerManager : MonoBehaviour
{
    public GameObject crawler;
    private List<Crawler> crawlers;
    private int index;
    public int crawlerCount = 10;
    void Awake()
    {
        crawlers = new List<Crawler>();
        
        for (int i = 0; i < crawlerCount; i++)
        {
            Crawler newCrawler = Instantiate(crawler, transform).GetComponent<Crawler>();
            crawlers.Add(newCrawler);
        }

        Services.main.OnReset += Reset;
    }

    public void AddCrawler(Spline s)
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
            toUse = crawlers[Mathf.Clamp((index % crawlers.Count) + 1, 0, crawlers.Count-1)];
            index++;
        }
        
        
        toUse.Init(s);
    }

    public void Reset()
    {
        foreach (Crawler c in crawlers)
        {
            c.Stop();
        }
    }
}
