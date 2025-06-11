using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using Firebase.Database;
using Firebase.Extensions;
using System.Threading.Tasks;
using System;

public class SolveOperationVerticallyMiniGame : MathoriaMiniGameWidget
{
    [Header("Références UI")]
    public Transform operationZone;
    public GameObject digitSlotPrefab;
    public GameObject plusSignPrefab;
    public GameObject equalsSignPrefab;
    public TextMeshProUGUI topNumberText;
    public TextMeshProUGUI bottomNumberText;
    public TextMeshProUGUI resultText;
    public Button nextButton;

    [Header("Références Barre de Progression")]
    public Image progressBarBackground;
    public Image progressBarFill;
    public TextMeshProUGUI progressText;
    public RectTransform progressStar;
    private RectTransform progressBarRect;

    [Header("Paramètres d'Animation")]
    public float shineDuration = 0.5f;
    public float shineScale = 1.5f;
    public Color shineColor = new Color(1f, 1f, 0.5f, 1f);
    private Vector3 originalStarScale;

    [Header("Références Audio")]
    public AudioSource correctSound;
    public AudioSource incorrectSound;
    public AudioSource backgroundMusic;

    private int number1;
    private int number2;
    private string result;
    private string[] intermediateResults;

    private int currentOperationCount = 0;
    private int numOperations;
    private int correctAnswers = 0;
    private int maxNumberRange;
    private float requiredCorrectAnswersMinimumPercent;
    private bool isGameOver = false;

    // Firebase variables
    private DatabaseReference databaseReference;
    private string playerUid;
    private bool isFirebaseInitialized = false;

    void Start()
    {
        // Démarrer la musique immédiatement si disponible
        if (backgroundMusic != null)
        {
            backgroundMusic.loop = true;
            backgroundMusic.PlayScheduled(AudioSettings.dspTime + 0.1f);
            Debug.Log("Musique de fond démarrée immédiatement.");
        }
        else
        {
            Debug.LogWarning("backgroundMusic non assigné !");
        }

        // Lancer l'initialisation
        StartCoroutine(Initialize());
    }

    private IEnumerator Initialize()
    {
        // Afficher un message de chargement dès le début
        if (resultText != null)
        {
            resultText.text = "Chargement...";
            resultText.color = Color.white;
        }

        // Initialiser Firebase
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
            if (resultText != null)
            {
                resultText.text = "Erreur : Échec de l'initialisation Firebase !";
                resultText.color = Color.red;
            }
            if (nextButton != null) nextButton.interactable = false;
            isGameOver = true;
            yield break;
        }

        // Attendre que UserSession soit initialisé
        float timeout = 10f;
        float elapsed = 0f;
        while (UserSession.Instance == null || UserSession.Instance.CurrentUser == null && elapsed < timeout)
        {
            Debug.Log("Waiting for UserSession to authenticate user...");
            yield return new WaitForSecondsRealtime(0.1f);
            elapsed += 0.1f;
        }

        if (UserSession.Instance == null || UserSession.Instance.CurrentUser == null)
        {
            Debug.LogError("No authenticated user found in UserSession after timeout!");
            if (resultText != null)
            {
                resultText.text = "Erreur : Utilisateur non authentifié !";
                resultText.color = Color.red;
            }
            if (nextButton != null) nextButton.interactable = false;
            isGameOver = true;
            yield break;
        }

        playerUid = UserSession.Instance.CurrentUser.uid;
        Debug.Log($"Firebase initialized for player: {playerUid}");
        isFirebaseInitialized = true;

        // Attendre que TestConfiguration soit chargé
        int maxAttempts = 50;
        int attempts = 0;
        while (TestConfiguration.MiniGameConfigs == null && attempts < maxAttempts)
        {
            attempts++;
            yield return new WaitForSecondsRealtime(0.1f);
        }

        if (TestConfiguration.MiniGameConfigs == null)
        {
            Debug.LogError("TestConfiguration.MiniGameConfigs non chargé après 5 secondes.");
            isGameOver = true;
            if (resultText != null)
            {
                resultText.text = "Erreur : Configuration non chargée !";
                resultText.color = Color.red;
            }
            if (nextButton != null) nextButton.interactable = false;
            yield break;
        }

        // Charger les paramètres depuis TestConfiguration
        if (TestConfiguration.MiniGameConfigs.ContainsKey("vertical_operations"))
        {
            var config = TestConfiguration.MiniGameConfigs["vertical_operations"];
            try
            {
                maxNumberRange = config["maxNumberRange"] is long
                    ? (int)(long)config["maxNumberRange"]
                    : int.Parse(config["maxNumberRange"].ToString());
                numOperations = config["numOperations"] is long
                    ? (int)(long)config["numOperations"]
                    : int.Parse(config["numOperations"].ToString());
                requiredCorrectAnswersMinimumPercent = config["requiredCorrectAnswersMinimumPercent"] is long
                    ? (float)(long)config["requiredCorrectAnswersMinimumPercent"]
                    : float.Parse(config["requiredCorrectAnswersMinimumPercent"].ToString());

                bool isValidOperation = false;
                if (config.ContainsKey("operationsAllowed"))
                {
                    if (config["operationsAllowed"] is string opString)
                    {
                        isValidOperation = string.Equals(opString, "Multiplication", StringComparison.OrdinalIgnoreCase)
                                        || string.Equals(opString, "Mutiplication", StringComparison.OrdinalIgnoreCase);
                    }
                    else if (config["operationsAllowed"] is List<object> opList)
                    {
                        isValidOperation = opList.Contains("Multiplication") || opList.Contains("Mutiplication");
                    }
                }

                if (!isValidOperation)
                {
                    throw new System.Exception($"operationsAllowed doit inclure 'Multiplication' (reçu : {config["operationsAllowed"]})");
                }

                Debug.Log($"Paramètres chargés : maxNumberRange={maxNumberRange}, numOperations={numOperations}, requiredCorrectAnswersMinimumPercent={requiredCorrectAnswersMinimumPercent}, operationsAllowed=Multiplication");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Erreur lors du parsing des paramètres : {e.Message}");
                isGameOver = true;
                if (resultText != null)
                {
                    resultText.text = $"Erreur : {e.Message}";
                    resultText.color = Color.red;
                }
                if (nextButton != null) nextButton.interactable = false;
                yield break;
            }
        }
        else
        {
            Debug.LogError("Aucune configuration pour vertical_operations.");
            isGameOver = true;
            if (resultText != null)
            {
                resultText.text = "Erreur : Configuration non trouvée !";
                resultText.color = Color.red;
            }
            if (nextButton != null) nextButton.interactable = false;
            yield break;
        }

        // Vérifier numOperations
        if (numOperations <= 0)
        {
            Debug.LogError($"numOperations est invalide ({numOperations}).");
            isGameOver = true;
            if (resultText != null)
            {
                resultText.text = "Erreur : Nombre d'opérations invalide !";
                resultText.color = Color.red;
            }
            if (nextButton != null) nextButton.interactable = false;
            yield break;
        }

        // Vérifier les références UI
        if (progressBarFill == null) Debug.LogError("progressBarFill non assigné !");
        if (progressText == null) Debug.LogError("progressText non assigné !");
        if (progressStar == null) Debug.LogError("progressStar non assigné !");
        if (operationZone == null) Debug.LogError("operationZone non assigné !");
        if (digitSlotPrefab == null) Debug.LogError("digitSlotPrefab non assigné !");
        if (plusSignPrefab == null) Debug.LogError("plusSignPrefab non assigné !");
        if (equalsSignPrefab == null) Debug.LogError("equalsSignPrefab non assigné !");
        if (topNumberText == null) Debug.LogError("topNumberText non assigné !");
        if (bottomNumberText == null) Debug.LogError("bottomNumberText non assigné !");
        if (resultText == null) Debug.LogError("resultText non assigné !");
        if (nextButton == null) Debug.LogError("nextButton non assigné !");

        // Initialiser la barre de progression
        if (progressBarFill != null)
        {
            progressBarRect = progressBarFill.GetComponent<RectTransform>();
            if (progressBarRect == null) Debug.LogError("progressBarRect non assigné !");
            progressBarFill.fillAmount = 0f;
        }

        if (progressStar != null)
        {
            originalStarScale = progressStar.localScale;
        }

        // Configurer le bouton Next
        if (nextButton != null)
        {
            nextButton.interactable = true;
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(OnNextButtonClicked);
            Debug.Log("Bouton Next configuré pour appeler OnNextButtonClicked.");
        }

        // Initialiser la première opération
        UpdateProgressBar();
        GenerateOperation();
        Debug.Log($"Initialisation : Opération 1/{numOperations}");

        // Effacer le message de chargement
        if (resultText != null)
        {
            resultText.text = "";
        }
    }

    private void UpdateProgressBar()
    {
        if (isGameOver)
        {
            Debug.Log("Jeu terminé, barre de progression figée.");
            return;
        }

        if (progressBarFill != null && progressText != null && progressBarRect != null && progressStar != null)
        {
            float progress = numOperations > 0 ? (float)currentOperationCount / numOperations : 0f;
            progressBarFill.fillAmount = Mathf.Clamp01(progress);
            progressText.text = $"{(progress * 100):F0}%";

            float barWidth = progressBarRect.rect.width;
            float newX = -barWidth / 2f + (progress * barWidth);
            progressStar.anchoredPosition = new Vector2(newX, progressStar.anchoredPosition.y);

            StartCoroutine(ShineAnimationCoroutine());
            Debug.Log($"Barre de progression : {progress * 100:F0}% (opération {currentOperationCount}/{numOperations})");
        }
        else
        {
            Debug.LogError("Références de progression manquantes !");
        }
    }

    private IEnumerator ShineAnimationCoroutine()
    {
        if (progressStar == null)
        {
            Debug.LogError("progressStar is null!");
            yield break;
        }

        Image starImage = progressStar.GetComponent<Image>();
        if (starImage == null)
        {
            Debug.LogError("Aucun composant Image sur progressStar !");
            yield break;
        }

        progressStar.localScale = originalStarScale;
        starImage.color = Color.white;

        float elapsed = 0f;
        float halfDuration = shineDuration / 2f;

        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            progressStar.localScale = Vector3.Lerp(originalStarScale, originalStarScale * shineScale, t);
            starImage.color = Color.Lerp(Color.white, shineColor, t);
            yield return null;
        }

        progressStar.localScale = originalStarScale * shineScale;
        starImage.color = shineColor;

        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            progressStar.localScale = Vector3.Lerp(originalStarScale * shineScale, originalStarScale, t);
            starImage.color = Color.Lerp(shineColor, Color.white, t);
            yield return null;
        }

        progressStar.localScale = originalStarScale;
        starImage.color = Color.white;
    }

    public void GenerateOperation()
    {
        if (isGameOver)
        {
            Debug.Log("Jeu terminé, aucune opération générée.");
            return;
        }

        Debug.Log($"Génération opération : {currentOperationCount + 1}/{numOperations}");

        try
        {
            int maxValue = (int)Mathf.Pow(10, maxNumberRange);
            number1 = UnityEngine.Random.Range(10, maxValue);
            number2 = UnityEngine.Random.Range(10, maxValue);
            result = (number1 * number2).ToString();
            Debug.Log($"Multiplication : {number1} × {number2} = {result}");

            if (topNumberText != null && bottomNumberText != null)
            {
                topNumberText.text = number1.ToString().PadLeft(result.Length, ' ');
                bottomNumberText.text = $"× " + number2.ToString().PadLeft(result.Length - 2, ' ');
            }
            else
            {
                Debug.LogError("topNumberText ou bottomNumberText non assigné !");
                return;
            }

            if (operationZone == null || digitSlotPrefab == null || plusSignPrefab == null || equalsSignPrefab == null)
            {
                Debug.LogError("operationZone ou prefabs non assignés !");
                return;
            }

            foreach (Transform child in operationZone)
            {
                Destroy(child.gameObject);
            }

            if (operationZone.GetComponent<HorizontalLayoutGroup>() != null)
            {
                Destroy(operationZone.GetComponent<HorizontalLayoutGroup>());
            }
            if (operationZone.GetComponent<VerticalLayoutGroup>() != null)
            {
                Destroy(operationZone.GetComponent<VerticalLayoutGroup>());
            }

            RectTransform operationZoneRect = operationZone.GetComponent<RectTransform>();
            if (operationZoneRect != null)
            {
                operationZoneRect.anchorMin = new Vector2(0.5f, 1f);
                operationZoneRect.anchorMax = new Vector2(0.5f, 1f);
                operationZoneRect.pivot = new Vector2(0.5f, 1f);
                operationZoneRect.anchoredPosition = Vector2.zero;
                operationZoneRect.sizeDelta = new Vector2(result.Length * 160, 500);
            }

            CalculateIntermediateResults();

            float verticalOffset = 0f;
            for (int i = 0; i < intermediateResults.Length; i++)
            {
                GameObject intermediateZoneGO = new GameObject("IntermediateZone_" + i);
                intermediateZoneGO.transform.SetParent(operationZone, false);

                RectTransform zoneRect = intermediateZoneGO.AddComponent<RectTransform>();
                zoneRect.anchorMin = new Vector2(0.5f, 1f);
                zoneRect.anchorMax = new Vector2(0.5f, 1f);
                zoneRect.pivot = new Vector2(0.5f, 1f);
                zoneRect.anchoredPosition = new Vector2(0, verticalOffset);
                zoneRect.sizeDelta = new Vector2(result.Length * 160, 150);
                verticalOffset -= 120f;

                GenerateDigitSlots(intermediateZoneGO.transform, intermediateResults[i], i);

                if (i < intermediateResults.Length - 1)
                {
                    GameObject plusSignGO = Instantiate(plusSignPrefab, operationZone);
                    plusSignGO.name = "PlusSign_" + i;

                    RectTransform plusSignRect = plusSignGO.GetComponent<RectTransform>();
                    plusSignRect.anchorMin = new Vector2(0.5f, 1f);
                    plusSignRect.anchorMax = new Vector2(0.5f, 1f);
                    plusSignRect.pivot = new Vector2(0.5f, 0.5f);
                    float xOffset = -((result.Length * 160) / 2f) - 50f;
                    float yOffset = verticalOffset - 60f;
                    plusSignRect.anchoredPosition = new Vector2(xOffset, yOffset);
                    plusSignRect.sizeDelta = new Vector2(50, 50);
                    TextMeshProUGUI plusText = plusSignGO.GetComponent<TextMeshProUGUI>();
                    if (plusText != null)
                    {
                        plusText.fontSize = 36;
                        plusText.color = new Color(0.2f, 0.2f, 0.2f);
                    }
                    verticalOffset -= 40f;
                }
            }

            GameObject separatorGO = new GameObject("LineSeparator");
            separatorGO.transform.SetParent(operationZone, false);
            RectTransform separatorRect = separatorGO.AddComponent<RectTransform>();
            separatorRect.anchorMin = new Vector2(0.5f, 1f);
            separatorRect.anchorMax = new Vector2(0.5f, 1f);
            separatorRect.pivot = new Vector2(0.5f, 1f);
            separatorRect.anchoredPosition = new Vector2(0, verticalOffset - 40f);
            separatorRect.sizeDelta = new Vector2(result.Length * 160, 2);
            Image separatorImage = separatorGO.AddComponent<Image>();
            separatorImage.color = Color.black;
            verticalOffset -= 40f;

            GameObject finalZoneGO = new GameObject("FinalResultZone");
            finalZoneGO.transform.SetParent(operationZone, false);
            RectTransform finalZoneRect = finalZoneGO.AddComponent<RectTransform>();
            finalZoneRect.anchorMin = new Vector2(0.5f, 1f);
            finalZoneRect.anchorMax = new Vector2(0.5f, 1f);
            finalZoneRect.pivot = new Vector2(0.5f, 1f);
            finalZoneRect.anchoredPosition = new Vector2(0, verticalOffset - 20f);
            finalZoneRect.sizeDelta = new Vector2(result.Length * 160, 150);

            GenerateDigitSlots(finalZoneGO.transform, result, -1);

            GameObject equalsSignGO = Instantiate(equalsSignPrefab, operationZone);
            equalsSignGO.name = "EqualsSign";
            RectTransform equalsSignRect = equalsSignGO.GetComponent<RectTransform>();
            equalsSignRect.anchorMin = new Vector2(0.5f, 1f);
            equalsSignRect.anchorMax = new Vector2(0.5f, 1f);
            equalsSignRect.pivot = new Vector2(0.5f, 0.5f);
            float xOffsetEq = -((result.Length * 160) / 2f) - 50f;
            float yOffsetEq = verticalOffset - 60f;
            equalsSignRect.anchoredPosition = new Vector2(xOffsetEq, yOffsetEq);
            equalsSignRect.sizeDelta = new Vector2(50, 50);
            TextMeshProUGUI equalsText = equalsSignGO.GetComponent<TextMeshProUGUI>();
            if (equalsText != null)
            {
                equalsText.fontSize = 36;
                equalsText.text = "=";
                equalsText.color = new Color(0.2f, 0.2f, 0.2f);
                equalsText.alignment = TextAlignmentOptions.Center;
            }

            if (resultText != null)
            {
                resultText.text = "";
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erreur génération opération : {e.Message}");
            isGameOver = true;
            if (resultText != null)
            {
                resultText.text = "Erreur lors de la génération !";
                resultText.color = Color.red;
            }
            if (nextButton != null) nextButton.interactable = false;
        }
    }

    private void GenerateDigitSlots(Transform parent, string targetResult, int stepIndex)
    {
        int slotCount = result.Length;
        int shift = (stepIndex > 0) ? stepIndex : 0;

        for (int i = 0; i < slotCount; i++)
        {
            GameObject slotGO = Instantiate(digitSlotPrefab, parent);
            slotGO.name = "DigitSlot_" + i;

            RectTransform slotRect = slotGO.GetComponent<RectTransform>();
            if (slotRect != null)
            {
                slotRect.anchorMin = new Vector2(0, 0.5f);
                slotRect.anchorMax = new Vector2(0, 0.5f);
                slotRect.pivot = new Vector2(0, 0.5f);
                slotRect.sizeDelta = new Vector2(150, 150);
                slotRect.localScale = Vector3.one;
                slotRect.anchoredPosition = new Vector2((slotCount - 1 - i) * 160, 0);
            }

            DigitSlot digitSlot = slotGO.GetComponent<DigitSlot>();
            if (digitSlot != null)
            {
                if (stepIndex > 0 && i == 0)
                {
                    digitSlot.slotText.text = ".";
                    digitSlot.slotText.color = Color.gray;
                    digitSlot.GetComponent<Image>().raycastTarget = false;
                }
                else
                {
                    digitSlot.slotText.text = "";
                }
            }
            else
            {
                Debug.LogError($"Aucun composant DigitSlot sur {slotGO.name}");
            }
        }
    }

    private void CalculateIntermediateResults()
    {
        try
        {
            string num2Str = number2.ToString();
            intermediateResults = new string[num2Str.Length];

            for (int i = 0; i < num2Str.Length; i++)
            {
                int digitIndex = num2Str.Length - 1 - i;
                int digit = int.Parse(num2Str[digitIndex].ToString());
                int partialProduct = number1 * digit * (int)Mathf.Pow(10, i);
                intermediateResults[i] = partialProduct.ToString().PadLeft(result.Length, '0');
                Debug.Log($"Résultat intermédiaire {i} : {intermediateResults[i]}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erreur CalculateIntermediateResults : {e.Message}");
            throw;
        }
    }

    public bool IsAnswerCorrect()
    {
        try
        {
            string[] userIntermediateResults = new string[intermediateResults.Length];
            bool allStepsCorrect = true;

            for (int i = 0; i < intermediateResults.Length; i++)
            {
                Transform intermediateZone = operationZone.Find("IntermediateZone_" + i);
                if (intermediateZone != null)
                {
                    List<string> digits = new List<string>();
                    int slotIndex = 0;
                    foreach (Transform slot in intermediateZone)
                    {
                        DigitSlot digitSlot = slot.GetComponent<DigitSlot>();
                        if (digitSlot != null)
                        {
                            string digitText = digitSlot.slotText.text;
                            if (i > 0 && slotIndex == 0 && digitText == ".")
                            {
                                digits.Add("0");
                            }
                            else
                            {
                                digits.Add((string.IsNullOrEmpty(digitText) || digitText == ".") ? "0" : digitText);
                            }
                            slotIndex++;
                        }
                    }

                    digits.Reverse();
                    userIntermediateResults[i] = string.Join("", digits);

                    string expectedStep = intermediateResults[i].TrimStart('0').TrimEnd('0');
                    string userStep = userIntermediateResults[i].TrimStart('0').TrimEnd('0');
                    if (string.IsNullOrEmpty(expectedStep)) expectedStep = "0";
                    if (string.IsNullOrEmpty(userStep)) userStep = "0";

                    Debug.Log($"Étape {i} - Réponse utilisateur : {userIntermediateResults[i]} / Attendu : {intermediateResults[i]}");

                    if (userStep != expectedStep)
                    {
                        allStepsCorrect = false;
                        Debug.Log($"Étape {i} incorrecte ! Attendu '{expectedStep}', reçu '{userStep}'");
                    }
                }
                else
                {
                    Debug.LogError($"IntermediateZone_{i} non trouvé !");
                    allStepsCorrect = false;
                }
            }

            int sum = 0;
            for (int i = 0; i < userIntermediateResults.Length; i++)
            {
                if (int.TryParse(userIntermediateResults[i], out int stepValue))
                {
                    sum += stepValue;
                }
            }

            string userAnswerText = "";
            Transform finalZone = operationZone.Find("FinalResultZone");
            if (finalZone != null)
            {
                List<string> digits = new List<string>();
                foreach (Transform slot in finalZone)
                {
                    DigitSlot digitSlot = slot.GetComponent<DigitSlot>();
                    if (digitSlot != null)
                    {
                        string digitText = digitSlot.slotText.text;
                        digits.Add((string.IsNullOrEmpty(digitText) || digitText == ".") ? "0" : digitText);
                    }
                }
                digits.Reverse();
                userAnswerText = string.Join("", digits);
            }

            string trimmedUserAnswer = userAnswerText.TrimStart('0');
            string trimmedResult = result.TrimStart('0');
            if (string.IsNullOrEmpty(trimmedUserAnswer)) trimmedUserAnswer = "0";
            if (string.IsNullOrEmpty(trimmedResult)) trimmedResult = "0";

            bool finalResultCorrect = trimmedUserAnswer == trimmedResult;
            bool isCorrect = allStepsCorrect && finalResultCorrect;

            if (isCorrect)
            {
                correctAnswers++;
                Debug.Log($"Réponse correcte ! correctAnswers={correctAnswers}");
            }

            if (resultText != null && currentOperationCount + 1 < numOperations)
            {
                resultText.text = isCorrect ? "Correct !" : "Incorrect !";
                resultText.color = isCorrect ? Color.green : Color.red;
            }

            if (isCorrect && correctSound != null)
            {
                correctSound.Play();
            }
            else if (!isCorrect && incorrectSound != null)
            {
                incorrectSound.Play();
            }

            return isCorrect;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erreur IsAnswerCorrect : {e.Message}");
            return false;
        }
    }

    private void OnNextButtonClicked()
    {
        if (isGameOver)
        {
            Debug.Log("Jeu terminé, clic ignoré !");
            return;
        }

        Debug.Log($"Clic Next : Opération {currentOperationCount + 1}/{numOperations}");

        bool isCorrect = IsAnswerCorrect();

        currentOperationCount++;

        UpdateProgressBar();

        if (currentOperationCount >= numOperations)
        {
            isGameOver = true;
            float successPercent = ((float)correctAnswers / numOperations) * 100f;
            bool isSuccess = successPercent >= requiredCorrectAnswersMinimumPercent;
            Debug.Log(isSuccess ? $"Succès : ({successPercent:F0}%)" : $"Échec ! ({successPercent:F0}% < {requiredCorrectAnswersMinimumPercent:F0}%)");

            if (nextButton != null)
            {
                nextButton.interactable = false;
                Debug.Log("Bouton Next désactivé.");
            }

            foreach (Transform child in operationZone)
            {
                DigitSlot digitSlot = child.GetComponent<DigitSlot>();
                if (digitSlot != null)
                {
                    digitSlot.GetComponent<Image>().raycastTarget = false;
                }
            }

            if (isFirebaseInitialized)
            {
                UpdatePlayerScores(successPercent).ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        Debug.LogError($"Erreur lors de la mise à jour des scores : {task.Exception}");
                    }
                    else
                    {
                        Debug.Log("Scores et profil mis à jour avec succès dans Firebase");
                    }
                });
            }
            else
            {
                Debug.LogWarning("Firebase non initialisé, impossible de sauvegarder les scores.");
            }

            return;
        }

        GenerateOperation();
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
            // Récupérer les données actuelles de gameProgress et playerProfile
            DataSnapshot snapshot = await databaseReference
                .Child("users")
                .Child(playerUid)
                .GetValueAsync();

            float bestScore = 0f;
            float lastScore = 0f;
            int questionsSolved = 0;
            int iScore = 0;

            if (snapshot.Exists)
            {
                // Récupérer les scores de vertical_operations
                if (snapshot.HasChild("gameProgress/vertical_operations"))
                {
                    var verticalOps = snapshot.Child("gameProgress/vertical_operations");
                    bestScore = float.TryParse(verticalOps.Child("bestScore").Value?.ToString(), out float bs) ? bs : 0f;
                    lastScore = float.TryParse(verticalOps.Child("lastScore").Value?.ToString(), out float ls) ? ls : 0f;
                }

                // Récupérer questionsSolved et iScore depuis playerProfile
                if (snapshot.HasChild("playerProfile/questionsSolved"))
                {
                    questionsSolved = int.TryParse(snapshot.Child("playerProfile/questionsSolved").Value?.ToString(), out int qs) ? qs : 0;
                }
                if (snapshot.HasChild("playerProfile/rewardProfile/iScore"))
                {
                    iScore = int.TryParse(snapshot.Child("playerProfile/rewardProfile/iScore").Value?.ToString(), out int iscore) ? iscore : 0;
                }

                Debug.Log($"Fetched data for {playerUid}: bestScore={bestScore}, lastScore={lastScore}, questionsSolved={questionsSolved}, iScore={iScore}");
            }
            else
            {
                Debug.Log($"No existing data found for {playerUid}, initializing with zeros.");
            }

            // Calculer les nouvelles valeurs
            float newBestScore = Mathf.Max(currentProgress, bestScore);
            int newLastScore = Mathf.RoundToInt(currentProgress); // Arrondi à l'entier
            int newQuestionsSolved = questionsSolved + correctAnswers;
            int newIScore = iScore + Mathf.RoundToInt(currentProgress); // Ajouter le score du jeu à iScore
            string completedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

            // Préparer les mises à jour
            Dictionary<string, object> updates = new Dictionary<string, object>
            {
                // Mise à jour de gameProgress/vertical_operations
                { $"users/{playerUid}/gameProgress/vertical_operations/bestScore", newBestScore },
                { $"users/{playerUid}/gameProgress/vertical_operations/lastScore", newLastScore },
                { $"users/{playerUid}/gameProgress/vertical_operations/completedAt", completedAt },
                // Mise à jour de playerProfile/questionsSolved
                { $"users/{playerUid}/playerProfile/questionsSolved", newQuestionsSolved },
                // Mise à jour de playerProfile/rewardProfile/iScore
                { $"users/{playerUid}/playerProfile/rewardProfile/iScore", newIScore }
            };

            // Effectuer les mises à jour dans Firebase
            Debug.Log($"Preparing to update Firebase with iScore={newIScore}, lastScore={newLastScore} for {playerUid}");
            await databaseReference.UpdateChildrenAsync(updates);
            Debug.Log($"Updated data for {playerUid}: bestScore={newBestScore}, lastScore={newLastScore}, questionsSolved={newQuestionsSolved}, iScore={newIScore}, completedAt={completedAt}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to update scores for {playerUid}: {e.Message}\nStackTrace: {e.StackTrace}");
        }
    }

    public override bool CheckSuccess(int requiredCorrectAnswers)
    {
        float successPercent = ((float)correctAnswers / numOperations) * 100f;
        Debug.Log($"Succès : {successPercent:F0}% >= {requiredCorrectAnswersMinimumPercent:F0}%");
        return successPercent >= requiredCorrectAnswersMinimumPercent;
    }
}