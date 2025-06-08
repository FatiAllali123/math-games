using UnityEngine;

public class UserSession : MonoBehaviour
{
    public static UserSession Instance { get; private set; }

    public UserData CurrentUser { get; private set; }

    private void Awake()
    {
        // Singleton pattern : on d�truit les doublons
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Garde ce GameObject entre les sc�nes
    }

    // M�thode pour d�finir les donn�es user apr�s chargement Firebase
    public void SetUserData(UserData user)
    {
        CurrentUser = user;
        Debug.Log($"UserSession: data loaded for {user.firstName} {user.lastName}");
    }
}
