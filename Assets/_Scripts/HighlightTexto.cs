using UnityEngine;
using UnityEngine.EventSystems; // Necessário para detectar o mouse
using TMPro; // Necessário para TextMeshPro

public class HighlightTexto : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Color corDestaque = Color.yellow; // Escolha a cor no Inspector
    private Color corOriginal;
    private TextMeshProUGUI texto;

    void Awake()
    {
        texto = GetComponent<TextMeshProUGUI>();
        corOriginal = texto.color;
    }

    // Executa quando o mouse entra na área do texto
    public void OnPointerEnter(PointerEventData eventData)
    {
        texto.color = corDestaque;
    }

    // Executa quando o mouse sai da área do texto
    public void OnPointerExit(PointerEventData eventData)
    {
        texto.color = corOriginal;
    }
}