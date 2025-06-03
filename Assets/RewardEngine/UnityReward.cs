using System;
using com.glups.Reward;

public class UnityReward
{

    // Main to test the new controller.
    // Ce qui doit etre stocké dans l'objet permant, c'est le Model est non le controller.
    // Le modèle est comme la base de données. Il est permanant au jeu.
    // Une scene crée un controleur pour travailler avec le modèle, ensuite le détruit.
    // Le code suivant simule ce comportement
    // la méthode startScene est à rattaché à la scene (avec le bon code Unity), pour démarer.
    // Dans mon exemple, je passe view. Dans unity, ce n'est pas la peine.

    internal static RewardModel theModel = null!;

    public static void playTable(TableController controller)
    {

        // ouvrir la table. Normalement, il faut récupérer l'ancien score attaché à la table et l'ouvrir avec.
        long randomTableOldScore = (long)Math.Round(Math.Floor(MathHelper.NextDouble * 10), MidpointRounding.AwayFromZero);
        controller.openTable(10, (int)randomTableOldScore);

        // cette boucle simule les réponses du joueur.
        for (int i = 0; i <= 10; i++)
        {
            long randomResult = (long)Math.Round(MathHelper.NextDouble, MidpointRounding.AwayFromZero); // random prendra les valeurs 0 et 1
            if (randomResult == 1)
            {
                controller.addPositive(1);
            }
            else
            {
                controller.addNegative(1);
            }
            controller.traceModel(123456, 87654321);
        }

        // fermer la table et récupérer le nouveau score.
        // sauvegarder le score
        int tableNewScore = controller.closeTable();

        // sauvegarder le tableNewScore

    }

    public static void startScene(View view, int level)
    {
        // créer un controller local à la scéne. Mais il référence le module unique du jeu.
        TableController controller = new TableController(UnityReward.theModel, view);

        // Ouvrir le level.
        controller.openLevel(level);

        // la premiere boucle simule le joueur qui ouvre des tables
        for (int table = 0; table <= 10; table++)
        {
            playTable(controller);
        }
    }

    public static void Main(string[] args)
    {
        theModel = new RewardModel();
        View view = new View();

        UnityReward.startScene(view, 1);

        UnityReward.startScene(view, 2);
    }
}