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
	public Camera playerCamera;
	public Transform target;
	public float walkRadius; //Remove this when we have better movement logic
	public Transform viewTransform;
	public float fieldOfView;
	public float detectionDistance;
	public float detectionSpeed;
	public float undetectionSpeed;
	private int numSpots = 0; // How many mirrors spotted the monster last frame?
	private float awareness;
	private Vector3 refugePoint = Vector3.positiveInfinity; //Where the monster should run to after it detects itself
	private float idleTime; // is picked at random each time monster enters 'Idle' state.
	public Vector2 idleTimeRange = new Vector2(3f, 12f);
	// The location the monster is interested in, either the player's
	// last location or a source of sound.
	private Vector3 locationOfInterest = Vector3.positiveInfinity;
	public Transform sphere;
	public bool debug = false;
	public bool resetAwareness = false; // temporary debug tool to reset awareness to 0.


	private bool AgentReachedDestination() {
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
		numSpots += 1;
		refugePoint = pathManager.GetRefugePoint(spotter.transform.position).transform.position;
		locationOfInterest = spotter.transform.position;
	}

	public void MonsterNotDetected(MonsterSpotter spotter) {
		
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

        fsm = new StateMachine(this);
        StateMachine patrol = new StateMachine(this, needsExitTime:false);
        fsm.AddState("Patrolling", patrol);
        patrol.AddState("Walking", new State(
        	onEnter: (state) => {
        		pathManager.MoveAgent();
        		agent.speed = .5f;
        		locationOfInterest = Vector3.positiveInfinity;
        		}, debug: debug
        	)
        );
        patrol.AddState("Idle", new State(
        	onEnter: (state) => {
        		locationOfInterest = Vector3.positiveInfinity;
        		idleTime = Random.Range(idleTimeRange[0], idleTimeRange[1]);
        		pathManager.GetNextRandomDestination();
        	},
        	onLogic: (state) => {
        		if(state.timer > idleTime)
        			state.fsm.StateCanExit();
        		},
        		needsExitTime: true, debug: debug));
        patrol.AddTransition(new Transition("Walking", "Idle", 
        	(transition) => AgentReachedDestination()));
        patrol.AddTransition(new Transition("Idle", "Walking"));
        
        fsm.AddState("Run Away", new State(
        	onEnter: (state) => {
        		agent.speed = 1f;
        		agent.SetDestination(refugePoint);
        	}, onLogic: (state) => agent.SetDestination(refugePoint),
        debug: debug));

        fsm.AddState("Suspicious", new State(
        	onEnter: (state) => {
        		animator.SetBool("crouched", false);
        		agent.speed = .75f;
        		agent.SetDestination(locationOfInterest);
        	}, onLogic: (state) => {
        		if(state.timer>15f)
        			state.fsm.StateCanExit();
        		agent.SetDestination(locationOfInterest);
        	}, debug: debug, needsExitTime: true
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
        		}, debug: debug, needsExitTime: true
        ));



        fsm.AddState("Attacking", new State(
        	onEnter: (state) => {
        		animator.SetBool("crouched", false);
        		agent.speed = 1f;
        		agent.SetDestination(locationOfInterest);
        		playerCamera.cullingMask |= 1 << LayerMask.NameToLayer("Monster");
        	},
        	onLogic: (state) => agent.SetDestination(locationOfInterest),
        	onExit: (state) => {
        		playerCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Monster"));
        	}, debug: debug
        ));

        fsm.AddTransitionFromAny(new Transition("", "Run Away", (transition) => numSpots>0));
        fsm.AddTransition(new Transition("Run Away", "Sneaking Up", (transition) => AgentReachedDestination()));
        fsm.AddTransition(new Transition("Patrolling", "Sneaking Up", (transition) => awareness>=.25f));
        fsm.AddTransition(new Transition("Sneaking Up", "Patrolling", (transition) => awareness<.25f));
        fsm.AddTransition(new Transition("Sneaking Up", "Attacking", (transition) => awareness>=.75f && agent.remainingDistance <= 4f));
        fsm.AddTransition(new Transition("Attacking", "Sneaking Up", (transition) => !CheckForPlayer() && AgentReachedDestination() && awareness<=.74f));
        // Don't change
        patrol.SetStartState("Idle");
        fsm.Init();
    }

    void FixedUpdate() {
    	
    }

    // Update is called once per frame
    void Update()
    {
    	CheckForPlayer();
    	fsm.OnLogic();
    	//Debug.Log("awareness:" + awareness);
    	if(agent.enabled)
    		sphere.position = agent.destination;
        Vector3 velocity = transform.InverseTransformDirection(agent.velocity);
        animator.SetFloat("Forward", velocity.z);
        animator.SetFloat("Turn", velocity.x);
        numSpots = 0;
    }
}
