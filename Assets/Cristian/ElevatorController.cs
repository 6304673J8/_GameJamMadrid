using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class ElevatorController : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("The Animator component controlling the elevator doors.")]
    public Animator elevatorAnimator;
    [Tooltip("The exact name of the door-opening animation state in the Animator.")]
    public string animationName = "OpenDoors";

    [Header("Audio Settings")]
    [Tooltip("The AudioSource component used to play the sound.")]
    public AudioSource audioSource;
    [Tooltip("The audio clip for the door opening sound effect.")]
    public AudioClip openSound;

    [Header("Timing Settings")]
    [Tooltip("Seconds to wait after the sound plays before the animation starts.")]
    public float delayDuration = 1.0f;

    // Internal state tracking to prevent multiple triggers at the same time
    private bool isTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the colliding object is tagged as the Player and hasn't triggered yet
        if (other.CompareTag("Player") && !isTriggered)
        {
            StartCoroutine(PlayOpeningSequence());
        }
    }

    private IEnumerator PlayOpeningSequence()
    {
        isTriggered = true;

        // 1. Play the audio clip if both the source and clip are assigned
        if (audioSource != null && openSound != null)
        {
            audioSource.PlayOneShot(openSound);
        }

        // 2. Wait for the specified delay duration
        yield return new WaitForSeconds(delayDuration);

        // 3. Play the animation state directly by name
        if (elevatorAnimator != null)
        {
            elevatorAnimator.Play(animationName);
            
            // NOTE: If you prefer using an Animator Trigger parameter instead of the state name, 
            // you can comment out the line above and use: elevatorAnimator.SetTrigger("YourTriggerName");
        }
    }

    // Optional: Reset the trigger status when the player leaves the area
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isTriggered = false;
        }
    }
}