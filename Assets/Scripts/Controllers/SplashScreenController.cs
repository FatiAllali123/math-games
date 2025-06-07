using UnityEngine;
using UnityEngine.SceneManagement; // pour changer de sc�ne 
using Firebase.Auth;
using Firebase.Extensions;
using Firebase;


public class SplashScreenController : MonoBehaviour //h�rite de MonoBehaviour, donc je peux l�attacher � un objet dans Unity
{
    public string firebasePlayerId; // stocker l�ID du joueur connect� avec Firebase.
    [SerializeField] private FirebasePlayerDataManager firebasePlayerDataManager; // lien vers un autre script qui s�occupe de charger les donn�es du joueur , SerializeField permet de le voir dans l'inspecteur Unity, m�me s�il est private
    // des �l�ments visuels (panneau de login, texte de chargement).
    [SerializeField] private GameObject authenticationPanel; // panneau login  
    [SerializeField] private GameObject loadingText; // texte loading 

    void Start()
    {
        // FirebaseInitializer est un script singleton qui g�re la configuration Firebase.
        FirebaseInitializer.Instance.InitializeFirebase(() =>
        {
            if (FirebaseInitializer.Instance.IsFirebaseInitialized) // si Firebase est bien initialis�.
            {
                FirebaseUser currentUser = FirebaseInitializer.Instance.Auth.CurrentUser; // On r�cup�re l�utilisateur actuellement connect� (ou null s�il n�y a personne)

                if (currentUser != null)
                {
                    string firebasePlayerId = currentUser.UserId; //prend ID Firebase du  user 
                    Debug.Log("User already signed in: " + firebasePlayerId); // message console 

                    firebasePlayerDataManager.LoadPlayerData(firebasePlayerId, OnPlayerDataLoaded); // charge ses donn�es enregistr�es (nom, score, etc.) via firebasePlayerDataManager
                }
                else
                {
                    Debug.Log("No user is currently signed in."); // message console 
                    ShowAuthenticationWidget();// l��cran d'auth
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
            ShowAuthenticationWidget(); // on remontre l��cran de login
        }
    }


    //  Afficher l��cran de login
    private void ShowAuthenticationWidget()
    {
        if (loadingText != null) // masque le texte de chargement si jamais il existe

            loadingText.SetActive(false);

        Debug.Log("Redirecting to authentication screen.");
        authenticationPanel.SetActive(true);
    }
}
