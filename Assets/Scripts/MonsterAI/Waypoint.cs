using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
	public static readonly Color unidirectional = new Color(1f, .59f, 1f, 1f);
	public static readonly Color bidirectional = new Color(1f, .59f, 0f, 1f);
	public float probability;
    public float chosen;
    public List<Waypoint> neighbors = new List<Waypoint>();
    public bool oneWay;
    public Waypoint previous
    	{ get; set; }
    public float distance
    	{ get; set; }

    void OnDrawGizmos() {
        Gizmos.color = new Color(0f, 0f, 0f, .5f);
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
