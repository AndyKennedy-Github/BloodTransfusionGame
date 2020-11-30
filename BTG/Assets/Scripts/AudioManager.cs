using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

    public AudioClip[] sounds;

    public void PlaySoundOnce(GameObject g, int n)
    {
        AudioSource a = g.GetComponent<AudioSource>();
        a.PlayOneShot(sounds[n], 0.5f);
    }

    public void PlaySound(GameObject g, int n)
    {
        AudioSource a = g.GetComponent<AudioSource>();
        if(a.isPlaying == false)
        {
            a.clip = sounds[n];
            a.Play();
        }
    }

    public void StopSound(GameObject g)
    {
        AudioSource a = g.GetComponent<AudioSource>();
        a.Stop();
    }
}
