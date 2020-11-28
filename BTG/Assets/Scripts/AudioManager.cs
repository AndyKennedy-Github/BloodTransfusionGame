using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

    public AudioClip[] sounds;

    public void PlaySoundOnce(gameobject g, int n)
    {
        AudioSource a = g.GetComponent<AudioSource>();
        a.PlayOneShot(sounds[n], 0.5f);
    }

    public void PlaySound(gameobject g, int n)
    {
        AudioSource a = g.GetComponent<AudioSource>();
        if(a.isPlaying == false)
        {
            a.clip = sounds[n];
            a.Play();
        }
    }

    public void StopSound()
    {
        AudioSource a = g.GetComponent<AudioSource>();
        a.Stop();
    }
}
