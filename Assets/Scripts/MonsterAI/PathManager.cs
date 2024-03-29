using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

public class PathManager : MonoBehaviour
{
	private GameObject[] allWaypoints;
	public float temperature;
	private NavMeshAgent agent;
	private Animator animator;
	public Transform player;
	public bool debug = false;

	private Stack<Vector3> shortestPath;
	private bool followedLastOptimalNode = false;
	private Waypoint currentWaypoint;
	public Waypoint nextWaypoint;

	public void NavigateTo(Vector3 destination) {
		shortestPath = new Stack<Vector3>();
		Waypoint currentNode = FindClosestWaypoint(transform.position);
		Waypoint endNode = FindClosestWaypoint(destination);
		if(currentNode == null || endNode == null || currentNode == endNode)
			return;

		SortedList<float, Waypoint> openList = new SortedList<float, Waypoint>();
		List<Waypoint> closedList = new List<Waypoint>();
		openList.Add(0, currentNode);
		currentNode.previous = null;
		currentNode.distance = 0f;

		while(openList.Count > 0) {
			currentNode = openList.Values[0];
			openList.RemoveAt(0);
			float dist = currentNode.distance;
			closedList.Add(currentNode);
			if(currentNode == endNode)
				break;
			foreach(var neighbor in currentNode.neighbors) {
				if(closedList.Contains(neighbor) || openList.ContainsValue(neighbor))
					continue;
				if(neighbor==null) {
					currentNode.neighbors.Remove(neighbor);
					continue;
				}
				neighbor.previous = currentNode;
				neighbor.distance = dist + (neighbor.transform.position - currentNode.transform.position).magnitude;
				var distanceToTarget = (neighbor.transform.position - endNode.transform.position).magnitude;
				openList.Add(neighbor.distance + distanceToTarget, neighbor);
			}
		}
		if(currentNode == endNode) {
			while(currentNode.previous != null) {
				shortestPath.Push(currentNode.transform.position);
				currentNode = currentNode.previous;
			}
			shortestPath.Push(transform.position);
		}
	}

	public void ClearPath() {
		shortestPath = null;
	}

	public void ResetWaypointProbabilities() {
		foreach(GameObject waypoint in allWaypoints) {
			waypoint.GetComponent<Waypoint>().probability = 0f;
		}
	}

	private Waypoint FindClosestWaypoint(Vector3 target) {
		GameObject closest = null;
		float closestDist = Mathf.Infinity;
		foreach(var waypoint in allWaypoints) {
			var dist = (waypoint.transform.position - target).sqrMagnitude;
			if(dist < closestDist) {
				closest = waypoint;
				closestDist = dist;
			}
		}
		if(closest != null) {
			return closest.GetComponent<Waypoint>();
		}
		return null;
	}

	private bool AgentReachedDestination() {
		float dist = agent.remainingDistance;
		return dist!=Mathf.Infinity && agent.pathStatus==NavMeshPathStatus.PathComplete
			&& dist<=agent.stoppingDistance;
	}

    // Start gets called before the first frame update
    void Start()
    {
    	allWaypoints = GameObject.FindGameObjectsWithTag("Waypoint");
    	agent = GetComponent<NavMeshAgent>();
    	animator = GetComponent<Animator>();
    }

    private int RandomChoice(List<float> probabilities) {
    	float x = Random.Range(0f,probabilities.Sum());
    	for(int i = 0; i < probabilities.Count; i++) {
    		if((x -= probabilities[i]) < 0 ) {
    			return i;
    		}
    	}
    	return probabilities.Count - 1;
    }

    public void GetNextRandomDestination() {
    	//if(!followedLastOptimalNode)
    	NavigateTo(player.position);
    	if(shortestPath.Count <= 0)
    		return;
    	Waypoint currentNode = FindClosestWaypoint(shortestPath.Pop());
    	currentWaypoint = currentNode;
    	int num_neighbors = currentNode.neighbors.Count;
    	int remainingDistanceInOptimalPath = shortestPath.Count;
    	//shortestPath.Pop(); //skip the first node which is where monster already is
    	Waypoint nextOptimalNode = FindClosestWaypoint(shortestPath.Pop());
    	shortestPath.Push(nextOptimalNode.transform.position); //put back on stack to not mess up next step
    	List<Waypoint> neighbors = new List<Waypoint>();
    	List<float> probabilities = new List<float>();
    	neighbors.Add(nextOptimalNode);
    	probabilities.Add((remainingDistanceInOptimalPath+1)/num_neighbors);
    	foreach(Waypoint waypoint in currentNode.neighbors) {
    		if(nextOptimalNode==waypoint) {
    			continue;
    		}
    		neighbors.Add(waypoint);
    		probabilities.Add(1/num_neighbors);
    	}
    	
    	var z_exp = probabilities.Select(i => Mathf.Exp(i / temperature));
    	var sum_z_exp = z_exp.Sum();
    	var softmax = z_exp.Select(i => i/sum_z_exp).ToList();
    	for(int i = 0; i<neighbors.Count; i++) {
    		neighbors[i].probability = softmax[i];
    	}
    	if(debug) {
	    	for(int i=0; i<neighbors.Count; i++) {
	    		Debug.Log("Neighbor " + i + ": " + softmax[i]*100 + "%");
	    	}
	    }
    	int rand_index = RandomChoice(softmax);
    	if(debug)
    		Debug.Log("Chosen index: " + rand_index);
    	if(rand_index == 0)
    		followedLastOptimalNode = true;
    	else
    		followedLastOptimalNode = false;
    	nextWaypoint = neighbors[rand_index];
    }

    private Waypoint GetNeighborInDirection(Waypoint origin, Vector3 direction) {
    	float smallest_angle = Mathf.Infinity;
    	Waypoint smallest_angle_waypoint = null;
    	foreach(var neighbor in origin.neighbors) {
			if(neighbor==null)
				continue;
			float angle = Vector3.Angle(direction, (neighbor.transform.position - origin.transform.position));
			if(angle<smallest_angle) {
				smallest_angle = angle;
				smallest_angle_waypoint = neighbor;
			}
    	}
    	return smallest_angle_waypoint;
    }

    public Waypoint GetRefugePoint(Vector3 mirrorPosition) {
    	if(currentWaypoint==null) {
    		currentWaypoint = FindClosestWaypoint(transform.position);
    	}
    	return GetNeighborInDirection(currentWaypoint, (transform.position - mirrorPosition));
    }


    // Update is called once per frame
    void Update()
    {
    	

    }
}
