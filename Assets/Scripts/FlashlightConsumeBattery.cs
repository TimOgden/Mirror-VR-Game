using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
public class FlashlightConsumeBattery : XRSocketInteractor
{
	public Flashlight light;
	public float delay;

    protected override void OnSelectEntering(XRBaseInteractable interactable) {
    	base.OnSelectEntering(interactable);
    	interactable.colliders.Clear();
    	Object.Destroy(interactable.gameObject);
    	light.batteryPercent = 100f;
    }
}
