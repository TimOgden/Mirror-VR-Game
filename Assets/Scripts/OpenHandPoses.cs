using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
public class OpenHandPoses : MonoBehaviour
{
    public Animator animator;
    public XRNode leftNode;
    public XRNode rightNode;
    public float animationSpeed;
    private float trigger;
    private float grip;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GetValues(leftNode);
        UpdateLeftValues();
        GetValues(rightNode);
        UpdateRightValues();
    }

    private void GetValues(XRNode node)
    {
        InputDevice device = InputDevices.GetDeviceAtXRNode(node);
        device.TryGetFeatureValue(CommonUsages.trigger, out trigger);
        device.TryGetFeatureValue(CommonUsages.grip, out grip);
    }

    private void UpdateLeftValues()
    {
        float last_trigger = animator.GetFloat("leftTrigger");
        float last_grip = animator.GetFloat("leftGrip");
        animator.SetFloat("leftTrigger", Mathf.Lerp(last_trigger, trigger, Time.deltaTime * animationSpeed));
        animator.SetFloat("leftGrip", Mathf.Lerp(last_grip, grip, Time.deltaTime * animationSpeed));
    }

    private void UpdateRightValues()
    {
        float last_trigger = animator.GetFloat("rightTrigger");
        float last_grip = animator.GetFloat("rightGrip");
        animator.SetFloat("rightTrigger", Mathf.Lerp(last_trigger, trigger, Time.deltaTime * animationSpeed));
        animator.SetFloat("rightGrip", Mathf.Lerp(last_grip, grip, Time.deltaTime * animationSpeed));
    }

}
