using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events; // Add this namespace

public class CompositionPanelManager : MonoBehaviour
{
    public GameObject compositionPanelPrefab;
    public Transform panelsParent;
    public Button addCompositionButton;
    public float verticalSpacing = 200f;
    public Vector2 panelPosition = new Vector2(0, -100f);

    private void Start()
    {
        // Fix: Use proper method reference
        addCompositionButton.onClick.AddListener(() => AddNewCompositionPanel());

        // Initialize first panel
        AddNewCompositionPanel(true);
    }

    public void AddNewCompositionPanel(bool isFirstPanel = false)
    {
        GameObject newPanel = Instantiate(compositionPanelPrefab, panelsParent);

        RectTransform rt = newPanel.GetComponent<RectTransform>();
        rt.anchoredPosition = isFirstPanel ? panelPosition :
            new Vector2(panelPosition.x, panelPosition.y - (panelsParent.childCount - 1) * verticalSpacing);

        CompositionGameController controller = newPanel.GetComponentInChildren<CompositionGameController>(true);
        if (controller != null)
        {
            controller.Initialize();
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(panelsParent.GetComponent<RectTransform>());
    }
}