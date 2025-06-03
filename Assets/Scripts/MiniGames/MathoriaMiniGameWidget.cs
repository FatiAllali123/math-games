using System;
using UnityEngine;

public class MathoriaMiniGameWidget : MonoBehaviour
{
    // Mini-game properties
    public MiniGameName miniGameName;

    // Player's score in the mini-game
    public int playerCorrectAnswers;

    // Success Criteria
    public MiniGameSuccessCriteria successCriteria;

    // Customization settings for the mini-game
    public MiniGameCustomization customizationSettings;

    // Starts the mini-game
    public virtual void StartMiniGame()
    {
        Debug.Log("Starting mini game!!!!!!!!!!");
        Debug.Log("Current Mini Game's name: " + miniGameName.ToString());
    }

    // Determines if the player succeeded in the mini-game
    public virtual bool CheckSuccess(int requiredCorrectAnswers)
    {
        return playerCorrectAnswers >= requiredCorrectAnswers;
    }

    // Set the mini-game customization
    public virtual void SetMiniGameCustomization(MiniGameCustomization customization)
    {
        // Use customization settings to adjust the mini-game behavior
        customizationSettings.numberRange = customization.numberRange;
        customizationSettings.numberOfOptions = customization.numberOfOptions;
        customizationSettings.numberOfQuestions = customization.numberOfQuestions;
        customizationSettings.timeLimit = customization.timeLimit;
        customizationSettings.duration = customization.duration;
    }

    // Set the mini-game success criteria
    public virtual void SetMiniGameSuccessCriteria(MiniGameSuccessCriteria successCriteria)
    {
        // Use success criteria settings to adjust the mini-game behavior
        this.successCriteria.requiredCorrectAnswers = successCriteria.requiredCorrectAnswers;
    }
}
