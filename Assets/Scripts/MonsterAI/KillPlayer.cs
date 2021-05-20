using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class KillPlayer : MonoBehaviour
{
	public RawImage splashScreen;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider collider) {
    	if(collider.transform.tag == "Monster" && collider.GetComponent<MonsterAI>().fsm.ActiveStateName == "Attacking") {
    		Debug.Log("Caught player, restarting scene...");
    		collider.transform.position = transform.position + transform.forward;
    		collider.transform.parent = transform;
    		collider.transform.LookAt(transform.position);
    		collider.GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = false;
    		splashScreen.enabled = true;
    		StartCoroutine(ResetScene());
    	}
    }

    private IEnumerator ResetScene() {
    	yield return new WaitForSeconds(5f);
    	SceneManager.LoadScene("ai_testing");
    }
}
