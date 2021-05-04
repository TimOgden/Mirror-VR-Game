using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SwitchGrabpoint : XRGrabInteractable
{
    public Transform left_grabpoint;
    public Transform right_grabpoint;
    protected override void OnSelectEntering(XRBaseInteractor interactor)
    {
        base.OnSelectEntering(interactor);
        if(interactor.transform.tag=="Left")
        {
            attachTransform = left_grabpoint;
        } else
        {
            attachTransform = right_grabpoint;
        }
    }

}
