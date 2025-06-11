using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MiniGameButtonController : MonoBehaviour
{
    public Image iconImage; // image circulaire à changer dynamiquement
    private string sceneToLoad;

    // appelé depuis un autre script
    public void Setup(Sprite icon, string sceneName)
    {
        iconImage.sprite = icon;
        sceneToLoad = sceneName;
    }

    public void OnClick()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
