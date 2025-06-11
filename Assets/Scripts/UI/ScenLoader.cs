using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;

    public string targetSceneName;

    private void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Ne pas détruire entre les scènes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadSceneWithTransition(string targetScene)
    {
        targetSceneName = targetScene;
        // UnityEngine.SceneManagement.SceneManager.LoadScene("transition"  , LoadSceneMode.Additive); // charge la scène intermédiaire
        //UnityEngine.SceneManagement.SceneManager.LoadScene("transition", LoadSceneMode.Additive);
        SceneManager.LoadScene("transition", LoadSceneMode.Additive);
    }
}


