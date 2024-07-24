using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AudioType {
    bottlePickup,
    bottleDrop,
    liquidPour,
    bottleFilledUp,
    levelComplete,
    confettiPop
}

public class AudioHandler : MonoBehaviour
{
    public AudioSource bottlePickup;
    public AudioSource bottleDrop;
    public AudioSource liquidPour;
    public AudioSource bottleFilledUp;
    public AudioSource levelComplete;
    public AudioSource confettiPop;

    public static AudioHandler instance;

    void Awake() {
        if(instance == null) {
            instance = this;
        }
        else {
            Destroy(gameObject);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayAudio(AudioType audioType) 
    {
        switch(audioType) 
        {
            case AudioType.bottlePickup:
                PlaySound(bottlePickup);
                break;
            case AudioType.bottleDrop:
                PlaySound(bottleDrop);
                break;
            case AudioType.liquidPour:
                PlaySound(liquidPour);
                break;
            case AudioType.bottleFilledUp:
                PlaySound(bottleFilledUp);
                break;
            case AudioType.levelComplete:
                PlaySound(levelComplete);
                break;
            case AudioType.confettiPop:
                PlaySound(confettiPop);
                break;
        }
    }

    private void PlaySound(AudioSource audioSource)
    {
        audioSource.Stop();
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }
    public void StopAudio(AudioType audioType) {
        switch(audioType) {
            case AudioType.bottlePickup:
                bottlePickup.Stop();
                break;
            case AudioType.bottleDrop:
                bottleDrop.Stop();
                break;
            case AudioType.liquidPour:
                liquidPour.Stop();
                break;
            case AudioType.bottleFilledUp:
                bottleFilledUp.Stop();
                break;
            case AudioType.levelComplete:
                levelComplete.Stop();
                break;
            case AudioType.confettiPop:
                confettiPop.Stop();
                break;
        }
    }
}
