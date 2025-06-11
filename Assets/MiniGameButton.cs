using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;



public class MiniGameButton : MonoBehaviour
{
    public Image iconImage; // image circulaire � changer dynamiquement
    private string sceneToLoad;

    // appel� depuis un autre script
    public void Setup(Sprite icon, string sceneName)
    {

        Debug.Log("Scene affect�e au bouton : " + sceneName);
        iconImage.sprite = icon;
        sceneToLoad = sceneName;
    }

    public void OnClick()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.Log("Chargement de la sc�ne : " + sceneToLoad);
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
