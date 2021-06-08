using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class CoverFinder : MonoBehaviour
{
	public int numberOfPoints;
	public float multiplier;

	public Transform player;
	public float coverQuality;
	public float playerRunInRadius;
	public bool debug;

	private NavMeshAgent agent;
	private Vector3 dest;
	private float phi = (1 + Mathf.Sqrt(5));
	private NavMeshPath path;
	private Vector3[] testPoints;
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        dest = Vector3.positiveInfinity;
        path = new NavMeshPath();
    }

    // Update is called once per frame
    void Update()
    {
    	RaycastHit hit;
    	if(AgentStopped() 
    		&& Physics.Raycast(transform.position, player.position - transform.position, out hit, 30f)
    		&& hit.transform.tag=="Player") {
    		dest = GetOptimalCoverPosition();
    		agent.SetDestination(dest);
    		Debug.Log("Spotted, setting new destination");
    	}
    }

    void OnValidate() {
    	testPoints = GeneratePoints(numberOfPoints, multiplier);
    }

    void OnDrawGizmos() {
    	Gizmos.color = Color.white;
    	for(int i=0; i<testPoints.Length; i++) {
    		Vector3 pos = transform.position + testPoints[i];
    		Gizmos.DrawSphere(pos, .25f);
    	}

    	Gizmos.color = Color.red;
    	Gizmos.DrawSphere(dest, 1f);
    }

    private Vector3 GetOptimalCoverPosition() {
    	List<NavMeshHit> correctPoints = new List<NavMeshHit>();
		for(int i=0; i<testPoints.Length; i++) {
			Vector3 point = testPoints[i] + transform.position;
			NavMeshHit hit;
			if(NavMesh.FindClosestEdge(point, out hit, NavMesh.AllAreas))
				if(Vector3.Dot(hit.normal, (player.position - transform.position))<coverQuality) {
					RaycastHit rayHit;
					if(!Physics.SphereCast(transform.position, playerRunInRadius, hit.position - transform.position, out rayHit)
						|| rayHit.transform.tag!="Player") {
						agent.CalculatePath(hit.position, path);
						if(path.status == NavMeshPathStatus.PathComplete)
							correctPoints.Add(hit);
					}
				}
		}
		correctPoints.Sort((hit_a, hit_b) => hit_a.distance.CompareTo(hit_b.distance));
		return correctPoints[0].position;
    }

    private Vector3[] GeneratePoints(int n, float multiplier, bool debug = false) {
    	Vector3[] points = new Vector3[n];
    	float angle_increment = Mathf.PI * 2 * phi;
    	for(float i = 0; i<n; i+=1f) {
    		float dist = (i / (float)n) * multiplier;
    		float angle = angle_increment * i;
    		float x = dist * Mathf.Cos(angle);
    		float z = dist * Mathf.Sin(angle);
    		Vector3 pos = transform.position + new Vector3(x, 0f, z);
    		points[(int)i] = pos;
    	}

    	return points;
    }

    private bool AgentStopped() {
    	if (!agent.pathPending) {
		    if (agent.remainingDistance <= agent.stoppingDistance) {
		        return !agent.hasPath || agent.velocity.sqrMagnitude == 0f;
     		}
 		}
 		return false;
    }

}
