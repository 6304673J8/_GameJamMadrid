using UnityEngine;
using UnityEngine.SceneManagement;

public class ToNextLevel : MonoBehaviour
{
    private int nextSceneToLoad;
    [SerializeField] private GameObject _avoidReturn;

    void Start()
    {
        nextSceneToLoad = SceneManager.GetActiveScene().buildIndex + 1;
    }

    private void OnTriggerEnter(Collider other)
    {
        _avoidReturn.SetActive(true);
        //SceneTransitionManager.singleton.GoToSceneAsync(nextSceneToLoad);
        TransitionManager.singleton.GoToSceneAsync(nextSceneToLoad);
    }
}
