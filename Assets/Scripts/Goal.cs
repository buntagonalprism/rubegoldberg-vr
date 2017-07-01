using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour {

    public AudioClip successClip;
    public AudioClip failureClip;

    public event Action GoalEntered;

    void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Throwable"))
        {
            if (GoalEntered != null)
                GoalEntered();
        }
    }


}
