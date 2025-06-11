using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;
using Firebase.Database;
using Firebase.Extensions;
using System.Threading.Tasks;
using System.Linq;

public class CompositionGameController : MonoBehaviour
{
    [Header("Slot References")]
    public DigitSlot leftDigitSlot;
    public DigitSlot rightDigitSlot;

    [Header("UI References")]
    public Button submitButton;
    public Button nextButton;
    public TMP_Text feedbackText;
    public TMP_Text resultText;
    public TMP_Text compositionsFoundText;
    public Slider progressBar;
    public RectTransform starHandle; // Slider handle (star)
    public TMP_Text scoreText; // Coin display

    private int targetNumber; // Non-static to avoid instance conflicts
    private int[] validTargets;

    private static int sharedTargetNumber; // Kept static for initialization logic
    private static bool isTargetInitialized = false;
    private static HashSet<string> foundCompositions = new HashSet<string>();
    private static HashSet<string> allPossibleCompositionKeys = new HashSet<string>();
    private static List<Vector2Int> allPossibleCompositions = new List<Vector2Int>(); // For logging
    private static int nextClickCount = 0;
    private static int totalTargets = 0;
    private static int correctTargets = 0;

    private int minNumCompositions = 3; // Prevent instant win
    private float requiredCorrectAnswersMinimumPercent = 75f; // Prevent instant win

    private bool targetCountedAsCorrect = false;

    private DatabaseReference databaseReference;
    private string playerUid;
    private bool isFirebaseInitialized;
    private int currentCoins = 0; // Player's coin balance
    private int earnedCoins = 0; // Coins earned in this session

    private void Awake()
    {
        FindReferences();
        SetupButtons();
        Debug.Log($"Awake completed for {gameObject.name}. nextButton={(nextButton != null ? nextButton.name : "null")}");
    }

    private void Start()
    {
        StartCoroutine(InitializeWithFirebase());
    }

    private IEnumerator InitializeWithFirebase()
    {
        var dependencyTask = Firebase.FirebaseApp.CheckAndFixDependenciesAsync();
        yield return new WaitUntil(() => dependencyTask.IsCompleted);

        if (dependencyTask.Result == Firebase.DependencyStatus.Available)
        {
            databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
            Debug.Log("Firebase dependencies resolved.");
        }
        else
        {
            Debug.LogError($"Firebase dependencies failed: {dependencyTask.Result}");
            yield break;
        }

        float timeout = 10f;
        float elapsed = 0f;
        while (UserSession.Instance == null || UserSession.Instance.CurrentUser == null && elapsed < timeout)
        {
            Debug.Log("Waiting for UserSession to authenticate user...");
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        if (UserSession.Instance == null || UserSession.Instance.CurrentUser == null)
        {
            Debug.LogError("No authenticated user found in UserSession after timeout!");
            yield break;
        }

        playerUid = UserSession.Instance.CurrentUser.uid;
        Debug.Log($"Firebase initialized for player: {playerUid}");
        isFirebaseInitialized = true;

        yield return StartCoroutine(FetchCoins());
        UpdateCoinDisplay();

        yield return StartCoroutine(InitializeWithFirebaseConfig());
    }

    private IEnumerator FetchCoins()
    {
        Debug.Log($"Fetching coins for player: {playerUid}");
        var coinTask = databaseReference
            .Child("users")
            .Child(playerUid)
            .Child("playerProfile")
            .Child("coins")
            .GetValueAsync();
        yield return new WaitUntil(() => coinTask.IsCompleted);

        if (coinTask.IsFaulted)
        {
            Debug.LogError($"Failed to fetch coins for {playerUid}: {coinTask.Exception?.InnerExceptions?.Aggregate("", (current, ex) => current + ex.Message + "\n")}");
            currentCoins = 0;
        }
        else if (coinTask.Result.Exists)
        {
            if (long.TryParse(coinTask.Result.Value?.ToString(), out long coins))
            {
                currentCoins = (int)coins;
                Debug.Log($"Fetched coins for {playerUid}: {currentCoins}");
            }
            else
            {
                Debug.LogWarning($"Invalid coin value for {playerUid}: '{coinTask.Result.Value}', setting to 0.");
                currentCoins = 0;
            }
        }
        else
        {
            Debug.Log($"No coins found for {playerUid}, initializing to 0.");
            currentCoins = 0;
        }
    }

    private void UpdateCoinDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Coins: {currentCoins + earnedCoins}";
            Debug.Log($"Updated scoreText: Coins: {currentCoins + earnedCoins}");
        }
        else
        {
            Debug.LogWarning("Cannot update scoreText: scoreText is null!");
        }
    }

    private async Task SaveCoinsToDatabase()
    {
        if (!isFirebaseInitialized || string.IsNullOrEmpty(playerUid) || databaseReference == null)
        {
            Debug.LogError("Cannot save coins: Firebase not initialized or playerUid/databaseReference is null!");
            return;
        }

        try
        {
            await databaseReference
                .Child("users")
                .Child(playerUid)
                .Child("playerProfile")
                .Child("coins")
                .SetValueAsync(currentCoins + earnedCoins);
            Debug.Log($"Saved coins for {playerUid}: {currentCoins + earnedCoins}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save coins for {playerUid}: {e.Message}\nStackTrace: {e.StackTrace}");
        }
    }

    private IEnumerator InitializeWithFirebaseConfig()
    {
        string miniGameName = "find_compositions";
        float timeout = 5f;
        float elapsed = 0f;
        while (!TestConfiguration.MiniGameConfigs.ContainsKey(miniGameName) && elapsed < timeout)
        {
            Debug.Log($"Waiting for Firebase config for {miniGameName}...");
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        if (!TestConfiguration.MiniGameConfigs.ContainsKey(miniGameName))
        {
            Debug.LogWarning($"Firebase config timeout for {miniGameName}, using defaults: minNumCompositions={minNumCompositions}, requiredCorrectAnswersMinimumPercent={requiredCorrectAnswersMinimumPercent}");
        }

        LoadFirebaseConfig();
        InitializeProgressBar();
        InitializeProblem();
        Debug.Log("CompositionGameController initialized.");
    }

    private void FindReferences()
    {
        Debug.Log($"FindReferences: Starting reference checks for {gameObject.name}");

        if (leftDigitSlot == null)
        {
            leftDigitSlot = transform.Find("DigitSlotLeft")?.GetComponent<DigitSlot>()
                         ?? transform.Find("DigitSlotCanvasLeft/DigitSlotPanel/DigitSlot")?.GetComponent<DigitSlot>()
                         ?? GameObject.FindWithTag("LeftDigitSlot")?.GetComponent<DigitSlot>();
            if (leftDigitSlot == null) Debug.LogError("leftDigitSlot not found in hierarchy or tags!");
            else Debug.Log($"Found leftDigitSlot: {leftDigitSlot.gameObject.name}");
        }

        if (rightDigitSlot == null)
        {
            rightDigitSlot = transform.Find("DigitSlotRight")?.GetComponent<DigitSlot>()
                          ?? transform.Find("DigitSlotCanvasRight/DigitSlotPanel/DigitSlot")?.GetComponent<DigitSlot>()
                          ?? GameObject.FindWithTag("RightDigitSlot")?.GetComponent<DigitSlot>();
            if (rightDigitSlot == null) Debug.LogError("rightDigitSlot not found in hierarchy or tags!");
            else Debug.Log($"Found rightDigitSlot: {rightDigitSlot.gameObject.name}");
        }

        if (resultText == null)
        {
            resultText = transform.Find("ResultText")?.GetComponent<TMP_Text>()
                      ?? GameObject.Find("ResultText")?.GetComponent<TMP_Text>();
            if (resultText == null) Debug.LogError("resultText not found in hierarchy!");
            else Debug.Log($"Found resultText: {resultText.gameObject.name}");
        }

        if (submitButton == null)
        {
            submitButton = transform.Find("SubmitButton")?.GetComponent<Button>()
                        ?? GameObject.Find("SubmitButton")?.GetComponent<Button>();
            if (submitButton == null) Debug.LogError("submitButton not found in hierarchy!");
            else Debug.Log($"Found submitButton: {submitButton.gameObject.name}");
        }

        if (feedbackText == null)
        {
            feedbackText = transform.Find("FeedbackText")?.GetComponent<TMP_Text>()
                        ?? GameObject.Find("FeedbackText")?.GetComponent<TMP_Text>();
            if (feedbackText == null) Debug.LogError("feedbackText not found in hierarchy!");
            else Debug.Log($"Found feedbackText: {feedbackText.gameObject.name}");
        }

        if (compositionsFoundText == null)
        {
            compositionsFoundText = transform.Find("CompositionsFoundText")?.GetComponent<TMP_Text>()
                                 ?? GameObject.Find("CompositionsFoundText")?.GetComponent<TMP_Text>();
            if (compositionsFoundText == null) Debug.LogError("compositionsFoundText not found in hierarchy!");
            else Debug.Log($"Found compositionsFoundText: {compositionsFoundText.gameObject.name}");
        }

        if (nextButton == null)
        {
            nextButton = transform.Find("NextButton")?.GetComponent<Button>()
                      ?? transform.Find("Next")?.GetComponent<Button>()
                      ?? transform.Find("nextButton")?.GetComponent<Button>()
                      ?? GameObject.Find("NextButton")?.GetComponent<Button>()
                      ?? GetComponentInChildren<Button>(true);
            if (nextButton == null) Debug.LogError("nextButton not found in hierarchy! Ensure a GameObject named 'NextButton' is a child of GameController or assigned in Inspector.");
            else Debug.Log($"Found nextButton: {nextButton.gameObject.name}");
        }

        if (progressBar == null)
        {
            progressBar = transform.Find("ProgressSlider")?.GetComponent<Slider>()
                       ?? GameObject.Find("ProgressSlider")?.GetComponent<Slider>();
            if (progressBar == null) Debug.LogError("progressBar not found in hierarchy!");
            else Debug.Log($"Found progressBar: {progressBar.gameObject.name}");
        }

        if (scoreText == null)
        {
            scoreText = transform.Find("ScoreText")?.GetComponent<TMP_Text>()
                     ?? GameObject.Find("ScoreText")?.GetComponent<TMP_Text>();
            if (scoreText == null) Debug.LogError("scoreText not found in hierarchy!");
            else Debug.Log($"Found scoreText: {scoreText.gameObject.name}");
        }

        if (progressBar != null && starHandle == null)
        {
            starHandle = progressBar.handleRect;
            if (starHandle == null) Debug.LogError("starHandle not found on progressBar!");
            else Debug.Log($"Found starHandle: {starHandle.gameObject.name}");
        }
    }

    private void SetupButtons()
    {
        if (submitButton != null)
        {
            submitButton.onClick.RemoveAllListeners();
            submitButton.onClick.AddListener(CheckSolution);
            Debug.Log("submitButton listener set.");
        }
        else
        {
            Debug.LogError("submitButton is null, cannot set up click listener!");
        }

        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(OnNextButtonClicked);
            Debug.Log("nextButton listener set.");
        }
        else
        {
            Debug.LogError("nextButton is null, cannot set up click listener! Game progression will be blocked.");
        }
    }

    private void LoadFirebaseConfig()
    {
        string miniGameName = "find_compositions";
        if (TestConfiguration.MiniGameConfigs.ContainsKey(miniGameName))
        {
            var config = TestConfiguration.MiniGameConfigs[miniGameName];
            if (config.ContainsKey("minNumCompositions") && int.TryParse(config["minNumCompositions"].ToString(), out int minComps))
            {
                minNumCompositions = Mathf.Max(1, minComps);
            }
            if (config.ContainsKey("requiredCorrectAnswersMinimumPercent") && float.TryParse(config["requiredCorrectAnswersMinimumPercent"].ToString(), out float percent))
            {
                requiredCorrectAnswersMinimumPercent = Mathf.Clamp(percent, 50f, 100f);
            }
            Debug.Log($"Loaded config for {miniGameName}: minNumCompositions={minNumCompositions}, requiredCorrectAnswersMinimumPercent={requiredCorrectAnswersMinimumPercent}");
        }
        else
        {
            Debug.LogWarning($"No config found for {miniGameName}, using defaults: minNumCompositions={minNumCompositions}, requiredCorrectAnswersMinimumPercent={requiredCorrectAnswersMinimumPercent}");
        }

        validTargets = GenerateValidTargets();
    }

    private void InitializeProgressBar()
    {
        if (progressBar != null)
        {
            progressBar.minValue = 0;
            progressBar.maxValue = 100;
            progressBar.value = 0;
            Debug.Log($"ProgressBar initialized: min={progressBar.minValue}, max={progressBar.maxValue}, value={progressBar.value}");
        }
        else
        {
            Debug.LogError("Cannot initialize ProgressBar: progressBar is null!");
        }
    }

    private int[] GenerateValidTargets()
    {
        HashSet<int> targets = new HashSet<int>();
        for (int i = 1; i <= 9; i++)
        {
            for (int j = 1; j <= 9; j++)
            {
                targets.Add(i * j);
            }
        }

        int[] validTargets = new int[targets.Count];
        targets.CopyTo(validTargets);
        System.Array.Sort(validTargets);

        if (validTargets.Length == 0)
        {
            Debug.LogWarning("No valid targets generated, using fallback targets.");
            return new int[] { 6, 12, 16, 20, 24, 36 };
        }

        return validTargets;
    }

    public void InitializeProblem()
    {
        if (!isTargetInitialized)
        {
            GenerateValidTargetNumber();
            isTargetInitialized = true;
        }

        targetNumber = sharedTargetNumber;
        if (resultText != null)
        {
            resultText.text = targetNumber.ToString();
            Debug.Log($"Updated resultText with target: {targetNumber}");
        }
        else
        {
            Debug.LogError("Cannot update resultText: resultText is null!");
        }

        foundCompositions.Clear();
        UpdateCompositionsFoundText();
        UpdateProgressBar();

        if (leftDigitSlot != null && leftDigitSlot.slotText != null)
            leftDigitSlot.slotText.text = "";
        else
            Debug.LogError("Cannot clear leftDigitSlot: slot or slotText is null!");

        if (rightDigitSlot != null && rightDigitSlot.slotText != null)
            rightDigitSlot.slotText.text = "";
        else
            Debug.LogError("Cannot clear rightDigitSlot: slot or slotText is null!");

        if (feedbackText != null)
            feedbackText.text = "";
        else
            Debug.LogError("Cannot clear feedbackText: feedbackText is null!");

        Debug.Log($"Initialized problem: targetNumber={targetNumber}, sharedTargetNumber={sharedTargetNumber}, allPossibleCompositionKeys=[{string.Join(", ", allPossibleCompositionKeys)}]");
    }

    private void GenerateValidTargetNumber()
    {
        sharedTargetNumber = validTargets[Random.Range(0, validTargets.Length)];
        allPossibleCompositions = FindUniqueCompositions(sharedTargetNumber);
        foundCompositions.Clear();
        targetCountedAsCorrect = false;
    }

    private List<Vector2Int> FindUniqueCompositions(int target)
    {
        var compositions = new List<Vector2Int>();
        var usedPairs = new HashSet<string>();

        for (int i = 1; i <= 9; i++)
        {
            if (target % i == 0)
            {
                int j = target / i;
                if (j <= 9)
                {
                    string pairKey = i <= j ? $"{i},{j}" : $"{j},{i}";
                    if (!usedPairs.Contains(pairKey))
                    {
                        usedPairs.Add(pairKey);
                        compositions.Add(new Vector2Int(i, j));
                    }
                }
            }
        }
        return compositions;
    }

    public void ShowFeedback(string message, float duration = 2f)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            Debug.Log($"Showing feedback: {message}");
        }
        else
        {
            Debug.LogWarning($"Cannot show feedback '{message}': feedbackText is null!");
        }
    }

    private void OnNextButtonClicked()
    {
        if (nextClickCount < minNumCompositions)
        {
            nextClickCount++;
            totalTargets++;
            Debug.Log($"Next button clicked: nextClickCount={nextClickCount}, totalTargets={totalTargets}");

            UpdateProgressBar();

            if (leftDigitSlot != null && leftDigitSlot.slotText != null)
                leftDigitSlot.slotText.text = "";
            else
                Debug.LogError("Cannot clear leftDigitSlot: slot or slotText is null!");

            if (rightDigitSlot != null && rightDigitSlot.slotText != null)
                rightDigitSlot.slotText.text = "";
            else
                Debug.LogError("Cannot clear rightDigitSlot: slot or slotText is null!");

            if (feedbackText != null)
                feedbackText.text = "";
            else
                Debug.LogError("Cannot clear feedbackText: feedbackText is null!");

            ClearExtraPanels();

            isTargetInitialized = false;
            InitializeProblem();

            if (nextClickCount >= minNumCompositions && nextButton != null)
            {
                nextButton.interactable = false;
                Debug.Log("Minimum compositions reached, disabling nextButton and checking win condition.");
                CheckGameWinCondition();
            }
        }
    }

    private async void CheckGameWinCondition()
    {
        float progress = totalTargets > 0 ? (correctTargets / (float)totalTargets) * 100f : 0f;
        Debug.Log($"Checking win condition: progress={progress:F1}%, correctTargets={correctTargets}, totalTargets={totalTargets}");

        if (progress >= requiredCorrectAnswersMinimumPercent)
        {
            ShowFeedback($"You win! Progress: {progress:F1}%", 5f);
            await UpdatePlayerScores(progress);
            StartCoroutine(DelayedSceneLoad("WinScene", 5f));
        }
        else
        {
            ShowFeedback($"Game Over! Progress: {progress:F1}% (Need {requiredCorrectAnswersMinimumPercent}%)", 5f);
            await UpdatePlayerScores(progress);
            StartCoroutine(DelayedSceneLoad("GameOver", 5f));
        }
    }

    private async Task UpdatePlayerScores(float currentProgress)
    {
        if (!isFirebaseInitialized || string.IsNullOrEmpty(playerUid) || databaseReference == null)
        {
            Debug.LogError("Cannot update scores: Firebase not initialized or playerUid/databaseReference is null!");
            return;
        }

        try
        {
            DataSnapshot snapshot = await databaseReference
                .Child("users")
                .Child(playerUid)
                .Child("gameProgress")
                .Child("find_compositions")
                .GetValueAsync();

            float bestScore = 0f;
            float lastScore = 0f;

            if (snapshot.Exists)
            {
                bestScore = float.TryParse(snapshot.Child("bestScore").Value?.ToString(), out float bs) ? bs : 0f;
                lastScore = float.TryParse(snapshot.Child("lastScore").Value?.ToString(), out float ls) ? ls : 0f;
                Debug.Log($"Fetched scores for {playerUid}: bestScore={bestScore}, lastScore={lastScore}");
            }
            else
            {
                Debug.Log($"No existing scores found for {playerUid}, initializing with zeros.");
            }

            float newBestScore = Mathf.Max(currentProgress, bestScore);
            float newLastScore = currentProgress;

            Dictionary<string, object> scoreUpdates = new Dictionary<string, object>
            {
                { "bestScore", newBestScore },
                { "lastScore", newLastScore }
            };

            await databaseReference
                .Child("users")
                .Child(playerUid)
                .Child("gameProgress")
                .Child("find_compositions")
                .UpdateChildrenAsync(scoreUpdates);

            Debug.Log($"Updated scores for {playerUid}: bestScore={newBestScore}, lastScore={newLastScore}");

            currentCoins += earnedCoins;
            await SaveCoinsToDatabase();
            earnedCoins = 0;
            UpdateCoinDisplay();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to update scores or coins for {playerUid}: {e.Message}\nStackTrace: {e.StackTrace}");
        }
    }

    private IEnumerator DelayedSceneLoad(string sceneName, float delay)
    {
        Debug.Log($"Attempting to load scene: {sceneName} in {delay} seconds");
        yield return new WaitForSeconds(delay);
        try
        {
            SceneManager.LoadScene(sceneName);
            Debug.Log($"Loaded scene: {sceneName}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load scene {sceneName}: {e.Message}");
        }
    }

    private void ClearExtraPanels()
    {
        CompositionPanelManager manager = FindObjectOfType<CompositionPanelManager>();
        if (manager != null)
        {
            manager.ClearPanels();
            Debug.Log("Cleared extra panels via CompositionPanelManager.");
        }
        else
        {
            Debug.LogWarning("CompositionPanelManager not found, attempting to clear panels manually.");
            var panels = FindObjectsOfType<Transform>().Where(t => t.CompareTag("CompositionPanel") || t.name.Contains("Panel")).ToList();
            foreach (var panel in panels)
            {
                if (panel != null && panel.gameObject != gameObject)
                {
                    panel.gameObject.SetActive(false);
                    Debug.Log($"Deactivated panel: {panel.gameObject.name}");
                }
            }
            if (panels.Count == 0)
            {
                Debug.LogWarning("No panels found to clear!");
            }
        }
    }

    public void ClearSlotsForNewComposition()
    {
        if (leftDigitSlot != null && leftDigitSlot.slotText != null)
            leftDigitSlot.slotText.text = "";
        else
            Debug.LogError("Cannot clear leftDigitSlot: slot or slotText is null!");

        if (rightDigitSlot != null && rightDigitSlot.slotText != null)
            rightDigitSlot.slotText.text = "";
        else
            Debug.LogError("Cannot clear rightDigitSlot: slot or slotText is null!");

        if (feedbackText != null)
            feedbackText.text = "";
        else
            Debug.LogError("Cannot clear feedbackText: feedbackText is null!");
    }

    private void IncrementQuestionsSolved()
    {
        if (!isFirebaseInitialized || string.IsNullOrEmpty(playerUid) || databaseReference == null)
        {
            Debug.LogError("Cannot increment questionsSolved: Firebase not initialized or playerUid/databaseReference is null!");
            return;
        }

        DatabaseReference questionsRef = databaseReference
            .Child("users")
            .Child(playerUid)
            .Child("playerProfile")
            .Child("questionsSolved");

        questionsRef.RunTransaction(mutableData =>
        {
            long currentValue = mutableData.Value != null && long.TryParse(mutableData.Value.ToString(), out long val) ? val : 0;
            mutableData.Value = currentValue + 1;
            return TransactionResult.Success(mutableData);
        }).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"Failed to increment questionsSolved for {playerUid}: {task.Exception?.InnerExceptions?.Aggregate("", (current, ex) => current + ex.Message + "\n")}");
            }
            else
            {
                Debug.Log($"Incremented questionsSolved for {playerUid}");
            }
        });
    }

    public void CheckSolution()
    {
        if (leftDigitSlot == null || leftDigitSlot.slotText == null || rightDigitSlot == null || rightDigitSlot.slotText == null)
        {
            ShowFeedback("System Error: Missing digit slots!", 3f);
            return;
        }

        if (!int.TryParse(leftDigitSlot.slotText.text, out int leftDigit) || !int.TryParse(rightDigitSlot.slotText.text, out int rightDigit))
        {
            ShowFeedback("Please enter valid numbers!", 1.5f);
            return;
        }

        if (leftDigit < 1 || rightDigit < 1)
        {
            ShowFeedback("Numbers must be positive!", 1.5f);
            return;
        }

        int product = leftDigit * rightDigit;

        if (product == targetNumber && leftDigit <= 9 && rightDigit <= 9)
        {
            int a = Mathf.Min(leftDigit, rightDigit);
            int b = Mathf.Max(leftDigit, rightDigit);
            string compositionKey = $"{a},{b}";

            if (foundCompositions.Contains(compositionKey))
            {
                ShowFeedback($"You already found {a}×{b}!", 1.5f);
            }
            else
            {
                foundCompositions.Add(compositionKey);
                UpdateCompositionsFoundText();
                ShowFeedback($"Correct! {leftDigit}×{rightDigit}={targetNumber}\nFound {foundCompositions.Count}/{allPossibleCompositions.Count} unique pairs", 2f);
                earnedCoins += 10; // Increment earned coins
                UpdateCoinDisplay();
                StartCoroutine(SaveCoinsToDatabaseCoroutine()); // Save coins to Firebase
                IncrementQuestionsSolved(); // Increment questions solved

                float halfPairs = Mathf.Ceil(allPossibleCompositions.Count / 2f);
                if (foundCompositions.Count >= halfPairs && !targetCountedAsCorrect)
                {
                    correctTargets++;
                    targetCountedAsCorrect = true;
                    UpdateProgressBar();
                }

                float requiredPairs = (requiredCorrectAnswersMinimumPercent / 100f) * allPossibleCompositions.Count;
                int requiredToWin = Mathf.CeilToInt(requiredPairs);
                if (foundCompositions.Count >= requiredToWin)
                {
                    ShowFeedback($"Target complete! Found enough pairs!", 3f);
                }
            }
        }
        else
        {
            ShowFeedback($"Incorrect! {leftDigit}×{rightDigit}={product}, not {targetNumber}", 1.5f);
            if (earnedCoins >= 5)
            {
                earnedCoins -= 5; // Deduct coins for incorrect answer
                UpdateCoinDisplay();
                StartCoroutine(SaveCoinsToDatabaseCoroutine()); // Save coins to Firebase
            }
        }
    }

    private IEnumerator SaveCoinsToDatabaseCoroutine()
    {
        yield return SaveCoinsToDatabase();
    }

    private void UpdateProgressBar()
    {
        if (progressBar != null)
        {
            float progress = totalTargets > 0 ? (correctTargets / (float)totalTargets) * 100f : 0f;
            progressBar.value = Mathf.Clamp(progress, progressBar.minValue, progressBar.maxValue);
            Debug.Log($"ProgressBar updated: value={progress:F1}%, handlePos={(starHandle != null ? starHandle.anchoredPosition : Vector2.zero)}, correctTargets={correctTargets}, totalTargets={totalTargets}");
        }
        else
        {
            Debug.LogError("Cannot update ProgressBar: progressBar is null!");
        }
    }

    private void UpdateCompositionsFoundText()
    {
        if (compositionsFoundText != null)
        {
            compositionsFoundText.text = $"Unique pairs found: {foundCompositions.Count}/{allPossibleCompositions.Count}";
        }
        else
        {
            Debug.LogWarning("Cannot update compositionsFoundText: compositionsFoundText is null!");
        }
    }
}