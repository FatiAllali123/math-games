using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MiniGameCustomization
{
    // Number of operations/questions in this mini game
    public int numberOfQuestions;

    // Adjust number ranges based on the grade level
    public int numberRange;

    // Number of options to choose from
    public int numberOfOptions;

    // Duration of the mini game in seconds
    public float duration;

    // Time limit per operation/question
    public float timeLimit;

    // List of allowed operations (lower grades focus on addition & subtraction, higher grades include multiplication and division)
    public List<OperationType> operationsAllowed;
}

