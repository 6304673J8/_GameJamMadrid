using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Escenas")]
    [Tooltip("Adonde lleva el botón NO.")]
    [SerializeField] private string titleSceneName = "Title_Screen";

    public static GameManager Instance { get; private set; }

    public bool HasWon { get; private set; }

    // Historial de escenas para saber cuál es "la anterior".
    private readonly Stack<string> sceneHistory = new Stack<string>();

    // Canvas y EventSystem persistentes (evita duplicados al recargar escenas).
    private Canvas persistentCanvas;
    private EventSystem persistentEventSystem;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        sceneHistory.Clear();
        sceneHistory.Push(SceneManager.GetActiveScene().name);

        MakeEventSystemPersistent();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (sceneHistory.Count == 0 || sceneHistory.Peek() != scene.name)
        {
            sceneHistory.Push(scene.name);
        }

        MakeEventSystemPersistent();
        DestroyTransientCanvases();
    }

    // --- Persistencia y anti-duplicados ---

    /// <summary>
    /// Marca el Canvas de la partida como persistente (lo llama Time_controller).
    /// </summary>
    public void RegisterCanvas(Canvas canvas)
    {
        if (persistentCanvas == null)
        {
            persistentCanvas = canvas;
        }
    }

    private void MakeEventSystemPersistent()
    {
        EventSystem[] eventSystems =
            FindObjectsByType<EventSystem>(FindObjectsSortMode.None);

        foreach (EventSystem eventSystem in eventSystems)
        {
            if (persistentEventSystem == null)
            {
                persistentEventSystem = eventSystem;
                DontDestroyOnLoad(eventSystem.gameObject);
            }
            else if (eventSystem != persistentEventSystem)
            {
                Destroy(eventSystem.gameObject);
            }
        }
    }

    private void DestroyTransientCanvases()
    {
        if (persistentCanvas == null)
            return;

        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);

        foreach (Canvas canvas in canvases)
        {
            if (canvas != persistentCanvas)
            {
                Destroy(canvas.gameObject);
            }
        }
    }

    // --- Navegación ---

    public void ReturnToPreviousScene()
    {
        if (sceneHistory.Count > 1)
        {
            // Sale la escena actual y deja arriba la anterior.
            sceneHistory.Pop();
        }

        string sceneToLoad = sceneHistory.Peek();

        if (!Application.CanStreamedLevelBeLoaded(sceneToLoad))
        {
            Debug.LogWarning(
                $"GameManager: la escena '{sceneToLoad}' no está en Build Settings."
            );

            return;
        }

        SceneManager.LoadScene(sceneToLoad);
    }

    public void GoToScreenTitle()
    {
        Time_controller timeController = GetComponent<Time_controller>();
        if (timeController != null)
        {
            timeController.ResetTimer();
            timeController.StopTimer();

            // En la Title_Screen la cafetera no tiene efecto.
            timeController.DeactivateCafetera();
        }

        if (!Application.CanStreamedLevelBeLoaded(titleSceneName))
        {
            Debug.LogWarning(
                $"GameManager: la escena '{titleSceneName}' no está en Build Settings."
            );
            return;
        }

        SceneManager.LoadScene(titleSceneName);
    }

    // --- Métodos públicos para los botones YES / NO ---

    public void OnYesButtonClick()
    {
        HasWon = false;

        ResetTimer();

        // Volvemos a gameplay: la cafetera se muestra de nuevo.
        Time_controller timeController = GetComponent<Time_controller>();
        if (timeController != null)
        {
            timeController.ActivateCafetera();
        }

        ReturnToPreviousScene();
    }

    public void OnNoButtonClick()
    {
        HasWon = false;

        GoToScreenTitle();
    }

    public void WinGame()
    {
        HasWon = true;

        Debug.Log("Has ganado");
    }

    private void ResetTimer()
    {
        Time_controller timeController = GetComponent<Time_controller>();
        if (timeController != null)
        {
            timeController.ResetTimer();
        }
    }
}