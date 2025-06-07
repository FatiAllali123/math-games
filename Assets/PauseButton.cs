using UnityEngine;
using UnityEngine.UI;

public class TruePauseController : MonoBehaviour
{
    [Header("UI Settings")]
    public Sprite pauseIcon;    
    public Sprite playIcon;   
    public Image buttonImage;   // Assign the button's Image component

    [Header("Pause Settings")]
    public bool freezePhysics = true;   // Should physics stop?
    public bool muteAudio = true;       // Should audio pause?

    public static bool IsGamePaused { get; private set; } // Other scripts can check this

    private void Start()
    {
        // Initialize button
        if (buttonImage == null) buttonImage = GetComponent<Image>();
        buttonImage.sprite = pauseIcon;

        // Set up click listener
        GetComponent<Button>().onClick.AddListener(ToggleTruePause);

        // Ensure game starts unpaused
        IsGamePaused = false;
        ApplyPauseState(false); // Force reset all systems
    }

    public void ToggleTruePause()
    {
        // Only allow pause/unpause if the game state matches
        if ((IsGamePaused && Time.timeScale == 0) || (!IsGamePaused && Time.timeScale == 1))
        {
            IsGamePaused = !IsGamePaused;
            ApplyPauseState(IsGamePaused);
        }
        else
        {
            Debug.LogWarning("Pause state mismatch! Resetting...");
            ForceSyncPauseState();
        }
    }

    private void ApplyPauseState(bool paused)
    {
        // 1. Update timescale (core pause)
        Time.timeScale = paused ? 0 : 1;

        // 2. Handle audio
        if (muteAudio) AudioListener.pause = paused;

        // 3. Handle physics
        if (freezePhysics) Physics.autoSimulation = !paused;

        // 4. Update button icon
        buttonImage.sprite = paused ? playIcon : pauseIcon;

        Debug.Log($"Game {(paused ? "PAUSED" : "UNPAUSED")}");
    }

    // Emergency reset if desync happens
    private void ForceSyncPauseState()
    {
        IsGamePaused = (Time.timeScale == 0);
        ApplyPauseState(IsGamePaused);
    }
}