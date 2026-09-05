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

    // Indica si ya hemos elegido la dirección
    private bool directionSelected = false;

    void Start()
    {
        playerTransform = transform.parent;

        if (playerTransform == null)
        {
            Debug.LogError("chairDirection necesita ser hijo del jugador.");
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        // Mientras NO hayamos elegido dirección, el indicador gira
        if (!directionSelected)
        {
            angle += rotationSpeed * Time.deltaTime;

            if (angle >= 360f)
            {
                angle -= 360f;
            }

            float x = Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
            float z = Mathf.Sin(angle * Mathf.Deg2Rad) * radius;

            transform.localPosition = new Vector3(x, 0, z);
        }

        Mouse mouse = Mouse.current;

        if (mouse == null) return;

        // Click izquierdo
        if (mouse.leftButton.wasPressedThisFrame && !directionSelected)
        {
            // Calculamos la dirección
            direction = transform.position - playerTransform.position;

            direction.Normalize();

            // Detenemos el indicador
            directionSelected = true;

            Debug.Log("Dirección elegida: " + direction);
        }
    }
}