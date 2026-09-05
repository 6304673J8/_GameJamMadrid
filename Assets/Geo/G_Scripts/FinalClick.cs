using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class FinalClick : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject aimArrow;
    [SerializeField] private Image forceBar;

    [Header("Arrow")]
    [SerializeField] private float rotationSpeed = 180f;

    [Header("Throw Force")]
    [SerializeField] private float minThrowForce = 3f;
    [SerializeField] private float maxThrowForce = 20f;

    [Header("Power Bar")]
    [SerializeField] private float powerChangeSpeed = 1f;

    [Header("Physics")]
    [SerializeField] private float stopSpeed = 0.15f;
    [SerializeField] private float stopTime = 0.5f;

    private Rigidbody rb;

    private enum State
    {
        Idle,
        SelectingDirection,
        Flying
    }

    private State currentState = State.Idle;

    private float powerAmount = 0f;
    private bool powerIncreasing = true;

    private float stopTimer = 0f;

    private Vector3 throwDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Hide arrow
        if (aimArrow != null)
        {
            aimArrow.SetActive(false);
        }

        // Hide power bar
        if (forceBar != null)
        {
            forceBar.type = Image.Type.Filled;
            forceBar.fillMethod = Image.FillMethod.Horizontal;
            forceBar.fillOrigin = 0;

            forceBar.fillAmount = 0f;
            forceBar.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        HandleInput();

        if (currentState == State.SelectingDirection)
        {
            RotateArrow();
            UpdatePowerBar();
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

        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        // -----------------------------------------------------
        // FIRST CLICK
        // Arrow appears and power bar starts
        // -----------------------------------------------------

        if (currentState == State.Idle)
        {
            StartDirectionSelection();
        }

        // -----------------------------------------------------
        // SECOND CLICK
        // Lock direction and throw
        // -----------------------------------------------------

        else if (currentState == State.SelectingDirection)
        {
            Throw();
        }
    }

    // =========================================================
    // FIRST CLICK
    // =========================================================

    private void StartDirectionSelection()
    {
        currentState = State.SelectingDirection;

        // -----------------------------
        // Show arrow
        // -----------------------------

        if (aimArrow != null)
        {
            aimArrow.SetActive(true);

            aimArrow.transform.position =
                transform.position + Vector3.up * 0.1f;

            // Start from forward direction
            aimArrow.transform.rotation =
                Quaternion.LookRotation(Vector3.forward);
        }

        // -----------------------------
        // Start power bar
        // -----------------------------

        powerAmount = 0f;
        powerIncreasing = true;

        if (forceBar != null)
        {
            forceBar.fillAmount = 0f;
            forceBar.gameObject.SetActive(true);
        }
    }

    // =========================================================
    // ROTATE ARROW
    // =========================================================

    private void RotateArrow()
    {
        if (aimArrow == null)
            return;

        aimArrow.transform.Rotate(
            Vector3.up,
            rotationSpeed * Time.deltaTime,
            Space.World
        );
    }

    // =========================================================
    // POWER BAR
    // Goes 0 -> 1 -> 0 -> 1...
    // =========================================================

    private void UpdatePowerBar()
    {
        if (powerIncreasing)
        {
            powerAmount += powerChangeSpeed * Time.deltaTime;

            if (powerAmount >= 1f)
            {
                powerAmount = 1f;
                powerIncreasing = false;
            }
        }
        else
        {
            powerAmount -= powerChangeSpeed * Time.deltaTime;

            if (powerAmount <= 0f)
            {
                powerAmount = 0f;
                powerIncreasing = true;
            }
        }

        if (forceBar != null)
            forceBar.fillAmount = powerAmount;
    }

    // =========================================================
    // SECOND CLICK
    // THROW
    // =========================================================

    private void Throw()
    {
        currentState = State.Flying;

        // -----------------------------
        // Save arrow direction
        // -----------------------------

        if (aimArrow != null)
        {
            throwDirection = aimArrow.transform.forward;
        }

        throwDirection.y = 0f;
        throwDirection.Normalize();

        // -----------------------------
        // Hide UI
        // -----------------------------

        if (aimArrow != null)
            aimArrow.SetActive(false);

        if (forceBar != null)
        {
            forceBar.fillAmount = 0f;
            forceBar.gameObject.SetActive(false);
        }

        // -----------------------------
        // Calculate force
        // -----------------------------

        float throwForce = Mathf.Lerp(
            minThrowForce,
            maxThrowForce,
            powerAmount
        );

        // -----------------------------
        // Reset velocity
        // -----------------------------

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // -----------------------------
        // Apply impulse
        // -----------------------------

        rb.AddForce(
            throwDirection * throwForce,
            ForceMode.Impulse
        );

        // Reset power
        powerAmount = 0f;
        powerIncreasing = true;

        stopTimer = 0f;
    }

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

