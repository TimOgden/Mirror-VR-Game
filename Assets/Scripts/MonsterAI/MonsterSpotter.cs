using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class MonsterSpotter : MonoBehaviour
{
	public MonsterAI monsterAI;
    public Transform monster_transform;
	public float maxDistance;
	public float fieldOfView;
	[SerializeField]
	private LayerMask layerMask;

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    	if(MonsterToMirror() && MirrorToMonster()) {
    		monsterAI.MonsterDetected(this);
    	} else {
            monsterAI.MonsterNotDetected(this);
        }
    }

    private bool MonsterToMirror() {
    	RaycastHit hit;
        if(Mathf.Abs(Vector3.Angle(transform.position - monster_transform.position, monster_transform.right))<=fieldOfView) {
        	if (Physics.Raycast(monster_transform.position, transform.position - monster_transform.position, out hit, maxDistance, layerMask)) {
        		if(hit.transform.tag=="Mirror") {
        			Debug.DrawLine(monster_transform.position, hit.transform.position, Color.yellow);
        			return true;
        		}
        	}
        }
        return false;
    }

    private bool MirrorToMonster() {
    	RaycastHit hit;
    	if(Mathf.Abs(Vector3.Angle(monster_transform.position - transform.position, transform.up))<=fieldOfView) {
			Debug.DrawLine(transform.position, monster_transform.position, Color.blue);
			return true;
    	}
    	return false;
    }
}
