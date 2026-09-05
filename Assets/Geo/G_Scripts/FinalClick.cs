using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class FinalClick : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject aimArrow;
    [SerializeField] private SpriteRenderer visualsPlayer;
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

        // Arrow rotates while mouse button is held
        if (currentState == State.SelectingDirection)
        {
            RotateArrow();
        }

        // Power bar moves after releasing the mouse
        if (currentState == State.ChargingPower)
        {
            UpdatePowerBar();
        }

        CheckIfStopped();
    }


    private void LateUpdate()
    {
        if (aimArrow != null && aimArrow.activeSelf)
            aimArrow.transform.position = transform.position + Vector3.up * 0.1f; 
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
        // Make player visuals match arrow
        // -----------------------------

        /*if (visualsPlayer != null && aimArrow != null)
        {
            visualsPlayer.transform.rotation =
                aimArrow.transform.rotation;
        }*/

        if (visualsPlayer != null)
            visualsPlayer.transform.position = transform.position + Vector3.up * 0.1f;

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

        // Rotate player visual in the OPPOSITE direction
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
    // SECOND CLICK
    // THROW
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

        float throwForce = Mathf.Lerp(
            minThrowForce,
            maxThrowForce,
            powerAmount
        );

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
    }


    // =========================================================
    // MOUSE UP
    // LOCK DIRECTION + START POWER BAR
    // =========================================================

    private void StartPowerSelection()
    {
        currentState = State.ChargingPower;

        // Arrow stays visible but stops rotating.
        // Its current forward direction is now locked.

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
    }

    private void UpdateSpriteDirection()
    {
        if (visualsPlayer == null || aimArrow == null)
            return;

        Vector3 arrowDirection = aimArrow.transform.forward;

        // We want the sprite to face OPPOSITE the arrow.
        Vector3 characterDirection = -arrowDirection;

        // If the character is facing right by default:
        // flip the sprite when its desired direction is left.
        if (characterDirection.x < 0f)
        {
            visualsPlayer.flipX = true;
        }
        else
        {
            visualsPlayer.flipX = false;
        }
    }

}
