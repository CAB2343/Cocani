using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SliderHighlight : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    public Image background; // arraste aqui a imagem do fundo do slider
    public Color normalColor = Color.white;
    public Color selectedColor = Color.yellow;

    public void OnSelect(BaseEventData eventData)
    {
        if (background != null)
            background.color = selectedColor;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (background != null)
            background.color = normalColor;
    }
}
