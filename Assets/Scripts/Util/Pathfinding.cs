using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{

    //how can you avoid running this?
    //agents only update when the player has changed their current point?
    //what happens if the player is flying, or they are not connected to the agents spline?
    //are you populating distances well enough to use A star?



    //use static reference to player 
    //return next point
    public static Dictionary<Point, int> distToPlayer;
    public static Point furthestPoint;
    
    public static List<Point> FindPlayer(Point p){
        Point target;
        switch(Services.PlayerBehaviour.state){
            case PlayerState.Traversing:
                target = Services.PlayerBehaviour.pointDest;
            break;

            case PlayerState.Switching:
                target = Services.PlayerBehaviour.curPoint;
            break;

            default:
            return null;

        }
        return GetCriticalPath(p, target);
    }
    
    

    //this is a greedy search and very stupid
    public static List<Point> EscapePlayer(){
        Point start = Services.PlayerBehaviour.curPoint;
        List<Point> totalPath = new List<Point>();
        
        Point cur = start;
        bool running = true;
        while(running){
            
            totalPath.Add(cur);
            float curDist = cur.distanceFromPlayer;
            
            bool done = true;
            foreach(Point p in cur._neighbours){
    
                if(p.distanceFromPlayer > curDist){
                    done = false;
                    cur = p;
                    curDist = p.distanceFromPlayer;
                }
            }

            if(done) running = false;
        }

        return totalPath;
    }

    public static void PopulateGraphDistances(Point start){
        
        HashSet<Point> visited = new HashSet<Point>();
        Queue<Point> q = new Queue<Point>();
        q.Enqueue(start);
        start.distanceFromPlayer = 0;
        visited.Add(start);
        float d = 0;

        while(q.Count > 0){
                //pop
            Point cur = q.Dequeue();
            foreach(Point p in cur._neighbours){
                
                //get distance to neighbour through current point
                float curDist = cur.distanceFromPlayer;
                float newDist = curDist + Vector3.Distance(p.Pos, cur.Pos);
                
                //if this isnt in the map, add it       
                if(!visited.Contains(p)){
                   
                    if(newDist > d){
                        d = newDist;
                        furthestPoint = p;
                    }
                    
                   visited.Add(p);
                   p.distanceFromPlayer = newDist;
                   q.Enqueue(p);

                //else, check whether its shorter than the current distance
                }else{
                    if(newDist < p.distanceFromPlayer){
                        p.distanceFromPlayer = newDist;
                    }
                }

            }
        }
    }
    

    static List<Point> reconstruct_path(Dictionary<Point, Point> cameFrom, Point current){

    HashSet<Point> visited = new HashSet<Point>();
    visited.Add(current);
    List<Point> totalPath = new List<Point>();
    totalPath.Add(current);

    while(cameFrom.ContainsKey(current)){    

        current = cameFrom[current];
        totalPath.Add(current);
        if(visited.Contains(current)) break;

        visited.Add(current);
    }

    totalPath.Reverse();
    
    string n = "";
    for(int i = 0; i < totalPath.Count; i++){
        n += totalPath[i].name;

        if(i < totalPath.Count -1){
            n += " > ";
        }
    }
    
    // Debug.Log(n);
    return totalPath;

    }
    
    //Like get critical path but it uses weight for point's distance from player

    public static List<Point> GetLongestPath(Point start, Point goal){
        List<Point> openSet = new List<Point>
        {
            start
        };
        
        HashSet<Point> visited = new HashSet<Point>
        {
            start
        };

        Dictionary<Point, Point> cameFrom = new Dictionary<Point, Point>();
        Dictionary<Point, float> toPoint = new Dictionary<Point, float>
        {
            { start, 0 }
        };

        while (openSet.Count > 0) {
            Point cur = openSet[0];

            if (cur == goal){
                return reconstruct_path(cameFrom, cur);
                //return next point in sequence
            }

            openSet.Remove(cur);

            foreach (Point neighbor in cur._neighbours){
                
                float curDist = neighbor.distanceFromPlayer;
                bool newRoute = false;

                if(!toPoint.ContainsKey(neighbor)){
                    newRoute = true;
                    toPoint.Add(neighbor, curDist);
                }
                
                if (newRoute || toPoint[neighbor] > curDist){
                    if(!cameFrom.ContainsKey(neighbor)){
                        cameFrom.Add(neighbor, cur);
                    }else{
                        cameFrom[neighbor] = cur;
                    }

                    //toPoint[neighbor] = curDist;

                    if (!visited.Contains(neighbor)){
                        visited.Add(neighbor);
                        openSet.Add(neighbor);
                    }
                }
            }
        }
        return null;
    }

    //gets next point and appropriate spline to get to destination
    public static List<Point> GetCriticalPath(Point start, Point goal){
        // The set of discovered nodes that may need to be (re-)expanded.
        // Initially, only the start node is known.
        // This is usually implemented as a min-heap or priority queue rather than a hash-set.
        List<Point> openSet = new List<Point>();
        openSet.Add(start);

        // For node n, cameFrom[n] is the node immediately preceding it on the cheapest path from the start
        // to n currently known.
        Dictionary<Point, Point> cameFrom = new Dictionary<Point, Point>();

        // For node n, gScore[n] is the cost of the cheapest path from start to n currently known.
        Dictionary<Point, float> toPoint = new Dictionary<Point, float>();
        toPoint.Add(start, 0);

        // For node n, fScore[n] := gScore[n] + h(n). fScore[n] represents our current best guess as to
        // how cheap a path could be from start to finish if it goes through n.
        Dictionary<Point, float> toEnd = new Dictionary<Point, float>();
        toEnd.Add(start, Vector3.Distance(start.Pos, goal.Pos));

        while (openSet.Count > 0) {
            // This operation can occur in O(Log(N)) time if openSet is a min-heap or a priority queue
            // current := the node in openSet having the lowest fScore[] value
            Point cur = openSet[0];

            if (cur == goal){
                return reconstruct_path(cameFrom, cur);
                //return next point in sequence
            }

            openSet.Remove(cur);

            foreach (Point neighbor in cur._neighbours){
                // d(current,neighbor) is the weight of the edge from current to neighbor
                // tentative_gScore is the distance from start to the neighbor through current
                float curDist = toPoint[cur] + Vector3.Distance(cur.Pos, neighbor.Pos);
                //can use spline distance[] array, but need to make sure you're not using
                //the wrong indices based on direction
                
                bool newRoute = false;
                if(!toPoint.ContainsKey(neighbor)){
                    newRoute = true;
                    toPoint.Add(neighbor, curDist);
                }
                //what if we dont have toPoint[neighbor]
                //how could we have it if we are just reaching it now?
                if (newRoute || curDist < toPoint[neighbor]){
                    // This path to neighbor is better than any previous one. Record it!
                    if(!cameFrom.ContainsKey(neighbor)){
                        cameFrom.Add(neighbor, cur);
                    }else{
                        cameFrom[neighbor] = cur;
                    }

                    toPoint[neighbor] = curDist;
                    float total = curDist + Vector3.Distance(neighbor.Pos, goal.Pos);

                    if(toEnd.ContainsKey(neighbor)){
                        toEnd[neighbor] = total;
                    }else{
                        toEnd.Add(neighbor, total);
                    }
                    

                    if (!openSet.Contains(neighbor)){
                        openSet.Add(neighbor);
                    }
                }
            }
        }

        Debug.Log("never reached goal");

        return null;
    }

}
