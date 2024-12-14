using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerAudioController : MonoBehaviour
{
    private AudioSource audioSource;
    public AudioClip[] greetAudio;
    public AudioClip[] agreeAudio;
    public AudioClip[] linesAudio;
    // public enum AudioType
    // {
    //     Greet,
    //     Agree,
    //     Lines,
    // }
    // private AudioType audioType = AudioType.Lines;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayGreetAudio()
    {
        int i = Random.Range(0, greetAudio.Length);
        audioSource.PlayOneShot(greetAudio[i]);
    }

    public void PlayAgreeAudio()
    {
        int i = Random.Range(0, agreeAudio.Length);
        audioSource.PlayOneShot(agreeAudio[i]);
    }

    public void PlayLinesAudio()
    {
        
    }
}
