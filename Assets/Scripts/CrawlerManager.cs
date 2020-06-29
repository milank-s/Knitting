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
    }

    public void AddCrawler(List<Point> p, float f)
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
            toUse = crawlers[index % crawlers.Count];
            index++;
        }
        
        toUse.startSpline = p[0].GetConnectingSpline(p[1]);
        toUse.points = p;
        toUse.speed = f;
        
        toUse.Init();
    }

    public void Reset()
    {
        foreach (Crawler c in crawlers)
        {
            c.Stop();
        }
    }
}
