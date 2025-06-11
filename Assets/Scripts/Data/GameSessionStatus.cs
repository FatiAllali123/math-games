using System.Collections.Generic;
using UnityEngine;

public static class GameSessionStatus
{
    public static Dictionary<string, string> gameStatus = new Dictionary<string, string>();

    public static void Initialize(List<string> miniGames)
    {
        gameStatus.Clear();
        foreach (string game in miniGames)
        {
            gameStatus[game] = "non_commencé"; // valeurs possibles : non_commencé, en_cours, terminé
        }
    }

    public static void SetStatus(string game, string status)
    {
        if (gameStatus.ContainsKey(game))
            gameStatus[game] = status;
    }

    public static string GetStatus(string game)
    {
        return gameStatus.ContainsKey(game) ? gameStatus[game] : "non_commencé";
    }

    public static string GetNextPlayableGame()
    {
        foreach (var pair in gameStatus)
        {
            if (pair.Value != "terminé")
                return pair.Key;
        }
        return null; // tous terminés
    }
}
