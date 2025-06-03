using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class MiniGameList : MonoBehaviour
{
    public List<MathoriaMiniGameWidget> miniGamesWidgets;

    public void AddMiniGame(MathoriaMiniGameWidget widget)
    {
        if (widget != null && !miniGamesWidgets.Contains(widget))
        {
            miniGamesWidgets.Add(widget);
        }
    }

    public void RemoveMiniGame(MathoriaMiniGameWidget widget)
    {
        if (widget != null && miniGamesWidgets.Contains(widget))
        {
            miniGamesWidgets.Remove(widget);
        }
    }
}
