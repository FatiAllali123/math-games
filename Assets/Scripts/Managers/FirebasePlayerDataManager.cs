using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

public class FirebasePlayerDataManager : MonoBehaviour
{
    private DatabaseReference playerRef;

    private void Start()
    {
    }

    private void InitializeDatabaseReference(string firebasePlayerId)
    {
        playerRef = FirebaseDatabase.DefaultInstance.GetReference("users").Child(firebasePlayerId);
        Debug.Log($"Initialized player reference for: {firebasePlayerId}");
    }

    public void SavePlayerData(string firebasePlayerId, PlayerProfile profile)
    {
        // Ensure Firebase is initialized before performing database operations
        if (FirebaseInitializer.Instance.IsFirebaseInitialized)
        {
            InitializeDatabaseReference(firebasePlayerId);

            string json = JsonUtility.ToJson(profile);
            playerRef.Child("playerProfile").SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    Debug.Log("Player data saved successfully!");
                }
                else
                {
                    Debug.LogError("Failed to save player data: " + task.Exception);
                }
            });
        }
        else
        {
            Debug.LogError("Firebase not initialized yet.");
        }
    }

    public void LoadPlayerData(string firebasePlayerId, System.Action<PlayerProfile> onComplete)
    {
        // Ensure Firebase is initialized before performing database operations
        if (FirebaseInitializer.Instance.IsFirebaseInitialized)
        {
            InitializeDatabaseReference(firebasePlayerId);

            playerRef.GetValueAsync().ContinueWithOnMainThread(task => {
                if (task.IsCompletedSuccessfully && task.Result.Exists)
                {
                    string json = task.Result.GetRawJsonValue();
                    UserData user = JsonUtility.FromJson<UserData>(json);
                    Debug.Log("Player data loaded.");
                    onComplete?.Invoke(user.playerProfile);
                }
                else
                {
                    Debug.LogError("Failed to load player data: " + task.Exception);
                    onComplete?.Invoke(null);
                }
            });
        }
        else
        {
            Debug.LogError("Firebase not initialized yet.");
        }
    }
}
