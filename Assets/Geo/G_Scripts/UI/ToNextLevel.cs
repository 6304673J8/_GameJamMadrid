using UnityEngine;
using UnityEngine.SceneManagement;

public class ToNextLevel : MonoBehaviour
{
    [SerializeField] private int nextSceneToLoad = 0;

    void Start()
    {
        if (nextSceneToLoad == 0)
            nextSceneToLoad = SceneManager.GetActiveScene().buildIndex + 1;
    }

    private void OnTriggerEnter(Collider other)
    {
        //SceneTransitionManager.singleton.GoToSceneAsync(nextSceneToLoad);
        TransitionManager.singleton.GoToSceneAsync(nextSceneToLoad);
    }
}
