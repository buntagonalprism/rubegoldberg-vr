using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour {


    public Collider startAreaTrigger;
    public AudioClip springBounceClip;
    public AudioClip regularBouncClip;
    public int totalResets = 0;

    private Rigidbody rb;
    private MeshRenderer mr;
    private Vector3 startingPosition;
    private Rigidbody rigidBody;
    private CollectibleManager collectibleManager;

    private bool isInStartArea = true;

	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody>();
        mr = GetComponent<MeshRenderer>();
        startingPosition = transform.position;
        rigidBody = GetComponent<Rigidbody>();
        collectibleManager = FindObjectOfType<CollectibleManager>();
	}
	

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Ground"))
        {
            ResetBall();
        } else
        {
            PhysicMaterial physMat = collision.collider.material;
            bool springBounce = false;
            if (physMat != null)
            {
                if (physMat.bounciness > 0.9)
                {
                    springBounce = true;
                    
                }
            }
            if (springBounce)
            {
                AudioSource.PlayClipAtPoint(springBounceClip, transform.position);
            } else
            {
                AudioSource.PlayClipAtPoint(regularBouncClip, transform.position, Mathf.Clamp(collision.relativeVelocity.magnitude / 10, 0, 1));
            }
        }
    }

    public void ResetBall()
    {
        transform.position = startingPosition;
        rigidBody.velocity = Vector3.zero;
        rigidBody.angularVelocity = Vector3.zero;
        collectibleManager.OnLevelReset();
        deactivateAntiCheat();
        totalResets++;
    }

    void OnTriggerStay(Collider collider)
    {
        // Check if we are leaving the valid start area
        if (collider == startAreaTrigger)
        {
            isInStartArea = true;
        }
        // Trigger could be the controller - this should fire anti-cheat if we are outside the start area
        else if (!isInStartArea)
        {
            // Check if the user is currently holding the ball
            if (rb.isKinematic)
            //if (!rb.isKinematic)
            {
                // If user is holding ball outside start area, fire anti-cheat code
                activateAntiCheat();
            }
        }
    }

    void OnTriggerExit(Collider collider)
    {
        // Check if we are leaving the valid start area
        if (collider == startAreaTrigger)
        {
            isInStartArea = false;
        }
    }

    void activateAntiCheat()
    {
        mr.material.color = new Color(1f, 0f, 0f);
        gameObject.layer = LayerMask.NameToLayer("Error");
    }
    void deactivateAntiCheat()
    {
        mr.material.color = new Color(1f, 1f, 1f);
        gameObject.layer = LayerMask.NameToLayer("Default");
    }

    // 
    /* Only way to reset is to reset ball
    void OnTriggerEnter(Collider collider)
    {
        if (collider == startAreaTrigger)
        {
            // If the user has carried the ball back into the start area, deactivate anti-cheat
            if (rb.isKinematic)
            //if (!rb.isKinematic)
            {
                // If user is holding ball outside start area, fire anti-cheat code
                deactivateAntiCheat();
            }
        }
    }*/
}
