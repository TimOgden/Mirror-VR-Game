using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
	public static readonly Color unidirectional = new Color(1f, .59f, 1f, 1f);
	public static readonly Color bidirectional = new Color(1f, .59f, 0f, 1f);
	public float redChannel = 0f;
	public float greenChannel = 0f;
    public List<Waypoint> neighbors = new List<Waypoint>();
    public bool oneWay;
    public Waypoint previous
    	{ get; set; }
    public float distance
    	{ get; set; }

    void OnDrawGizmos() {
    	if (neighbors == null)
    		return;
    	Gizmos.color = new Color(redChannel, greenChannel, 0f, 1f);
    	Gizmos.DrawSphere(transform.position, .5f);
    	foreach(var neighbor in neighbors) {
    		if (neighbor!=null) {
    			if(neighbor.neighbors.Contains(this))
    				Gizmos.color = bidirectional;
    			else
    				Gizmos.color = unidirectional;
    			Gizmos.DrawLine(transform.position, neighbor.transform.position);
    		}
    	}
    }

    public IEnumerator ResetRedChannel() {
    	yield return new WaitForSeconds(3f);
    	redChannel = 0f;
    }

    public IEnumerator ResetGreenChannel() {
    	yield return new WaitForSeconds(3f);
    	greenChannel = 0f;
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
