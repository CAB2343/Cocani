using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// PlayerTrioController — modo "movimento totalmente dentro do grid"
/// - Os heldChars nunca mudam ao se mover.
/// - A movimentação altera quais células exibem (temporariamente) os heldChars.
/// - overlayMode = true -> aplicação visual temporária (não altera valores do grid).
/// - overlayMode = false -> stampMode: escreve os chars na célula (persistente).
/// </summary>
public class PlayerTrioController : MonoBehaviour
{
    [Header("Refs")]
    public GridManager gridManager;    // seu GridManager
    public RectTransform playerParent; // só para exibir a UI do trio (opcional)
    public GameObject cellPrefab;      // caso precise instanciar localmente (não obrigatório)

    [Header("Config")]
    public int startRow = 0;           // linha inicial lógica dentro do grid
    public int startCol = 0;           // coluna inicial lógica (0 .. cols-3)
    public bool wrapHorizontally = false;
    [Tooltip("true = apenas sobrepor visualmente (recomendado). false = escrever nos cells (persistente).")]
    public bool overlayMode = true;

    // estado
    private int curRow;
    private int curColStart;
    private char[] heldChars = new char[3];

    // referência para células que atualmente tem overlay (para limpar ao mover)
    private Cell[] lastOverlayCells = new Cell[3];

    void Start()
    {
        if (gridManager == null)
        {
            Debug.LogError("PlayerTrioController: atribua GridManager no inspector.");
            enabled = false;
            return;
        }

        // posição lógica inicial
        curRow = Mathf.Clamp(startRow, 0, Mathf.Max(0, gridManager.GetRows() - 1));
        curColStart = Mathf.Clamp(startCol, 0, Mathf.Max(0, gridManager.GetCols() - 3));

        // inicia heldChars com o trio fixo do GridManager (ou substitua chamando SetHeldChars de outro script)
        string initial = gridManager.GetFixedTrioString();
        if (string.IsNullOrEmpty(initial) || initial.Length < 3) initial = "   ";
        SetHeldChars(initial);

        // aplica overlay inicial na posição lógica
        ApplyOverlayAt(curRow, curColStart);
    }

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        int newRow = curRow;
        int newCol = curColStart;
        bool moved = false;

        if (Input.GetKeyDown(KeyCode.A))
        {
            newCol--;
            moved = true;
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            newCol++;
            moved = true;
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            newRow--;
            moved = true;
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            newRow++;
            moved = true;
        }

        if (!moved) return;

        // clamp/warp
        newRow = Mathf.Clamp(newRow, 0, gridManager.GetRows() - 1);
        int maxColStart = Mathf.Max(0, gridManager.GetCols() - 3);
        if (wrapHorizontally)
        {
            if (newCol < 0) newCol = maxColStart;
            else if (newCol > maxColStart) newCol = 0;
        }
        else
        {
            newCol = Mathf.Clamp(newCol, 0, maxColStart);
        }

        // se posição mudou: limpar overlay antigo e aplicar no novo local
        if (newRow != curRow || newCol != curColStart)
        {
            ClearOverlayAt(curRow, curColStart);
            curRow = newRow;
            curColStart = newCol;
            ApplyOverlayAt(curRow, curColStart);

            Debug.Log($"Player move -> row:{curRow} colStart:{curColStart} held:{new string(heldChars)}");
        }
    }

    /// <summary>
    /// Define os 3 chars que o jogador carrega (não serão alterados ao mover).
    /// </summary>
    public void SetHeldChars(string s)
    {
        if (string.IsNullOrEmpty(s)) s = "   ";
        for (int i = 0; i < 3; i++)
            heldChars[i] = (i < s.Length) ? s[i] : '\0';
        // atualiza overlay se já aplicamos antes
        ApplyOverlayAt(curRow, curColStart);
    }

    // aplica o heldChars sobre as 3 células na posição (row, colStart)
    void ApplyOverlayAt(int row, int colStart)
    {
        if (gridManager == null) return;

        // limpa last overlay (por segurança) antes de marcar novos
        ClearLastOverlayCache();

        for (int i = 0; i < 3; i++)
        {
            int c = colStart + i;
            Cell cell = gridManager.GetCell(row, c);
            lastOverlayCells[i] = cell;

            if (cell == null) continue;

            if (overlayMode)
            {
                // visual temporário: sobrepõe apenas a visual, não altera currentChar
                cell.SetOverlayChar(heldChars[i]);
            }
            else
            {
                // stamp mode: escreve permanentemente o held char na célula
                cell.SetChar(heldChars[i]);
            }
        }
    }

    // limpa overlay das células que armazenamos antes
    void ClearOverlayAt(int row, int colStart)
    {
        if (overlayMode)
        {
            for (int i = 0; i < 3; i++)
            {
                Cell cell = lastOverlayCells[i];
                if (cell != null)
                {
                    cell.ClearOverlay();
                    lastOverlayCells[i] = null;
                }
            }
        }
        else
        {
            // em stampMode não temos overlay pra limpar; se quiser reverter algo, implementa aqui.
        }
    }

    void ClearLastOverlayCache()
    {
        for (int i = 0; i < 3; i++)
            lastOverlayCells[i] = null;
    }

    // utilitários
    public (int row, int colStart) GetCurrentGridPosition() => (curRow, curColStart);
    public string GetHeldString() => new string(heldChars);
}
