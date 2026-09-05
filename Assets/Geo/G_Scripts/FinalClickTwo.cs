using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class FinalClickTwo : MonoBehaviour
{
    [Header("CollisionChange")]
    [SerializeField] private float chairModeImpactThreshold = 8f;
    [SerializeField] private string bouncyTag = "Bouncy";

    [Header("References")]
    [SerializeField] private GameObject aimArrow;
    [SerializeField] private GameObject crawlPlayer;
    [SerializeField] private GameObject visualObject;
    [SerializeField] private SpriteRenderer visualsPlayer;
    [SerializeField] private Image forceBar;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string flyingParameter = "IsFlying";
    [SerializeField] private string spinSpeedParameter = "SpinSpeed";
    [SerializeField] private float maxSpinSpeed = 3f;

    [Header("Feedback")]
    [SerializeField] private PlayerImpactFeedback impactFeedback;

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
    [SerializeField] private float wetForce = 2f;
    [SerializeField] private float throwForce = 0;
    [SerializeField] private bool isWet;

    private Rigidbody rb;

    private enum State
    {
        Idle,
        SelectingDirection,
        ChargingPower,
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
        isWet = false;

        // Hide arrow
        if (aimArrow != null)
            aimArrow.SetActive(false);

        if (crawlPlayer != null)
            crawlPlayer.SetActive(false);

        // Hide power bar
        if (forceBar != null)
        {
            forceBar.type = Image.Type.Filled;
            forceBar.fillMethod = Image.FillMethod.Horizontal;
            forceBar.fillOrigin = 0;

            forceBar.fillAmount = 0f;
            forceBar.gameObject.SetActive(false);
        }

        // Make sure animation starts in Idle
        SetFlyingAnimation(false);
    }

    private void Update()
    {
        HandleInput();

        if (currentState == State.SelectingDirection)
        {
            RotateArrow();
        }

        if (currentState == State.ChargingPower)
        {
            UpdatePowerBar();
        }

        CheckIfStopped();
        UpdateFlyingAnimation();
    }

    private void LateUpdate()
    {
        // Keep arrow above player
        if (aimArrow != null && aimArrow.activeSelf)
        {
            aimArrow.transform.position =
                transform.position + Vector3.up * 0.1f;
        }

        // Keep visual above player
        if (visualsPlayer != null)
        {
            visualsPlayer.transform.position =
                transform.position + Vector3.up * 0.1f;
        }
    }

    // =========================================================
    // INPUT
    // =========================================================

    private void HandleInput()
    {
        if (Mouse.current == null)
            return;

        // =====================================================
        // MOUSE DOWN
        // =====================================================

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            // First click: start selecting direction
            if (currentState == State.Idle)
            {
                StartDirectionSelection();
            }

            // Second click: throw
            else if (currentState == State.ChargingPower)
            {
                Throw();
            }
        }

        // =====================================================
        // MOUSE UP
        // =====================================================

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            // Release after selecting direction
            if (currentState == State.SelectingDirection)
            {
                StartPowerSelection();
            }
        }
    }

    // =========================================================
    // FIRST CLICK / MOUSE DOWN
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

            // Start facing forward
            aimArrow.transform.rotation =
                Quaternion.LookRotation(Vector3.forward);
        }

        // -----------------------------
        // Reset player visual
        // -----------------------------

        if (visualsPlayer != null)
        {
            visualsPlayer.transform.position =
                transform.position + Vector3.up * 0.1f;
        }

        // -----------------------------
        // Hide power bar
        // -----------------------------

        if (forceBar != null)
        {
            forceBar.fillAmount = 0f;
            forceBar.gameObject.SetActive(false);
        }
    }

    // =========================================================
    // ARROW ROTATION
    // =========================================================

    private void RotateArrow()
    {
        if (aimArrow == null)
            return;

        // Rotate arrow clockwise
        aimArrow.transform.Rotate(
            Vector3.up,
            rotationSpeed * Time.deltaTime,
            Space.World
        );

        // Rotate player visual in opposite direction
        if (visualsPlayer != null)
        {
            visualsPlayer.transform.Rotate(
                Vector3.up,
                -rotationSpeed * Time.deltaTime,
                Space.World
            );
        }
    }

    // =========================================================
    // SECOND CLICK / THROW
    // =========================================================

    private void Throw()
    {
        currentState = State.Flying;

        // -----------------------------
        // GET OPPOSITE ARROW DIRECTION
        // -----------------------------

        if (aimArrow != null)
        {
            // Throw opposite to the arrow
            throwDirection = -aimArrow.transform.forward;
        }

        throwDirection.y = 0f;

        if (throwDirection.sqrMagnitude > 0.001f)
        {
            throwDirection.Normalize();
        }

        // -----------------------------
        // HIDE ARROW
        // -----------------------------

        if (aimArrow != null)
        {
            aimArrow.SetActive(false);
        }

        // -----------------------------
        // HIDE POWER BAR
        // -----------------------------

        if (forceBar != null)
        {
            forceBar.fillAmount = 0f;
            forceBar.gameObject.SetActive(false);
        }

        // -----------------------------
        // CALCULATE FORCE
        // -----------------------------
        if (isWet)
        {
            throwForce = Mathf.Lerp(
            minThrowForce,
            maxThrowForce,
            powerAmount + wetForce);
        }
        else
        {
            throwForce = Mathf.Lerp(
                minThrowForce,
                maxThrowForce,
                powerAmount
            );
        }

        // -----------------------------
        // RESET VELOCITY
        // -----------------------------

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // -----------------------------
        // APPLY IMPULSE
        // -----------------------------

        rb.AddForce(
            throwDirection * throwForce,
            ForceMode.Impulse
        );

        // -----------------------------
        // RESET POWER
        // -----------------------------

        powerAmount = 0f;
        powerIncreasing = true;

        stopTimer = 0f;

        // -----------------------------
        // START FLYING ANIMATION
        // -----------------------------

        SetFlyingAnimation(true);
    }

    // =========================================================
    // MOUSE UP / START POWER BAR
    // =========================================================

    private void StartPowerSelection()
    {
        currentState = State.ChargingPower;

        // Arrow stops rotating.
        // Its current direction is now locked.

        powerAmount = 0f;
        powerIncreasing = true;

        if (forceBar != null)
        {
            forceBar.fillAmount = 0f;
            forceBar.gameObject.SetActive(true);
        }
    }

    // =========================================================
    // POWER BAR
    // 0 -> 1 -> 0 -> 1...
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
        {
            forceBar.fillAmount = powerAmount;
        }
    }

    // =========================================================
    // FLYING ANIMATION
    // =========================================================

    private void UpdateFlyingAnimation()
    {
        if (animator == null)
            return;

        if (currentState != State.Flying)
        {
            animator.SetFloat(spinSpeedParameter, 0f);
            return;
        }

        float speed = rb.linearVelocity.magnitude;

        float normalizedSpeed = Mathf.InverseLerp(
            minThrowForce,
            maxThrowForce,
            speed
        );

        float spinSpeed = normalizedSpeed * maxSpinSpeed;

        animator.SetFloat(
            spinSpeedParameter,
            spinSpeed
        );
    }

    private void SetFlyingAnimation(bool flying)
    {
        if (animator == null)
            return;

        animator.SetBool(
            flyingParameter,
            flying
        );

        if (!flying)
        {
            animator.SetFloat(
                spinSpeedParameter,
                0f
            );
        }
    }

    // =========================================================
    // COLLISION
    // =========================================================

    private void OnCollisionEnter(Collision collision)
    {
        if (currentState != State.Flying)
            return;

        if (collision.contactCount == 0)
            return;

        ContactPoint contact = collision.GetContact(0);

        // Only measure the velocity going INTO the surface.
        float impactSpeed = Mathf.Abs(
            Vector3.Dot(
                rb.linearVelocity,
                contact.normal
            )
        );

        if (impactSpeed <= 0.01f)
            return;

        if (impactFeedback != null)
        {
            impactFeedback.PlayImpact(
                collision,
                contact.point,
                contact.normal,
                impactSpeed
            );
        }
        if (collision.gameObject.CompareTag(bouncyTag))
        {
            Debug.Log(
                $"Bouncy impact! Speed: {impactSpeed:F2}"
            );

            if (impactSpeed >= chairModeImpactThreshold)
            {
                EnterCrawlMode();
            }
        }
        if (collision.gameObject.CompareTag("Carpet"))
        {
            EnterCrawlMode();
        }


        if (collision.gameObject.CompareTag("Wet"))
        {
            isWet = true;
            Debug.Log("Mojadite");
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wet"))
        {
            Debug.Log("Sequite");
            isWet = false;
        }
    }
    // =========================================================
    // CHECK IF PLAYER STOPPED
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

        SetFlyingAnimation(false);
    }

    // =========================================================
    // SPRITE DIRECTION
    // =========================================================

    private void UpdateSpriteDirection()
    {
        if (visualsPlayer == null || aimArrow == null)
            return;

        Vector3 arrowDirection = aimArrow.transform.forward;

        // Character faces opposite arrow.
        Vector3 characterDirection = -arrowDirection;

        if (characterDirection.x < 0f)
        {
            visualsPlayer.flipX = true;
        }
        else
        {
            visualsPlayer.flipX = false;
        }
    }

    private void EnterCrawlMode()
    {
        if (crawlPlayer == null)
        {
            Debug.LogWarning("Chair Player is not assigned!");
            return;
        }

        Debug.Log("HARD BOUNCY IMPACT → CHAIR MODE");

        // Put the chair player exactly where the current player is.
        crawlPlayer.transform.position = transform.position;

        // Match the current rotation.
        crawlPlayer.transform.rotation = transform.rotation;

        // Disable the current player.
        gameObject.SetActive(false);
        visualObject.SetActive(false);
        // Activate chair player.
        crawlPlayer.SetActive(true);
    }
}