using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class ElevatorController1 : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("The Animator component controlling the elevator doors.")]
    public Animator elevatorAnimator;
    [Tooltip("The exact name of the Trigger parameter in the Animator to open the doors.")]
    public string openTriggerName = "OpenDoors";

    [Header("Audio Settings")]
    [Tooltip("The AudioSource component used to play the sound.")]
    public AudioSource audioSource;
    [Tooltip("The audio clip for the door opening sound effect.")]
    public AudioClip openSound;

    [Header("Timing Settings")]
    [Tooltip("Seconds to wait after the sound plays before the doors start opening.")]
    public float delayBeforeOpen = 1.0f;

    private bool isTriggered = false;

    void Start()
    {
        
        isTriggered = false;
        StartCoroutine(PlayElevatorSequence());
    }

    private void OnTriggerEnter(Collider other)
    {

    }

    private IEnumerator PlayElevatorSequence()
    {
        isTriggered = true;

        // 1. Play the door opening sound effect
        if (audioSource != null && openSound != null)
        {
            audioSource.PlayOneShot(openSound);
        }

        // 2. Wait before opening the doors
        yield return new WaitForSeconds(delayBeforeOpen);

        // 3. Trigger the animation using the Animator parameter
        if (elevatorAnimator != null)
        {
            elevatorAnimator.SetTrigger(openTriggerName);
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
