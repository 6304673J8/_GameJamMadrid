using UnityEngine;
using UnityEngine.InputSystem;

public class chairDirection : MonoBehaviour
{
    [Header("Configuración del círculo")]
    public float rotationSpeed = 180f;
    public float radius = 1.5f;

    private float angle = 0f;
    private Vector3 direction;

    private Transform playerTransform;

    void Start()
    {
        // El objeto que tiene este script debe ser hijo del jugador
        playerTransform = transform.parent;

        if (playerTransform == null)
        {
            Debug.LogError("chairDirection necesita ser hijo del jugador.");
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        // 1. Hacemos girar el ángulo
        angle += rotationSpeed * Time.deltaTime;

        // Evitamos que el ángulo crezca indefinidamente
        if (angle >= 360f)
        {
            angle -= 360f;
        }

        // 2. Calculamos la posición del indicador
        float x = Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
        float z = Mathf.Sin(angle * Mathf.Deg2Rad) * radius;

        // 3. Movemos el indicador alrededor del jugador
        transform.localPosition = new Vector3(x, 0, z);

        // 4. Comprobamos si existe el ratón
        Mouse mouse = Mouse.current;

        if (mouse == null) return;

        // 5. Click izquierdo = elegir dirección
        if (mouse.leftButton.wasPressedThisFrame)
        {
            direction = transform.position - playerTransform.position;

            direction.Normalize();

            Debug.Log("Dirección elegida: " + direction);
        }
    }
}