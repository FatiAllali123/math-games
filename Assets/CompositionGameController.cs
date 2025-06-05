using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CompositionGameController : MonoBehaviour
{
    public static CompositionGameController Instance;

    [Header("Slot References")]
    public DigitSlot leftDigitSlot;
    public DigitSlot rightDigitSlot;

    [Header("UI References")]
    public Button submitButton;
    public TMP_Text feedbackText;
    [Header("References")]
    public TMP_Text resultText;

    private int targetNumber;
    private float feedbackTimer;
    private bool showingFeedback;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (resultText == null)
            resultText = GameObject.Find("result")?.GetComponent<TMP_Text>();
    }

    public void Initialize()
    {
        Transform panelRoot = transform;

        leftDigitSlot = panelRoot.Find("DigitSlotCanvasLeft/DigitSlotPanel/DigitSlot")?.GetComponent<DigitSlot>();
        rightDigitSlot = panelRoot.Find("DigitSlotCanvasRight/DigitSlotPanel/DigitSlot")?.GetComponent<DigitSlot>();
        resultText = panelRoot.Find("result")?.GetComponent<TMP_Text>();
        submitButton = panelRoot.GetComponentInChildren<Button>(true);

        if (leftDigitSlot != null)
        {
            leftDigitSlot.GetComponent<CanvasGroup>().blocksRaycasts = true;
        }
        if (rightDigitSlot != null)
        {
            rightDigitSlot.GetComponent<CanvasGroup>().blocksRaycasts = true;
        }

        if (submitButton != null)
        {
            submitButton.onClick.RemoveAllListeners();
            submitButton.onClick.AddListener(CheckSolution);
        }

        GenerateNewProblem();
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

    public void GenerateNewProblem()
    {
        int left = Random.Range(1, 10);
        int right = Random.Range(1, 10);
        targetNumber = left * right;

        resultText.text = targetNumber.ToString();

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
        if (leftDigitSlot == null || leftDigitSlot.slotText == null || rightDigitSlot == null || rightDigitSlot.slotText == null || resultText == null)
        {
            ShowFeedback("System Error!", 3f);
            return;
        }

        if (int.TryParse(leftDigitSlot.slotText.text, out int leftDigit) && int.TryParse(rightDigitSlot.slotText.text, out int rightDigit))
        {
            if (leftDigit * rightDigit == targetNumber)
            {
                ShowFeedback("Correct!", 1.5f);
            }
            else
            {
                ShowFeedback("Incorrect!", 1.5f);
            }
        }
        else
        {
            ShowFeedback("Invalid numbers!", 1f);
        }
    }
}