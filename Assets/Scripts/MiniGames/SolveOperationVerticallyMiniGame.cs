using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class SolveOperationVerticallyMiniGame : MathoriaMiniGameWidget
{
    [Header("Références UI")]
    public Transform operationZone;           // Panel parent des DigitSlots
    public GameObject digitSlotPrefab;        // Prefab DigitSlotCanvas (doit contenir DigitSlot script)
    public GameObject plusSignPrefab;         // Prefab contenant le signe "+" (TextMeshProUGUI)
    public GameObject equalsSignPrefab;       // Prefab contenant le signe "=" (TextMeshProUGUI)
    public TextMeshProUGUI topNumberText;     // Premier nombre (ex: 23)
    public TextMeshProUGUI bottomNumberText;  // Deuxième nombre avec signe (ex: avec "× 4")
    public TextMeshProUGUI resultText;        // Texte pour afficher si la réponse est correcte ou non

    [Header("Références Barre de Progression")]
    public Image progressBarBackground;       // Image de fond de la barre de progression
    public Image progressBarFill;             // Image de remplissage de la barre de progression
    public TextMeshProUGUI progressText;      // Texte pour afficher le pourcentage (ex: "20%")

    [Header("Références Audio")]
    public AudioSource correctSound;          // AudioSource pour le son de résultat correct
    public AudioSource incorrectSound;        // AudioSource pour le son de résultat incorrect
    public AudioSource backgroundMusic;       // AudioSource pour la musique de fond

    private int number1;
    private int number2;
    private string result;
    private string[] intermediateResults;     // Résultats intermédiaires pour chaque étape

    // Variables pour la boucle des 5 opérations
    private int currentOperationCount = 0;    // Compteur d'opérations effectuées
    private const int totalOperations = 5;    // Nombre total d'opérations à résoudre

    void Start()
    {
        // Initialiser la barre de progression
        UpdateProgressBar();
        // Générer la première opération
        GenerateOperation();
        if (backgroundMusic != null)
        {
            backgroundMusic.loop = true; // S'assurer que la musique boucle
            backgroundMusic.Play();      // Lancer la musique
        }
        else
        {
            Debug.LogWarning("backgroundMusic n'est pas assigné dans l'Inspector !");
        }
    }

    /// <summary>
    /// Met à jour la barre de progression en fonction du nombre d'opérations effectuées
    /// </summary>
    private void UpdateProgressBar()
    {
        if (progressBarFill != null && progressText != null)
        {
            float progress = (float)currentOperationCount / totalOperations;
            progressBarFill.fillAmount = progress; // Remplissage de la barre (de 0 à 1)
            progressText.text = $"{(progress * 100):F0}%"; // Afficher le pourcentage
        }
        else
        {
            Debug.LogWarning("progressBarFill ou progressText non assigné dans l'Inspector !");
        }
    }

    /// <summary>
    /// Génère l'opération et crée les cases avec un point pour le résultat
    /// </summary>
    public void GenerateOperation()
    {
        // Vérifier si toutes les opérations sont terminées
        if (currentOperationCount >= totalOperations)
        {
            resultText.text = "Jeu terminé ! Toutes les opérations sont complétées !";
            resultText.color = Color.blue;
            Debug.Log("Jeu terminé !");
            return;
        }

        // Générer des nombres aléatoires
        number1 = Random.Range(10, 100);   // 2 chiffres
        number2 = Random.Range(10, 100);   // Jusqu'à 2 chiffres
        result = (number1 * number2).ToString();

        // Afficher les nombres, alignés à droite
        topNumberText.text = number1.ToString().PadLeft(result.Length, ' ');
        bottomNumberText.text = "× " + number2.ToString().PadLeft(result.Length - 2, ' ');

        // Vérifier si operationZone, plusSignPrefab et equalsSignPrefab sont assignés
        if (operationZone == null)
        {
            Debug.LogError("operationZone n'est pas assigné dans l'Inspector !");
            return;
        }
        if (plusSignPrefab == null)
        {
            Debug.LogError("plusSignPrefab n'est pas assigné dans l'Inspector !");
            return;
        }
        if (equalsSignPrefab == null)
        {
            Debug.LogError("equalsSignPrefab n'est pas assigné dans l'Inspector !");
            return;
        }

        // Nettoyer les anciens slots et zones
        foreach (Transform child in operationZone)
        {
            Destroy(child.gameObject);
        }

        // Désactiver tout composant de disposition qui pourrait interférer
        if (operationZone.GetComponent<HorizontalLayoutGroup>() != null)
        {
            Destroy(operationZone.GetComponent<HorizontalLayoutGroup>());
        }
        if (operationZone.GetComponent<VerticalLayoutGroup>() != null)
        {
            Destroy(operationZone.GetComponent<VerticalLayoutGroup>());
        }

        // Ajuster le RectTransform de la operationZone pour un alignement vertical
        RectTransform operationZoneRect = operationZone.GetComponent<RectTransform>();
        if (operationZoneRect != null)
        {
            operationZoneRect.anchorMin = new Vector2(0.5f, 1f); // Ancre en haut au centre
            operationZoneRect.anchorMax = new Vector2(0.5f, 1f);
            operationZoneRect.pivot = new Vector2(0.5f, 1f); // Pivot en haut au centre
            operationZoneRect.anchoredPosition = Vector2.zero;
            operationZoneRect.sizeDelta = new Vector2(result.Length * 160, 500); // Largeur dynamique, hauteur suffisante
        }

        // Calculer les résultats intermédiaires pour la multiplication verticale
        CalculateIntermediateResults();

        // Créer les zones pour les étapes intermédiaires
        float verticalOffset = 0f;
        for (int i = 0; i < intermediateResults.Length; i++)
        {
            GameObject intermediateZoneGO = new GameObject("IntermediateZone_" + i);
            intermediateZoneGO.transform.SetParent(operationZone, false);

            RectTransform zoneRect = intermediateZoneGO.AddComponent<RectTransform>();
            if (zoneRect != null)
            {
                zoneRect.anchorMin = new Vector2(0.5f, 1f); // Ancre en haut au centre
                zoneRect.anchorMax = new Vector2(0.5f, 1f);
                zoneRect.pivot = new Vector2(0.5f, 1f);
                zoneRect.anchoredPosition = new Vector2(0, verticalOffset); // Position verticale
                zoneRect.sizeDelta = new Vector2(result.Length * 160, 150); // Largeur selon le résultat final
                verticalOffset -= 120f; // Espacement vertical réduit pour rapprocher les lignes
            }

            // Générer les DigitSlots pour cette étape intermédiaire
            GenerateDigitSlots(intermediateZoneGO.transform, intermediateResults[i], i);

            // Instancier le prefab du signe "+" entre les étapes intermédiaires (sauf après la dernière)
            if (i < intermediateResults.Length - 1)
            {
                GameObject plusSignGO = Instantiate(plusSignPrefab, operationZone);
                plusSignGO.name = "PlusSign_" + i;

                RectTransform plusSignRect = plusSignGO.GetComponent<RectTransform>();
                if (plusSignRect != null)
                {
                    plusSignRect.anchorMin = new Vector2(0.5f, 1f); // Ancre au centre
                    plusSignRect.anchorMax = new Vector2(0.5f, 1f);
                    plusSignRect.pivot = new Vector2(0.5f, 0.5f); // Pivot au centre
                    float xOffset = -((result.Length * 160) / 2f) - 50f; // Juste à gauche des cases
                    float yOffset = verticalOffset - 60f; // Centré entre les deux lignes
                    plusSignRect.anchoredPosition = new Vector2(xOffset, yOffset);
                    plusSignRect.sizeDelta = new Vector2(50, 50);
                    TextMeshProUGUI plusText = plusSignGO.GetComponent<TextMeshProUGUI>();
                    if (plusText != null)
                    {
                        plusText.fontSize = 36;
                        plusText.color = new Color(0.2f, 0.2f, 0.2f);
                    }
                }
                verticalOffset -= 40f; // Espacement supplémentaire après le signe +
            }
        }

        // Ajouter le LineSeparator après les étapes intermédiaires
        GameObject separatorGO = new GameObject("LineSeparator");
        separatorGO.transform.SetParent(operationZone, false);
        RectTransform separatorRect = separatorGO.AddComponent<RectTransform>();
        if (separatorRect != null)
        {
            separatorRect.anchorMin = new Vector2(0.5f, 1f);
            separatorRect.anchorMax = new Vector2(0.5f, 1f);
            separatorRect.pivot = new Vector2(0.5f, 1f);
            separatorRect.anchoredPosition = new Vector2(0, verticalOffset - 40f); // Décalé plus bas
            separatorRect.sizeDelta = new Vector2(result.Length * 160, 2); // Largeur dynamique
            Image separatorImage = separatorGO.AddComponent<Image>();
            separatorImage.color = Color.black; // Couleur du séparateur
            verticalOffset -= 40f;
        }

        // Créer la zone pour le résultat final
        GameObject finalZoneGO = new GameObject("FinalResultZone");
        finalZoneGO.transform.SetParent(operationZone, false);
        RectTransform finalZoneRect = finalZoneGO.AddComponent<RectTransform>();
        if (finalZoneRect != null)
        {
            finalZoneRect.anchorMin = new Vector2(0.5f, 1f);
            finalZoneRect.anchorMax = new Vector2(0.5f, 1f);
            finalZoneRect.pivot = new Vector2(0.5f, 1f);
            finalZoneRect.anchoredPosition = new Vector2(0, verticalOffset - 20f); // Décalé plus bas
            finalZoneRect.sizeDelta = new Vector2(result.Length * 160, 150); // Largeur selon les DigitSlots
        }

        // Générer les DigitSlots pour le résultat final
        GenerateDigitSlots(finalZoneGO.transform, result, -1); // -1 indique pas d'étape intermédiaire

        // Ajouter le signe "=" en bas de tout, après la FinalResultZone
        GameObject equalsSignGO = Instantiate(equalsSignPrefab, operationZone);
        equalsSignGO.name = "EqualsSign";
        RectTransform equalsSignRect = equalsSignGO.GetComponent<RectTransform>();
        if (equalsSignRect != null)
        {
            equalsSignRect.anchorMin = new Vector2(0.5f, 1f); // Ancre au centre
            equalsSignRect.anchorMax = new Vector2(0.5f, 1f);
            equalsSignRect.pivot = new Vector2(0.5f, 0.5f); // Pivot au centre
            float xOffset = -((result.Length * 160) / 2f) - 50f; // Juste à gauche des cases
            float yOffset = verticalOffset - 60f; // En bas, après la FinalResultZone
            equalsSignRect.anchoredPosition = new Vector2(xOffset, yOffset);
            equalsSignRect.sizeDelta = new Vector2(50, 50);
            TextMeshProUGUI equalsText = equalsSignGO.GetComponent<TextMeshProUGUI>();
            if (equalsText != null)
            {
                equalsText.fontSize = 36;
                equalsText.color = new Color(0.2f, 0.2f, 0.2f);
            }
        }
        verticalOffset -= 40f; // Espacement supplémentaire après le signe =
    }

    /// <summary>
    /// Génère les DigitSlots pour une zone donnée
    /// </summary>
    private void GenerateDigitSlots(Transform parent, string targetResult, int stepIndex)
    {
        // Utiliser la longueur maximale pour toutes les étapes
        int slotCount = result.Length; // Alignement avec le résultat final
        int shift = (stepIndex > 0) ? stepIndex : 0; // Décalage pour les étapes après la première

        for (int i = 0; i < slotCount; i++)
        {
            GameObject slotGO = Instantiate(digitSlotPrefab, parent);
            slotGO.name = "DigitSlot_" + i;

            RectTransform slotRect = slotGO.GetComponent<RectTransform>();
            if (slotRect != null)
            {
                slotRect.anchorMin = new Vector2(0, 0.5f); // Ancre au centre gauche
                slotRect.anchorMax = new Vector2(0, 0.5f);
                slotRect.pivot = new Vector2(0, 0.5f);
                slotRect.sizeDelta = new Vector2(150, 150); // Taille fixe
                slotRect.localScale = Vector3.one; // Échelle à 1
                slotRect.anchoredPosition = new Vector2((slotCount - 1 - i) * 160, 0); // Alignement à droite
            }

            DigitSlot digitSlot = slotGO.GetComponent<DigitSlot>();
            if (digitSlot != null)
            {
                // À partir de la deuxième étape, la dernière case (à droite, index 0) doit avoir un point
                if (stepIndex > 0 && i == 0)
                {
                    digitSlot.slotText.text = ".";
                    digitSlot.slotText.color = Color.gray; // Différencier visuellement
                    digitSlot.GetComponent<Image>().raycastTarget = false; // Désactiver l'interaction
                }
                else
                {
                    digitSlot.slotText.text = ""; // Initialise vide pour drag-and-drop
                }
            }
            else
            {
                Debug.LogError($"Aucun composant DigitSlot trouvé sur {slotGO.name}");
            }
        }
    }

    /// <summary>
    /// Calcule les résultats intermédiaires pour la multiplication verticale
    /// </summary>
    private void CalculateIntermediateResults()
    {
        string num2Str = number2.ToString();
        intermediateResults = new string[num2Str.Length];

        // Remplir dans l'ordre inverse pour correspondre à l'affichage (dernier chiffre en premier)
        for (int i = 0; i < num2Str.Length; i++)
        {
            int digitIndex = num2Str.Length - 1 - i; // Inverser l'ordre des chiffres
            int digit = int.Parse(num2Str[digitIndex].ToString());
            int partialProduct = number1 * digit * (int)Mathf.Pow(10, i);
            intermediateResults[i] = partialProduct.ToString().PadLeft(result.Length, '0'); // Aligné à la longueur max
            Debug.Log($"Intermediate Result {i}: {intermediateResults[i]} (digit {digit} at position {digitIndex})");
        }
    }

    /// <summary>
    /// Vérifie si la réponse utilisateur est correcte et met à jour l'affichage
    /// </summary>
    [ContextMenu("Vérifier la réponse")]
    public bool IsAnswerCorrect()
    {
        string userAnswer = "";
        string[] userIntermediateResults = new string[intermediateResults.Length];

        // Vérifier les étapes intermédiaires
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
                        // Gérer le cas spécial pour la dernière case dans les étapes intermédiaires
                        if (i > 0 && slotIndex == 0 && digitText == ".")
                        {
                            digits.Add("0"); // Traiter le point comme 0
                        }
                        else
                        {
                            // Traiter les slots vides ou avec "." comme 0
                            digits.Add((digitText == "." || string.IsNullOrEmpty(digitText)) ? "0" : digitText);
                        }
                        slotIndex++;
                    }
                }

                // Inverser les chiffres pour lire de gauche à droite
                digits.Reverse();
                userIntermediateResults[i] = string.Join("", digits);

                // Supprimer les zéros initiaux et finaux pour comparer
                string expectedStep = intermediateResults[i].TrimStart('0').TrimEnd('0');
                string userStep = userIntermediateResults[i].TrimStart('0').TrimEnd('0');
                if (string.IsNullOrEmpty(expectedStep)) expectedStep = "0";
                if (string.IsNullOrEmpty(userStep)) userStep = "0";

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

        // Vérifier l'addition des étapes intermédiaires
        int sum = 0;
        for (int i = 0; i < userIntermediateResults.Length; i++)
        {
            if (int.TryParse(userIntermediateResults[i], out int stepValue))
            {
                Debug.Log($"Étape {i} - Valeur ajoutée à la somme : {stepValue}");
                sum += stepValue;
            }
        }

        // Vérifier la zone finale
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
                    digits.Add((digitText == "." || string.IsNullOrEmpty(digitText)) ? "0" : digitText);
                }
            }
            // Inverser les chiffres pour lire de gauche à droite
            digits.Reverse();
            userAnswer = string.Join("", digits);
            Debug.Log($"Résultat final - Chiffres collectés : {string.Join(", ", digits)}");
        }
        else
        {
            Debug.LogError("FinalResultZone non trouvé !");
        }

        // Supprimer les zéros initiaux pour le résultat final
        string trimmedUserAnswer = userAnswer.TrimStart('0');
        string trimmedResult = result.TrimStart('0');
        if (string.IsNullOrEmpty(trimmedUserAnswer)) trimmedUserAnswer = "0";
        if (string.IsNullOrEmpty(trimmedResult)) trimmedResult = "0";

        bool finalResultCorrect = trimmedUserAnswer == trimmedResult;
        bool isCorrect = allStepsCorrect && finalResultCorrect;

        // Met à jour le texte d'affichage
        if (resultText != null)
        {
            resultText.text = isCorrect ? "Correct !" : "Incorrect";
            resultText.color = isCorrect ? Color.green : Color.red;
        }
        else
        {
            Debug.LogWarning("resultText n'est pas assigné dans l'Inspector !");
        }

        // Jouer le son selon le résultat
        if (isCorrect && correctSound != null)
        {
            correctSound.Play();
        }
        else if (isCorrect && correctSound == null)
        {
            Debug.LogWarning("correctSound n'est pas assigné dans l'Inspector !");
        }
        else if (!isCorrect && incorrectSound != null)
        {
            incorrectSound.Play();
        }
        else if (!isCorrect && incorrectSound == null)
        {
            Debug.LogWarning("incorrectSound n'est pas assigné dans l'Inspector !");
        }

        return isCorrect;
    }

    /// <summary>
    /// Fonction wrapper pour appeler IsAnswerCorrect depuis l'UI et passer à l'opération suivante
    /// </summary>
    public void CheckAnswer()
    {
        // Vérifier la réponse
        bool isCorrect = IsAnswerCorrect();

        // Incrémenter le compteur d'opérations, qu'elles soient correctes ou non
        currentOperationCount++;

        // Mettre à jour la barre de progression
        UpdateProgressBar();

        // Générer une nouvelle opération ou terminer le jeu
        GenerateOperation();
    }
}