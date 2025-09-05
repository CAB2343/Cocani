using UnityEngine;
using UnityEngine.UI;

public class CellController : MonoBehaviour
{
    public Text label;            // arraste o Text do prefab
    public Image background;      // arraste o Image do prefab
    [HideInInspector] public bool locked = false;   // se true, não muda
    [HideInInspector] public char currentChar;

    Color normalColor = new Color(1f,1f,1f,1f);
    Color selectedColor = new Color(0.9f, 0.9f, 0.4f, 1f);
    Color lockedColor = new Color(0.8f, 1f, 0.8f, 1f);

    void Reset() {
        if (label == null) label = GetComponentInChildren<Text>();
        if (background == null) background = GetComponent<Image>();
    }

    public void SetChar(char c, bool keepLocked = false)
    {
        currentChar = c;
        if (label != null) label.text = c.ToString();
        if (keepLocked) locked = true;
        UpdateVisual();
    }

    public void SetLocked(bool b)
    {
        locked = b;
        UpdateVisual();
    }

    public void SetSelected(bool sel)
    {
        if (background == null) return;
        if (locked)
            background.color = lockedColor;
        else
            background.color = sel ? selectedColor : normalColor;
    }

    void UpdateVisual()
    {
        if (background == null) return;
        background.color = locked ? lockedColor : normalColor;
        if (label != null) label.text = currentChar.ToString();
    }
}
