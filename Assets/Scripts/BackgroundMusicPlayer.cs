using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMusicPlayer : MonoBehaviour {

    public AudioClip[] clips;
    private AudioSource audioSource;
    private int clipIdx;
    private static BackgroundMusicPlayer instance = null;

    void Awake()
    {
        // Static reference to our singleton music player instance
        if (instance == null)
        {
            instance = this;
        }
        // If the singleton instance already exists, and this current game object isn't it, destroy this duplicate
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

	// Use this for initialization
	void Start () {
        audioSource = GetComponent<AudioSource>();
        StartCoroutine(playEngineSound());
        DontDestroyOnLoad(gameObject);
	}

    IEnumerator playEngineSound()
    {
        while (true)
        {
            audioSource.clip = clips[clipIdx];
            audioSource.Play();
            yield return new WaitForSeconds(audioSource.clip.length);
            clipIdx++;
            if (clipIdx >= clips.Length)
                clipIdx = 0;
        }
    }
}
