// Cell.cs
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Cell: representa uma célula do grid.
/// - currentChar: valor persistente da célula (SetChar/GetChar).
/// - overlayChar: valor temporário desenhado sobre a célula sem alterar currentChar (SetOverlayChar/ClearOverlay).
/// - ApplyLabelText garante que '\0' nunca apareça visivelmente.
/// </summary>
public class Cell : MonoBehaviour
{
    public TMP_Text label;
    public Image background;

    [HideInInspector] public int row;
    [HideInInspector] public int col;
    [HideInInspector] public bool isStatic;

    private char currentChar = '\0';

    // overlay (visual temporário)
    private char overlayChar = '\0';
    private bool hasOverlay = false;

    /// <summary>
    /// Define o char persistente desta célula.
    /// </summary>
    public void SetChar(char c)
    {
        currentChar = c;
        ApplyLabelText();
    }

    /// <summary>
    /// Retorna o char persistente da célula.
    /// </summary>
    public char GetChar()
    {
        return currentChar;
    }

    /// <summary>
    /// Define um char como overlay (apenas visual). Não altera o currentChar.
    /// Use ClearOverlay() para voltar a exibir o currentChar.
    /// </summary>
    public void SetOverlayChar(char c)
    {
        overlayChar = c;
        hasOverlay = (c != '\0');
        ApplyLabelText();
    }

    /// <summary>
    /// Remove o overlay e volta a mostrar o char persistente.
    /// </summary>
    public void ClearOverlay()
    {
        overlayChar = '\0';
        hasOverlay = false;
        ApplyLabelText();
    }

    /// <summary>
    /// Controla se label/background estão visíveis.
    /// </summary>
    public void SetVisible(bool v)
    {
        if (label != null) label.enabled = v;
        if (background != null) background.enabled = v;
    }

    /// <summary>
    /// Atualiza label.text escolhendo overlay (se presente) ou currentChar.
    /// Garante que '\0' exiba string vazia.
    /// </summary>
    private void ApplyLabelText()
    {
        if (label == null) return;

        if (hasOverlay)
        {
            label.text = (overlayChar == '\0') ? "" : overlayChar.ToString();
        }
        else
        {
            label.text = (currentChar == '\0') ? "" : currentChar.ToString();
        }
    }

    // --- métodos utilitários solicitados pelo GridManager / PlayerTrioController

    /// <summary>
    /// Copia apenas visual (sprite, cor e texto) de outra célula sem alterar estado de grid.
    /// </summary>
    public void CopyFrom(Cell src)
    {
        if (src == null) { Clear(); return; }

        if (label != null && src.label != null)
            label.text = src.label.text;

        if (background != null && src.background != null)
        {
            background.sprite = src.background.sprite;
            background.color = src.background.color;
        }
        // não copia row/col/isStatic/currentChar
        overlayChar = '\0';
        hasOverlay = false;
    }

    /// <summary>
    /// Limpa visual da célula usada para a coluna do jogador.
    /// </summary>
    public void Clear()
    {
        if (label != null) label.text = "";
        if (background != null)
        {
            Color c = background.color;
            c.a = 0f;
            background.color = c;
        }
        overlayChar = '\0';
        hasOverlay = false;
    }

    /// <summary>
    /// Define alpha do background de forma segura.
    /// </summary>
    public void SetBackgroundAlpha(float a)
    {
        if (background == null) return;
        Color c = background.color;
        c.a = Mathf.Clamp01(a);
        background.color = c;
    }
}
