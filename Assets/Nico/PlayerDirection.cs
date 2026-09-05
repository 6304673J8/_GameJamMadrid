using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerDirection : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject aimArrow;
    [SerializeField] private Slider forceBar;

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
        SelectingDirection,
        Charging,
        Flying
    }

    private State currentState = State.SelectingDirection;

    private float chargeAmount = 0f;
    private bool chargingUp = true;
    private float stopTimer = 0f;

    private Vector3 throwDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // La flecha empieza visible
        if (aimArrow != null)
        {
            aimArrow.SetActive(true);

            aimArrow.transform.position =
                transform.position + Vector3.up * 0.1f;

            aimArrow.transform.rotation =
                Quaternion.LookRotation(Vector3.forward);
        }

        // La barra empieza escondida
        if (forceBar != null)
        {
            forceBar.gameObject.SetActive(false);
            forceBar.minValue = 0f;
            forceBar.maxValue = 1f;
            forceBar.value = 0f;
        }
    }

    private void Update()
    {
        HandleInput();

        // La flecha gira mientras elegimos dirección
        if (currentState == State.SelectingDirection)
        {
            RotateArrow();
        }

        // La potencia se mueve mientras mantenemos pulsado
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

        // =====================================================
        // CLICK → FIJAR DIRECCIÓN
        // =====================================================

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (currentState == State.SelectingDirection)
            {
                StartCharging();
            }
        }

        // =====================================================
        // SOLTAR → LANZAR
        // =====================================================

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            if (currentState == State.Charging)
            {
                Throw();
            }
        }
    }

    // =========================================================
    // ARROW ROTATION
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
    // CLICK
    // FIJAR DIRECCIÓN + COMENZAR POTENCIA
    // =========================================================

    private void StartCharging()
    {
        // Guardamos la dirección de la flecha
        throwDirection = aimArrow.transform.forward;

        // Nos aseguramos de que sea horizontal
        throwDirection.y = 0f;
        throwDirection.Normalize();

        // Reiniciamos potencia
        chargeAmount = 0f;
        chargingUp = true;

        // Cambiamos al estado de carga
        currentState = State.Charging;

        // Mostramos la barra
        if (forceBar != null)
        {
            forceBar.gameObject.SetActive(true);
            forceBar.value = 0f;
        }

        Debug.Log("Dirección elegida: " + throwDirection);
    }

    // =========================================================
    // POTENCIA
    // SUBE Y BAJA MIENTRAS MANTENEMOS EL CLIC
    // =========================================================

    private void ChargeForce()
    {
        if (chargingUp)
        {
            // La barra SUBE
            chargeAmount += Time.deltaTime / chargeTime;

            if (chargeAmount >= 1f)
            {
                chargeAmount = 1f;
                chargingUp = false;
            }
        }
        else
        {
            // La barra BAJA
            chargeAmount -= Time.deltaTime / chargeTime;

            if (chargeAmount <= 0f)
            {
                chargeAmount = 0f;
                chargingUp = true;
            }
        }

        // Movemos el Handle del Slider
        if (forceBar != null)
        {
            forceBar.value = chargeAmount;
        }
    }

    // =========================================================
    // SOLTAR CLIC
    // ELEGIR POTENCIA + LANZAR
    // =========================================================

    private void Throw()
    {
        // Guardamos la potencia exacta en el momento de soltar
        float powerPercentage = chargeAmount * 100f;

        // Calculamos la fuerza real
        float throwForce = Mathf.Lerp(
            minThrowForce,
            maxThrowForce,
            chargeAmount
        );

        Debug.Log(
            "Potencia elegida: " +
            powerPercentage.ToString("F1") +
            "%"
        );

        Debug.Log(
            "Fuerza del lanzamiento: " +
            throwForce.ToString("F2")
        );

        // Cambiamos al estado de vuelo
        currentState = State.Flying;

        // Escondemos flecha
        if (aimArrow != null)
        {
            aimArrow.SetActive(false);
        }

        // Escondemos barra
        if (forceBar != null)
        {
            forceBar.gameObject.SetActive(false);
        }

        // Detenemos movimiento anterior
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // LANZAMOS
        rb.AddForce(
            throwDirection * throwForce,
            ForceMode.Impulse
        );

        chargeAmount = 0f;
        stopTimer = 0f;
    }

    // =========================================================
    // COMPROBAR SI EL JUGADOR SE DETUVO
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
    // VOLVER A ELEGIR DIRECCIÓN
    // =========================================================

    private void StopFlying()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        stopTimer = 0f;

        // Volvemos a seleccionar dirección
        currentState = State.SelectingDirection;

        // Mostramos la flecha
        if (aimArrow != null)
        {
            aimArrow.SetActive(true);

            aimArrow.transform.position =
                transform.position + Vector3.up * 0.1f;
        }

        // Reiniciamos potencia
        chargeAmount = 0f;
        chargingUp = true;
    }
}