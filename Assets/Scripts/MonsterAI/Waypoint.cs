using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Waypoint : MonoBehaviour
{
	public static readonly Color unidirectional = new Color(1f, .59f, 1f, 1f);
	public static readonly Color bidirectional = new Color(1f, .59f, 0f, 1f);
    public GameObject waypointPrefab;
	public float probability;
    public List<Waypoint> neighbors = new List<Waypoint>();
    public bool oneWay;
    public bool createNeighbor;
    public bool connect;
    public bool disconnect;
    public Waypoint previous
    	{ get; set; }
    public float distance
    	{ get; set; }

    void OnDrawGizmos() {
        Gizmos.color = new Color(0.01f, 0.84f, 1f, .25f);
    	Gizmos.DrawSphere(transform.position, .5f);
        if (neighbors == null)
            return;
    	foreach(var neighbor in neighbors) {
    		if (neighbor!=null) {
    			if(neighbor.neighbors.Contains(this))
    				Gizmos.color = bidirectional;
    			else
    				Gizmos.color = unidirectional;
    			Gizmos.DrawLine(transform.position, neighbor.transform.position);
    		}
    	}
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, Vector3.up * probability * 15f);
    }


    void OnValidate() {
    	if(!oneWay) {
	    	foreach (var neighbor in neighbors) {
	    		if(neighbor != null) {
		    		if(neighbor.neighbors.Contains(this))
		    			continue;
		    		neighbor.neighbors.Add(this);
		    	}
	    	}
	    }
	    if(oneWay) {
	    	foreach(var neighbor in neighbors) {
	    		if(neighbor != null) {
	    			if(neighbor.neighbors.Contains(this))
	    				neighbor.neighbors.Remove(this);
	    		}
	    	}
	    }

        if(createNeighbor) {
            if(Selection.objects.Length>1) {
                Debug.Log("Too many waypoints selected, adjacent nodes would be ambiguous.");
                return;
            }
            createNeighbor = false;
            Waypoint newPoint = Object.Instantiate(waypointPrefab, transform.parent).GetComponent<Waypoint>();
            newPoint.neighbors = new List<Waypoint>();
            newPoint.neighbors.Add(this);
            if(!oneWay)
                neighbors.Add(newPoint);
            Selection.activeGameObject = newPoint.gameObject;
        }

        if(connect) {
            connect = false;
            if(Selection.objects.Length<2) {
                Debug.Log("Not enough waypoints selected!");
                return;
            }
            foreach(GameObject waypoint in Selection.objects) {
                if(waypoint.TryGetComponent(out Waypoint way)) {
                    if(way==this)
                        continue;
                    neighbors.Add(way);
                }
            }
        }

        if(disconnect) {
            disconnect = false;
            if(Selection.objects.Length<2) {
                Debug.Log("Not enough waypoints selected!");
                return;
            }
            foreach(GameObject a in Selection.objects) {
                foreach(GameObject b in Selection.objects) {
                    if(a.TryGetComponent(out Waypoint waypoint_a) && b.TryGetComponent(out Waypoint waypoint_b)) {
                        if(waypoint_a.neighbors.Contains(waypoint_b))
                            waypoint_a.neighbors.Remove(waypoint_b);
                    }
                }
            }
        }
    }

    void OnDisable() {
    	foreach(var neighbor in neighbors) {
    		if(neighbor != null) {
    			if(neighbor.neighbors.Contains(this))
    				neighbor.neighbors.Remove(this);
    		}
    	}
    }
}
