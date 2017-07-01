using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FanStream : MonoBehaviour {

    public delegate void TriggerHandler(Collider collider);
    public event TriggerHandler OnColliderTriggerStay;

    void OnTriggerStay(Collider collider)
    {
        if (OnColliderTriggerStay != null)
            OnColliderTriggerStay(collider);
    }
}
