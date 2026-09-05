using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerDirection : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject aimArrow;
    [SerializeField] private Image forceBar;

    [Header("Arrow")]
    [SerializeField] private float rotationSpeed = 180f;

    [Header("Throw Force")]
    [SerializeField] private float minThrowForce = 3f;
    [SerializeField] private float maxThrowForce = 20f;
    [SerializeField] private float chargeTime = 2f;

    [Header("Physics")]
    [SerializeField] private float stopSpeed = 0.15f;
    [SerializeField] private float stopTime = 0.5f;

    private Rigidbody rb;

    private enum State
    {
        Idle,
        SelectingDirection,
        Charging,
        Flying
    }

    private State currentState = State.Idle;

    private float chargeAmount = 0f;
    private float stopTimer = 0f;

    private Vector3 throwDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (aimArrow != null)
        {
            aimArrow.SetActive(false);
        }

        if (forceBar != null)
        {
            forceBar.gameObject.SetActive(false);
            forceBar.fillAmount = 0f;
        }
    }

    private void Update()
    {
        HandleInput();

        if (currentState == State.SelectingDirection)
        {
            RotateArrow();
        }

        if (currentState == State.Charging)
        {
            ChargeForce();
        }

        CheckIfStopped();
    }

    // =========================================================
    // INPUT
    // =========================================================

    private void HandleInput()
    {
        if (Mouse.current == null)
            return;

        // -----------------------------------------------------
        // MOUSE BUTTON PRESSED
        // -----------------------------------------------------

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            // Primer clic → empezar a elegir dirección
            if (currentState == State.Idle)
            {
                StartDirectionSelection();
            }

            // Segundo clic → fijar dirección y empezar medidor
            else if (currentState == State.SelectingDirection)
            {
                StartCharging();
            }

            // Tercer clic → elegir potencia y lanzar
            else if (currentState == State.Charging)
            {
                Throw();
            }
        }

        // -----------------------------------------------------
        // MOUSE BUTTON RELEASED
        // -----------------------------------------------------

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            if (currentState == State.Charging)
            {
                Throw();
            }
        }
    }

    // =========================================================
    // STEP 1: START ROTATING ARROW
    // =========================================================

    private void StartDirectionSelection()
    {
        currentState = State.SelectingDirection;

        if (aimArrow != null)
        {
            aimArrow.SetActive(true);

            // Put arrow above the player
            aimArrow.transform.position =
                transform.position + Vector3.up * 0.1f;

            // Start from a known direction
            aimArrow.transform.rotation =
                Quaternion.LookRotation(Vector3.forward);
        }
    }

    // =========================================================
    // ARROW ROTATION
    // =========================================================

    private void RotateArrow()
    {
        if (aimArrow == null)
            return;

        // Rotate around the Y axis
        aimArrow.transform.Rotate(
            Vector3.up,
            rotationSpeed * Time.deltaTime,
            Space.World
        );
    }

    // =========================================================
    // STEP 2: LOCK DIRECTION AND START CHARGING
    // =========================================================

    private void StartCharging()
    {
        currentState = State.Charging;

        // Save the direction the arrow is currently pointing
        throwDirection = aimArrow.transform.forward;

        // Make sure we stay horizontal
        throwDirection.y = 0f;
        throwDirection.Normalize();

        chargeAmount = 0f;

        // Show force bar
        if (forceBar != null)
        {
            forceBar.gameObject.SetActive(true);
            forceBar.fillAmount = 0f;
        }

        // Stop the arrow visually
        // It remains visible while charging.
        if (aimArrow != null)
        {
            aimArrow.SetActive(true);
        }
    }

    // =========================================================
    // STEP 3: CHARGE FORCE
    // =========================================================

    private void ChargeForce()
    {
        chargeAmount += Time.deltaTime / chargeTime;

        chargeAmount = Mathf.Clamp01(chargeAmount);

        if (forceBar != null)
        {
            forceBar.fillAmount = chargeAmount;
        }
    }

    // =========================================================
    // STEP 4: RELEASE = THROW
    // =========================================================

    private void Throw()
    {
        currentState = State.Flying;

        // Hide arrow
        if (aimArrow != null)
        {
            aimArrow.SetActive(false);
        }

        // Hide force bar
        if (forceBar != null)
        {
            forceBar.gameObject.SetActive(false);
        }

        // Calculate force from charge
        float throwForce = Mathf.Lerp(
            minThrowForce,
            maxThrowForce,
            chargeAmount
        );

        // Reset current movement
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Launch player
        rb.AddForce(
            throwDirection * throwForce,
            ForceMode.Impulse
        );

        chargeAmount = 0f;
        stopTimer = 0f;
    }

    // =========================================================
    // DETECT WHEN PLAYER STOPS
    // =========================================================

    private void CheckIfStopped()
    {
        if (currentState != State.Flying)
            return;

        float speed = rb.linearVelocity.magnitude;

        if (speed < stopSpeed)
        {
            stopTimer += Time.deltaTime;

            if (stopTimer >= stopTime)
            {
                StopFlying();
            }
        }
        else
        {
            stopTimer = 0f;
        }
    }

    // =========================================================
    // RETURN TO IDLE
    // =========================================================

    private void StopFlying()
    {
        currentState = State.Idle;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        stopTimer = 0f;
    }
}
