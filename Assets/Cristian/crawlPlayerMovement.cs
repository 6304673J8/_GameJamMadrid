using UnityEngine;
using UnityEngine.InputSystem; // Required for the new Input System


public class crawlPlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    // Note: Since Drag will be high, you will need a larger forwardForce value (e.g., 15 to 30)
    public float forwardForce = 20f;
    public float turnDegrees = 15f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // Dynamic safety check: Ensure there is at least some Drag set up in the Rigidbody
        if (rb.linearDamping == 0)
        {
            rb.linearDamping = 5f; // Set a default friction value if it was left at 0
        }
    }

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.aKey.wasPressedThisFrame)
        {
            Crawl(-turnDegrees);
        }
        else if (keyboard.dKey.wasPressedThisFrame)
        {
            Crawl(turnDegrees);
        }
    }

    void Crawl(float turnAngle)
    {
        // 1. Apply instant rotation on the Y axis
        transform.Rotate(0f, turnAngle, 0f);

        // 2. Apply a strong forward impulse
        // The high Rigidbody Drag will instantly start slowing the player down right after this impulse
        rb.AddForce(transform.forward * forwardForce, ForceMode.Impulse);
    }
}