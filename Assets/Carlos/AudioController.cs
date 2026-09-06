using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioController : MonoBehaviour
{
    [SerializeField] private AudioSource persistentAudioSource;

    private void Awake()
    {
        RegisterPersistentAudio();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void RegisterPersistentAudio()
    {
        if (persistentAudioSource == null)
            return;

        DontDestroyOnLoad(persistentAudioSource.gameObject);

        if (!persistentAudioSource.isPlaying)
        {
            persistentAudioSource.Play();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex >= 3)
        {
            StopPersistentAudio();
        }
    }

    private void StopPersistentAudio()
    {
        if (persistentAudioSource == null)
            return;

        Destroy(persistentAudioSource.gameObject);
        persistentAudioSource = null;
    }
}