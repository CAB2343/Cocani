using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Cell : MonoBehaviour
{
    public TMP_Text label;
    public Image background;
    [HideInInspector] public int row;
    [HideInInspector] public int col;
    [HideInInspector] public bool isStatic;

    public void SetChar(char c)
    {
        if (label != null) label.text = c.ToString();
    }

    public void SetVisible(bool v)
    {
        if (label != null) label.enabled = v;
        if (background != null) background.enabled = v;
    }
}
