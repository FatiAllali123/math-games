using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;
using TMPro; // Pour afficher dans l'UI (optionnel)



public class TestSelector : MonoBehaviour
{
    public TextMeshProUGUI debugText; // Optionnel, pour afficher l’état

    private DatabaseReference dbReference;

    void Start()
    {
        // Vérifie si un utilisateur est connecté
        if (UserSession.Instance == null || UserSession.Instance.CurrentUser == null)
        {
            Debug.LogError("Aucun utilisateur connecté !");
            if (debugText) debugText.text = "Aucun utilisateur connecté !";
            return;
        }

        // Init Firebase
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;

        // Cherche les tests
        LoadNextTestToPass(UserSession.Instance.CurrentUser.uid);
    }

    void LoadNextTestToPass(string uid)
    {
        dbReference.Child("users").Child(uid).Child("tests").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Erreur de lecture des tests.");
                if (debugText) debugText.text = "Erreur lors du chargement des tests.";
                return;
            }

            if (!task.Result.Exists)
            {
                Debug.Log("Aucun test assigné.");
                if (debugText) debugText.text = "Aucun test assigné.";
                return;
            }

            DataSnapshot testsSnapshot = task.Result;

            string nextTestId = null;

            foreach (var test in testsSnapshot.Children)
            {
                string testId = test.Key;
                string status = test.Child("status").Value?.ToString();

                if (status == "not_yet_passed")
                {
                    nextTestId = testId;
                    break; // prend le premier
                }
            }

            if (nextTestId != null)
            {
                Debug.Log("Prochain test à passer : " + nextTestId);
                if (debugText) debugText.text = "Prochain test : " + nextTestId;

                // Stocke le test dans une variable statique accessible partout
                TestSession.CurrentTestId = nextTestId;

              
                // ou SceneManager.LoadScene("MiniGameScene");

            }
            else
            {
                Debug.Log("Tous les tests sont déjà passés !");
                if (debugText) debugText.text = "Tous les tests sont complétés.";
            }
        });
    }
}
