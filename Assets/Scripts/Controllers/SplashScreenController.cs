using UnityEngine;
using UnityEngine.SceneManagement; // pour changer de scène 
using Firebase.Auth;
using Firebase.Extensions;
using Firebase;


public class SplashScreenController : MonoBehaviour //hérite de MonoBehaviour, donc je peux l’attacher à un objet dans Unity
{
    public string firebasePlayerId; // stocker l’ID du joueur connecté avec Firebase.
    [SerializeField] private FirebasePlayerDataManager firebasePlayerDataManager; // lien vers un autre script qui s’occupe de charger les données du joueur , SerializeField permet de le voir dans l'inspecteur Unity, même s’il est private
    // des éléments visuels (panneau de login, texte de chargement).
    [SerializeField] private GameObject authenticationPanel; // panneau login  
    [SerializeField] private GameObject loadingText; // texte loading 

    void Start()
    {
        // FirebaseInitializer est un script singleton qui gère la configuration Firebase.
        FirebaseInitializer.Instance.InitializeFirebase(() =>
        {
            if (FirebaseInitializer.Instance.IsFirebaseInitialized) // si Firebase est bien initialisé.
            {
                FirebaseUser currentUser = FirebaseInitializer.Instance.Auth.CurrentUser; // On récupère l’utilisateur actuellement connecté (ou null s’il n’y a personne)

                if (currentUser != null)
                {
                    string firebasePlayerId = currentUser.UserId; //prend ID Firebase du  user 
                    Debug.Log("User already signed in: " + firebasePlayerId); // message console 

                    firebasePlayerDataManager.LoadPlayerData(firebasePlayerId, OnPlayerDataLoaded); // charge ses données enregistrées (nom, score, etc.) via firebasePlayerDataManager
                }
                else
                {
                    Debug.Log("No user is currently signed in."); // message console 
                    ShowAuthenticationWidget();// l’écran d'auth
                }
            }
        });
    }

    // apres que  les donnees du user  
    private void OnPlayerDataLoaded(PlayerProfile profile)
    {
        if (profile != null)
        {
            Debug.Log("Player data loaded successfully.");
            SceneManager.LoadScene("TestScene"); // on passe a a scene TestScene
        }
        else
        {
            Debug.Log("Player data load failed.");
            ShowAuthenticationWidget(); // on remontre l’écran de login
        }
    }


    //  Afficher l’écran de login
    private void ShowAuthenticationWidget()
    {
        if (loadingText != null) // masque le texte de chargement si jamais il existe

            loadingText.SetActive(false);

        Debug.Log("Redirecting to authentication screen.");
        authenticationPanel.SetActive(true);
    }
}
