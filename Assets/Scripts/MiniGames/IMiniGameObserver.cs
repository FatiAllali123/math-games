using System;
using UnityEngine;

public interface IMiniGameObserver
{
    // Interface function to be implemented by any class that observes mini-games
    void OnMiniGameCompleted(MiniGameName miniGameName, bool result);
}
