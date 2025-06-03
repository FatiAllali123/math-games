using UnityEngine;

[System.Serializable] 
public class MiniGameSuccessCriteria
{
    // The number of correct answers required for success
    public int requiredCorrectAnswers;

    // Whether the player has completed this mini-game successfully
    public bool completed = false;
}