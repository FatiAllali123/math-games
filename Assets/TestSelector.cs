using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;
using TMPro; // Pour afficher dans l'UI (optionnel)



public class TestSelector : MonoBehaviour
{
    public TextMeshProUGUI debugText; // Optionnel, pour afficher l��tat

    private DatabaseReference dbReference;

    void Start()
    {
        // V�rifie si un utilisateur est connect�
        if (UserSession.Instance == null || UserSession.Instance.CurrentUser == null)
        {
            Debug.LogError("Aucun utilisateur connect� !");
            if (debugText) debugText.text = "Aucun utilisateur connect� !";
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
                Debug.Log("Aucun test assign�.");
                if (debugText) debugText.text = "Aucun test assign�.";
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
                Debug.Log("Prochain test � passer : " + nextTestId);
                if (debugText) debugText.text = "Prochain test : " + nextTestId;

                // Stocke le test dans une variable statique accessible partout
                TestSession.CurrentTestId = nextTestId;

              
                // ou SceneManager.LoadScene("MiniGameScene");

            }
            else
            {
                Debug.Log("Tous les tests sont d�j� pass�s !");
                if (debugText) debugText.text = "Tous les tests sont compl�t�s.";
            }
        });
    }
}
