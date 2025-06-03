using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CompositionGameController : MonoBehaviour
{
    public static CompositionGameController Instance;
    [Header("UI References")]
   
    public Button submitButton; // Add this line

    [Header("Slot References")]
    public DigitSlot leftDigitSlot;  // Assign the left DigitSlot object
    public DigitSlot rightDigitSlot; // Assign the right DigitSlot object

    [Header("References")]
    public TMP_Text resultText;     // The target result display

    private int targetNumber;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Fallback to find result text if not assigned
        if (resultText == null)
        {
            resultText = GameObject.Find("result")?.GetComponent<TMP_Text>();
            if (resultText == null) Debug.LogError("Couldn't find result text object!");
        }
    }
 

    private void Start()
    {
        // Add this to connect the button click event
        if (submitButton != null)
        {
            submitButton.onClick.AddListener(CheckSolution);
        }
        else
        {
            Debug.LogError("Submit button not assigned!");
        }

        GenerateNewProblem();
    }

    public void GenerateNewProblem()
    {
        int left = Random.Range(1, 10);
        int right = Random.Range(1, 10);
        targetNumber = left * right;

        resultText.text = targetNumber.ToString();

        // Clear the slots through their existing functionality
        if (leftDigitSlot != null) leftDigitSlot.slotText.text = "";
        if (rightDigitSlot != null) rightDigitSlot.slotText.text = "";
    }

    public void CheckSolution()
    {
        if (leftDigitSlot == null || rightDigitSlot == null || resultText == null)
        {
            Debug.LogError("Missing references!");
            return;
        }

        // Use the existing slot text values
        string leftText = leftDigitSlot.slotText.text;
        string rightText = rightDigitSlot.slotText.text;

        if (string.IsNullOrEmpty(leftText)) return;
        if (string.IsNullOrEmpty(rightText)) return;

        if (int.TryParse(leftText, out int leftDigit) && int.TryParse(rightText, out int rightDigit))
        {
            if (leftDigit * rightDigit == targetNumber)
            {
                Debug.Log("Correct!");
                GenerateNewProblem();
            }
            else
            {
                Debug.Log("Incorrect. Try again!");
            }
        }
    }
}