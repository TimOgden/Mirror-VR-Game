using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverFinder : MonoBehaviour
{
	public int numberOfPoints;
	public float multiplier;

	private float phi = (1 + Mathf.Sqrt(5));
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDrawGizmos() {
    	Gizmos.color = Color.white;
    	GeneratePoints(numberOfPoints);
    }

    private void GeneratePoints(int n) {
    	Debug.Log("phi:" + phi);
    	float angle_increment = Mathf.PI * 2 * phi;
    	for(float i = 0; i<n; i+=1f) {
    		float dist = (i / (float)n) * multiplier;
    		float angle = angle_increment * i;
    		float x = dist * Mathf.Cos(angle);
    		float z = dist * Mathf.Sin(angle);
    		Debug.Log("dist: " + dist + ", angle: " + angle + ", x: " + x + ", z: " + z);
    		Vector3 pos = transform.position + new Vector3(x, 0f, z);
    		Gizmos.DrawSphere(pos, .25f);
    	}
    }
}
