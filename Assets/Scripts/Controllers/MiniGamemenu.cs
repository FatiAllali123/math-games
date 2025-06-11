using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGameMenu : MonoBehaviour
{
    public GameObject miniGameButtonPrefab;
    public Transform buttonContainer;


    private IEnumerator PulseIcon(UnityEngine.UI.Image iconImage)
    {
        float pulseSpeed = 2f;
        float scaleAmount = 0.1f;

        Vector3 originalScale = iconImage.rectTransform.localScale;

        while (true)
        {
            float scale = 1 + Mathf.Sin(Time.time * pulseSpeed) * scaleAmount;
            iconImage.rectTransform.localScale = originalScale * scale;
            yield return null;
        }
    }




    public void ShowMenu(List<string> scenesToLoad)
    {


        string nextGame = GameSessionStatus.GetNextPlayableGame();


        Debug.Log("Nombre de jeux reçus dans ShowMenu: " + scenesToLoad.Count);
        Debug.Log("Liste des jeux : " + string.Join(", ", scenesToLoad));

        foreach (var originalSceneName in scenesToLoad)
        {
            string sceneName = originalSceneName; // copie locale modifiable

            try
            {
                Debug.Log("Tentative d'instanciation pour : " + sceneName);
                Sprite icon = Resources.Load<Sprite>("MiniGameIcons/" + sceneName);

                Debug.Log("Chargement sprite de " + sceneName + " => " + (icon != null ? "SUCCÈS" : "ÉCHEC"));

                if (icon == null)
                {
                    Debug.LogWarning("Image non trouvée pour : " + sceneName);
                    continue;
                }

                GameObject btn = Instantiate(miniGameButtonPrefab, buttonContainer);
                MiniGameButton controller = btn.GetComponent<MiniGameButton>();

                // mapping des noms
                if (sceneName == "find_compositions")
                {
                    sceneName = "Compositionscene";
                }
                else if (sceneName == "vertical_operations")
                {
                    sceneName = "VerticalOperationScenee";
                }
                else if (sceneName == "choose_answer")
                {
                    sceneName = "ChoiceScene";
                }

                controller.Setup(icon, sceneName);







                if (originalSceneName != nextGame)
                {
                    var button = btn.GetComponent<UnityEngine.UI.Button>();
                    button.interactable = false;

                    if (controller != null && controller.iconImage != null)
                    {
                        // Assombrir seulement l'icône
                        var iconImage = controller.iconImage;
                        iconImage.color = new Color(iconImage.color.r, iconImage.color.g, iconImage.color.b, 0.4f); // Opacité réduite

                        // Ajouter une ombre directement sur l'icône (optionnel)
                        var existingShadow = iconImage.GetComponent<UnityEngine.UI.Shadow>();
                        if (existingShadow == null)
                        {
                            UnityEngine.UI.Shadow shadow = iconImage.gameObject.AddComponent<UnityEngine.UI.Shadow>();
                            shadow.effectColor = new Color(0, 0, 0, 0.5f); // Ombre douce
                            shadow.effectDistance = new Vector2(2, -2);
                        }
                    }


                }

                if (originalSceneName == nextGame)
                {
                    if (controller != null && controller.iconImage != null)
                    {
                        StartCoroutine(PulseIcon(controller.iconImage));
                    }
                }


            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Erreur lors de l'ajout du mini-jeu {sceneName} : {ex.Message}\n{ex.StackTrace}");
            }
        }





    }






}