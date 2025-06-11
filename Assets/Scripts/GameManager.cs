using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int MaxNumberRange { get; private set; }
    public int NumOperations { get; private set; }
    public float RequiredCorrectAnswersMinimumPercent { get; private set; }
    public string StudentId { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("GameManager initialisé.");
    }

    public void SetTestParameters(int maxNumberRange, int numOperations, float requiredCorrectAnswersMinimumPercent, string studentId)
    {
        MaxNumberRange = maxNumberRange;
        NumOperations = numOperations;
        RequiredCorrectAnswersMinimumPercent = requiredCorrectAnswersMinimumPercent;
        StudentId = studentId;

        Debug.Log($"Paramètres définis dans GameManager : MaxNumberRange={MaxNumberRange}, NumOperations={NumOperations}, RequiredCorrectAnswersMinimumPercent={RequiredCorrectAnswersMinimumPercent}, StudentId={StudentId}");
    }
}