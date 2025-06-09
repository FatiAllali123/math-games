using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Random = UnityEngine.Random;
using ArabicSupport;
using Firebase.Database;
using Firebase.Extensions;

public class MultiplicationChoiceGame : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text questionFRText;
    public TMP_Text questionARText;
    public Button[] answerButtons;
    public TMP_Text feedbackText;
    public TMP_Text scoreText;

    [Header("Avatar Settings")]
    public Image avatarImage;
    public Sprite happyAvatar;
    public Sprite sadAvatar;
    public TMP_Text avatarCommentText; // Nouveau texte pour les commentaires
    public GameObject avatarContainer; // Parent de l'avatar et du texte
    public int streakThreshold = 3; // Seuil pour afficher l'avatar
    public float avatarDisplayTime = 2f;

    [Header("Avatar Comments")]
    public string positiveComment = "Bravo ! Continue comme ça !";
    public string negativeComment = "Courage, tu peux y arriver !";

    [Header("Game Settings")]
    public string jsonFileName = "questions.json";
    public int xpPerCorrectAnswer = 10;
    public float delayBetweenQuestions = 1.5f;
    public Color correctColor = Color.green;
    public Color wrongColor = Color.red;
    public float feedbackDisplayTime = 1f;

    [Header("Audio Settings")]
    public AudioClip correctAnswerSound;
    public AudioClip wrongAnswerSound;
    [Range(0, 1)] public float soundVolume = 0.8f;

    private List<QuestionData> questions = new List<QuestionData>();
    private int currentQuestionIndex = 0;
    private int score = 0;
    private int correctStreak = 0;
    private int wrongStreak = 0;
    private AudioSource audioSource;
    private Coroutine feedbackCoroutine;
    private DatabaseReference dbReference;
    private bool usingCustomQuestions = false;

    [System.Serializable]
    public class QuestionData
    {
        public QuestionText text;
        public QuestionOption[] options;
        public int suggestedGrade;
    }

    [System.Serializable]
    public class QuestionText
    {
        public string ar;
        public string fr;
    }

    [System.Serializable]
    public class QuestionOption
    {
        public string text;
        public bool correct;
    }

    [System.Serializable]
    private class QuestionList { public QuestionData[] questions; }

    [System.Serializable]
    public class FirebaseTest
    {
        public int grade;
        public MiniGameConfigs miniGameConfigs;
        public string testName;
    }

    [System.Serializable] public class MiniGameConfigs { public MiniGameConfig choose_answer; }
    [System.Serializable] public class MiniGameConfig { public GradeConfig gradeConfig; public GroupsConfig groupsConfig; }
    [System.Serializable] public class GradeConfig { public Config config; }
    [System.Serializable] public class GroupsConfig { public Dictionary<string, GroupConfig> groupConfigs; }
    [System.Serializable] public class GroupConfig { public Config config; public List<string> studentIds; }
    [System.Serializable]
    public class Config
    {
        public List<CustomQuestion> customQuestions;
        public string mode;
        public int maxNumberRange;
        public int numOptions;
        public List<string> operationsAllowed;
    }
    [System.Serializable]
    public class CustomQuestion
    {
        public int correctAnswerIndex;
        public List<string> options;
        public QuestionText text;
    }

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.volume = soundVolume;
        ConfigureArabicText();
    }

    void Start()
    {
        // Cache l'avatar au démarrage
        if (avatarContainer != null)
            avatarContainer.SetActive(false);

        FirebaseInitializer.Instance.InitializeFirebase(() =>
        {
            if (FirebaseInitializer.Instance.IsFirebaseInitialized)
            {
                dbReference = FirebaseInitializer.Instance.DbReference;
                StartCoroutine(InitializeGame());
            }
        });
    }

    private IEnumerator InitializeGame()
    {
        string studentUID = PlayerPrefs.GetString("StudentUID");
        if (string.IsNullOrEmpty(studentUID))
        {
            Debug.LogError("Student UID not found!");
            yield break;
        }

        yield return LoadTestsFromFirebase(studentUID);

        if (questions.Count > 0)
        {
            ShuffleQuestions();
            DisplayQuestion();
        }
        else
        {
            Debug.Log("No custom questions found, loading default questions");
            yield return LoadDefaultQuestions();

            if (questions.Count > 0)
            {
                ShuffleQuestions();
                DisplayQuestion();
            }
            else
            {
                Debug.LogError("No questions available!");
                feedbackText.text = "Error: No questions available";
            }
        }
    }

    private IEnumerator LoadTestsFromFirebase(string studentUID)
    {
        var task = dbReference.Child("tests").GetValueAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.IsFaulted) { Debug.LogError("Failed to load tests"); yield break; }
        if (!task.Result.Exists) { Debug.Log("No tests found"); yield break; }

        int studentGrade = int.Parse(PlayerPrefs.GetString("StudentGrade", "6"));

        foreach (var testSnapshot in task.Result.Children)
        {
            var testData = JsonUtility.FromJson<FirebaseTest>(testSnapshot.GetRawJsonValue());
            if (testData.grade != studentGrade) continue;
            if (testData.miniGameConfigs?.choose_answer == null) continue;

            var choose = testData.miniGameConfigs.choose_answer;
            if (choose.groupsConfig?.groupConfigs != null)
            {
                foreach (var group in choose.groupsConfig.groupConfigs)
                {
                    if (group.Value.studentIds.Contains(studentUID))
                    {
                        if (group.Value.config.mode == "custom" && group.Value.config.customQuestions != null)
                        {
                            ConvertCustomQuestions(group.Value.config.customQuestions);
                            usingCustomQuestions = true;
                            yield break;
                        }
                        else if (group.Value.config.mode == "default") yield break;
                    }
                }
            }

            if (choose.gradeConfig?.config != null)
            {
                if (choose.gradeConfig.config.mode == "custom" && choose.gradeConfig.config.customQuestions != null)
                {
                    ConvertCustomQuestions(choose.gradeConfig.config.customQuestions);
                    usingCustomQuestions = true;
                    yield break;
                }
                else if (choose.gradeConfig.config.mode == "default") yield break;
            }
        }
    }

    private void ConvertCustomQuestions(List<CustomQuestion> cQ)
    {
        questions.Clear();
        foreach (var customQ in cQ)
        {
            var question = new QuestionData()
            {
                text = customQ.text,
                options = new QuestionOption[customQ.options.Count],
                suggestedGrade = int.Parse(PlayerPrefs.GetString("StudentGrade", "6"))
            };
            for (int i = 0; i < customQ.options.Count; i++)
                question.options[i] = new QuestionOption()
                {
                    text = customQ.options[i],
                    correct = (i == customQ.correctAnswerIndex)
                };

            questions.Add(question);
        }
    }

    private IEnumerator LoadDefaultQuestions()
    {
        string path = Path.Combine(Application.streamingAssetsPath, jsonFileName);
        if (!File.Exists(path)) yield break;
        string json = File.ReadAllText(path);
        QuestionList loaded = JsonUtility.FromJson<QuestionList>(json);
        int grade = int.Parse(PlayerPrefs.GetString("StudentGrade", "6"));

        questions = loaded.questions
            .Where(q => q.options != null && q.options.Length > 0 && q.suggestedGrade == grade)
            .OrderBy(q => q.suggestedGrade).ToList();
    }

    private void DisplayQuestion()
    {
        ClearFeedback();
        if (currentQuestionIndex >= questions.Count) return;

        QuestionData current = questions[currentQuestionIndex];
        questionFRText.text = current.text.fr;
        questionARText.text = ArabicFixer.Fix(current.text.ar, true, true);

        int btns = Mathf.Min(current.options.Length, answerButtons.Length);
        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (i < btns)
            {
                var btn = answerButtons[i];
                btn.gameObject.SetActive(true);
                btn.GetComponentInChildren<TMP_Text>().text = current.options[i].text;
                btn.image.color = Color.white;
                btn.onClick.RemoveAllListeners();
                int index = i;
                btn.onClick.AddListener(() => OnAnswerSelected(index));
                btn.interactable = true;
            }
            else answerButtons[i].gameObject.SetActive(false);
        }
    }

    private void OnAnswerSelected(int idx)
    {
        bool isCorrect = questions[currentQuestionIndex].options[idx].correct;
        foreach (var btn in answerButtons.Where(b => b.gameObject.activeSelf))
            btn.interactable = false;

        if (isCorrect)
        {
            PlaySound(correctAnswerSound);
            answerButtons[idx].image.color = correctColor;
            score += xpPerCorrectAnswer;
            scoreText.text = score.ToString();
            ShowFeedback("Correct!", correctColor);
            correctStreak++;
            wrongStreak = 0;
        }
        else
        {
            PlaySound(wrongAnswerSound);
            answerButtons[idx].image.color = wrongColor;
            ShowFeedback("Incorrect!", wrongColor);
            HighlightCorrectAnswer();
            wrongStreak++;
            correctStreak = 0;
        }

        UpdateAvatar(isCorrect);
        StartCoroutine(NextQuestionAfterDelay());
    }

    private void UpdateAvatar(bool correct)
    {
        if (avatarContainer == null) return;

        bool showAvatar = (correctStreak >= streakThreshold) || (wrongStreak >= streakThreshold);
        avatarContainer.SetActive(showAvatar);

        if (showAvatar)
        {
            avatarImage.sprite = correct ? happyAvatar : sadAvatar;
            avatarCommentText.text = correct ? positiveComment : negativeComment;
        }
    }

    private void HighlightCorrectAnswer()
    {
        var opts = questions[currentQuestionIndex].options;
        for (int i = 0; i < opts.Length; i++)
        {
            if (opts[i].correct && i < answerButtons.Length)
            {
                answerButtons[i].image.color = correctColor;
                break;
            }
        }
    }

    private void ShowFeedback(string msg, Color col)
    {
        if (feedbackCoroutine != null) StopCoroutine(feedbackCoroutine);
        feedbackText.text = msg;
        feedbackText.color = col;
        feedbackCoroutine = StartCoroutine(HideFeedbackAfterDelay());
    }

    private IEnumerator HideFeedbackAfterDelay()
    {
        yield return new WaitForSeconds(feedbackDisplayTime);
        ClearFeedback();
    }

    private void ClearFeedback() => feedbackText.text = "";

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }

    private IEnumerator NextQuestionAfterDelay()
    {
        yield return new WaitForSeconds(delayBetweenQuestions);
        currentQuestionIndex = (currentQuestionIndex + 1) % questions.Count;
        DisplayQuestion();
    }

    private void ShuffleQuestions()
    {
        questions = questions.OrderBy(x => Random.value).ToList();
    }

    private void ConfigureArabicText()
    {
        if (questionARText != null)
        {
            questionARText.font = Resources.Load<TMP_FontAsset>("Fonts/ArabicFont") ?? TMP_Settings.defaultFontAsset;
            questionARText.horizontalAlignment = HorizontalAlignmentOptions.Right;
        }
    }
}