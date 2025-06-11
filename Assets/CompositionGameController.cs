using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Threading.Tasks;
using Firebase.Database;

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

    private static int sharedTargetNumber;
    private static bool isTargetInitialized = false;
    private static HashSet<string> foundCompositions = new HashSet<string>();
    private static List<Vector2Int> allPossibleCompositions = new List<Vector2Int>();
    private static int nextClickCount = 0;
    private static int totalTargets = 0;
    private static int correctTargets = 0;

    private int targetNumber;
    private int[] validTargets;

    private int maxNumberRange = 2; // Default
    private int minNumCompositions = 3; // Default
    private float requiredCorrectAnswersMinimumPercent = 50f; // Default

    private bool targetCountedAsCorrect = false;

    private DatabaseReference databaseReference;
    private string playerUid;
    private bool isFirebaseInitialized;

    private Canvas panelCanvas; // Reference to the panel's Canvas

    private void Awake()
    {
        FindReferences();
        SetupButtons();
        // Find the Canvas containing the panel
        panelCanvas = GetComponentInParent<Canvas>();
        if (panelCanvas == null)
        {
            panelCanvas = FindObjectOfType<Canvas>();
            if (panelCanvas == null) Debug.LogError("No Canvas found in scene! UI may not render correctly.");
            else Debug.Log($"Found panelCanvas: {panelCanvas.name}");
        }
        else Debug.Log($"Found panelCanvas via GetComponentInParent: {panelCanvas.name}");
    }

    private void Start()
    {
        StartCoroutine(InitializeWithFirebase());
    }

    private IEnumerator InitializeWithFirebase()
    {
        // Wait for Firebase dependencies
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

        // Wait for UserSession to authenticate user
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

        // Proceed with initialization
        yield return StartCoroutine(InitializeWithFirebaseConfig());
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
            Debug.LogWarning($"Firebase config timeout for {miniGameName}, using defaults: maxNumberRange={maxNumberRange}, minNumCompositions={minNumCompositions}, requiredCorrectAnswersMinimumPercent={requiredCorrectAnswersMinimumPercent}");
        }

        LoadFirebaseConfig();
        InitializeProgressBar();
        InitializeProblem();
        Debug.Log("CompositionGameController initialized.");
    }

    private void FindReferences()
    {
        Debug.Log($"FindReferences: Starting reference checks for {gameObject.name}");

        // Left Digit Slot
        if (leftDigitSlot == null)
        {
            leftDigitSlot = transform.Find("DigitSlotCanvasLeft/DigitSlotPanel/DigitSlot")?.GetComponent<DigitSlot>();
            if (leftDigitSlot == null)
            {
                leftDigitSlot = FindObjectOfType<DigitSlot>();
                if (leftDigitSlot == null) Debug.LogError("leftDigitSlot not found in scene!");
                else Debug.Log($"Found leftDigitSlot via FindObjectOfType: {leftDigitSlot.gameObject.name}");
            }
            else Debug.Log($"Found leftDigitSlot via transform.Find: {leftDigitSlot.gameObject.name}");
        }
        else Debug.Log($"leftDigitSlot assigned via Inspector: {leftDigitSlot.gameObject.name}");

        // Right Digit Slot
        if (rightDigitSlot == null)
        {
            rightDigitSlot = transform.Find("DigitSlotCanvasRight/DigitSlotPanel/DigitSlot")?.GetComponent<DigitSlot>();
            if (rightDigitSlot == null)
            {
                var slots = FindObjectsOfType<DigitSlot>();
                foreach (var slot in slots)
                {
                    if (slot != leftDigitSlot)
                    {
                        rightDigitSlot = slot;
                        break;
                    }
                }
                if (rightDigitSlot == null) Debug.LogError("rightDigitSlot not found in scene!");
                else Debug.Log($"Found rightDigitSlot via FindObjectOfType: {rightDigitSlot.gameObject.name}");
            }
            else Debug.Log($"Found rightDigitSlot via transform.Find: {rightDigitSlot.gameObject.name}");
        }
        else Debug.Log($"rightDigitSlot assigned via Inspector: {rightDigitSlot.gameObject.name}");

        // Result Text
        if (resultText == null)
        {
            resultText = transform.Find("ResultText")?.GetComponent<TMP_Text>();
            if (resultText == null)
            {
                resultText = FindObjectOfType<TMP_Text>();
                if (resultText == null) Debug.LogError("resultText not found in scene!");
                else Debug.Log($"Found resultText via FindObjectOfType: {resultText.gameObject.name}");
            }
            else Debug.Log($"Found resultText via transform.Find: {resultText.gameObject.name}");
        }
        else Debug.Log($"resultText assigned via Inspector: {resultText.gameObject.name}");

        // Submit Button
        if (submitButton == null)
        {
            submitButton = GetComponentInChildren<Button>(true);
            if (submitButton == null)
            {
                var buttons = FindObjectsOfType<Button>(true);
                foreach (var button in buttons)
                {
                    if (button.gameObject.name.ToLower().Contains("submit"))
                    {
                        submitButton = button;
                        break;
                    }
                }
                if (submitButton == null) Debug.LogError("submitButton not found in scene!");
                else Debug.Log($"Found submitButton via FindObjectOfType: {submitButton.gameObject.name}");
            }
            else Debug.Log($"Found submitButton via GetComponentInChildren: {submitButton.gameObject.name}");
        }
        else Debug.Log($"submitButton assigned via Inspector: {submitButton.gameObject.name}");

        // Feedback Text
        if (feedbackText == null)
        {
            feedbackText = transform.Find("FeedbackText")?.GetComponent<TMP_Text>();
            if (feedbackText == null)
            {
                var texts = FindObjectsOfType<TMP_Text>();
                foreach (var text in texts)
                {
                    if (text != resultText && text.gameObject.name.ToLower().Contains("feedback"))
                    {
                        feedbackText = text;
                        break;
                    }
                }
                if (feedbackText == null) Debug.LogError("feedbackText not found in scene!");
                else Debug.Log($"Found feedbackText via FindObjectOfType: {feedbackText.gameObject.name}");
            }
            else Debug.Log($"Found feedbackText via transform.Find: {feedbackText.gameObject.name}");
        }
        else Debug.Log($"feedbackText assigned via Inspector: {feedbackText.gameObject.name}");

        // Compositions Found Text
        if (compositionsFoundText == null)
        {
            compositionsFoundText = transform.Find("CompositionsFoundText")?.GetComponent<TMP_Text>();
            if (compositionsFoundText == null)
            {
                var texts = FindObjectsOfType<TMP_Text>();
                foreach (var text in texts)
                {
                    if (text != resultText && text != feedbackText && text.gameObject.name.ToLower().Contains("composition"))
                    {
                        compositionsFoundText = text;
                        break;
                    }
                }
                if (compositionsFoundText == null) Debug.LogError("compositionsFoundText not found in scene!");
                else Debug.Log($"Found compositionsFoundText via FindObjectOfType: {compositionsFoundText.gameObject.name}");
            }
            else Debug.Log($"Found compositionsFoundText via transform.Find: {compositionsFoundText.gameObject.name}");
        }
        else Debug.Log($"compositionsFoundText assigned via Inspector: {compositionsFoundText.gameObject.name}");

        // Next Button
        if (nextButton == null)
        {
            nextButton = transform.Find("NextButton")?.GetComponent<Button>();
            if (nextButton == null)
            {
                var buttons = FindObjectsOfType<Button>(true);
                foreach (var button in buttons)
                {
                    if (button != submitButton && button.gameObject.name.ToLower().Contains("next"))
                    {
                        nextButton = button;
                        break;
                    }
                }
                if (nextButton == null) Debug.LogError("nextButton not found in scene! Ensure a Button named 'NextButton' or similar exists.");
                else Debug.Log($"Found nextButton via FindObjectOfType: {nextButton.gameObject.name}");
            }
            else Debug.Log($"Found nextButton via transform.Find: {nextButton.gameObject.name}");
        }
        else Debug.Log($"nextButton assigned via Inspector: {nextButton.gameObject.name}");

        // Progress Bar
        if (progressBar == null)
        {
            progressBar = transform.Find("ProgressSlider")?.GetComponent<Slider>();
            if (progressBar == null)
            {
                progressBar = FindObjectOfType<Slider>(true);
                if (progressBar == null) Debug.LogError("progressBar not found in scene! Ensure a Slider named 'ProgressSlider' or similar exists.");
                else Debug.Log($"Found progressBar via FindObjectOfType: {progressBar.gameObject.name}");
            }
            else Debug.Log($"Found progressBar via transform.Find: {progressBar.gameObject.name}");
        }
        else Debug.Log($"progressBar assigned via Inspector: {progressBar.gameObject.name}");

        // Star Handle
        if (progressBar != null && starHandle == null)
        {
            starHandle = progressBar.handleRect;
            if (starHandle == null) Debug.LogError("starHandle not found! Ensure ProgressSlider has a valid Handle Rect assigned.");
            else Debug.Log($"Found starHandle: {starHandle.gameObject.name}");
        }
        else if (progressBar == null)
        {
            Debug.LogError("Cannot find starHandle because progressBar is null!");
        }
        else Debug.Log($"starHandle assigned via Inspector: {starHandle.gameObject.name}");
    }

    private void SetupButtons()
    {
        if (submitButton != null)
        {
            submitButton.onClick.RemoveAllListeners();
            submitButton.onClick.AddListener(CheckSolution);
            Debug.Log("submitButton click listener set up.");
        }
        else Debug.LogError("submitButton is null, cannot set up click listener!");

        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(OnNextButtonClicked);
            Debug.Log("nextButton click listener set up.");
        }
        else Debug.LogError("nextButton is null, cannot set up click listener!");
    }

    private void LoadFirebaseConfig()
    {
        string miniGameName = "find_compositions";
        if (TestConfiguration.MiniGameConfigs.ContainsKey(miniGameName))
        {
            var config = TestConfiguration.MiniGameConfigs[miniGameName];
            if (config.ContainsKey("maxNumberRange") && int.TryParse(config["maxNumberRange"].ToString(), out int range))
            {
                maxNumberRange = range;
            }
            if (config.ContainsKey("minNumCompositions") && int.TryParse(config["minNumCompositions"].ToString(), out int minComps))
            {
                minNumCompositions = minComps;
            }
            if (config.ContainsKey("requiredCorrectAnswersMinimumPercent") && float.TryParse(config["requiredCorrectAnswersMinimumPercent"].ToString(), out float percent))
            {
                requiredCorrectAnswersMinimumPercent = percent;
            }
            Debug.Log($"Loaded config for {miniGameName}: maxNumberRange={maxNumberRange}, minNumCompositions={minNumCompositions}, requiredCorrectAnswersMinimumPercent={requiredCorrectAnswersMinimumPercent}");
        }
        else
        {
            Debug.LogWarning($"No config found for {miniGameName}, using default values: maxNumberRange={maxNumberRange}, minNumCompositions={minNumCompositions}, requiredCorrectAnswersMinimumPercent={requiredCorrectAnswersMinimumPercent}");
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
        // Always generate a new target number to ensure refresh
        GenerateValidTargetNumber();
        isTargetInitialized = true;

        targetNumber = sharedTargetNumber;
        if (resultText != null)
        {
            resultText.text = targetNumber.ToString();
            Debug.Log($"Updated resultText to target number: {targetNumber}, actual text: {resultText.text}");
        }
        else
        {
            Debug.LogWarning("Cannot update resultText: resultText is null!");
        }

        // Clear and refresh slots
        ClearSlot(leftDigitSlot, "leftDigitSlot");
        ClearSlot(rightDigitSlot, "rightDigitSlot");

        if (feedbackText != null)
        {
            feedbackText.text = "";
            Debug.Log("Cleared feedbackText.");
        }
        else
        {
            Debug.LogWarning("Cannot clear feedbackText: feedbackText is null!");
        }

        UpdateCompositionsFoundText();
        UpdateProgressBar();
        RefreshPanel();
    }

    private void ClearSlot(DigitSlot slot, string slotName)
    {
        if (slot != null && slot.slotText != null)
        {
            // Handle TMP_InputField if slotText is an InputField
            TMP_InputField inputField = slot.slotText.GetComponent<TMP_InputField>();
            if (inputField != null)
            {
                inputField.text = "";
                inputField.DeactivateInputField();
                inputField.ActivateInputField(); // Refresh input field
                Debug.Log($"Cleared {slotName} (TMP_InputField) text: {inputField.text}");
            }
            else
            {
                slot.slotText.text = "";
                Debug.Log($"Cleared {slotName} (TMP_Text) text: {slot.slotText.text}");
            }
        }
        else
        {
            Debug.LogWarning($"Cannot clear {slotName}: {slotName} or slotText is null!");
        }
    }

    private void RefreshPanel()
    {
        if (panelCanvas != null)
        {
            // Ensure panel is active
            if (!panelCanvas.enabled)
            {
                panelCanvas.enabled = true;
                Debug.Log($"Re-enabled panelCanvas: {panelCanvas.name}");
            }
            // Force UI update
            Canvas.ForceUpdateCanvases();
            Debug.Log($"Forced Canvas update for panel: {panelCanvas.name}");
        }
        else
        {
            Debug.LogWarning("Cannot refresh panel: panelCanvas is null!");
        }
    }

    private void GenerateValidTargetNumber()
    {
        sharedTargetNumber = validTargets[Random.Range(0, validTargets.Length)];
        allPossibleCompositions = FindUniqueCompositions(sharedTargetNumber);
        foundCompositions.Clear();
        targetCountedAsCorrect = false;
        Debug.Log($"Generated new target number: {sharedTargetNumber}");
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

            // Clear UI elements before initializing new problem
            ClearSlot(leftDigitSlot, "leftDigitSlot");
            ClearSlot(rightDigitSlot, "rightDigitSlot");

            if (feedbackText != null)
            {
                feedbackText.text = "";
                Debug.Log("Cleared feedbackText in OnNextButtonClicked.");
            }
            else
            {
                Debug.LogWarning("Cannot clear feedbackText in OnNextButtonClicked: feedbackText is null!");
            }

            ClearExtraPanels();
            // Force a new target by resetting initialization flag
            isTargetInitialized = false;
            InitializeProblem();

            if (nextClickCount >= minNumCompositions && nextButton != null)
            {
                nextButton.interactable = false;
                CheckGameWinCondition();
                Debug.Log("Minimum compositions reached, nextButton disabled.");
            }
        }
    }

    private async void CheckGameWinCondition()
    {
        float progress = totalTargets > 0 ? (correctTargets / (float)totalTargets) * 100f : 0f;
        if (progress >= 75f)
        {
            ShowFeedback($"You win! Progress: {progress:F1}%", 5f);
            await UpdatePlayerScores(progress);
            StartCoroutine(DelayedSceneLoad("WinScene", 5f));
        }
        else
        {
            ShowFeedback($"Game Over! Progress: {progress:F1}% (Need 75%)", 5f);
            await UpdatePlayerScores(progress);
            StartCoroutine(DelayedSceneLoad("GameOver", 5f));
        }
    }

    private async Task UpdatePlayerScores(float currentProgress)
    {
        if (!isFirebaseInitialized || playerUid == null || databaseReference == null)
        {
            Debug.LogError("Cannot update scores: Firebase not initialized or playerUid/databaseReference is null!");
            return;
        }

        try
        {
            // Fetch current gameProgress
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

            // Determine new scores
            float newBestScore = Mathf.Max(currentProgress, bestScore);
            float newLastScore = currentProgress;

            // Prepare updates
            Dictionary<string, object> scoreUpdates = new Dictionary<string, object>
            {
                { "bestScore", newBestScore },
                { "lastScore", newLastScore }
            };

            // Update Firebase
            await databaseReference
                .Child("users")
                .Child(playerUid)
                .Child("gameProgress")
                .Child("find_compositions")
                .UpdateChildrenAsync(scoreUpdates);

            Debug.Log($"Updated scores for {playerUid}: bestScore={newBestScore}, lastScore={newLastScore}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to update scores for {playerUid}: {e.Message}");
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
        }
        else
        {
            Debug.LogWarning("Could not find CompositionPanelManager in scene!");
        }
    }

    public void ClearSlotsForNewComposition()
    {
        ClearSlot(leftDigitSlot, "leftDigitSlot");
        ClearSlot(rightDigitSlot, "rightDigitSlot");
        if (feedbackText != null)
        {
            feedbackText.text = "";
        }
    }

    public void CheckSolution()
    {
        if (leftDigitSlot == null || leftDigitSlot.slotText == null || rightDigitSlot == null || rightDigitSlot.slotText == null)
        {
            ShowFeedback("System Error: Missing digit slots!", 3f);
            return;
        }

        string leftText = leftDigitSlot.slotText.GetComponent<TMP_InputField>()?.text ?? leftDigitSlot.slotText.text;
        string rightText = rightDigitSlot.slotText.GetComponent<TMP_InputField>()?.text ?? rightDigitSlot.slotText.text;

        if (!int.TryParse(leftText, out int leftDigit) || !int.TryParse(rightText, out int rightDigit))
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
                if (ScoreManager.Instance != null)
                    ScoreManager.Instance.AddScore(10);

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
            if (ScoreManager.Instance != null)
                ScoreManager.Instance.AddScore(-5);
        }
    }

    private void UpdateProgressBar()
    {
        if (progressBar != null)
        {
            float progress = totalTargets > 0 ? (correctTargets / (float)totalTargets) * 100 : 0;
            progressBar.value = Mathf.Clamp(progress, 0f, 100f);
            Debug.Log($"ProgressBar updated: value={progressBar.value:F1}%, handlePos={(starHandle != null ? starHandle.anchoredPosition : Vector2.zero)}, correctTargets={correctTargets}, totalTargets={totalTargets}");
        }
        else
        {
            Debug.LogWarning("Cannot update ProgressBar: progressBar is null!");
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