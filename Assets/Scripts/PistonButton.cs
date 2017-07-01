using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PistonButton : MonoBehaviour {

    public Action OnButtonTrigger;
    public AudioClip btnClickClip;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerEnter(Collider collider)
    {
        Debug.Log("Collider enter at button child level");
        if (collider.CompareTag("Throwable"))
        {
            OnButtonTrigger();
            AudioSource.PlayClipAtPoint(btnClickClip, transform.position);
        }
    }


}
