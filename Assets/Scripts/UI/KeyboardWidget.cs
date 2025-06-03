using UnityEngine;
using UnityEngine.UI;

public class KeyboardWidget : MonoBehaviour
{
    public GridLayoutGroup keyboardGrid;
    public GameObject buttonPrefab;
    public string[] symbols;

    // Start is called before the first frame update
    void Start()
    {
        PopulateKeyboard();
    }

    // Populate the grid with buttons
    void PopulateKeyboard()
    {
        if (keyboardGrid == null || buttonPrefab == null) return;

        if (symbols == null || symbols.Length == 0)
        {
            // Default symbols if none are provided
            symbols = new string[] {
                "1", "2", "3",
                "4", "5", "6",
                "7", "8", "9",
                "0", ".", "÷",
                "×", "-", "+"
            };
        }

        foreach (string symbol in symbols)
        {
            // Instantiate a new button
            GameObject newButton = Instantiate(buttonPrefab, keyboardGrid.transform);
            KeyboardButton keyboardButton = newButton.GetComponent<KeyboardButton>();
            if (keyboardButton != null)
            {
                keyboardButton.symbol = symbol; // Set the button symbol
            }
        }
    }
}
