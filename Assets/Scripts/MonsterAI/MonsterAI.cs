using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using FSM;
public class MonsterAI : MonoBehaviour
{
	public StateMachine fsm 
		{ get; set; }
	private Animator animator;
	private PathManager pathManager;
	private NavMeshAgent agent;
    private MonsterSounds sounds;
	public Camera playerCamera;
	public Transform target;
	public float walkRadius; //Remove this when we have better movement logic
	public Transform viewTransform;
	public float fieldOfView;
	public float detectionDistance;
	public float detectionSpeed;
	public float undetectionSpeed;
	public float selfDetectionSpeed;
	private float awareness;
    private float selfAwareness;
	private Vector3 refugePoint = Vector3.zero; //Where the monster should run to after it detects itself
	private float idleTime; // is picked at random each time monster enters 'Idle' state.
	public Vector2 idleTimeRange = new Vector2(3f, 12f);
	// The location the monster is interested in, either the player's
	// last location or a source of sound.
	private Vector3 locationOfInterest = Vector3.zero;
	public Transform refugeSphere;
	public Transform locationOfInterestSphere;
	public Transform destinationSphere;
	public bool debug = false;
	public bool resetAwareness = false; // temporary debug tool to reset awareness to 0.


	private bool AgentReachedDestination() {
		//return (transform.position - destination).magnitude <= agent.stoppingDistance || agent.pathStatus==NavMeshPathStatus.PathComplete;
		float dist = agent.remainingDistance;
		return dist!=Mathf.Infinity && agent.pathStatus==NavMeshPathStatus.PathComplete
			&& dist<=agent.stoppingDistance;
	}


	private Vector3 RandomNavMeshPoint(Vector3 position, float radius) {
		Vector3 randomDirection = Random.insideUnitSphere * radius;
		randomDirection += position;
		NavMeshHit hit;
		NavMesh.SamplePosition(randomDirection, out hit, walkRadius, 1);
		return hit.position;
	}


	public void MonsterDetected(MonsterSpotter spotter) {
		// monster saw itself, note location of spotter and set refugePoint to
		// opposite direction of spotter and maybe find hiding point near there.
        selfAwareness += selfDetectionSpeed * Mathf.Pow((spotter.transform.position - transform.position).magnitude, -2f) * Time.deltaTime;
	}

	public void MonsterNotDetected(MonsterSpotter spotter) {
		selfAwareness -= undetectionSpeed * Time.deltaTime;
	}

	private bool CheckForPlayer() {
		RaycastHit hit;
		if(Vector3.Angle(viewTransform.right, (target.position - viewTransform.position))<=fieldOfView) {
			if(Physics.Raycast(viewTransform.position, (target.position - viewTransform.position), out hit, detectionDistance)
				&& hit.transform.tag == "Player") {
				awareness += detectionSpeed * Mathf.Pow(hit.distance,-2) * Time.deltaTime;
				awareness = Mathf.Clamp(awareness,0f,1f);
				Debug.DrawRay(viewTransform.position, (target.position - viewTransform.position).normalized * detectionDistance, Color.red);
				locationOfInterest = hit.transform.position;
				return true;
			} else {
				Debug.DrawRay(viewTransform.position, (target.position - viewTransform.position).normalized * detectionDistance, Color.white);
			}
		}
		awareness -= undetectionSpeed * Time.deltaTime;
		awareness = Mathf.Clamp(awareness,0f,1f);
		return false;
	}


	void OnValidate() {
		if(resetAwareness) {
			awareness = 0f;
			resetAwareness = false;
		}
	}
    // Start is called before the first frame update
    void Start()
    {
    	animator = GetComponent<Animator>();
    	agent = GetComponent<NavMeshAgent>();
    	pathManager = GetComponent<PathManager>();
        sounds = GetComponent<MonsterSounds>();

        fsm = new StateMachine(this);
        StateMachine patrol = new StateMachine(this, needsExitTime:false);
        fsm.AddState("Patrolling", patrol);
        patrol.AddState("Walking", new State(
        	onEnter: (state) => {
                if(pathManager.nextWaypoint==null)
                    pathManager.GetNextRandomDestination();
        		agent.SetDestination(pathManager.nextWaypoint.transform.position);
        		agent.speed = .5f;
        	}, debug: debug
        ));
        patrol.AddState("Idle", new State(
        	onEnter: (state) => {
                pathManager.ResetWaypointProbabilities(); // remove after debugging
        		idleTime = Random.Range(idleTimeRange[0], idleTimeRange[1]);
        		pathManager.GetNextRandomDestination();
        	},
        	onLogic: (state) => {
        		if(state.timer > idleTime)
        			state.fsm.StateCanExit();
        		},
        		needsExitTime: true, debug: debug
        ));
        patrol.AddTransition(new Transition("Walking", "Idle", 
        	(transition) => AgentReachedDestination()));
        patrol.AddTransition(new Transition("Idle", "Walking"));
        
        fsm.AddState("Run Away", new State(
        	onEnter: (state) => {
        		animator.SetBool("crouched", false);
        		agent.speed = 1f;
        		agent.SetDestination(refugePoint);
        	},
        debug: debug));

        fsm.AddState("Suspicious", new State(
        	onEnter: (state) => {
        		animator.SetBool("crouched", false);
        		awareness = Mathf.Max(awareness, .2f);
        		agent.speed = .6f;
        	}, onLogic: (state) => {
        		agent.SetDestination(RandomNavMeshPoint(locationOfInterest,2f));
        	}, debug: debug, needsExitTime: false
        ));

        fsm.AddState("Sneaking Up", new State(
        	onEnter: (state) => {
        		animator.SetBool("crouched", true);
        		agent.speed = .5f;
        		agent.SetDestination(locationOfInterest);
        		},
        	onLogic: (state) => {
    			agent.SetDestination(locationOfInterest);
        	},
        	onExit: (state) => {
        		animator.SetBool("crouched", false);
        		}, debug: debug, needsExitTime: false
        ));



        fsm.AddState("Attacking", new State(
        	onEnter: (state) => {
        		animator.SetBool("crouched", false);
                sounds.PlayDetection();
        		agent.speed = 1f;
        		agent.SetDestination(locationOfInterest);
        		playerCamera.cullingMask |= 1 << LayerMask.NameToLayer("Monster");
        	},
        	onLogic: (state) => {
        		agent.SetDestination(locationOfInterest);
        	},
        	onExit: (state) => {
        		playerCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Monster"));
        	}, debug: debug
        ));

        //fsm.AddTransitionFromAny(new Transition("", "Run Away", (transition) => selfAwareness>=.1f));
        fsm.AddTransition(new Transition("Run Away", "Suspicious", (transition) => AgentReachedDestination()));
        fsm.AddTransition(new Transition("Patrolling","Suspicious",(transition) => awareness>=.1f));
        fsm.AddTransition(new Transition("Suspicious", "Patrolling", (transition) => awareness<.1f && AgentReachedDestination()));
        fsm.AddTransition(new Transition("Suspicious", "Sneaking Up", (transition) => awareness>=.25f));
        fsm.AddTransition(new Transition("Sneaking Up", "Suspicious", (transition) => awareness<.25f));
        fsm.AddTransition(new Transition("Sneaking Up", "Attacking", (transition) => awareness>=.75f && agent.remainingDistance <= 4f));
        fsm.AddTransition(new Transition("Attacking", "Suspicious", (transition) => !CheckForPlayer() && AgentReachedDestination() && awareness<=.74f));
        // Don't change
        patrol.SetStartState("Idle");
        fsm.Init();
    }

    void FixedUpdate() {
    	
    }

    // Update is called once per frame
    void Update()
    {
        selfAwareness = Mathf.Clamp(selfAwareness, 0f, 1f);
    	CheckForPlayer();
    	if(selfAwareness>=.1f && fsm.ActiveStateName != "Run Away") {
            refugePoint = pathManager.GetRefugePoint(target.transform.position).transform.position;
            locationOfInterest = target.transform.position;
            fsm.RequestStateChange("Run Away", forceInstantly: true);
        }
        if(debug) {
        	refugeSphere.position = new Vector3(refugePoint.x, 0f,  refugePoint.z);;
    		locationOfInterestSphere.position = new Vector3(locationOfInterest.x, 0f, locationOfInterest.z);
    		destinationSphere.position = agent.destination;
        }
        Vector3 velocity = transform.InverseTransformDirection(agent.velocity);
        animator.SetFloat("Forward", velocity.z);
        animator.SetFloat("Turn", velocity.x);
        fsm.OnLogic();
    }
}
