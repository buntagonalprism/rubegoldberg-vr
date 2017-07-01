using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{

    public GameObject collectEffect;
    public AudioClip collectClip;
    private ParticleSystem effectInstance = null;

    public delegate void OnCollectibleHit(Collectible collectible);
    public event OnCollectibleHit CollectibleHit;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Throwable"))
        {
            if (CollectibleHit != null)
                CollectibleHit(this);
        }
    }

    public void collect()
    {
        if (effectInstance == null) {
            GameObject particleObj = Instantiate(collectEffect, transform.position, Quaternion.identity);
            effectInstance = particleObj.GetComponent<ParticleSystem>();
        }
        effectInstance.Play();
        AudioSource.PlayClipAtPoint(collectClip, transform.position);
        gameObject.SetActive(false);
    }
    public void reset()
    {
        gameObject.SetActive(true);
    }
}
