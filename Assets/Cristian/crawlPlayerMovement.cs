using UnityEngine;
using UnityEngine.InputSystem; // Required for the new Input System

public class crawlPlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float forwardForce = 20f;
    public float turnDegrees = 15f;

    [Header("Anti-Spam Settings")]
    public float inputCooldown = 0.2f; 
    private float nextAllowedInputTime = 0f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // Configurar la fricción lineal si está por defecto
        if (rb.linearDamping == 0)
        {
            rb.linearDamping = 5f; 
        }

        // Evitar que el personaje se vuelque o rote en ejes no deseados al chocar
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        if (Time.time < nextAllowedInputTime) return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        bool pressedA = keyboard.aKey.wasPressedThisFrame;
        bool pressedD = keyboard.dKey.wasPressedThisFrame;

        if (pressedA && pressedD) return;

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
        // 1. Aplicar rotación en el eje Y
        transform.Rotate(0f, turnAngle, 0f);

        // 2. Aplicar impulso hacia adelante
        rb.AddForce(transform.forward * forwardForce, ForceMode.Impulse);
    }

    void TriggerCooldown()
    {
        nextAllowedInputTime = Time.time + inputCooldown;
    }
}