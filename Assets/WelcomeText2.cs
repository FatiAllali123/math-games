using TMPro;
using UnityEngine;

public class WelcomeText2 : MonoBehaviour
{
    public TMP_Text welcomeText;

    void Start()
    {
        // Vérifie que la session et le playerName existent
        if (UserSession.Instance != null &&
            UserSession.Instance.CurrentUser != null &&
            UserSession.Instance.CurrentUser.playerProfile != null &&
            !string.IsNullOrEmpty(UserSession.Instance.CurrentUser.playerProfile.playerName))
        {
            string playerName = UserSession.Instance.CurrentUser.playerProfile.playerName;
            welcomeText.text = $" Welcome, <color=#FFA500>{playerName}</color>! ";
            Debug.Log( playerName);
          
        }
        else
        {
            // Nom par défaut ou message d’erreur
            welcomeText.text = " Welcome, <color=#FFA500>Player</color>! ";
            Debug.LogWarning("Player name not found in UserSession.");
        }
    }
}
