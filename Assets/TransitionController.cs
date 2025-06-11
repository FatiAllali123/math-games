using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;




public class TransitionController : MonoBehaviour
{
    private string nextSceneName;

    public RectTransform imageHaut;
    public RectTransform imageBas;
    public Image logo;
    public Slider loadingBar;
    public float animDuration = 1.0f;
    public float fadeDuration = 0.5f; // durée du fade-in


    private Color goldColor = new Color(1f, 0.843f, 0f);

    public Image fillImage;

    void Start()
    {
        // Logo invisible au départ
        logo.canvasRenderer.SetAlpha(0f);
        loadingBar.gameObject.SetActive(false);

        StartCoroutine(PlayTransition());
    }

    IEnumerator PlayTransition()
    {
        float demiHauteurHaut = imageHaut.rect.height;
        float demiHauteurBas = imageBas.rect.height;

        Vector2 hautStart = new Vector2(0, demiHauteurHaut);
        Vector2 basStart = new Vector2(0, -demiHauteurBas);

        Vector2 hautTarget = new Vector2(0, demiHauteurHaut / 2f);
        Vector2 basTarget = -hautTarget;

        imageHaut.anchoredPosition = hautStart;
        imageBas.anchoredPosition = basStart;

        float time = 0;
        while (time < animDuration)
        {
            time += Time.deltaTime;
            float t = time / animDuration;

            imageHaut.anchoredPosition = Vector2.Lerp(hautStart, hautTarget, t);
            imageBas.anchoredPosition = Vector2.Lerp(basStart, basTarget, t);
            yield return null;
        }

        imageHaut.anchoredPosition = hautTarget;
        imageBas.anchoredPosition = basTarget;

        // Fade-in du logo uniquement
        yield return StartCoroutine(FadeInLogo());

        yield return StartCoroutine(LoadSceneWithBar());
    }

    IEnumerator FadeInLogo()
    {
        logo.gameObject.SetActive(true);
        logo.CrossFadeAlpha(1f, fadeDuration, false);
        yield return new WaitForSeconds(fadeDuration);
    }


    IEnumerator LoadSceneWithBar()
    {
        loadingBar.gameObject.SetActive(true);

        nextSceneName = SceneLoader.Instance.targetSceneName;

        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogError("Nom de la scène cible vide !");
            yield break;
        }

        AsyncOperation op = SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Single); // ou Additive si tu veux gérer manuellement
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
        {
            loadingBar.value = op.progress;
            fillImage.color = Color.Lerp(Color.red, goldColor, loadingBar.value);
            yield return null;
        }

        // Remplissage final
        float time = 0;
        while (time < 1f)
        {
            loadingBar.value = Mathf.Lerp(0.9f, 1f, time);
            fillImage.color = Color.Lerp(Color.red, goldColor, loadingBar.value);
            time += Time.deltaTime;
            yield return null;
        }

        loadingBar.value = 1f;
        fillImage.color = goldColor;

        yield return new WaitForSeconds(0.5f);

        op.allowSceneActivation = true;

        // Optionnel : attendre l'activation de la scène
        while (!op.isDone)
        {
            yield return null;
        }

        // Détruire la scène de transition après l’activation
        SceneManager.UnloadSceneAsync("NomDeLaSceneDeTransition");
    }



}
