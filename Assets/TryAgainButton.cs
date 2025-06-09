using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(Button), typeof(AudioSource))]
public class TryAgainButton : MonoBehaviour
{
    [Header("Visual Effects")]
    [SerializeField] private float hoverScale = 1.2f;
    [SerializeField] private float clickScale = 0.9f;
    [SerializeField] private float animationDuration = 0.2f;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip clickSound;

    [Header("Particles")]
    [SerializeField] private ParticleSystem confettiParticles;

    private Vector3 originalScale;
    private AudioSource audioSource;
    private Button button;
    private Coroutine currentAnimation;

    void Awake()
    {
        button = GetComponent<Button>();
        audioSource = GetComponent<AudioSource>();
        originalScale = transform.localScale;

        button.onClick.AddListener(OnClick);
    }

    public void OnHoverStart()
    {
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        currentAnimation = StartCoroutine(ScaleAnimation(originalScale * hoverScale, animationDuration));

        if (hoverSound) audioSource.PlayOneShot(hoverSound);
    }

    public void OnHoverEnd()
    {
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        currentAnimation = StartCoroutine(ScaleAnimation(originalScale, animationDuration));
    }

    public void OnClick()
    {
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        // Play click sound
        if (clickSound) audioSource.PlayOneShot(clickSound);

        // Play particles
        if (confettiParticles) confettiParticles.Play();

        // Start click animation
        StartCoroutine(ClickAnimation());
    }

    private IEnumerator ScaleAnimation(Vector3 targetScale, float duration)
    {
        Vector3 startScale = transform.localScale;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            // Smooth step interpolation
            t = t * t * (3f - 2f * t);
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        transform.localScale = targetScale;
    }

    private IEnumerator ClickAnimation()
    {
        // Scale down
        yield return ScaleAnimation(originalScale * clickScale, animationDuration / 2);

        // Scale up
        yield return ScaleAnimation(originalScale, animationDuration / 2);

        // Reload scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}