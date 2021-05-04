using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flashlight : MonoBehaviour
{
    private Light light;
    private Animator animator;
    public float batteryPercent = 100f;
    public float batteryDrainSpeed = .5f;
    public float almostDeadIntensity = 1f;
    public float lowBatteryIntensity = 1.5f;
    public float defaultIntensity = 2.5f;
    // Start is called before the first frame update
    void Start()
    {
        light = GetComponent<Light>();

        animator = transform.parent.GetComponent<Animator>();
    }

    void Update()
    {
        if (light.enabled)
            batteryPercent -= batteryDrainSpeed * Time.deltaTime;


        if (batteryPercent<=0f)
        {
            light.intensity = 0.0f;
        } else if (batteryPercent>0f && batteryPercent<=20f)
        {
            light.intensity = (Mathf.PerlinNoise(Time.time, 0.0f) + Mathf.PerlinNoise(Time.time * 0.5f, 0.0f)) * almostDeadIntensity + .75f;
        } else if(batteryPercent>20f && batteryPercent<40f)
        {
            light.intensity = Mathf.PerlinNoise(Time.time, 0.0f) * lowBatteryIntensity + 1.0f;
        } else
        {
            light.intensity = defaultIntensity;
        }
    }

    public void Toggle()
    {
        bool isActive = !light.enabled;
        light.enabled = isActive && (batteryPercent!=0f);
        animator.SetBool("isActive", isActive);
    }
}
