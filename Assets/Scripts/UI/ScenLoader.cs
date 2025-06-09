using System.Diagnostics;
using UnityEngine;

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
        UnityEngine.SceneManagement.SceneManager.LoadScene("transition"); // charge la scène intermédiaire
    }
}


