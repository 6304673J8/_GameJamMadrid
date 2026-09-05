using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class ClickAndThrow : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject aimArrow;

    [Header("Throw Settings")]
    [SerializeField] private float maxThrowForce = 15f;
    [SerializeField] private float maxAimDistance = 5f;
    [SerializeField] private float minimumThrowDistance = 0.2f;

    [Header("Stopping Settings")]
    [SerializeField] private float stopSpeed = 0.15f;
    [SerializeField] private float stopTime = 0.5f;

    [Header("Arrow Settings")]
    [SerializeField] private float arrowHeight = 0.1f;
    [SerializeField] private float arrowMinLength = 0.5f;
    [SerializeField] private float arrowMaxLength = 5f;

    private Rigidbody rb;

    private bool isAiming = false;
    private bool isFlying = false;

    private Vector3 aimDirection;
    private float aimDistance;

    private float timeBelowStopSpeed = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (aimArrow != null)
        {
            aimArrow.SetActive(false);
        }
    }

    private void Update()
    {
        HandleInput();

        if (isAiming)
        {
            UpdateAimArrow();
        }

        CheckIfStopped();
    }

    private void HandleInput()
    {
        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        if (!isAiming && !isFlying)
        {
            StartAiming();
        }
        else if (isAiming)
        {
            Throw();
        }
    }

    private void StartAiming()
    {
        isAiming = true;

        if (aimArrow != null)
        {
            aimArrow.SetActive(true);
        }
    }

    private void UpdateAimArrow()
    {
        Ray ray = Camera.main.ScreenPointToRay(
            Mouse.current.position.ReadValue()
        );

        Plane groundPlane = new Plane(
            Vector3.up,
            transform.position
        );

        if (!groundPlane.Raycast(ray, out float distance))
            return;

        Vector3 mouseWorldPosition = ray.GetPoint(distance);

        Vector3 direction = mouseWorldPosition - transform.position;
        direction.y = 0f;

        aimDistance = direction.magnitude;

        if (aimDistance < minimumThrowDistance)
            return;

        aimDistance = Mathf.Min(aimDistance, maxAimDistance);

        aimDirection = direction.normalized;

        if (aimArrow != null)
        {
            aimArrow.transform.position =
                transform.position + Vector3.up * arrowHeight;

            aimArrow.transform.rotation =
                Quaternion.LookRotation(aimDirection);

            float normalizedDistance =
                aimDistance / maxAimDistance;

            float arrowLength =
                Mathf.Lerp(
                    arrowMinLength,
                    arrowMaxLength,
                    normalizedDistance
                );

            Vector3 arrowScale = aimArrow.transform.localScale;
            arrowScale.z = arrowLength;
            aimArrow.transform.localScale = arrowScale;
        }
    }

    private void Throw()
    {
        // Don't throw if we don't have a valid direction
        if (aimDirection.sqrMagnitude < 0.01f)
            return;

        // Don't throw if mouse is too close
        if (aimDistance < minimumThrowDistance)
            return;

        isAiming = false;
        isFlying = true;

        if (aimArrow != null)
        {
            aimArrow.SetActive(false);
        }

        // Stop previous movement
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Calculate throw strength
        float normalizedDistance =
            Mathf.Clamp01(aimDistance / maxAimDistance);

        float throwForce =
            normalizedDistance * maxThrowForce;

        // Throw!
        rb.AddForce(
            aimDirection * throwForce,
            ForceMode.Impulse
        );

        timeBelowStopSpeed = 0f;
    }

    private void CheckIfStopped()
    {
        if (!isFlying)
            return;

        // Rigidbody speed
        float speed = rb.linearVelocity.magnitude;

        if (speed < stopSpeed)
        {
            timeBelowStopSpeed += Time.deltaTime;

            if (timeBelowStopSpeed >= stopTime)
            {
                StopFlying();
            }
        }
        else
        {
            timeBelowStopSpeed = 0f;
        }
    }

    private void StopFlying()
    {
        isFlying = false;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        timeBelowStopSpeed = 0f;
    }
}