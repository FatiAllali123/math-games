using System.Collections.Generic;
using UnityEngine;

public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }

    public string CurrentTestId { get; set; }
    public string StudentId { get; set; } // Ajout� pour Firebase
    public string StudentGroup { get; set; }
    public List<string> MiniGameOrder { get; set; }
    public Dictionary<string, Dictionary<string, object>> MiniGameConfigs { get; private set; }
    public int TotalPossibleScore { get; set; }
    public int CurrentScore { get; set; }
    public int Coins { get; set; } // Nouvelle propri�t� pour les pi�ces
    public List<string> CompletedMiniGames { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            MiniGameConfigs = new Dictionary<string, Dictionary<string, object>>();
            MiniGameOrder = new List<string>();
            CompletedMiniGames = new List<string>();
            CurrentScore = 0;
            TotalPossibleScore = 0;
            Coins = 0; // Initialisation des pi�ces
            Debug.Log("GameSession initialis�e.");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ClearSessionData()
    {
        Debug.Log($"ClearSessionData appel�. StudentId avant r�initialisation : {StudentId}");
        CurrentTestId = null;
        StudentId = null;
        StudentGroup = null;
        MiniGameOrder.Clear();
        MiniGameConfigs.Clear();
        CompletedMiniGames.Clear();
        CurrentScore = 0;
        TotalPossibleScore = 0;
        Coins = 0; // R�initialisation des pi�ces
    }
}

