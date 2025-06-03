using System;
using UnityEngine;


public class MiniGameFactory : MonoBehaviour
{
    // Singleton instance of MiniGameFactory
    private static MiniGameFactory instance;

    // Singleton pattern for accessing the factory instance
    public static MiniGameFactory Get()
    {
        if (instance == null)
        {
            instance = new GameObject("MiniGameFactory").AddComponent<MiniGameFactory>();
        }
        return instance;
    }

    // Method to create mini-game widgets based on MiniGameName
    public MathoriaMiniGameWidget CreateMiniGame(MiniGameName miniGameName, MiniGameCustomization customization, MiniGameSuccessCriteria successCriteria)
    {
        MathoriaMiniGameWidget newMiniGame = null;

        // Instantiate the corresponding mini-game based on the MiniGameName
        switch (miniGameName)
        {
            case MiniGameName.FindPreviousNextNumber:
                newMiniGame = new FindPreviousNextNumberMiniGame();
                break;

            case MiniGameName.TapMatchingPairs:
                newMiniGame = new TapMatchingPairsMiniGame();
                break;

            case MiniGameName.OrderNumbers:
                newMiniGame = new OrderNumbersMiniGame();
                break;

            case MiniGameName.CompareNumbers:
                newMiniGame = new CompareNumbersMiniGame();
                break;

            case MiniGameName.WhatNumberDoYouHear:
                newMiniGame = new WhatNumberDoYouHearMiniGame();
                break;

            case MiniGameName.DecomposeNumber:
                newMiniGame = new DecomposeNumberMiniGame();
                break;

            case MiniGameName.WriteNumberInLetters:
                newMiniGame = new WriteNumberInLettersMiniGame();
                break;

            case MiniGameName.IdentifyPlaceValues:
                newMiniGame = new IdentifyPlaceValuesMiniGame();
                break;

            case MiniGameName.ReadNumberAloud:
                newMiniGame = new ReadNumberAloudMiniGame();
                break;

            case MiniGameName.FindCompositions:
                newMiniGame = new FindCompositionsMiniGame();
                break;

            case MiniGameName.SolveOperationVertically:
                newMiniGame = new SolveOperationVerticallyMiniGame();
                break;

            case MiniGameName.ChooseRightAnswer:
                newMiniGame = new ChooseRightAnswerMiniGame();
                break;

            case MiniGameName.SolveMultiStepProblem:
                newMiniGame = new SolveMultiStepProblemMiniGame();
                break;

            default:
                return null;
        }

        // Pass the customization settings and success criteria to the mini-game widget
        newMiniGame?.SetMiniGameCustomization(customization);
        newMiniGame?.SetMiniGameSuccessCriteria(successCriteria);

        return newMiniGame;
    }
}

