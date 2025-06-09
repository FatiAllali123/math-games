using UnityEngine;
using TMPro;
using System.Collections;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("UI References")]
    public TMP_Text scoreText;
    public TMP_Text scoreGainText;
    public Transform coinIcon;

    [Header("Settings")]
    public int pointsPerCorrectAnswer = 10;
    public float coinShakeDuration = 0.5f;
    public float gainDisplayDuration = 1f;

    private int _currentScore;
    private Vector3 _originalCoinRotation;
    private Vector3 _originalScoreScale;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        _originalCoinRotation = coinIcon.eulerAngles;
        _originalScoreScale = scoreText.transform.localScale;
    }

    public void AddScore(int amount)
    {
        if (amount > 0)
        {
            _currentScore += amount; // Add positive
        }
        else if (amount < 0)
        {
            _currentScore -= Mathf.Abs(amount); // Subtract absolute value
        }

        _currentScore = Mathf.Max(0, _currentScore); // Clamp to 0
        UpdateScoreUI();

        // Animation for positive gains only
        if (amount > 0)
        {
            scoreGainText.text = $"+{amount}";
            scoreGainText.gameObject.SetActive(true);
            StartCoroutine(ShowScoreGain());
            StartCoroutine(ShakeCoin());
        }
        else if (amount < 0)
        {
            // Show negative feedback (e.g., red "-5" popup)
            scoreGainText.text = $"-{Mathf.Abs(amount)}"; // Will show "-5"
            scoreGainText.color = Color.red;
            scoreGainText.gameObject.SetActive(true);
            StartCoroutine(ShowScoreGain());
        }
    }
    private IEnumerator ShowScoreGain()
    {
        float elapsed = 0f;
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one * 1.2f;

        // Animation d'apparition
        while (elapsed < 0.3f)
        {
            scoreGainText.transform.localScale = Vector3.Lerp(startScale, endScale, elapsed / 0.3f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        // Animation de disparition
        elapsed = 0f;
        while (elapsed < 0.2f)
        {
            scoreGainText.transform.localScale = Vector3.Lerp(endScale, Vector3.zero, elapsed / 0.2f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        scoreGainText.gameObject.SetActive(false);
    }

    private IEnumerator ShakeCoin()
    {
        float elapsed = 0f;
        while (elapsed < coinShakeDuration)
        {
            float zRot = Mathf.Sin(elapsed * 30f) * 30f; // Oscillation rapide
            coinIcon.eulerAngles = _originalCoinRotation + new Vector3(0, 0, zRot);
            elapsed += Time.deltaTime;
            yield return null;
        }
        coinIcon.eulerAngles = _originalCoinRotation;
    }

    private void UpdateScoreUI()
    {
        scoreText.text = _currentScore.ToString();
        StartCoroutine(PulseScoreText());
    }

    private IEnumerator PulseScoreText()
    {
        float elapsed = 0f;
        float duration = 0.3f;
        Vector3 targetScale = _originalScoreScale * 1.2f;

        while (elapsed < duration / 2)
        {
            scoreText.transform.localScale = Vector3.Lerp(_originalScoreScale, targetScale, elapsed / (duration / 2));
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < duration / 2)
        {
            scoreText.transform.localScale = Vector3.Lerp(targetScale, _originalScoreScale, elapsed / (duration / 2));
            elapsed += Time.deltaTime;
            yield return null;
        }

        scoreText.transform.localScale = _originalScoreScale;
    }
}