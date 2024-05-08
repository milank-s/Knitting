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
        return GetNextPoint(p, target);
    }

    static List<Point> reconstruct_path(Dictionary<Point, Point> cameFrom, Point current){

    List<Point> totalPath = new List<Point>();
    totalPath.Add(current);

    while(cameFrom.ContainsKey(current)){
        current = cameFrom[current];
        totalPath.Add(current);
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

    //gets next point and appropriate spline to get to destination
    public static List<Point> GetNextPoint(Point start, Point goal){
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

            float curMin = Mathf.Infinity;

            foreach (Point neighbor in cur._neighbours){
                // d(current,neighbor) is the weight of the edge from current to neighbor
                // tentative_gScore is the distance from start to the neighbor through current
                float curDist = toPoint[cur] + Vector3.Distance(cur.Pos, neighbor.Pos);
                //can use spline distance[] array, but need to make sure you're not using
                //the wrong indices based on direction

                
                bool newRoute = false;
                if(toPoint.ContainsKey(neighbor)){
                    toPoint[neighbor] = curDist;
                }else{
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
