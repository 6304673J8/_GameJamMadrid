using UnityEngine;
using UnityEngine.SceneManagement;

public class sceneLoader : MonoBehaviour
{
    [SerializeField] private string targetSceneName;

    private void OnTriggerEnter(Collider other)
    {
        // Asegúrate de que el objeto que entra sea el jugador
        if (other.CompareTag("Player"))
        {
            if (!string.IsNullOrEmpty(targetSceneName))
            {
                SceneManager.LoadScene(targetSceneName);
            }
            else
            {
                Debug.LogWarning("No se ha asignado una escena de destino en el inspector.");
            }
        }
    }
}