using UnityEngine;

public class UserSession : MonoBehaviour
{
    public static UserSession Instance { get; private set; }

    public UserData CurrentUser { get; private set; }

    private void Awake()
    {
        // Singleton pattern : on détruit les doublons
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Garde ce GameObject entre les scènes
    }

    // Méthode pour définir les données user après chargement Firebase
    public void SetUserData(UserData user)
    {
        CurrentUser = user;
        Debug.Log($"UserSession: data loaded for {user.firstName} {user.lastName}");
    }
}
