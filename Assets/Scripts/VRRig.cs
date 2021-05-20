using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VRMap
{
    public float movementSpeed;
    public float rotationSpeed;
    public Transform vrTarget;
    public Transform rigTarget;
    public Vector3 trackingPositionOffset;
    public Vector3 trackingRotationOffset;
    
    public void Map()
    {
        rigTarget.position = Vector3.Lerp(rigTarget.position, 
            vrTarget.TransformPoint(trackingPositionOffset), Time.fixedDeltaTime * movementSpeed);
        rigTarget.rotation = Quaternion.Slerp(rigTarget.rotation, 
            vrTarget.rotation * Quaternion.Euler(trackingRotationOffset), Time.fixedDeltaTime * rotationSpeed);
    }
}
public class VRRig : MonoBehaviour
{
    public float turnSmoothness;
    public VRMap head;
    public VRMap leftHand;
    public VRMap rightHand;
    public Transform headConstraint;
    public Vector3 headBodyOffset;
    // Start is called before the first frame update
    void Start()
    {
        headBodyOffset = transform.position - headConstraint.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position = headConstraint.position + headBodyOffset;
        transform.forward = Vector3.Lerp(transform.forward,
            Vector3.ProjectOnPlane(-headConstraint.right,Vector3.up).normalized,Time.fixedDeltaTime * turnSmoothness);

        head.Map();
        leftHand.Map();
        rightHand.Map();
    }
}
