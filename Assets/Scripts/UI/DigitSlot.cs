using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DigitSlot : MonoBehaviour, IDropHandler
{
    public Text slotText;

    void Start()
    {
        if (slotText == null)
            Debug.LogWarning("DigitSlot: slotText is not assigned!", this);
        else
            slotText.text = "";
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("Drop detected on: " + gameObject.name);

        string symbol = GhostButtonController.Instance.CurrentSymbol;
        if (!string.IsNullOrEmpty(symbol))
        {
            slotText.text = symbol;
            Debug.Log("Symbol Set: " + symbol);
        }
    }

}
