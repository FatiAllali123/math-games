using System.Collections.Generic;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement; // pour la gestion des scènes

public class GroupChecker : MonoBehaviour
{
    public List<string> orderedMiniGames = new List<string>();
    public string userGroup = null; // Peut rester null si pas de groupe

    private DatabaseReference dbReference;

    void Start()
    {
        if (UserSession.Instance == null || UserSession.Instance.CurrentUser == null)
        {
            Debug.LogError("Aucun utilisateur connect� !");
            return;
        }

        if (string.IsNullOrEmpty(TestSession.CurrentTestId))
        {
            Debug.LogError("Aucun test s�lectionn� !");
            return;
        }

        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        LoadTestConfig(TestSession.CurrentTestId, UserSession.Instance.CurrentUser.uid);
    }

    void LoadTestConfig(string testId, string studentId)
    {
        dbReference.Child("tests").Child(testId).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || !task.Result.Exists)
            {
                Debug.LogError("Erreur ou test inexistant !");
                return;
            }

            DataSnapshot testSnapshot = task.Result;
            string foundGroup = null;

            // V�rifie chaque mini-jeu
            if (testSnapshot.Child("miniGameConfigs").Exists)
            {
                foreach (var miniGame in testSnapshot.Child("miniGameConfigs").Children)
                {
                    var gameKey = miniGame.Key;
                    var groupsConfig = miniGame.Child("groupsConfig");
                    if (!groupsConfig.Exists) continue;

                    foreach (var group in groupsConfig.Children)
                    {
                        var groupName = group.Key;
                        var studentIds = group.Child("studentIds");
                        if (!studentIds.Exists) continue;

                        foreach (var id in studentIds.Children)
                        {
                            if (id.Value != null && id.Value.ToString() == studentId)
                            {
                                foundGroup = groupName;
                                break;
                            }
                        }

                        if (foundGroup != null) break;
                    }

                    if (foundGroup != null) break;
                }

            }

            // D�cider de l�ordre � suivre
            if (foundGroup != null)
            {
                userGroup = foundGroup;
                Debug.Log("L'�tudiant appartient au groupe : " + userGroup);

                var groupOrder = testSnapshot.Child("groupsMiniGameOrder").Child(userGroup);
                if (groupOrder.Exists)
                {
                    foreach (var miniGame in groupOrder.Children)
                    {
                        orderedMiniGames.Add(miniGame.Value.ToString());
                    }
                }
            }
            else
            {
                Debug.Log("L��tudiant n'appartient � aucun groupe, ordre standard sera utilis�.");
                var defaultOrder = testSnapshot.Child("miniGameOrder");
                if (defaultOrder.Exists)
                {
                    foreach (var miniGame in defaultOrder.Children)
                    {
                        orderedMiniGames.Add(miniGame.Value.ToString());
                    }
                }
            }

            Debug.Log("Ordre des jeux s�lectionn� : " + string.Join(", ", orderedMiniGames));

            //Charger les configurations pour chaque mini-jeu (apr�s avoir rempli la liste)
            foreach (string gameName in orderedMiniGames)
            {
                var gameSnapshot = testSnapshot.Child("miniGameConfigs").Child(gameName);
                Dictionary<string, object> config = new Dictionary<string, object>();

                if (userGroup != null && gameSnapshot.Child("groupsConfig").Child(userGroup).Child("config").Exists)
                {
                    foreach (var field in gameSnapshot.Child("groupsConfig").Child(userGroup).Child("config").Children)
                    {
                        config[field.Key] = field.Value;
                    }
                }
                else if (gameSnapshot.Child("gradeConfig").Child("config").Exists)
                {
                    foreach (var field in gameSnapshot.Child("gradeConfig").Child("config").Children)
                    {
                        config[field.Key] = field.Value;
                    }
                }
                else
                {
                    Debug.LogWarning($"Aucune configuration trouv�e pour {gameName}");
                    continue;
                }

                TestConfiguration.MiniGameConfigs[gameName] = config;
                Debug.Log($"Config charg�e pour {gameName}: {string.Join(", ", config)}");
            }
<<<<<<< HEAD

            GameSessionStatus.Initialize(orderedMiniGames);
            FindObjectOfType<MiniGameMenu>().ShowMenu(orderedMiniGames);
=======
            // Redirection vers la scène verticalOperationsScene
            SceneManager.LoadScene("VerticalOperationsScene");
>>>>>>> e4ad734cdbbd9e150e8622b464f8d28d32999e9b
        });


   
    }


}



