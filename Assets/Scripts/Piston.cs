using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piston : MonoBehaviour {

    public AudioClip pistonGasClip;
    private PistonButton btn;
    public GameObject piston;
    public float timeUp;
    public float timeDown;
    public float distance;
    public bool isAnimating = false;

    private bool goingUp = true;
    private float startTime;
    private Vector3 startPosition;
    private Vector3 endPosition;
    private float ratio;

    private Rigidbody rb;

    private float elapsed = 0f;
	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody>();
        btn = GetComponentInChildren<PistonButton>();
        btn.OnButtonTrigger += ButtonTriggered;
    }
	
	// Update is called once per frame
	void Update () {
		if ( (elapsed += Time.deltaTime) > 5f)
        {
            elapsed = 0f;
            
        }
    }

    /*void OnTriggerEnter(Collider collider)
    {
        Debug.Log("Collider enter at piston level");
        if (collider.CompareTag("Throwable"))
        {
            ButtonTriggered();
        }
    }*/

    void ButtonTriggered()
    {
        if (!isAnimating)
        {
            goingUp = true;
            startTime = Time.time;
            startPosition = rb.position;
            endPosition = startPosition + (rb.transform.up * distance);
            isAnimating = true;
            AudioSource.PlayClipAtPoint(pistonGasClip, transform.position, 0.5f);
        }
    }

    void FixedUpdate()
    {
        if (isAnimating)
        {
            RunAnimation();
        }
    }

    void RunAnimation() {
        float ratio = 0.0f;
        if (goingUp)
        {
            ratio = (Time.time - startTime) / timeUp;
            if (ratio >= 1f)
            {
                goingUp = false;
                startTime = Time.time;
                endPosition = startPosition;
                startPosition = rb.position;
            }
        }
        if (!goingUp)
        {
            ratio = (Time.time - startTime) / timeDown;
            if (ratio >= 1f)
            {
                isAnimating = false;
                //return;
            }
        }
        Vector3 newPosition = Vector3.Lerp(startPosition, endPosition, ratio);
        rb.MovePosition(newPosition);

        float length = 0.0f;
        if (goingUp)
            length = (newPosition - startPosition).magnitude * 0.5f;
        else 
            length = (newPosition - endPosition).magnitude * 0.5f;
        piston.transform.localPosition = new Vector3(0, -length, 0);
        piston.transform.localScale = new Vector3(piston.transform.localScale.x, length, piston.transform.localScale.z);
    }
}
