using UnityEngine;
using UnityEngine.UI;

public class Time_controller : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image cafeImage;

    [Header("Panel CONTINUE")]
    [SerializeField] private GameObject continuePanel;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    [Header("Cafetera UI")]
    [Tooltip("Contenedor UI de la cafetera (hijo del Canvas). Se oculta en la Title_Screen.")]
    [SerializeField] private GameObject cafetera;

    [Header("Timer")]
    [SerializeField] private float timeInSeconds = 120f;

    private float totalTime;
    private float remainingTime;
    private bool isRunning = true;

    public float RemainingTime => remainingTime;

    private void Start()
    {
        totalTime = Mathf.Max(timeInSeconds, 0.0001f);
        remainingTime = timeInSeconds;

        if (cafeImage == null)
        {
            cafeImage = FindCafeImage();
        }

        if (cafeImage == null)
        {
            Debug.LogWarning(
                "Time_controller: no se encontró la UI Image \"Café\"."
            );

            return;
        }

        // El Canvas de la UI sobrevive a los cambios de escena.
        DontDestroyOnLoad(cafeImage.canvas.gameObject);

        // Evita duplicados de Canvas al volver a cargar la escena.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterCanvas(cafeImage.canvas);
        }

        // La Image ya viene configurada en Filled; solo controlamos el fillAmount.
        cafeImage.fillAmount = 1f;

        FindContinuePanel();
        WireButtons();

        // El panel CONTINUE y los botones solo se muestran al agotar el tiempo.
        SetContinuePanelVisible(false);
    }

    private void Update()
    {
        if (!isRunning)
            return;

        // Si ya se ganó, el contador se detiene.
        if (GameManager.Instance != null && GameManager.Instance.HasWon)
        {
            isRunning = false;
            return;
        }

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            isRunning = false;

            if (cafeImage != null)
            {
                cafeImage.fillAmount = 0f;
            }

            HandleTimeOut();
            return;
        }

        if (cafeImage != null)
        {
            cafeImage.fillAmount = remainingTime / totalTime;
        }
    }

    private void HandleTimeOut()
    {
        if (GameManager.Instance != null && GameManager.Instance.HasWon)
            return;

        Debug.Log("Has perdido");

        SetContinuePanelVisible(true);
    }

    public void ResetTimer()
    {
        remainingTime = timeInSeconds;
        isRunning = true;

        if (cafeImage != null)
        {
            cafeImage.fillAmount = 1f;
        }

        SetContinuePanelVisible(false);
    }

    public void StopTimer()
    {
        isRunning = false;
        SetContinuePanelVisible(false);
    }

    // --- Cafetera UI (visible solo en escenas de gameplay) ---

    public void ActivateCafetera()
    {
        SetCafeteraActive(true);
    }

    public void DeactivateCafetera()
    {
        SetCafeteraActive(false);
    }

    private void SetCafeteraActive(bool active)
    {
        if (cafetera != null)
        {
            cafetera.SetActive(active);
        }
    }

    private void SetContinuePanelVisible(bool visible)
    {
        if (continuePanel != null)
        {
            continuePanel.SetActive(visible);
        }

        if (yesButton != null)
        {
            yesButton.gameObject.SetActive(visible);
        }

        if (noButton != null)
        {
            noButton.gameObject.SetActive(visible);
        }
    }

    private void FindContinuePanel()
    {
        Transform canvasTransform = cafeImage.canvas.transform;

        if (continuePanel == null)
        {
            continuePanel = FindUIObject(canvasTransform, "CONTINUE");
        }

        if (yesButton == null)
        {
            yesButton = FindButton(canvasTransform, "YES");
        }

        if (noButton == null)
        {
            noButton = FindButton(canvasTransform, "NO");
        }

        if (cafetera == null)
        {
            cafetera = FindUIObject(canvasTransform, "Cafetera");
        }
    }

    private GameObject FindUIObject(Transform canvasTransform, string name)
    {
        foreach (Transform child in
            canvasTransform.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == name)
            {
                return child.gameObject;
            }
        }

        return null;
    }

    private Button FindButton(Transform canvasTransform, string name)
    {
        foreach (Button button in
            canvasTransform.GetComponentsInChildren<Button>(true))
        {
            if (button.name == name)
            {
                return button;
            }
        }

        return null;
    }

    private void WireButtons()
    {
        // Solo se conectan automáticamente si el OnClick está vacío, para no
        // pisar una configuración manual hecha en el Inspector.
        if (yesButton != null && yesButton.onClick.GetPersistentEventCount() == 0)
        {
            yesButton.onClick.AddListener(
                () => GameManager.Instance?.OnYesButtonClick()
            );
        }

        if (noButton != null && noButton.onClick.GetPersistentEventCount() == 0)
        {
            noButton.onClick.AddListener(
                () => GameManager.Instance?.OnNoButtonClick()
            );
        }
    }

    private Image FindCafeImage()
    {
        Image[] images = FindObjectsByType<Image>(FindObjectsSortMode.None);

        foreach (Image image in images)
        {
            if (image.gameObject.name == "Caf\u00E9")
            {
                return image;
            }
        }

        return null;
    }
}