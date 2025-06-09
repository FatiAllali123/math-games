using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class CompositionGameController : MonoBehaviour
{
    [Header("Slot References")]
    public DigitSlot leftDigitSlot;
    public DigitSlot rightDigitSlot;

    [Header("UI References")]
    public Button submitButton;
    public TMP_Text feedbackText;
    public TMP_Text resultText;
    public TMP_Text compositionsFoundText;
    public TMP_Text targetText;

    private static int sharedTargetNumber;
    private static bool isInitialized = false;
    private static HashSet<string> foundCompositions = new HashSet<string>();
    private static List<Vector2Int> allPossibleCompositions = new List<Vector2Int>();

    private int targetNumber;
    private float feedbackTimer;
    private bool showingFeedback;

    // Numbers with at least 3 unique factor pairs (order doesn't matter)
    private readonly int[] validTargets = {
        12,  // (1,12), (2,6), (3,4)
        18,  // (1,18), (2,9), (3,6)
        24,  // (1,24), (2,12), (3,8), (4,6)
        36,  // (1,36), (2,18), (3,12), (4,9), (6,6)
        48,  // (1,48), (2,24), (3,16), (4,12), (6,8)
        60,  // (1,60), (2,30), (3,20), (4,15), (5,12), (6,10)
        72   // (1,72), (2,36), (3,24), (4,18), (6,12), (8,9)
    };

    private void Awake()
    {
        FindReferences();
        SetupButton();
        InitializeProblem();
    }

    private void Update()
    {
        if (showingFeedback)
        {
            feedbackTimer -= Time.deltaTime;
            if (feedbackTimer <= 0)
            {
                feedbackText.text = "";
                showingFeedback = false;
            }
        }
    }

    private void FindReferences()
    {
        if (leftDigitSlot == null)
            leftDigitSlot = transform.Find("DigitSlotCanvasLeft/DigitSlotPanel/DigitSlot")?.GetComponent<DigitSlot>();

        if (rightDigitSlot == null)
            rightDigitSlot = transform.Find("DigitSlotCanvasRight/DigitSlotPanel/DigitSlot")?.GetComponent<DigitSlot>();

        if (resultText == null)
            resultText = transform.Find("ResultText")?.GetComponent<TMP_Text>();

        if (submitButton == null)
            submitButton = GetComponentInChildren<Button>(true);

        if (feedbackText == null)
            feedbackText = transform.Find("FeedbackText")?.GetComponent<TMP_Text>();

        if (compositionsFoundText == null)
            compositionsFoundText = transform.Find("CompositionsFoundText")?.GetComponent<TMP_Text>();

        if (targetText == null)
            targetText = transform.Find("TargetText")?.GetComponent<TMP_Text>();
    }

    private void SetupButton()
    {
        if (submitButton != null)
        {
            submitButton.onClick.RemoveAllListeners();
            submitButton.onClick.AddListener(CheckSolution);
        }
    }

    public void InitializeProblem()
    {
        if (!isInitialized)
        {
            GenerateValidTargetNumber();
            isInitialized = true;
        }

        targetNumber = sharedTargetNumber;
        resultText.text = targetNumber.ToString();
        targetText.text = $"{targetNumber}";
        UpdateCompositionsFoundText();

        if (leftDigitSlot != null && leftDigitSlot.slotText != null)
            leftDigitSlot.slotText.text = "";

        if (rightDigitSlot != null && rightDigitSlot.slotText != null)
            rightDigitSlot.slotText.text = "";
    }

    private void GenerateValidTargetNumber()
    {
        // Pick a random target from our valid numbers
        sharedTargetNumber = validTargets[Random.Range(0, validTargets.Length)];

        // Find all unique factor pairs (order doesn't matter)
        allPossibleCompositions = FindUniqueCompositions(sharedTargetNumber);
        foundCompositions.Clear();
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
                if (j >= 1 && j <= 9)
                {
                    // Create a normalized key (smaller number first)
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
            feedbackTimer = duration;
            showingFeedback = true;
        }
    }

    public void CheckSolution()
    {
        if (leftDigitSlot == null || leftDigitSlot.slotText == null ||
            rightDigitSlot == null || rightDigitSlot.slotText == null)
        {
            ShowFeedback("System Error: Missing digit slots!", 3f);
            return;
        }

        if (!int.TryParse(leftDigitSlot.slotText.text, out int leftDigit) ||
            !int.TryParse(rightDigitSlot.slotText.text, out int rightDigit))
        {
            ShowFeedback("Please enter numbers 1-9!", 1.5f);
            return;
        }

        if (leftDigit < 1 || leftDigit > 9 || rightDigit < 1 || rightDigit > 9)
        {
            ShowFeedback("Numbers must be between 1-9!", 1.5f);
            return;
        }

        int product = leftDigit * rightDigit;

        if (product == targetNumber)
        {
            // Create normalized key (smaller number first)
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
                ScoreManager.Instance.AddScore(10);

                // Check win condition (half rounded up)
                int requiredToWin = Mathf.CeilToInt(allPossibleCompositions.Count / 2f);
                if (foundCompositions.Count >= requiredToWin)
                {
                    ShowFeedback($"You win! Found enough unique pairs!", 3f);
                    // Add win celebration logic here
                }
            }
        }
        else
        {
            ShowFeedback($"Incorrect! {leftDigit}×{rightDigit}={product}, not {targetNumber}", 1.5f);
            ScoreManager.Instance.AddScore(-5);
        }
    }

    private void UpdateCompositionsFoundText()
    {
        if (compositionsFoundText != null)
        {
            compositionsFoundText.text = $"Unique pairs found: {foundCompositions.Count}/{allPossibleCompositions.Count}";
        }
    }
}