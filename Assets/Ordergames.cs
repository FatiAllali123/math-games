using System.Collections.Generic;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GroupChecker : MonoBehaviour
{
    public List<string> orderedMiniGames = new List<string>();
    public string userGroup = null;

    private DatabaseReference dbReference;

    void Start()
    {
        // Vérifiez que GameSession existe, sinon la crée
        if (GameSession.Instance == null)
        {
            var gameSessionObj = new GameObject("GameSession");
            gameSessionObj.AddComponent<GameSession>();
        }

        if (UserSession.Instance == null || UserSession.Instance.CurrentUser == null)
        {
            Debug.LogError("Aucun utilisateur connecté !");
            SceneManager.LoadScene("LoginScene");
            return;
        }

        if (string.IsNullOrEmpty(TestSession.CurrentTestId))
        {
            Debug.LogError("Aucun test sélectionné !");
            return;
        }

        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        LoadTestConfig(TestSession.CurrentTestId, UserSession.Instance.CurrentUser.uid);
    }

    void LoadTestConfig(string testId, string studentId)
    {
        dbReference.Child("tests").Child(testId).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Erreur de chargement: " + task.Exception);
                return;
            }

            if (!task.Result.Exists)
            {
                Debug.LogError("Test inexistant !");
                return;
            }

            DataSnapshot testSnapshot = task.Result;
            string foundGroup = null;

            // Recherche du groupe de l'étudiant
            if (testSnapshot.Child("miniGameConfigs").Exists)
            {
                foreach (var miniGame in testSnapshot.Child("miniGameConfigs").Children)
                {
                    var groupsConfig = miniGame.Child("groupsConfig");
                    if (!groupsConfig.Exists) continue;

                    foreach (var group in groupsConfig.Children)
                    {
                        var studentIds = group.Child("studentIds");
                        if (!studentIds.Exists) continue;

                        foreach (var id in studentIds.Children)
                        {
                            if (id.Value != null && id.Value.ToString() == studentId)
                            {
                                foundGroup = group.Key;
                                break;
                            }
                        }
                        if (foundGroup != null) break;
                    }
                    if (foundGroup != null) break;
                }
            }

            // Détermination de l'ordre des jeux
            if (foundGroup != null)
            {
                userGroup = foundGroup;
                Debug.Log("Groupe trouvé: " + userGroup);

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
                Debug.Log("Utilisation de l'ordre par défaut");
                var defaultOrder = testSnapshot.Child("miniGameOrder");
                if (defaultOrder.Exists)
                {
                    foreach (var miniGame in defaultOrder.Children)
                    {
                        orderedMiniGames.Add(miniGame.Value.ToString());
                    }
                }
            }

            // Initialisation de GameSession si nécessaire
            if (GameSession.Instance == null)
            {
                var gameSessionObj = new GameObject("GameSession");
                gameSessionObj.AddComponent<GameSession>();
            }

            // Sauvegarde des données
            GameSession.Instance.CurrentTestId = testId;
            GameSession.Instance.StudentGroup = userGroup;
            GameSession.Instance.MiniGameOrder = new List<string>(orderedMiniGames);

            // Chargement des configurations
            foreach (string gameName in orderedMiniGames)
            {
                var gameConfig = new Dictionary<string, object>();
                var gameSnapshot = testSnapshot.Child("miniGameConfigs").Child(gameName);

                if (!string.IsNullOrEmpty(userGroup))
                {
                    var groupConfig = gameSnapshot.Child("groupsConfig").Child(userGroup).Child("config");
                    if (groupConfig.Exists)
                    {
                        foreach (var field in groupConfig.Children)
                        {
                            gameConfig[field.Key] = field.Value;
                        }
                    }
                }

                if (gameConfig.Count == 0 && gameSnapshot.Child("gradeConfig").Child("config").Exists)
                {
                    var gradeConfig = gameSnapshot.Child("gradeConfig").Child("config");
                    foreach (var field in gradeConfig.Children)
                    {
                        gameConfig[field.Key] = field.Value;
                    }
                }

                if (gameConfig.Count > 0)
                {
                    GameSession.Instance.MiniGameConfigs[gameName] = gameConfig;
                    Debug.Log($"Config {gameName} chargée: {string.Join(", ", gameConfig)}");
                }
            }

            // Chargement de la scène de jeu
            if (orderedMiniGames.Count > 0)
            {
                Debug.Log("Redirection vers ChoiceScene");
                SceneManager.LoadScene("ChoiceScene");
            }
            else
            {
                Debug.LogError("Aucun mini-jeu configuré !");
            }
        });
    }
}