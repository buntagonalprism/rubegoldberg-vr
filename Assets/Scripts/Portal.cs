using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour {

    public Portal otherPortal;
    public SphereCollider teleportTrigger;
    public List<Collider> transportedObjects;

	// Use this for initialization
	void Start () {
        Portal[] portals = FindObjectsOfType<Portal>();
        if (portals.Length > 1)
        {
            if (portals[0] == this)
            {
                otherPortal = portals[1];
                otherPortal.otherPortal = this;
            } else
            {
                otherPortal = portals[0];
                otherPortal.otherPortal = this;
            }
        }
        teleportTrigger = GetComponentInChildren<SphereCollider>();
	}
	
	// Update is called once per frame
	void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Throwable"))
        {
            // Check if we have already transported this object
            if (transportedObjects.Count != 0)
            {
                if (transportedObjects.Contains(collider))
                    return;
            }
            // Move object, reflect the velocity, and notify other portal that it is transported object
            
            otherPortal.transportedObjects.Add(collider);
            Rigidbody rb = collider.GetComponent<Rigidbody>();
            float speed = rb.velocity.magnitude;
            Vector3 localVelocity = transform.InverseTransformVector(rb.velocity);
            Vector3 reflectedLocalVelocity = new Vector3(localVelocity.x, localVelocity.y, -localVelocity.z);
            Vector3 reflectedOtherVelcoity = otherPortal.transform.TransformVector(reflectedLocalVelocity);
            rb.MovePosition(otherPortal.teleportTrigger.transform.position);
            rb.velocity = reflectedOtherVelcoity;
        }
    }

    void OnTriggerExit(Collider collider)
    {
        // Remove this object from transported set
        transportedObjects.Remove(collider);
    }
}
