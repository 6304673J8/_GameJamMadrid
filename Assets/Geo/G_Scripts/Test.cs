using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class Test : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject aimArrow;
    [SerializeField] private Image forceBar;

    [Header("Throw Settings")]
    [SerializeField] private float minThrowForce = 3f;
    [SerializeField] private float maxThrowForce = 20f;
    [SerializeField] private float chargeTime = 2f;

    [Header("Arrow Settings")]
    [SerializeField] private float arrowHeight = 0.1f;

    [Header("Physics")]
    [SerializeField] private float stopSpeed = 0.15f;
    [SerializeField] private float stopTime = 0.5f;

    private Rigidbody rb;

    private bool isCharging = false;
    private bool isFlying = false;

    private Vector3 aimDirection;

    private float chargeAmount = 0f;
    private float stopTimer = 0f;

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

        if (isCharging)
        {
            UpdateAim();
            UpdateCharge();
        }

        CheckIfStopped();
    }

    // ---------------------------------------------------------
    // INPUT
    // ---------------------------------------------------------

    private void HandleInput()
    {
        if (Mouse.current == null)
            return;

        // Player presses mouse button
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (!isFlying && !isCharging)
            {
                StartCharging();
            }
        }

        // Player releases mouse button
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            if (isCharging)
            {
                Throw();
            }
        }
    }

    // ---------------------------------------------------------
    // START CHARGING
    // ---------------------------------------------------------

    private void StartCharging()
    {
        isCharging = true;

        chargeAmount = 0f;

        if (aimArrow != null)
        {
            aimArrow.SetActive(true);
        }

        if (forceBar != null)
        {
            forceBar.gameObject.SetActive(true);
            forceBar.fillAmount = 0f;
        }
    }

    // ---------------------------------------------------------
    // AIM
    // ---------------------------------------------------------

    private void UpdateAim()
    {
        if (Camera.main == null)
            return;

        Ray ray = Camera.main.ScreenPointToRay(
            Mouse.current.position.ReadValue()
        );

        // Horizontal plane at player's position
        Plane groundPlane = new Plane(
            Vector3.up,
            transform.position
        );

        if (!groundPlane.Raycast(ray, out float distance))
            return;

        Vector3 mouseWorldPosition =
            ray.GetPoint(distance);

        Vector3 direction =
            mouseWorldPosition - transform.position;

        // Ignore Y axis
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f)
            return;

        aimDirection = direction.normalized;

        // Position arrow above player
        if (aimArrow != null)
        {
            aimArrow.transform.position =
                transform.position +
                Vector3.up * arrowHeight;

            // Point arrow toward mouse
            aimArrow.transform.rotation =
                Quaternion.LookRotation(aimDirection);
        }
    }

    // ---------------------------------------------------------
    // CHARGE FORCE
    // ---------------------------------------------------------

    private void UpdateCharge()
    {
        chargeAmount += Time.deltaTime / chargeTime;

        chargeAmount = Mathf.Clamp01(chargeAmount);

        // Update UI bar
        if (forceBar != null)
        {
            forceBar.fillAmount = chargeAmount;
        }
    }

    // ---------------------------------------------------------
    // THROW
    // ---------------------------------------------------------

    private void Throw()
    {
        isCharging = false;
        isFlying = true;

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

        // Calculate force
        float throwForce = Mathf.Lerp(
            minThrowForce,
            maxThrowForce,
            chargeAmount
        );

        // Reset existing movement
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Apply impulse
        rb.AddForce(
            aimDirection * throwForce,
            ForceMode.Impulse
        );

        chargeAmount = 0f;
        stopTimer = 0f;
    }

    // ---------------------------------------------------------
    // DETECT WHEN PLAYER STOPS
    // ---------------------------------------------------------

    private void CheckIfStopped()
    {
        if (!isFlying)
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

    // ---------------------------------------------------------
    // STOP
    // ---------------------------------------------------------

    private void StopFlying()
    {
        isFlying = false;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        stopTimer = 0f;
    }
}
