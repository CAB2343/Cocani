using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// PlayerTrioController — aguarda GridManager.onGridReady, suporta startRow = -1 (overlay acima do grid).
/// OverlayMode: heldChars nunca mudam ao se mover.
/// </summary>
public class PlayerTrioController : MonoBehaviour
{
    [Header("Refs")]
    public GridManager gridManager;    // seu GridManager
    public RectTransform playerParent; // container UI fixo (overlay no canto superior-esquerdo)
    public GameObject cellPrefab;      // prefab cell (mesmo usado pelo GridManager)

    [Header("Config")]
    [Tooltip("-1 = overlay acima do grid")]
    public int startRow = -1;          // permite -1
    public int startCol = 0;
    public bool wrapHorizontally = false;
    public bool overlayMode = true;

    [Header("Sync")]
    public bool syncHeldWithFixedTrioOnStart = true;
    public bool autoSyncHeldWithGridTrio = false;

    // estado
    private int curRow;
    private int curColStart;
    private char[] heldChars = new char[3];

    // internals
    private GameObject[] playerSlots = new GameObject[3];
    private GridLayoutGroup playerLayout;
    private Cell[] lastOverlayCells = new Cell[3];

    IEnumerator Start()
    {
        // referências obrigatórias
        if (gridManager == null)
        {
            Debug.LogError("PlayerTrioController: atribua GridManager no inspector.");
            enabled = false;
            yield break;
        }
        if (playerParent == null)
        {
            Debug.LogError("PlayerTrioController: atribua playerParent (RectTransform) no inspector.");
            enabled = false;
            yield break;
        }
        if (cellPrefab == null)
        {
            Debug.LogError("PlayerTrioController: atribua cellPrefab no inspector.");
            enabled = false;
            yield break;
        }

        // Se o Grid já estiver pronto (cells geradas), inicialize na hora.
        bool gridReadyNow = (gridManager.GetCell(0, 0) != null) || (gridManager.GetFixedTrioCells() != null && gridManager.GetFixedTrioCells().Length >= 3 && gridManager.GetFixedTrioCells()[0] != null);
        if (gridReadyNow)
        {
            InitAfterGridReady();
            yield break;
        }

        // Se GridManager expõe onGridReady, inscrevemos e esperamos o evento. Senão, fallback: espera polling curto.
        bool subscribed = false;
        if (gridManager.onGridReady != null)
        {
            gridManager.onGridReady.AddListener(InitAfterGridReady);
            subscribed = true;
        }

        // fallback polling (timeout) caso o evento não exista ou não seja disparado
        float timeout = 2f;
        float t = 0f;
        while (gridManager.GetCell(0, 0) == null && t < timeout)
        {
            t += Time.deltaTime;
            yield return null;
        }

        // se assinamos o evento, InitAfterGridReady já será chamado quando disparar; se não, chamamos aqui.
        if (!subscribed)
        {
            InitAfterGridReady();
        }
    }

    void OnDestroy()
    {
        if (gridManager != null && gridManager.onGridReady != null)
            gridManager.onGridReady.RemoveListener(InitAfterGridReady);

        if (gridManager != null && gridManager.onTrioChanged != null)
            gridManager.onTrioChanged.RemoveListener(OnGridTrioChanged);
    }

    // inicialização segura depois que o Grid está pronto
    void InitAfterGridReady()
    {
        // evitar múltiplas chamadas
        if (playerSlots[0] != null) return;

        // setup GridLayoutGroup no playerParent
        playerLayout = playerParent.GetComponent<GridLayoutGroup>();
        if (playerLayout == null) playerLayout = playerParent.gameObject.AddComponent<GridLayoutGroup>();

        playerLayout.cellSize = new Vector2(gridManager.cellSize, gridManager.cellSize);
        playerLayout.spacing = new Vector2(gridManager.spacing, gridManager.spacing);
        playerLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        playerLayout.constraintCount = 3;
        playerLayout.childAlignment = TextAnchor.MiddleCenter;

        // clamp de posição inicial: permite -1
        curRow = Mathf.Clamp(startRow, -1, gridManager.GetRows() - 1);
        curColStart = Mathf.Clamp(startCol, 0, Mathf.Max(0, gridManager.GetCols() - 3));

        // instanciar slots do overlay
        for (int i = 0; i < 3; i++)
        {
            GameObject go = Instantiate(cellPrefab, playerParent);
            go.name = $"PlayerSlot_{i}";
            go.SetActive(true);
            playerSlots[i] = go;
        }

        // sincroniza heldChars com trio fixo do GridManager (opcional)
        if (syncHeldWithFixedTrioOnStart && gridManager != null)
        {
            string fixedStr = gridManager.GetFixedTrioString();
            if (!string.IsNullOrEmpty(fixedStr) && fixedStr.Length >= 3)
            {
                SetHeldChars(fixedStr);
                CopyFixedTrioBackgroundsToOverlay();
            }
            else
            {
                SetHeldChars("   ");
            }
        }
        else
        {
            SetHeldChars("   ");
        }

        // registrar onTrioChanged (opcional)
        if (gridManager.onTrioChanged != null)
            gridManager.onTrioChanged.AddListener(OnGridTrioChanged);

        // aplica overlay inicial (se curRow == -1 apenas atualiza o overlay fixo; se >=0 aplica sobre células)
        ApplyOverlayAt(curRow, curColStart);
        SnapSlotsToParent();

        Debug.Log("PlayerTrioController: initialization complete (grid ready).");
    }

    void Update()
    {
        // não processa input até ter inicializado
        if (playerSlots[0] == null) return;
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

        // clamp/warp (agora permite -1)
        newRow = Mathf.Clamp(newRow, -1, gridManager.GetRows() - 1);
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

        if (newRow != curRow || newCol != curColStart)
        {
            ClearOverlayAt(curRow, curColStart);
            curRow = newRow;
            curColStart = newCol;
            ApplyOverlayAt(curRow, curColStart);
            SnapSlotsToParent();

            Debug.Log($"Player move -> row:{curRow} colStart:{curColStart} held:{GetHeldString()}");
        }
    }

    public void SetHeldChars(string s)
    {
        if (string.IsNullOrEmpty(s)) s = "   ";
        for (int i = 0; i < 3; i++)
            heldChars[i] = (i < s.Length) ? s[i] : '\0';

        RefreshOverlayVisuals();
    }

    void ApplyOverlayAt(int row, int colStart)
    {
        if (gridManager == null) return;

        // se row == -1: não tentamos escrever/overlay nas células do grid — apenas atualizamos a UI do overlay
        if (row == -1)
        {
            // limpar overlays anteriores nas células (se houver)
            if (overlayMode) ClearOverlayOnLastCells();
            // atualizar apenas os slots do overlay
            RefreshOverlayVisuals();
            return;
        }

        // caso row >= 0: aplicamos overlay/stamp nas células correspondentes
        ClearLastOverlayCache();
        for (int i = 0; i < 3; i++)
        {
            int c = colStart + i;
            Cell cell = gridManager.GetCell(row, c);
            lastOverlayCells[i] = cell;
            if (cell == null) continue;

            if (overlayMode)
                cell.SetOverlayChar(heldChars[i]);
            else
                cell.SetChar(heldChars[i]);
        }

        // atualizar também o overlay visual fixo (playerSlots)
        RefreshOverlayVisuals();
    }

    void ClearOverlayAt(int row, int colStart)
    {
        if (row == -1)
        {
            // nothing to clear on grid cells; just clear cached overlays
            ClearOverlayOnLastCells();
            return;
        }

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
            // stamp mode: não reverte alterações por padrão
        }
    }

    void ClearOverlayOnLastCells()
    {
        for (int i = 0; i < 3; i++)
        {
            if (lastOverlayCells[i] != null)
            {
                lastOverlayCells[i].ClearOverlay();
                lastOverlayCells[i] = null;
            }
        }
    }

    void ClearLastOverlayCache()
    {
        for (int i = 0; i < 3; i++)
            lastOverlayCells[i] = null;
    }

    void RefreshOverlayVisuals()
    {
        for (int i = 0; i < 3; i++)
        {
            var slotGO = playerSlots[i];
            if (slotGO == null) continue;
            var slotCell = slotGO.GetComponent<Cell>();
            if (slotCell == null) continue;

            slotCell.SetOverlayChar(heldChars[i]);

            // preferir copiar background do trio fixo para manter identidade visual
            if (gridManager != null)
            {
                Cell[] fixedTrio = gridManager.GetFixedTrioCells();
                if (fixedTrio != null && fixedTrio.Length >= 3 && fixedTrio[i] != null)
                {
                    var src = fixedTrio[i];
                    if (slotCell.background != null && src.background != null)
                    {
                        slotCell.background.sprite = src.background.sprite;
                        slotCell.background.color = src.background.color;
                    }
                }
            }
        }
    }

    void CopyFixedTrioBackgroundsToOverlay()
    {
        if (gridManager == null) return;
        Cell[] fixedTrio = gridManager.GetFixedTrioCells();
        if (fixedTrio == null || fixedTrio.Length < 3) return;

        for (int i = 0; i < 3; i++)
        {
            var src = fixedTrio[i];
            var slotGO = playerSlots[i];
            if (src != null && slotGO != null)
            {
                var slotCell = slotGO.GetComponent<Cell>();
                if (slotCell != null && slotCell.background != null && src.background != null)
                {
                    slotCell.background.sprite = src.background.sprite;
                    slotCell.background.color = src.background.color;
                }
            }
        }
    }

    void OnGridTrioChanged()
    {
        Debug.Log("GridManager: trio mudou (evento recebido).");
        if (!autoSyncHeldWithGridTrio || gridManager == null) return;

        string fixedStr = gridManager.GetFixedTrioString();
        if (!string.IsNullOrEmpty(fixedStr) && fixedStr.Length >= 3)
        {
            SetHeldChars(fixedStr);
            CopyFixedTrioBackgroundsToOverlay();
            ApplyOverlayAt(curRow, curColStart);
        }
    }

    void SnapSlotsToParent()
    {
        if (playerParent == null) return;
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(playerParent);
    }

    // utilitários
    public (int row, int colStart) GetCurrentGridPosition() => (curRow, curColStart);
    public string GetHeldString() => new string(heldChars);
}
