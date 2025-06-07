using UnityEngine;
using UnityEngine.UI;

public class CompositionPanelManager : MonoBehaviour
{
    public GameObject compositionPanelPrefab;
    public Transform panelsParent;
    public Button addCompositionButton;
    public float verticalSpacing = 200f;
    public Vector2 firstPanelPosition = new Vector2(0, 400f);

    private void Start()
    {
        addCompositionButton.onClick.AddListener(() => AddNewCompositionPanel(false));

       
    }

    public void AddNewCompositionPanel(bool isFirstPanel)
    {
        GameObject newPanel = Instantiate(compositionPanelPrefab, panelsParent);

        // Position the panel vertically
        RectTransform rt = newPanel.GetComponent<RectTransform>();
        rt.anchoredPosition = isFirstPanel ?
            firstPanelPosition :
            new Vector2(firstPanelPosition.x, firstPanelPosition.y - (panelsParent.childCount - 1) * verticalSpacing);

        // Initialize the panel with the shared target number
        var controller = newPanel.GetComponent<CompositionGameController>();
        if (controller != null)
        {
            controller.InitializeProblem();
        }

        // Force UI layout update
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(panelsParent.GetComponent<RectTransform>());
    }
}