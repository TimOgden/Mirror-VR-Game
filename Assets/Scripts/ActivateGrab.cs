using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateGrab : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void SetLeftGrab(bool val)
    {
        animator.SetBool("leftHoldingObject", val);
    }

    public void SetRightGrab(bool val)
    {
        animator.SetBool("rightHoldingObject", val);
    }
}
