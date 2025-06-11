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
using UnityEngine.Networking;
using System.Threading.Tasks;
using System;

public class MultiplicationChoiceGame : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text questionFRText;
    public TMP_Text questionARText;
    public Button[] answerButtons;
    public TMP_Text feedbackText;
    public TMP_Text scoreText;
    public Slider progressBar;

    [Header("Reward UI")]
    public GameObject rewardPanel;
    public Image coinIcon;
    public TMP_Text coinsText;

    [Header("Reward Settings")]
    public Sprite coinSprite;
    public AudioClip coinSound;
    public int coinsPerCorrectAnswer = 10;
    public int bonusCoinsThreshold = 80;
    public int bonusCoins = 5;
    public float animationDuration = 2f;

    [Header("Avatar Settings")]
    public Image avatarImage;
    public Sprite happyAvatar;
    public Sprite sadAvatar;
    public TMP_Text avatarCommentText;
    public GameObject avatarContainer;
    public int streakThreshold = 3;
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
    public AudioClip backgroundMusic;
    public Button soundToggleButton;
    public Sprite soundOnIcon;
    public Sprite soundOffIcon;
    public AudioSource backgroundAudioSource; // Référence à l'AudioSource existant

    private List<QuestionData> questions = new List<QuestionData>();
    private int currentQuestionIndex = 0;
    private int score = 0;
    private int correctStreak = 0;
    private int wrongStreak = 0;
    private int correctAnswers = 0;
    private int incorrectAnswers = 0;
    private AudioSource audioSource;
    private Coroutine feedbackCoroutine;
    private DatabaseReference dbReference;
    private bool gameEnded = false;
    private bool isFirebaseInitialized = false;
    private bool isBackgroundMusicPlaying = true;

    [System.Serializable]
    public class QuestionData
    {
        public QuestionText text;
        public QuestionOption[] options;
    }

    [System.Serializable]
    public class QuestionText
    {
        public string fr;
        public string ar;
    }

    [System.Serializable]
    public class QuestionOption
    {
        public string text;
        public bool correct;
    }

    [System.Serializable]
    private class QuestionList { public QuestionData[] questions; }

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.volume = soundVolume;
        ConfigureArabicText();
    }

    void Start()
    {
        if (avatarContainer != null) avatarContainer.SetActive(false);
        if (progressBar != null) progressBar.gameObject.SetActive(false);
        if (rewardPanel != null) rewardPanel.SetActive(false);

        if (GameSession.Instance == null)
        {
            Debug.LogError("GameSession.Instance est null !");
            StartCoroutine(LoadDefaultQuestions());
            if (questions.Count == 0) CreateFallbackQuestions();
            InitializeProgressBar();
            DisplayQuestion();
            return;
        }

        if (string.IsNullOrEmpty(GameSession.Instance.StudentId))
        {
            GameSession.Instance.StudentId = "stu_1749326542767";
            Debug.LogWarning("StudentId défini par défaut : stu_1749326542767");
        }

        FirebaseInitializer.Instance.InitializeFirebase(() =>
        {
            if (FirebaseInitializer.Instance.IsFirebaseInitialized)
            {
                dbReference = FirebaseInitializer.Instance.DbReference;
                isFirebaseInitialized = true;
                StartCoroutine(InitializeGame());
            }
            else
            {
                Debug.LogError("Firebase non initialisé !");
                StartCoroutine(LoadDefaultQuestions());
                if (questions.Count == 0) CreateFallbackQuestions();
                InitializeProgressBar();
                DisplayQuestion();
            }
        });

        if (backgroundMusic != null && backgroundAudioSource != null)
        {
            backgroundAudioSource.clip = backgroundMusic;
            backgroundAudioSource.Play();
            Debug.Log("Background music started on " + backgroundAudioSource.gameObject.name + ". Is playing: " + backgroundAudioSource.isPlaying);
        }

        if (soundToggleButton != null)
        {
            soundToggleButton.onClick.AddListener(ToggleBackgroundMusic);
            UpdateSoundIcon();
            Debug.Log("Sound toggle button configured on " + gameObject.name);
        }
    }

    private IEnumerator InitializeGame()
    {
        if (GameSession.Instance == null) { Debug.LogError("GameSession non initialisée !"); yield break; }
        Debug.Log($"Initialisation avec TestID: {GameSession.Instance.CurrentTestId}, Groupe: {GameSession.Instance.StudentGroup}, StudentId: {GameSession.Instance.StudentId}");
        yield return LoadQuestions();
        if (questions.Count > 0)
        {
            GameSession.Instance.TotalPossibleScore = questions.Count * xpPerCorrectAnswer;
            InitializeProgressBar();
            ShuffleQuestions();
            DisplayQuestion();
        }
        else
        {
            Debug.LogError("Aucune question disponible !");
            feedbackText.text = "Erreur: Aucune question disponible";
            if (progressBar != null) progressBar.gameObject.SetActive(false);
        }
    }

    private void InitializeProgressBar()
    {
        if (progressBar == null) return;
        progressBar.gameObject.SetActive(questions.Count > 0);
        progressBar.minValue = 0;
        progressBar.maxValue = questions.Count;
        progressBar.value = currentQuestionIndex;
    }

    private IEnumerator LoadQuestions()
    {
        yield return LoadQuestionsFromFirebase();
        if (questions.Count == 0)
        {
            Debug.Log("Chargement des questions par défaut");
            yield return LoadDefaultQuestions();
        }
        if (questions.Count == 0) CreateFallbackQuestions();
        questions = questions.GroupBy(q => q.text.fr).Select(g => g.First()).ToList();
    }

    private IEnumerator LoadQuestionsFromFirebase()
    {
        if (string.IsNullOrEmpty(GameSession.Instance.CurrentTestId)) yield break;
        var task = dbReference.Child("tests").Child(GameSession.Instance.CurrentTestId).GetValueAsync();
        yield return new WaitUntil(() => task.IsCompleted);
        if (!task.IsCompletedSuccessfully || !task.Result.Exists) yield break;
        DataSnapshot testSnapshot = task.Result;
        if (!testSnapshot.Child("miniGameConfigs").Exists || !testSnapshot.Child("miniGameConfigs").Child("choose_answer").Exists) yield break;
        var chooseAnswerConfig = testSnapshot.Child("miniGameConfigs").Child("choose_answer");
        if (!string.IsNullOrEmpty(GameSession.Instance.StudentGroup))
        {
            var groupsConfig = chooseAnswerConfig.Child("groupsConfig");
            if (groupsConfig.Exists && groupsConfig.Child(GameSession.Instance.StudentGroup).Exists)
            {
                var groupConfig = groupsConfig.Child(GameSession.Instance.StudentGroup).Child("config");
                if (groupConfig.Exists)
                {
                    yield return ProcessConfig(groupConfig);
                    if (questions.Count > 0) yield break;
                }
            }
        }
        var gradeConfig = chooseAnswerConfig.Child("gradeConfig");
        if (gradeConfig.Exists && gradeConfig.Child("config").Exists) yield return ProcessConfig(gradeConfig.Child("config"));
    }

    private IEnumerator ProcessConfig(DataSnapshot config)
    {
        if (!config.Exists) yield break;
        var modeNode = config.Child("mode");
        if (!modeNode.Exists) yield break;
        string mode = modeNode.Value.ToString();
        if (mode == "custom" && config.Child("customQuestions").Exists)
        {
            DataSnapshot customQuestions = config.Child("customQuestions");
            foreach (var question in customQuestions.Children)
            {
                DataSnapshot textNode = question.Child("text");
                DataSnapshot optionsNode = question.Child("options");
                if (!textNode.Exists || !optionsNode.Exists) continue;
                string frText = textNode.Child("fr")?.Value?.ToString() ?? "Question sans texte";
                string arText = textNode.Child("ar")?.Value?.ToString() ?? "سؤال sans nص";
                int correctAnswerIndex = -1;
                var correctIndexNode = question.Child("correctAnswerIndex");
                if (correctIndexNode.Exists)
                {
                    if (correctIndexNode.Value is long) correctAnswerIndex = (int)(long)correctIndexNode.Value;
                    else if (correctIndexNode.Value is string) int.TryParse((string)correctIndexNode.Value, out correctAnswerIndex);
                }
                var qData = new QuestionData { text = new QuestionText { fr = frText, ar = arText }, options = new QuestionOption[optionsNode.ChildrenCount] };
                int i = 0;
                foreach (var option in optionsNode.Children)
                {
                    string optionText = option.Value?.ToString() ?? "Option";
                    bool isCorrect = (i == correctAnswerIndex);
                    qData.options[i] = new QuestionOption { text = optionText, correct = isCorrect };
                    i++;
                }
                questions.Add(qData);
            }
        }
        else if (mode == "default") yield return LoadDefaultQuestions();
    }

    private IEnumerator LoadDefaultQuestions()
    {
        string path = Path.Combine(Application.streamingAssetsPath, jsonFileName);
#if UNITY_ANDROID || UNITY_IOS
        UnityWebRequest www = UnityWebRequest.Get(path);
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Échec chargement JSON : {www.error}");
            yield break;
        }
        string json = www.downloadHandler.text;
#else
        if (!File.Exists(path))
        {
            Debug.LogError($"Fichier JSON introuvable : {path}");
            yield break;
        }
        string json = File.ReadAllText(path);
#endif
        try
        {
            QuestionList loaded = JsonUtility.FromJson<QuestionList>(json);
            if (loaded?.questions != null) questions.AddRange(loaded.questions.Where(q => q.options != null && q.options.Length > 0));
        }
        catch (System.Exception e) { Debug.LogError("Erreur JSON: " + e.Message); }
    }

    private void CreateFallbackQuestions()
    {
        questions = new List<QuestionData>
        {
            new QuestionData { text = new QuestionText { fr = "2 × 3 = ?", ar = "؟3 × 2" }, options = new[] { new QuestionOption { text = "5", correct = false }, new QuestionOption { text = "6", correct = true }, new QuestionOption { text = "7", correct = false } } },
            new QuestionData { text = new QuestionText { fr = "4 × 5 = ?", ar = "؟5 × 4" }, options = new[] { new QuestionOption { text = "18", correct = false }, new QuestionOption { text = "20", correct = true }, new QuestionOption { text = "22", correct = false } } }
        };
    }

    private void DisplayQuestion()
    {
        if (gameEnded) return;
        ClearFeedback();
        if (currentQuestionIndex >= questions.Count) { EndGame(); return; }
        var current = questions[currentQuestionIndex];
        questionFRText.text = current.text.fr;
        questionARText.text = ArabicFixer.Fix(current.text.ar, true, true);
        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (i < current.options.Length)
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
        if (progressBar != null) progressBar.value = currentQuestionIndex + 1;
    }

    private void OnAnswerSelected(int idx)
    {
        if (gameEnded) return;
        bool isCorrect = questions[currentQuestionIndex].options[idx].correct;
        foreach (var btn in answerButtons.Where(b => b.gameObject.activeSelf)) btn.interactable = false;
        if (isCorrect)
        {
            PlaySound(correctAnswerSound);
            answerButtons[idx].image.color = correctColor;
            score += xpPerCorrectAnswer;
            scoreText.text = score.ToString();
            ShowFeedback("Correct!", correctColor);
            correctStreak++;
            wrongStreak = 0;
            correctAnswers++;
        }
        else
        {
            PlaySound(wrongAnswerSound);
            answerButtons[idx].image.color = wrongColor;
            ShowFeedback("Incorrect!", wrongColor);
            HighlightCorrectAnswer();
            wrongStreak++;
            correctStreak = 0;
            incorrectAnswers++;
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
            StartCoroutine(HideAvatarAfterDelay());
        }
    }

    private IEnumerator HideAvatarAfterDelay()
    {
        yield return new WaitForSeconds(avatarDisplayTime);
        if (avatarContainer != null) avatarContainer.SetActive(false);
    }

    private void HighlightCorrectAnswer()
    {
        var opts = questions[currentQuestionIndex].options;
        for (int i = 0; i < opts.Length; i++)
            if (opts[i].correct && i < answerButtons.Length) answerButtons[i].image.color = correctColor;
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

    private void ClearFeedback()
    {
        if (feedbackText != null) feedbackText.text = "";
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null && isBackgroundMusicPlaying) audioSource.PlayOneShot(clip);
    }

    private IEnumerator NextQuestionAfterDelay()
    {
        if (gameEnded) yield break;
        yield return new WaitForSeconds(delayBetweenQuestions);
        currentQuestionIndex++;
        if (currentQuestionIndex >= questions.Count) EndGame();
        else DisplayQuestion();
    }

    private void EndGame()
    {
        if (gameEnded) return;
        gameEnded = true;
        if (GameSession.Instance != null)
        {
            GameSession.Instance.CurrentScore = score;
            GameSession.Instance.CompletedMiniGames.Add("choose_answer");
        }
        if (progressBar != null) progressBar.value = questions.Count;
        int totalQuestions = questions.Count;
        float correctPercentage = (float)correctAnswers / totalQuestions * 100;
        int baseCoins = correctAnswers * coinsPerCorrectAnswer;
        int earnedBonus = correctPercentage >= bonusCoinsThreshold ? bonusCoins : 0;
        StartCoroutine(ShowRewardAnimation(baseCoins, earnedBonus));
    }

    private IEnumerator ShowRewardAnimation(int baseCoins, int bonusCoins)
    {
        if (rewardPanel == null || coinsText == null || coinIcon == null)
        {
            Debug.LogWarning("Reward UI non configuré !");
            yield return SaveStudentData(baseCoins + bonusCoins);
            LoadNextScene();
            yield break;
        }
        rewardPanel.SetActive(true);
        if (coinIcon != null && coinSprite != null) coinIcon.sprite = coinSprite;
        PlaySound(coinSound);
        float elapsed = 0f;
        int totalCoins = baseCoins + bonusCoins;
        coinsText.text = "";
        Debug.Log($"ShowRewardAnimation: BaseCoins={baseCoins}, BonusCoins={bonusCoins}, TotalCoins={totalCoins}");
        if (baseCoins > 0)
        {
            while (elapsed < animationDuration / 2f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (animationDuration / 2f);
                int displayedCoins = Mathf.RoundToInt(Mathf.Lerp(0, baseCoins, t));
                coinsText.text = $"+{displayedCoins} Base Coins";
                coinsText.transform.localScale = Vector3.one * (1f + Mathf.Sin(Time.time * 10f) * 0.1f);
                yield return null;
            }
            coinsText.text = $"+{baseCoins} Base Coins";
            coinsText.transform.localScale = Vector3.one;
            yield return new WaitForSeconds(0.5f);
        }
        elapsed = 0f;
        if (bonusCoins > 0)
        {
            while (elapsed < animationDuration / 2f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (animationDuration / 2f);
                int displayedBonus = Mathf.RoundToInt(Mathf.Lerp(0, bonusCoins, t));
                coinsText.text = $"+{baseCoins} + {displayedBonus} Bonus = {baseCoins + displayedBonus}";
                coinsText.transform.localScale = Vector3.one * (1f + Mathf.Sin(Time.time * 10f) * 0.1f);
                yield return null;
            }
            coinsText.text = $"+{baseCoins} + {bonusCoins} Bonus = {totalCoins}";
            coinsText.transform.localScale = Vector3.one;
        }
        else coinsText.text = $"+{baseCoins} Total";
        yield return new WaitForSeconds(2f);
        yield return SaveStudentData(totalCoins);
        LoadNextScene();
    }

    private async Task SaveStudentData(int totalCoins)
    {
        if (!isFirebaseInitialized || string.IsNullOrEmpty(GameSession.Instance?.StudentId) || dbReference == null)
        {
            Debug.LogError("Cannot save data: Firebase not initialized or StudentId/dbReference is null!");
            return;
        }

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.LogError("No internet connection! Data save failed.");
            return;
        }

        try
        {
            var dbRef = dbReference.Child("users").Child(GameSession.Instance.StudentId);
            DataSnapshot snapshot = await dbRef.GetValueAsync();

            long currentCoins = snapshot.Child("playerProfile").Child("coins").Exists ? (long)snapshot.Child("playerProfile").Child("coins").Value : 0;
            long currentBestScore = snapshot.Child("gameProgress").Child("choose_answer").Child("bestScore").Exists ? (long)snapshot.Child("gameProgress").Child("choose_answer").Child("bestScore").Value : 0;
            long currentLastScore = snapshot.Child("gameProgress").Child("choose_answer").Child("lastScore").Exists ? (long)snapshot.Child("gameProgress").Child("choose_answer").Child("lastScore").Value : 0;

            long newCoins = currentCoins + totalCoins;
            long newBestScore = Mathf.Max(score, (int)currentBestScore);
            long newLastScore = score;
            string completedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

            var updates = new Dictionary<string, object>
            {
                { $"playerProfile/coins", newCoins },
                { $"gameProgress/choose_answer/bestScore", newBestScore },
                { $"gameProgress/choose_answer/lastScore", newLastScore },
                { $"gameProgress/choose_answer/completedAt", completedAt }
            };

            Debug.Log($"Updating Firebase at path: users/{GameSession.Instance.StudentId} with data: Coins={newCoins}, BestScore={newBestScore}, LastScore={newLastScore}, completedAt={completedAt}");
            await dbRef.UpdateChildrenAsync(updates);
            Debug.Log($"Successfully updated data for {GameSession.Instance.StudentId}: Coins={newCoins}, BestScore={newBestScore}, LastScore={newLastScore}, completedAt={completedAt}");
            // Forcer la synchronisation
            dbRef.KeepSynced(true);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save data for {GameSession.Instance.StudentId}: {e.Message}\nStackTrace: {e.StackTrace}");
        }
    }

    private void LoadNextScene()
    {
        Debug.Log($"Fin du jeu ! Score: {score}");
        feedbackText.text = $"Jeu terminé ! Score: {score}";
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

    private void ToggleBackgroundMusic()
    {
        isBackgroundMusicPlaying = !isBackgroundMusicPlaying;
        if (backgroundAudioSource != null)
        {
            if (isBackgroundMusicPlaying)
            {
                if (!backgroundAudioSource.isPlaying)
                {
                    backgroundAudioSource.Play();
                    Debug.Log("Background music resumed on " + backgroundAudioSource.gameObject.name + ". Is playing: " + backgroundAudioSource.isPlaying);
                }
            }
            else
            {
                backgroundAudioSource.Pause();
                Debug.Log("Background music paused on " + backgroundAudioSource.gameObject.name + ". Is playing: " + backgroundAudioSource.isPlaying);
            }
        }
        UpdateSoundIcon();
    }

    private void UpdateSoundIcon()
    {
        if (soundToggleButton != null && soundOnIcon != null && soundOffIcon != null)
        {
            soundToggleButton.image.sprite = isBackgroundMusicPlaying ? soundOnIcon : soundOffIcon;
            Debug.Log($"Sound icon updated to {(isBackgroundMusicPlaying ? "On" : "Off")} on " + gameObject.name);
        }
    }
}