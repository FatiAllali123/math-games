using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance; // Singleton for easy access

    [Header("UI References")]
    public Slider timeSlider; // Visual timeline (slider)
    public TMP_Text timeText; // Optional: Display remaining time
    public GameObject gameOverPanel; // Panel to show when time runs out
    public TMP_Text gameOverText; // "Game Over! You Lost!"

    [Header("Time Settings")]
    public float initialTime = 30f; // Starting time
    public float extraTimePerPanel = 20f; // Additional time when adding a panel

    private float currentTime;
    private bool isGameActive = true;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        ResetTimer();
        gameOverPanel.SetActive(false);
    }

    private void Update()
    {
        if (!isGameActive) return;

        currentTime -= Time.deltaTime;
        UpdateTimeUI();

        if (currentTime <= 0)
        {
            currentTime = 0;
            GameOver();
        }
    }

    public void AddExtraTime()
    {
        currentTime += extraTimePerPanel;
        UpdateTimeUI();
    }

    private void ResetTimer()
    {
        currentTime = initialTime;
        UpdateTimeUI();
    }

    private void UpdateTimeUI()
    {
        timeSlider.maxValue = initialTime;
        timeSlider.value = currentTime;

        if (timeText != null)
            timeText.text = $"Time: {Mathf.CeilToInt(currentTime)}s";
    }

    private void GameOver()
    {
        isGameActive = false;
        gameOverPanel.SetActive(true);
        gameOverText.text = "Game Over! You Lost!";
    }

    public bool IsGameActive() => isGameActive;
}