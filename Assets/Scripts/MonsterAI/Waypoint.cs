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
    public Waypoint previous
    	{ get; set; }
    public float distance
    	{ get; set; }


    void Start() {
        List<Waypoint> toRemove = new List<Waypoint>();
        foreach(var neighbor in neighbors) {
            if(ReferenceEquals(neighbor, null) ? false : (neighbor ? false : true))
                toRemove.Add(neighbor);
        }

        foreach(var neighbor in toRemove) {
            neighbors.Remove(neighbor);
        }
    }

    void OnDrawGizmos() {
        Gizmos.color = new Color(0.01f, 0.84f, 1f, .25f);
        foreach(GameObject go in Selection.objects) {
            if(go==this.gameObject)
                Gizmos.color = new Color(1f, .9f, 0f, .5f);
        }
            
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

    public void ToggleOneWay() {
        oneWay = !oneWay;
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

    public void CreateNeighbor() {
        if(Selection.objects.Length>1) {
            Debug.Log("Too many waypoints selected, adjacent nodes would be ambiguous.");
            return;
        }
        Waypoint newPoint = Object.Instantiate(waypointPrefab, transform.parent).GetComponent<Waypoint>();
        newPoint.neighbors = new List<Waypoint>();
        newPoint.neighbors.Add(this);
        if(!oneWay)
            neighbors.Add(newPoint);
        Selection.activeGameObject = newPoint.gameObject;
    }

    public void Connect() {
        foreach(GameObject waypoint_a in Selection.objects) {
            foreach(GameObject waypoint_b in Selection.objects) {
                if(waypoint_a.TryGetComponent(out Waypoint way_a) && waypoint_b.TryGetComponent(out Waypoint way_b)) {
                    if(way_b==way_a)
                        continue;
                    if(!way_a.neighbors.Contains(way_b))
                        way_a.neighbors.Add(way_b);
                }
            }
        }
    }

    public void Disconnect() {
        foreach(GameObject a in Selection.objects) {
            foreach(GameObject b in Selection.objects) {
                if(a.TryGetComponent(out Waypoint waypoint_a) && b.TryGetComponent(out Waypoint waypoint_b)) {
                    if(waypoint_a.neighbors.Contains(waypoint_b))
                        waypoint_a.neighbors.Remove(waypoint_b);
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


[CustomEditor(typeof(Waypoint)), CanEditMultipleObjects]
public class WaypointEditor : Editor {
    Waypoint waypoint;
    public override void OnInspectorGUI() {
        waypoint = target as Waypoint;
        waypoint.waypointPrefab = waypoint.gameObject;
        //waypoint.waypointPrefab = EditorGUILayout.ObjectField("Waypoint Prefab", waypoint.waypointPrefab, typeof(GameObject), false) as GameObject;
        //waypoint.neighbors = EditorGUILayout.ObjectField("Neighbors", waypoint.neighbors, typeof(List<GameObject>), false);
        if(waypoint.oneWay) {
            if(GUILayout.Button("Convert to Two-Way"))
                waypoint.ToggleOneWay();
        } else {
            if(GUILayout.Button("Convert to One-Way"))
                waypoint.ToggleOneWay();
        }
        if(GUILayout.Button("Add Adjacent Node")) {
            waypoint.CreateNeighbor();
        }
        if(Selection.objects.Length>1) {
            if(GUILayout.Button("Connect"))
                waypoint.Connect();
            if(GUILayout.Button("Disconnect")) {
                waypoint.Disconnect();
            }
        }

        if(Selection.objects.Length==2 
            && ((GameObject)Selection.objects[0]).TryGetComponent(out Waypoint a) 
            && ((GameObject)Selection.objects[1]).TryGetComponent(out Waypoint b)) {
            if(GUILayout.Button("Create Midpoint")) {
                CreateMidpoint(a, b);
            }
        }
    }


    private void CreateMidpoint(Waypoint a, Waypoint b) {
        Waypoint c = Object.Instantiate(waypoint.gameObject, (a.transform.position + b.transform.position)/2, Quaternion.identity).GetComponent<Waypoint>();
        if(a.neighbors.Contains(b))
            a.neighbors.Remove(b);
        if(b.neighbors.Contains(a))
            b.neighbors.Remove(a);
        c.neighbors = new List<Waypoint>();
        c.neighbors.Add(a);
        c.neighbors.Add(b);

        if(!c.oneWay) {
            a.neighbors.Add(c);
            b.neighbors.Add(c);
        }

        Selection.activeGameObject = c.gameObject;
    }
}