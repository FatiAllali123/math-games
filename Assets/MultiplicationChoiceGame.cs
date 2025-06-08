using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Random = UnityEngine.Random;
using ArabicSupport;

public class MultiplicationChoiceGame : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text questionFRText;
    public TMP_Text questionARText;
    public Button[] answerButtons; // Doit contenir 4 boutons
    public TMP_Text feedbackText;
    public TMP_Text scoreText;

    [Header("Configuration")]
    public string jsonFileName = "questions.json";
    public int xpPerCorrectAnswer = 10;
    public float delayBetweenQuestions = 1.5f;

    [Header("Audio")]
    public AudioClip correctAnswerSound;
    public AudioClip wrongAnswerSound;
    [Range(0, 1)] public float soundVolume = 0.8f;

    [Header("Colors")]
    public Color defaultButtonColor = Color.white;
    public Color correctAnswerColor = Color.green;
    public Color wrongAnswerColor = Color.red;

    private List<QuestionData> questions = new List<QuestionData>();
    private int currentQuestionIndex = 0;
    private int score = 0;
    private AudioSource audioSource;
    private Coroutine feedbackCoroutine;

    [System.Serializable]
    public class QuestionData
    {
        public QuestionText text;
        public QuestionOption[] options; // Doit contenir 4 options
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
    private class QuestionList
    {
        public QuestionData[] questions;
    }

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = soundVolume;

        ConfigureArabicText();
    }

    void Start()
    {
        StartCoroutine(InitializeGame());
    }

    private void ConfigureArabicText()
    {
        if (questionARText != null)
        {
            questionARText.font = GetArabicFont();
            questionARText.horizontalAlignment = HorizontalAlignmentOptions.Right;
        }
    }

    private TMP_FontAsset GetArabicFont()
    {
        TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts/ArabicFont");
        return font ? font : TMP_Settings.defaultFontAsset;
    }

    private IEnumerator InitializeGame()
    {
        yield return LoadQuestionsFromJSON();

        if (questions.Count > 0)
        {
            ShuffleQuestions();
            DisplayQuestion();
        }
        else
        {
            Debug.LogError("Aucune question valide chargée !");
            feedbackText.text = "Erreur : Aucune question trouvée";
        }
    }

    private IEnumerator LoadQuestionsFromJSON()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, jsonFileName);

        if (!File.Exists(filePath))
        {
            Debug.LogError("Fichier JSON introuvable : " + filePath);
            yield break;
        }

        string jsonData = File.ReadAllText(filePath);
        QuestionList loadedData = JsonUtility.FromJson<QuestionList>(jsonData);

        if (loadedData?.questions == null || loadedData.questions.Length == 0)
        {
            Debug.LogError("Format JSON invalide ou aucune question trouvée");
            yield break;
        }

        // Filtrage des questions valides (avec exactement 4 options)
        questions = loadedData.questions
            .Where(q => q.options != null && q.options.Length == 4)
            .ToList();

        Debug.Log($"{questions.Count} questions valides chargées");
    }

    private void DisplayQuestion()
    {
        if (questions.Count == 0) return;

        QuestionData currentQuestion = questions[currentQuestionIndex];

        questionFRText.text = currentQuestion.text.fr;
        questionARText.text = ArabicFixer.Fix(currentQuestion.text.ar, true, true);

        for (int i = 0; i < 4; i++) // Toujours 4 boutons
        {
            answerButtons[i].gameObject.SetActive(true);
            answerButtons[i].GetComponentInChildren<TMP_Text>().text = currentQuestion.options[i].text;
            answerButtons[i].image.color = defaultButtonColor;
            answerButtons[i].interactable = true;

            int index = i;
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => OnAnswerSelected(index));
        }
    }

    private void OnAnswerSelected(int selectedIndex)
    {
        QuestionData currentQuestion = questions[currentQuestionIndex];
        bool isCorrect = currentQuestion.options[selectedIndex].correct;

        // Désactive tous les boutons après sélection
        foreach (var button in answerButtons)
            button.interactable = false;

        if (isCorrect)
        {
            HandleCorrectAnswer(selectedIndex);
        }
        else
        {
            HandleWrongAnswer(currentQuestion, selectedIndex);
        }

        StartCoroutine(NextQuestionAfterDelay());
    }

    private void HandleCorrectAnswer(int selectedIndex)
    {
        PlaySound(correctAnswerSound);
        answerButtons[selectedIndex].image.color = correctAnswerColor;
        score += xpPerCorrectAnswer;
        scoreText.text = score.ToString();
        ShowFeedback("Correct !", correctAnswerColor);
    }

    private void HandleWrongAnswer(QuestionData question, int selectedIndex)
    {
        PlaySound(wrongAnswerSound);
        answerButtons[selectedIndex].image.color = wrongAnswerColor;
        ShowFeedback("Incorrect !", wrongAnswerColor);

        // Affiche la bonne réponse
        for (int i = 0; i < 4; i++)
        {
            if (question.options[i].correct)
            {
                answerButtons[i].image.color = correctAnswerColor;
                break;
            }
        }
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

    private void PlaySound(AudioClip clip)
    {
        if (clip && audioSource)
            audioSource.PlayOneShot(clip);
    }

    private void ShowFeedback(string message, Color color)
    {
        if (feedbackCoroutine != null)
            StopCoroutine(feedbackCoroutine);

        feedbackText.text = message;
        feedbackText.color = color;
        feedbackCoroutine = StartCoroutine(ClearFeedback());
    }

    private IEnumerator ClearFeedback()
    {
        yield return new WaitForSeconds(1.5f);
        feedbackText.text = "";
    }

    // Pour le débogage dans l'éditeur
#if UNITY_EDITOR
    [UnityEditor.MenuItem("Debug/Recharger les questions")]
    private static void DebugReloadQuestions()
    {
        var game = FindObjectOfType<MultiplicationChoiceGame>();
        if (game) game.StartCoroutine(game.LoadQuestionsFromJSON());
    }
#endif
}