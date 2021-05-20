using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowFacingForward : MonoBehaviour
{
	public Transform target;
	public Vector3 offset;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position = target.position + Vector3.up * offset.y
        						+ Vector3.ProjectOnPlane(target.right, Vector3.up).normalized * offset.x
        						+ Vector3.ProjectOnPlane(target.forward, Vector3.up).normalized * offset.z;
        Debug.DrawRay(transform.position, transform.up * 20f, Color.green);
        transform.rotation = Quaternion.LookRotation(-Vector3.up, Vector3.ProjectOnPlane(target.forward, Vector3.up));
    }
}
