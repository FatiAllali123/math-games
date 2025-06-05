using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CompositionGameController : MonoBehaviour
{
    [Header("Slot References")]
    public DigitSlot leftDigitSlot;
    public DigitSlot rightDigitSlot;

    [Header("UI References")]
    public Button submitButton;
    public TMP_Text feedbackText;
    public TMP_Text resultText;
    [Header("Avatar Settings")]
    public GameObject avatarObject;  // Drag your avatar GameObject here
    public TMP_Text avatarSpeechText; // Text for avatar's speech bubble
    public string[] correctMessages = {
        "Correct! Well done!",
        "Great job! Want to try another?",
        "Perfect! More coins await!",
        "You're good at this!"
    };

    private static int sharedTargetNumber;
    private static bool isInitialized = false;

    private int targetNumber;
    private float feedbackTimer;
    private bool showingFeedback;

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
        // Find references relative to this panel's transform
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
        // Only generate new numbers if this is the first panel
        if (!isInitialized)
        {
            int left = Random.Range(1, 10);
            int right = Random.Range(1, 10);
            sharedTargetNumber = left * right;
            isInitialized = true;
        }

        targetNumber = sharedTargetNumber;
        resultText.text = targetNumber.ToString();

        // Clear previous digits
        if (leftDigitSlot != null && leftDigitSlot.slotText != null)
            leftDigitSlot.slotText.text = "";

        if (rightDigitSlot != null && rightDigitSlot.slotText != null)
            rightDigitSlot.slotText.text = "";
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
            ShowFeedback("Please enter valid numbers!", 1.5f);
            return;
        }

        int product = leftDigit * rightDigit;

        if (product == targetNumber)
        {
            ShowFeedback("Correct! Well done!", 1.5f);
        }
        else
        {
            ShowFeedback($"Incorrect. Try again! (Your answer: {product})", 1.5f);
        }
    }
}