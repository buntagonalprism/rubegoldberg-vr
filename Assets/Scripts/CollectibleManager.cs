using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectibleManager : MonoBehaviour {

    private Collectible[] collectibles;
    public int collectiblesRemaining = 0;
    public int totalCollectibles = 0;

	// Use this for initialization
	void Start () {
        collectibles = FindObjectsOfType<Collectible>();
        collectiblesRemaining = collectibles.Length;
        totalCollectibles = collectiblesRemaining;
        foreach (Collectible collectible in collectibles)
        {
            collectible.CollectibleHit += new Collectible.OnCollectibleHit(OnCollectibleHit);
        }
	}
	

    public void OnCollectibleHit(Collectible collectible)
    {
        collectiblesRemaining--;
        collectible.collect();
    }

    public void OnLevelReset()
    {
        collectiblesRemaining = collectibles.Length;
        foreach(Collectible collectible in collectibles)
        {
            collectible.reset();
        }
    }
}
