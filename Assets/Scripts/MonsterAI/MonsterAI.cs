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


	private bool AgentReachedDestination(Vector3 destination) {
		return (transform.position - destination).magnitude < agent.stoppingDistance;
		//float dist = agent.remainingDistance;
		//return dist!=Mathf.Infinity && agent.pathStatus==NavMeshPathStatus.PathComplete
		//	&& dist<=agent.stoppingDistance;
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
        		agent.SetDestination(locationOfInterest);
        		agent.speed = .5f;
        	}, onLogic: (state) => {
        		if(numSpots > 0)
        			fsm.RequestStateChange("Run Away", forceInstantly: true);
        	}, debug: debug
        ));
        patrol.AddState("Idle", new State(
        	onEnter: (state) => {
        		idleTime = Random.Range(idleTimeRange[0], idleTimeRange[1]);
        		locationOfInterest = pathManager.GetNextRandomDestination().transform.position;
        	},
        	onLogic: (state) => {
        		if(numSpots > 0) {
        			fsm.RequestStateChange("Run Away", forceInstantly: true);
        		}
        		if(state.timer > idleTime)
        			state.fsm.StateCanExit();
        		},
        		needsExitTime: true, debug: debug
        ));
        patrol.AddTransition(new Transition("Walking", "Idle", 
        	(transition) => AgentReachedDestination(agent.destination)));
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
        		if(numSpots > 0)
        			fsm.RequestStateChange("Run Away", forceInstantly: true);
        		agent.SetDestination(RandomNavMeshPoint(locationOfInterest,2f));
        		Debug.Log(locationOfInterest);
        	}, debug: debug, needsExitTime: false
        ));

        fsm.AddState("Sneaking Up", new State(
        	onEnter: (state) => {
        		animator.SetBool("crouched", true);
        		agent.speed = .5f;
        		agent.SetDestination(locationOfInterest);
        		},
        	onLogic: (state) => {
        		if(numSpots > 0)
        			fsm.RequestStateChange("Run Away", forceInstantly: true);
    			agent.SetDestination(locationOfInterest);
        	},
        	onExit: (state) => {
        		animator.SetBool("crouched", false);
        		}, debug: debug, needsExitTime: false
        ));



        fsm.AddState("Attacking", new State(
        	onEnter: (state) => {
        		animator.SetBool("crouched", false);
        		agent.speed = 1f;
        		agent.SetDestination(locationOfInterest);
        		playerCamera.cullingMask |= 1 << LayerMask.NameToLayer("Monster");
        	},
        	onLogic: (state) => {
        		if(numSpots > 0)
        			fsm.RequestStateChange("Run Away", forceInstantly: true);
        		agent.SetDestination(locationOfInterest);
        	},
        	onExit: (state) => {
        		playerCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Monster"));
        	}, debug: debug
        ));

        fsm.AddTransitionFromAny(new Transition("", "Run Away", (transition) => numSpots>0));
        fsm.AddTransition(new Transition("Run Away", "Suspicious", (transition) => AgentReachedDestination(refugePoint)));
        fsm.AddTransition(new Transition("Patrolling","Suspicious",(transition) => awareness>=.1f));
        fsm.AddTransition(new Transition("Suspicious", "Patrolling", (transition) => awareness<.1f && AgentReachedDestination(locationOfInterest)));
        fsm.AddTransition(new Transition("Suspicious", "Sneaking Up", (transition) => awareness>=.25f));
        fsm.AddTransition(new Transition("Sneaking Up", "Suspicious", (transition) => awareness<.25f));
        fsm.AddTransition(new Transition("Sneaking Up", "Attacking", (transition) => awareness>=.75f && agent.remainingDistance <= 4f));
        fsm.AddTransition(new Transition("Attacking", "Suspicious", (transition) => !CheckForPlayer() && AgentReachedDestination(locationOfInterest) && awareness<=.74f));
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
    	
    	//Debug.Log("awareness:" + awareness);
    	refugeSphere.position = new Vector3(refugePoint.x, 0f,  refugePoint.z);;
		locationOfInterestSphere.position = new Vector3(locationOfInterest.x, 0f, locationOfInterest.z);
		destinationSphere.position = agent.destination;
        Vector3 velocity = transform.InverseTransformDirection(agent.velocity);
        animator.SetFloat("Forward", velocity.z);
        animator.SetFloat("Turn", velocity.x);
        fsm.OnLogic();
        numSpots = 0;
    }
}
