using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRDirectAnimatedInteractor : XRDirectInteractor
{
    public Animator animator;
    public bool isLeft;

    protected override void OnSelectEntering(XRBaseInteractable interactable)
    {
        base.OnSelectEntering(interactable);
        bool holdingObject = true;
        float handValue = 0.0f;
        if(interactable.transform.tag=="Flashlight")
        {
            handValue = 0.0f;
        } else if(interactable.transform.tag=="Mirror")
        {
            handValue = 0.5f;
        } else
        {
            handValue = 1.0f;
        }

        if (isLeft)
        {  
            Debug.Log("LeftHandGrab");
            animator.SetBool("leftHoldingObject", true);
            animator.SetFloat("leftGrab", handValue);
        } else
        {
            Debug.Log("RightHandGrab");
            animator.SetBool("rightHoldingObject", true);
            animator.SetFloat("rightGrab", handValue);
        }
    }

    protected override void OnSelectExiting(XRBaseInteractable interactable)
    {
        base.OnSelectExiting(interactable);
        if (isLeft)
        {
            Debug.Log("LeftHandRelease");
            animator.SetBool("leftHoldingObject", false);
        }
        else
        {
            Debug.Log("RightHandRelease");
            animator.SetBool("rightHoldingObject", false);
        }
    }
}
