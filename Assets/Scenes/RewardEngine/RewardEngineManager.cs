using UnityEngine;
using com.glups.Reward;

public class RewardEngineManager : MonoBehaviour
{
    private RewardModel model;
    private TableController controller;

    void Start()
    {
        model = new RewardModel();
        var view = new View();
        controller = new TableController(model, view);

        controller.openLevel(1); // e.g. level 1
        controller.openTable(10, 0); // max score = 10, old max = 0
    }

    public void AddSuccess()
    {
        controller.addPositive(1);
    }

    public void AddFailure()
    {
        controller.addNegative(1);
    }

    public void EndSession()
    {
        int newScore = controller.closeTable();
        Debug.Log($"New high score for this table: {newScore}");
    }
}
