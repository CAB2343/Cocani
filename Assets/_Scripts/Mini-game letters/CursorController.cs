using UnityEngine;
using UnityEngine.UI;

public class CursorController : MonoBehaviour
{
    public GridGenerator grid;
    public int curCol = 0;
    public int curRow = 0;
    public Text hudTargetText; // mostra "Alvo: XYZ"
    public Text feedbackText;  // feedback ao jogador
    public float inputCooldown = 0.08f;
    float lastInputTime;

    void Start()
    {
        if (grid == null) Debug.LogError("Arraste GridGenerator no Inspector do CursorController.");
        if (hudTargetText != null) hudTargetText.text = "Alvo: " + grid.targetString;
        RefreshSelectionVisual();
    }

    void Update()
    {
        HandleMovement();
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            TryCheck();
    }

    void HandleMovement()
    {
        if (Time.unscaledTime < lastInputTime + inputCooldown) return;

        int dx = 0, dy = 0;
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) dx = -1;
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) dx = 1;
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) dy = -1;
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) dy = 1;

        if (dx != 0 || dy != 0)
        {
            curCol = Mathf.Clamp(curCol + dx, 0, grid.cols - 3);
            curRow = Mathf.Clamp(curRow + dy, 0, grid.rows - 1);
            lastInputTime = Time.unscaledTime;
            RefreshSelectionVisual();
        }
    }

    void RefreshSelectionVisual()
    {
        // limpa seleção
        for (int y = 0; y < grid.rows; y++)
            for (int x = 0; x < grid.cols; x++)
                grid.cells[x, y].SetSelected(false);

        // marca as 3 células do cursor
        for (int i = 0; i < 3; i++)
            grid.cells[curCol + i, curRow].SetSelected(true);

        if (hudTargetText != null) hudTargetText.text = "Alvo: " + grid.targetString;
    }

    void TryCheck()
    {
        string s = grid.GetStringAt(curCol, curRow);
        if (s == null) return;

        if (s == grid.targetString)
        {
            if (feedbackText != null) feedbackText.text = "GG! Achou: " + s;
            Invoke(nameof(NextRound), 0.5f);
        }
        else
        {
            if (feedbackText != null) feedbackText.text = "Não é esse: " + s;
        }
    }

    void NextRound()
    {
        grid.NewRound();
        curCol = Mathf.Clamp(curCol, 0, grid.cols - 3);
        curRow = Mathf.Clamp(curRow, 0, grid.rows - 1);
        RefreshSelectionVisual();
        if (feedbackText != null) feedbackText.text = "";
    }
}
