using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class TarlManager : MonoBehaviour
{
    public GradeLevel FinalGradeLevel;
    public PlayerManager PlayerStateRef;

    public Dictionary<GradeLevel, MiniGameList> MiniGames = new Dictionary<GradeLevel, MiniGameList>();

    public int CurrentMiniGameIndex = -1;
    public GameObject CurrentMiniGame;

    public static TarlManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); 
    }

    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        PlayerStateRef = PlayerManager.Instance;
        if (PlayerStateRef != null)
        {
            FinalGradeLevel = PlayerStateRef.playerProfile.schoolGrade;
            Debug.Log("PlayerStateRef successfully initialized.");
        }
        else
        {
            Debug.LogError("Failed to get PlayerState.");
        }

        InitializeMiniGames();
    }

    void InitializeMiniGames()
    {
        // Example: Initialize mini-games for each grade level
        MiniGameCustomization gradeOneCustomization = new MiniGameCustomization
        {
            numberOfQuestions = 4,
            numberRange = 2,
            numberOfOptions = 4,
            duration = 30.0f,
            timeLimit = 10.0f
        };

        MiniGameSuccessCriteria gradeOneSuccessCriteria = new MiniGameSuccessCriteria
        { 
            requiredCorrectAnswers = 3,
            completed = false 
        };

        // Grade 1 Mini Games
        MiniGameList gradeOneMiniGames = new MiniGameList
        {
            miniGamesWidgets = new List<MathoriaMiniGameWidget>
            {
                MiniGameFactory.Get().CreateMiniGame(MiniGameName.FindPreviousNextNumber, gradeOneCustomization, gradeOneSuccessCriteria),
                MiniGameFactory.Get().CreateMiniGame(MiniGameName.TapMatchingPairs, gradeOneCustomization, gradeOneSuccessCriteria)
                // Add more mini-games as needed
            }
        };

        MiniGames[GradeLevel.One] = gradeOneMiniGames;

        // Repeat the process for other grades...
    }

    public MiniGameCustomization GetDefaultCustomizationSettings(GradeLevel gradeLevel, MiniGameName miniGameName)
    {
        MiniGameCustomization customization = new MiniGameCustomization();

        switch (gradeLevel)
        {
            case GradeLevel.One:
                customization.numberRange = 10;
                customization.numberOfQuestions = 4;
                customization.numberOfOptions = 2;
                customization.timeLimit = -1;
                customization.duration = -1;
                // Add operations and other game settings for Grade 1
                break;
            case GradeLevel.Two:
                customization.numberRange = 100;
                customization.numberOfQuestions = 7;
                customization.numberOfOptions = 3;
                break;
                // Add other grade cases here...
        }

        switch (miniGameName)
        {
            case MiniGameName.FindPreviousNextNumber:
                customization.numberOfOptions = 0; // No options needed
                break;
            case MiniGameName.TapMatchingPairs:
                customization.numberOfOptions = 4; // Number of pairs
                break;
                // Add other mini-game specific customizations here...
        }

        return customization;
    }

    public void StartTest()
    {
        if (PlayerStateRef == null)
        {
            Debug.LogError("StartTest() failed: PlayerStateRef is null!");
            return;
        }

        // Test starting logic here, use FinalGradeLevel to decide mini-game progression
        // Example: StartMiniGame();
    }

    public bool CanStartTest()
    {
        return PlayerStateRef != null && PlayerStateRef.playerProfile != null;
    }

    public bool EvaluateMiniGameSuccess(GameObject miniGame)
    {
        // Logic to evaluate if the mini-game was successful
        // Based on the current mini-game and its logic
        return true; // Placeholder return
    }

    public void NotifyMiniGameCompleted()
    {
        // Logic to notify that the mini-game was completed, then proceed to the next
        StartNextMiniGame();
    }

    public void SetCurrentMiniGame(GameObject currentGame)
    {
        CurrentMiniGame = currentGame;
    }

    public void SetPlayerMathLevel()
    {
        // Logic to set the player's math level based on performance, for example
    }

    public GameObject StartMiniGame(GameObject miniGame)
    {
        // Logic to start a mini-game
        return miniGame; // Placeholder return
    }

    public GameObject StartNextMiniGame()
    {
        // Logic to start the next mini-game based on current index or progression
        return null; // Placeholder return
    }
}