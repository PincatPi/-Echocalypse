using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerSoundController : MonoBehaviour
{
    private AudioSource audioSource;
    
    public AudioClip[] footSteps;
    public AudioClip[] jumpEfforts;
    public AudioClip[] landing;
    public AudioClip equip;
    public AudioClip unEquip;
    public AudioClip[] commonAttack;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayFootStepSound()
    {
        int i = Random.Range(0, footSteps.Length);
        audioSource.PlayOneShot(footSteps[i]);
    }
    
    public void PlayJumpEffortSound()
    {
        int i = Random.Range(0, jumpEfforts.Length);
        audioSource.PlayOneShot(jumpEfforts[i]);
    }
    
    public void PlayLandingSound()
    {
        int i = Random.Range(0, landing.Length);
        audioSource.PlayOneShot(landing[i]);
    }

    public void PlayEquipSound()
    {
        audioSource.PlayOneShot(equip);
    }

    public void PlayUnEquipSound()
    {
        audioSource.PlayOneShot(unEquip);
    }

    public void PlayCommonAttackSound(int currentAttack)
    {
        audioSource.PlayOneShot(commonAttack[currentAttack - 1]);
    }
}
