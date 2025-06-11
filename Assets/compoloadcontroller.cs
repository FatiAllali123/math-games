using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Firebase.Database;

public class CompositionConfigLoader : MonoBehaviour
{
    [System.Serializable]
    public class CompositionConfig
    {
        public int maxNumberRange = 4;
        public int minNumCompositions = 5;
        public int requiredCorrectPercent = 75;
        public string operation = "Multiplication";
    }

    public static CompositionConfig CurrentConfig { get; private set; }
    public static bool IsLoaded { get; private set; }

    private void Start()
    {
        StartCoroutine(LoadConfigCoroutine());
    }

    private IEnumerator LoadConfigCoroutine()
    {
        // Option 1: Try to get config from existing GroupChecker
        if (TryGetConfigFromGroupChecker(out var config))
        {
            CurrentConfig = config;
            IsLoaded = true;
            yield break;
        }

        // Option 2: Fallback - load directly from Firebase
        yield return LoadConfigDirectlyFromFirebase();
    }

    private bool TryGetConfigFromGroupChecker(out CompositionConfig config)
    {
        config = new CompositionConfig();

        var groupChecker = FindObjectOfType<GroupChecker>();
        if (groupChecker == null) return false;

        const string targetGame = "find_compositions";

        if (!TestConfiguration.MiniGameConfigs.ContainsKey(targetGame))
            return false;

        var firebaseConfig = TestConfiguration.MiniGameConfigs[targetGame];

        try
        {
            config.maxNumberRange = int.Parse(firebaseConfig["maxNumberRange"].ToString());
            config.minNumCompositions = int.Parse(firebaseConfig["minNumCompositions"].ToString());
            config.requiredCorrectPercent = int.Parse(firebaseConfig["requiredCorrectAnswersMinimumPercent"].ToString());
            config.operation = firebaseConfig["operation"].ToString();
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error parsing config: {e.Message}");
            return false;
        }
    }

    private IEnumerator LoadConfigDirectlyFromFirebase()
    {
        if (string.IsNullOrEmpty(TestSession.CurrentTestId))
        {
            Debug.LogError("No test ID available!");
            yield break;
        }

        var dbRef = FirebaseDatabase.DefaultInstance.GetReference(
            $"tests/{TestSession.CurrentTestId}/miniGameConfigs/find_compositions");

        var task = dbRef.GetValueAsync();

        yield return new WaitUntil(() => task.IsCompleted);

        if (task.IsFaulted)
        {
            Debug.LogError("Failed to load config from Firebase");
            yield break;
        }

        if (!task.Result.Exists)
        {
            Debug.LogError("No configuration found for find_compositions");
            yield break;
        }

        DataSnapshot snapshot = task.Result;
        CurrentConfig = new CompositionConfig();

        // Check if we have a group from GroupChecker
        GroupChecker groupChecker = FindObjectOfType<GroupChecker>();
        string userGroup = groupChecker?.userGroup;

        // Check group config first if available
        if (!string.IsNullOrEmpty(userGroup) &&
            snapshot.Child("groupsConfig").Child(userGroup).Exists)
        {
            var groupConfig = snapshot.Child("groupsConfig").Child(userGroup).Child("config");
            LoadConfigFromSnapshot(groupConfig);
        }
        // Fallback to grade config
        else if (snapshot.Child("gradeConfig").Child("config").Exists)
        {
            var gradeConfig = snapshot.Child("gradeConfig").Child("config");
            LoadConfigFromSnapshot(gradeConfig);
        }

        IsLoaded = true;
    }

    private void LoadConfigFromSnapshot(DataSnapshot configSnapshot)
    {
        foreach (var field in configSnapshot.Children)
        {
            switch (field.Key)
            {
                case "maxNumberRange":
                    CurrentConfig.maxNumberRange = int.Parse(field.Value.ToString());
                    break;
                case "minNumCompositions":
                    CurrentConfig.minNumCompositions = int.Parse(field.Value.ToString());
                    break;
                case "requiredCorrectAnswersMinimumPercent":
                    CurrentConfig.requiredCorrectPercent = int.Parse(field.Value.ToString());
                    break;
                case "operation":
                    CurrentConfig.operation = field.Value.ToString();
                    break;
            }
        }
    }
}