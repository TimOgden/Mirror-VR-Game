using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;
public class BatteryInventory : MonoBehaviour
{
	private XRSocketInteractor dropZone;
	private int num_batteries;
	private float cooldown = 1.0f;
	private bool canSelectExit = true;
	private bool canDestroyBattery = true;
	public GameObject batteryPrefab;
	public TextMeshPro batteryCount;
    // Start is called before the first frame update
    void Start()
	{
		num_batteries = int.Parse(batteryCount.text);
		dropZone = GetComponent<XRSocketInteractor>();
		dropZone.onSelectEntered.AddListener(AddBattery);
		dropZone.onSelectExited.AddListener(RemoveBattery);
    }

    private IEnumerator CoolDown() {
    	canSelectExit = false;
    	yield return new WaitForSeconds(cooldown);
    	canSelectExit = true;
    }


    private void OnTriggerEnter(Collider collider) {
    	//Debug.Log("Trigger enter: " + collider.transform.name + " with tag " + collider.transform.tag);
    	if(collider.transform.tag=="Left" || collider.transform.tag=="Right" 
    		&& collider.GetComponent<XRBaseInteractor>().selectTarget==null) {
	    	if(dropZone.selectTarget == null && num_batteries>0) {
	    		GameObject battery = GameObject.Instantiate(batteryPrefab, transform.position, Quaternion.identity);
	    		canDestroyBattery = false;
	    		//dropZone.selectTarget = battery.GetComponent<XRBaseInteractable>();
	    	}
	    }
    }

    private void OnTriggerExit(Collider collider) {
    	if(collider.transform.tag == "Left" || collider.transform.tag == "Right") {
	    	if(dropZone.selectTarget!=null) {
	    		dropZone.selectTarget.colliders.Clear();
	    		Object.Destroy(dropZone.selectTarget.gameObject);
	    		canDestroyBattery = true;
	    		StartCoroutine(CoolDown());
	    	}
	    }
    }

    public void AddBattery(XRBaseInteractable obj) {
    	if(canDestroyBattery) {
	    	obj.colliders.Clear();
	    	Object.Destroy(obj.gameObject);
	    	num_batteries++;
	    	batteryCount.SetText("" + num_batteries);
	    	StartCoroutine(CoolDown());
	    }
    }

    public void RemoveBattery(XRBaseInteractable obj) {
    	if(canSelectExit) {
	    	num_batteries--;
	    	batteryCount.SetText("" + num_batteries);
	    }
	    canDestroyBattery = true;
    }
}
