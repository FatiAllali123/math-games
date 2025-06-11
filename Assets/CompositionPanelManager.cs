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

        // Position the panel
        RectTransform rt = newPanel.GetComponent<RectTransform>();
        rt.anchoredPosition = isFirstPanel ?
            firstPanelPosition :
            new Vector2(firstPanelPosition.x, firstPanelPosition.y - (panelsParent.childCount - 1) * verticalSpacing);

        // Initialize the panel
        var controller = newPanel.GetComponent<CompositionGameController>();
        if (controller != null)
        {
            if (isFirstPanel)
            {
                controller.InitializeProblem(); // First panel sets target
            }
            else
            {
                controller.ClearSlotsForNewComposition(); // Additional panels use same target
            }
        }

        // Add extra time for additional panels
        if (!isFirstPanel && TimeManager.Instance != null)
        {
            TimeManager.Instance.AddExtraTime();
        }

        // Update layout
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(panelsParent.GetComponent<RectTransform>());
    }

    // *** New: Clear extra panels ***
    public void ClearPanels()
    {
        for (int i = 1; i < panelsParent.childCount; i++) // Skip first panel
        {
            Destroy(panelsParent.GetChild(i).gameObject);
        }
    }
}