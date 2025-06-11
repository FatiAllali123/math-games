using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MiniGameLoader : MonoBehaviour
{
    [SerializeField] private GroupChecker groupChecker;
    private bool isConfigLoaded = false;

    private void Start()
    {
        if (groupChecker == null)
        {
            groupChecker = FindObjectOfType<GroupChecker>();
            if (groupChecker == null)
            {
                Debug.LogError("GroupChecker not found in scene!");
                return;
            }
        }

        StartCoroutine(WaitForConfigAndLoad());
    }

    private IEnumerator WaitForConfigAndLoad()
    {
        while (groupChecker.orderedMiniGames.Count == 0)
        {
            Debug.Log("Waiting for GroupChecker to load mini-game order...");
            yield return new WaitForSeconds(0.1f);
        }

        if (groupChecker.orderedMiniGames.Count > 0)
        {
            string miniGameScene = groupChecker.orderedMiniGames[0];
            Debug.Log($"Loading mini-game scene: {miniGameScene}");
            try
            {
                SceneManager.LoadScene(miniGameScene);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load scene {miniGameScene}: {e.Message}");
            }
        }
        else
        {
            Debug.LogError("No mini-games found in orderedMiniGames!");
        }
    }
}