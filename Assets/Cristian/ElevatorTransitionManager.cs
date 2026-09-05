using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ElevatorTransitionManager : MonoBehaviour

{
    [SerializeField] private string nextSceneName = "MainGameScene";
    [SerializeField] private float waitTime = 10f;

    void Start()
    {
        StartCoroutine(LoadNextSceneAfterDelay());
    }

    IEnumerator LoadNextSceneAfterDelay()
    {
        yield return new WaitForSeconds(waitTime);
        SceneManager.LoadScene(nextSceneName);
    }
}