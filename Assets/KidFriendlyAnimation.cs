using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class KidFriendlyAnimations : MonoBehaviour
{
    [Header("UI References (Assign in Inspector)")]
    [SerializeField] private Button submitButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button addCompositionButton;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text compositionsFoundText;
    [SerializeField] private Slider progressBar;
    [SerializeField] private Image starImage;
    [SerializeField] private DigitSlot leftDigitSlot;
    [SerializeField] private DigitSlot rightDigitSlot;

    private void Start()
    {
        InitializeAnimations();
    }

    private void InitializeAnimations()
    {
        // Start idle animations
        if (submitButton != null) StartCoroutine(IdleScaleButton(submitButton.transform));
        if (nextButton != null) StartCoroutine(IdleScaleButton(nextButton.transform));
        if (addCompositionButton != null) StartCoroutine(IdleScaleButton(addCompositionButton.transform));
        if (resultText != null) StartCoroutine(IdleWiggleText(resultText.transform));
        if (compositionsFoundText != null) StartCoroutine(IdleWiggleText(compositionsFoundText.transform));
        if (leftDigitSlot != null && leftDigitSlot.slotText != null) StartCoroutine(IdleWiggleSlot(leftDigitSlot.slotText.transform));
        if (rightDigitSlot != null && rightDigitSlot.slotText != null) StartCoroutine(IdleWiggleSlot(rightDigitSlot.slotText.transform));
        if (starImage != null) StartCoroutine(IdleRotateStar(starImage.transform));
    }

    private IEnumerator IdleScaleButton(Transform target)
    {
        while (true)
        {
            yield return ScaleTransform(target, Vector3.one * 1.1f, 0.5f);
            yield return ScaleTransform(target, Vector3.one, 0.5f);
        }
    }

    private IEnumerator IdleWiggleText(Transform target)
    {
        while (true)
        {
            yield return RotateTransform(target, Quaternion.Euler(0, 0, 5), 0.7f);
            yield return RotateTransform(target, Quaternion.Euler(0, 0, -5), 0.7f);
        }
    }

    private IEnumerator IdleWiggleSlot(Transform target)
    {
        while (true)
        {
            yield return MoveTransform(target, new Vector3(5, 5, 0), 0.6f);
            yield return MoveTransform(target, new Vector3(-5, -5, 0), 0.6f);
        }
    }

    private IEnumerator IdleRotateStar(Transform target)
    {
        while (true)
        {
            yield return RotateTransform(target, Quaternion.Euler(0, 0, 360), 2f);
            target.rotation = Quaternion.identity; // Reset to avoid drift
        }
    }

    private IEnumerator ScaleTransform(Transform target, Vector3 targetScale, float duration)
    {
        Vector3 startScale = target.localScale;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            target.localScale = Vector3.Lerp(startScale, targetScale, elapsed / duration);
            yield return null;
        }
        target.localScale = targetScale;
    }

    private IEnumerator RotateTransform(Transform target, Quaternion targetRotation, float duration)
    {
        Quaternion startRotation = target.rotation;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            target.rotation = Quaternion.Lerp(startRotation, targetRotation, elapsed / duration);
            yield return null;
        }
        target.rotation = targetRotation;
    }

    private IEnumerator MoveTransform(Transform target, Vector3 offset, float duration)
    {
        Vector3 startPosition = target.localPosition;
        Vector3 targetPosition = startPosition + offset;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            target.localPosition = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
            yield return null;
        }
        target.localPosition = targetPosition;
    }

    public void AnimateButtonClick(Button button)
    {
        if (button != null)
        {
            StartCoroutine(PunchScale(button.transform));
        }
    }

    private IEnumerator PunchScale(Transform target)
    {
        Vector3 originalScale = target.localScale;
        yield return ScaleTransform(target, originalScale * 1.2f, 0.15f);
        yield return ScaleTransform(target, originalScale * 0.9f, 0.1f);
        yield return ScaleTransform(target, originalScale, 0.1f);
    }

    public void AnimateFeedback(TMP_Text text)
    {
        if (text == null) return;
        StartCoroutine(SlideAndFadeFeedback(text));
    }

    private IEnumerator SlideAndFadeFeedback(TMP_Text text)
    {
        CanvasGroup canvasGroup = text.gameObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = text.gameObject.AddComponent<CanvasGroup>();
        }

        Vector3 startPos = text.transform.localPosition;
        Vector3 slideUpPos = startPos + new Vector3(0, 20, 0);
        canvasGroup.alpha = 0;
        yield return StartCoroutine(FadeCanvasGroup(canvasGroup, 1, 0.4f));
        yield return StartCoroutine(MoveTransform(text.transform, slideUpPos - startPos, 0.4f));
        yield return new WaitForSeconds(1.5f);
        yield return StartCoroutine(FadeCanvasGroup(canvasGroup, 0, 0.4f));
        text.transform.localPosition = startPos; // Reset position
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup group, float targetAlpha, float duration)
    {
        float startAlpha = group.alpha;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }
        group.alpha = targetAlpha;
    }

    public void AnimateResultText(TMP_Text text)
    {
        if (text != null)
        {
            StartCoroutine(ShakeText(text.transform));
        }
    }

    private IEnumerator ShakeText(Transform target)
    {
        Vector3 originalPos = target.localPosition;
        for (int i = 0; i < 4; i++)
        {
            yield return MoveTransform(target, new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), 0), 0.1f);
        }
        target.localPosition = originalPos;
    }

    public void AnimateProgressBar(Slider slider, float targetValue)
    {
        if (slider != null)
        {
            StartCoroutine(SmoothProgress(slider, targetValue));
        }
    }

    private IEnumerator SmoothProgress(Slider slider, float targetValue)
    {
        float startValue = slider.value;
        float elapsed = 0f;
        float duration = 0.7f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            slider.value = Mathf.Lerp(startValue, targetValue, elapsed / duration);
            yield return null;
        }
        slider.value = targetValue;
    }

    public void AnimateCorrectAnswer(DigitSlot slot)
    {
        if (slot != null && slot.slotText != null)
        {
            StartCoroutine(BounceSlot(slot.slotText.transform));
        }
    }

    private IEnumerator BounceSlot(Transform target)
    {
        Vector3 originalScale = target.localScale;
        yield return ScaleTransform(target, originalScale * 1.4f, 0.2f);
        yield return ScaleTransform(target, originalScale * 0.9f, 0.1f);
        yield return ScaleTransform(target, originalScale, 0.1f);
    }

    public void AnimateCompositionsFound(TMP_Text text)
    {
        if (text != null)
        {
            StartCoroutine(ShakeText(text.transform));
        }
    }
}