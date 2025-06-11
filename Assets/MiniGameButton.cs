using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;



public class MiniGameButton : MonoBehaviour
{
    public Image iconImage; // image circulaire à changer dynamiquement
    private string sceneToLoad;

    // appelé depuis un autre script
    public void Setup(Sprite icon, string sceneName)
    {

        Debug.Log("Scene affectée au bouton : " + sceneName);
        iconImage.sprite = icon;
        sceneToLoad = sceneName;
    }

    public void OnClick()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.Log("Chargement de la scène : " + sceneToLoad);
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
