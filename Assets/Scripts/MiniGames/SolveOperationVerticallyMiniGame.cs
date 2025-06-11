using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

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
    public Button nextButton; // Référence au bouton "Next"

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

    // Variables pour la boucle des opérations
    private int currentOperationCount = 0;
    private int totalOperations;
    private int correctAnswers = 0;
    private int maxNumberRange;
    private float requiredCorrectAnswersMinimumPercent;
    private bool isGameOver = false; // Indique si le jeu est terminé

    void Start() => StartCoroutine(Initialize());
    private IEnumerator Initialize()
    {
        // Récupérer les paramètres depuis GameManager
        if (GameManager.Instance != null)
        {
            maxNumberRange = GameManager.Instance.MaxNumberRange;
            totalOperations = GameManager.Instance.NumOperations;
            requiredCorrectAnswersMinimumPercent = GameManager.Instance.RequiredCorrectAnswersMinimumPercent;
            Debug.Log($"Paramètres récupérés dans SolveOperationVerticallyMiniGame : maxNumberRange={maxNumberRange}, totalOperations={totalOperations}, requiredCorrectAnswersMinimumPercent={requiredCorrectAnswersMinimumPercent}");
        }
        else
        {
            Debug.LogWarning("GameManager.Instance est null. Utilisation des valeurs par défaut.");
            maxNumberRange = 3;
            totalOperations = 5;
            requiredCorrectAnswersMinimumPercent = 75f;
        }

        // Initialiser la barre de progression
        if (progressBarFill != null)
        {
            progressBarRect = progressBarFill.GetComponent<RectTransform>();
        }
        else
        {
            Debug.LogWarning("progressBarFill non assigné dans l'Inspector !");
        }

        if (progressStar != null)
        {
            originalStarScale = progressStar.localScale;
        }
        else
        {
            Debug.LogWarning("progressStar non assigné dans l'Inspector !");
        }

        // Vérifier que le bouton Next est assigné
        if (nextButton != null)
        {
            nextButton.interactable = true; // Actif au départ pour vérifier la première réponse
        }
        else
        {
            Debug.LogWarning("nextButton non assigné dans l'Inspector !");
        }

        UpdateProgressBar();
        GenerateOperation();

        if (backgroundMusic != null)
        {
            backgroundMusic.loop = true;
            backgroundMusic.Play();
        }
        else
        {
            Debug.LogWarning("backgroundMusic n'est pas assigné dans l'Inspector !");
        }
    }

    private void UpdateProgressBar()
    {
        if (isGameOver)
        {
            Debug.Log("Jeu terminé, la barre de progression ne se met plus à jour.");
            return;
        }

        if (progressBarFill != null && progressText != null && progressBarRect != null && progressStar != null)
        {
            float progress = (float)currentOperationCount / totalOperations;
            progressBarFill.fillAmount = progress;
            progressText.text = $"{(progress * 100):F0}%";

            float barWidth = progressBarRect.rect.width;
            float newX = -barWidth / 2f + (progress * barWidth);
            progressStar.anchoredPosition = new Vector2(newX, progressStar.anchoredPosition.y);

            StartCoroutine(ShineAnimationCoroutine());
        }
        else
        {
            Debug.LogWarning("progressBarFill, progressText, progressBarRect ou progressStar non assigné dans l'Inspector !");
        }
    }

    private System.Collections.IEnumerator ShineAnimationCoroutine()
    {
        Image starImage = progressStar.GetComponent<Image>();
        if (starImage == null)
        {
            Debug.LogWarning("Aucun composant Image trouvé sur progressStar !");
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
            Debug.Log("Jeu terminé, aucune nouvelle opération générée.");
            return;
        }

        if (currentOperationCount >= totalOperations)
        {
            isGameOver = true;
            float successPercent = ((float)correctAnswers / totalOperations) * 100f;
            bool isSuccess = successPercent >= requiredCorrectAnswersMinimumPercent;
            resultText.text = isSuccess ? $"Jeu terminé ! Succès ({successPercent:F0}%)" : $"Jeu terminé ! Échec ({successPercent:F0}% < {requiredCorrectAnswersMinimumPercent}%)";
            resultText.color = isSuccess ? Color.green : Color.red;
            Debug.Log(resultText.text);

            if (nextButton != null)
            {
                nextButton.interactable = false; // Désactiver le bouton à la fin
                Debug.Log("Bouton Next désactivé car le jeu est terminé.");
            }
            return;
        }

        number1 = Random.Range(10, (int)Mathf.Pow(10, maxNumberRange));
        number2 = Random.Range(10, (int)Mathf.Pow(10, maxNumberRange));
        result = (number1 * number2).ToString();

        topNumberText.text = number1.ToString().PadLeft(result.Length, ' ');
        bottomNumberText.text = "× " + number2.ToString().PadLeft(result.Length - 2, ' ');

        if (operationZone == null || plusSignPrefab == null || equalsSignPrefab == null)
        {
            Debug.LogError("operationZone, plusSignPrefab ou equalsSignPrefab non assigné dans l'Inspector !");
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
            if (zoneRect != null)
            {
                zoneRect.anchorMin = new Vector2(0.5f, 1f);
                zoneRect.anchorMax = new Vector2(0.5f, 1f);
                zoneRect.pivot = new Vector2(0.5f, 1f);
                zoneRect.anchoredPosition = new Vector2(0, verticalOffset);
                zoneRect.sizeDelta = new Vector2(result.Length * 160, 150);
                verticalOffset -= 120f;
            }

            GenerateDigitSlots(intermediateZoneGO.transform, intermediateResults[i], i);

            if (i < intermediateResults.Length - 1)
            {
                GameObject plusSignGO = Instantiate(plusSignPrefab, operationZone);
                plusSignGO.name = "PlusSign_" + i;

                RectTransform plusSignRect = plusSignGO.GetComponent<RectTransform>();
                if (plusSignRect != null)
                {
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
                }
                verticalOffset -= 40f;
            }
        }

        GameObject separatorGO = new GameObject("LineSeparator");
        separatorGO.transform.SetParent(operationZone, false);
        RectTransform separatorRect = separatorGO.AddComponent<RectTransform>();
        if (separatorRect != null)
        {
            separatorRect.anchorMin = new Vector2(0.5f, 1f);
            separatorRect.anchorMax = new Vector2(0.5f, 1f);
            separatorRect.pivot = new Vector2(0.5f, 1f);
            separatorRect.anchoredPosition = new Vector2(0, verticalOffset - 40f);
            separatorRect.sizeDelta = new Vector2(result.Length * 160, 2);
            Image separatorImage = separatorGO.AddComponent<Image>();
            separatorImage.color = Color.black;
            verticalOffset -= 40f;
        }

        GameObject finalZoneGO = new GameObject("FinalResultZone");
        finalZoneGO.transform.SetParent(operationZone, false);
        RectTransform finalZoneRect = finalZoneGO.AddComponent<RectTransform>();
        if (finalZoneRect != null)
        {
            finalZoneRect.anchorMin = new Vector2(0.5f, 1f);
            finalZoneRect.anchorMax = new Vector2(0.5f, 1f);
            finalZoneRect.pivot = new Vector2(0.5f, 1f);
            finalZoneRect.anchoredPosition = new Vector2(0, verticalOffset - 20f);
            finalZoneRect.sizeDelta = new Vector2(result.Length * 160, 150);
        }

        GenerateDigitSlots(finalZoneGO.transform, result, -1);

        GameObject equalsSignGO = Instantiate(equalsSignPrefab, operationZone);
        equalsSignGO.name = "EqualsSign";
        RectTransform equalsSignRect = equalsSignGO.GetComponent<RectTransform>();
        if (equalsSignRect != null)
        {
            equalsSignRect.anchorMin = new Vector2(0.5f, 1f);
            equalsSignRect.anchorMax = new Vector2(0.5f, 1f);
            equalsSignRect.pivot = new Vector2(0.5f, 0.5f);
            float xOffset = -((result.Length * 160) / 2f) - 50f;
            float yOffset = verticalOffset - 60f;
            equalsSignRect.anchoredPosition = new Vector2(xOffset, yOffset);
            equalsSignRect.sizeDelta = new Vector2(50, 50);
            TextMeshProUGUI equalsText = equalsSignGO.GetComponent<TextMeshProUGUI>();
            if (equalsText != null)
            {
                equalsText.fontSize = 36;
                equalsText.color = new Color(0.2f, 0.2f, 0.2f);
            }
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
                Debug.LogError($"Aucun composant DigitSlot trouvé sur {slotGO.name}");
            }
        }
    }

    private void CalculateIntermediateResults()
    {
        string num2Str = number2.ToString();
        intermediateResults = new string[num2Str.Length];

        for (int i = 0; i < num2Str.Length; i++)
        {
            int digitIndex = num2Str.Length - 1 - i;
            int digit = int.Parse(num2Str[digitIndex].ToString());
            int partialProduct = number1 * digit * (int)Mathf.Pow(10, i);
            intermediateResults[i] = partialProduct.ToString().PadLeft(result.Length, '0');
            Debug.Log($"Intermediate Result {i}: {intermediateResults[i]} (digit {digit} at position {i})");
        }
    }

    public bool IsAnswerCorrect()
    {
        string userAnswer = "";
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
                if (string.IsNullOrEmpty(expectedStep))
                    expectedStep = "0";
                if (string.IsNullOrEmpty(userStep))
                    userStep = "0";

                Debug.Log($"Étape {i} - Chiffres collectés : {string.Join(", ", digits)}");
                Debug.Log($"Étape {i} - Réponse utilisateur : {userIntermediateResults[i]} / Résultat attendu : {intermediateResults[i]}");
                Debug.Log($"Étape {i} - Après trim : Attendu '{expectedStep}', Reçu '{userStep}'");

                if (userStep != expectedStep)
                {
                    allStepsCorrect = false;
                    Debug.Log($"Étape {i} incorrecte : Attendu '{expectedStep}', mais reçu '{userStep}'");
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
                Debug.Log($"Étape {i} - Valeur ajoutée à la somme : {stepValue}");
                sum += stepValue;
            }
        }

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
            userAnswer = string.Join("", digits);
            Debug.Log($"Résultat final - Chiffres collectés : {string.Join(", ", digits)}");
        }
        else
        {
            Debug.LogError("FinalResultZone non trouvé !");
        }

        string trimmedUserAnswer = userAnswer.TrimStart('0');
        string trimmedResult = result.TrimStart('0');
        if (string.IsNullOrEmpty(trimmedUserAnswer))
            trimmedUserAnswer = "0";
        if (string.IsNullOrEmpty(trimmedResult))
            trimmedResult = "0";

        bool finalResultCorrect = trimmedUserAnswer == trimmedResult;
        bool isCorrect = allStepsCorrect && finalResultCorrect;

        if (isCorrect)
        {
            correctAnswers++;
            playerCorrectAnswers = correctAnswers; // Mettre à jour pour la classe parent
        }

        if (resultText != null)
        {
            resultText.text = isCorrect ? "Correct !" : "Incorrect !";
            resultText.color = isCorrect ? Color.green : Color.red;
        }
        else
        {
            Debug.LogWarning("resultText n'est pas assigné dans l'Inspector !");
        }

        if (isCorrect && correctSound != null)
        {
            correctSound.Play();
        }
        else if (isCorrect && correctSound == null)
            Debug.LogWarning("correctSound n'est pas assigné dans l'Inspector !");
        else if (!isCorrect && incorrectSound != null)
            incorrectSound.Play();
        else if (!isCorrect && incorrectSound == null)
        {
            Debug.LogWarning("incorrectSound n'est pas assigné dans l'Inspector !");
        }

        return isCorrect;
    }

    public void CheckAnswer()
    {
        if (isGameOver)
        {
            Debug.Log("Jeu terminé, aucune vérification possible.");
            return;
        }

        bool isCorrect = IsAnswerCorrect();

        currentOperationCount++;
        UpdateProgressBar();
        GenerateOperation();
    }

    public override bool CheckSuccess(int requiredCorrectAnswers)
    {
        float successPercent = ((float)correctAnswers / totalOperations) * 100f;
        return successPercent >= requiredCorrectAnswersMinimumPercent;
    }
}