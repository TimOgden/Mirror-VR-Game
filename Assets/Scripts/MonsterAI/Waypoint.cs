using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
	private static readonly Color bidirectionalColor = new Color(0, 254f, 255f);
	private static readonly Color monodirectionalColor = new Color(255f, 25f, 0f);
    public List<Waypoint> neighbors = new List<Waypoint>();
    public bool oneWay;
    public Waypoint previous
    	{ get; set; }
    public float distance
    	{ get; set; }

    void OnDrawGizmos() {
    	if (neighbors == null)
    		return;
    	foreach(var neighbor in neighbors) {
    		if (neighbor!=null) {
    			if(neighbor.neighbors.Contains(this))
    				Gizmos.color = bidirectionalColor;
    			else
    				Gizmos.color = monodirectionalColor;
    			Gizmos.DrawLine(transform.position, neighbor.transform.position);
    		}
    	}
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
