using UnityEngine;
using UnityEngine.InputSystem; // Required for the new Input System


public class crawlPlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float forwardForce = 20f;
    public float turnDegrees = 15f;

    [Header("Anti-Spam Settings")]
    // Short delay in seconds before the player can crawl again (e.g., 0.2 seconds)
    public float inputCooldown = 0.2f; 
    private float nextAllowedInputTime = 0f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        if (rb.linearDamping == 0)
        {
            rb.linearDamping = 5f; // Ensures friction is applied
        }
    }

    void Update()
    {
        // Check if the short cooldown delay has passed
        if (Time.time < nextAllowedInputTime) return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        // Read the exact status of both keys this frame
        bool pressedA = keyboard.aKey.wasPressedThisFrame;
        bool pressedD = keyboard.dKey.wasPressedThisFrame;

        // Block input if BOTH keys are pressed at the exact same time
        if (pressedA && pressedD) return;

        // Process only one key at a time
        if (pressedA)
        {
            Crawl(-turnDegrees);
            TriggerCooldown();
        }
        else if (pressedD)
        {
            Crawl(turnDegrees);
            TriggerCooldown();
        }
    }

    void Crawl(float turnAngle)
    {
        // 1. Apply instant rotation on the Y axis
        transform.Rotate(0f, turnAngle, 0f);

        // 2. Apply a strong forward impulse
        rb.AddForce(transform.forward * forwardForce, ForceMode.Impulse);
    }

    void TriggerCooldown()
    {
        // Set the timestamp for when the player is allowed to press a key again
        nextAllowedInputTime = Time.time + inputCooldown;
    }
}