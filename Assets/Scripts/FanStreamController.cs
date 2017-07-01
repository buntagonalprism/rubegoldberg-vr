using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FanStreamController : MonoBehaviour {

    public FanStream core;
    public FanStream outer1;
    public FanStream outer2;
    public float coreForce = 10f;

	// Use this for initialization
	void Start () {
        core.OnColliderTriggerStay += new FanStream.TriggerHandler(CoreStreamForce);
        outer1.OnColliderTriggerStay += new FanStream.TriggerHandler(Outer1StreamForce);
        outer2.OnColliderTriggerStay += new FanStream.TriggerHandler(Outer2StreamForce);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void CoreStreamForce(Collider collider)
    {
        if (collider.CompareTag("Throwable"))
        {
            Rigidbody rb = collider.GetComponent<Rigidbody>();
            if (rb == null)
                return;
            float dist = (collider.transform.position - transform.position).magnitude;
            if (dist < 2.5f)
                rb.AddForce(coreForce * transform.forward);
            else if (dist < 5f)
                rb.AddForce(coreForce * transform.forward * 0.6666f);
            else
                rb.AddForce(coreForce * transform.forward * 0.3333f);
        }

    }

    void Outer1StreamForce(Collider collider)
    {
        if (collider.CompareTag("Throwable"))
        {
            Rigidbody rb = collider.GetComponent<Rigidbody>();
            if (rb == null)
                return;
            float dist = (collider.transform.position - transform.position).magnitude;
            if (dist < 5f)
                rb.AddForce(coreForce * transform.forward * 0.3333f);
            else
                rb.AddForce(coreForce * transform.forward * 0.1666f);
        }
    }

    void Outer2StreamForce(Collider collider)
    {
        if (collider.CompareTag("Throwable"))
        {
            Rigidbody rb = collider.GetComponent<Rigidbody>();
            if (rb != null)
                rb.AddForce(coreForce * transform.forward * 0.1666f);
        }
    }
}
