using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class GridManager : MonoBehaviour
{
    [Header("Prefab / Container")]
    public GameObject cellPrefab;
    public RectTransform container;

    [Header("Player column (visual)")]
    public RectTransform playerColumnContainer;
    public bool showPlayerColumn = true;
    [Tooltip("Cor do texto exibido na coluna do jogador (override)")]
    public Color playerColumnTextColor = Color.white;

    [Header("Grid")]
    public int rows = 10;
    public int cols = 15;
    public float cellSize = 40f;
    public float spacing = 2f;

    [Header("Shuffle")]
    public float shuffleInterval = 0.15f;

    [Header("Fixed trio")]
    public bool randomizeOnStart = true;
    public bool includeNumbers = true;
    public string target = "ABC";
    public int fixedRow = 4;
    public int fixedColStart = 5;

    [Header("Restrictions")]
    public int reservedLeftColumns = 3;

    [Header("Runtime dynamics")]
    public bool respawnFixedTrio = false;
    public float respawnInterval = 8f;

    [Header("Failure reset")]
    public float failureResetDelay = 1.0f;

    public UnityEvent onTrioChanged;
    public UnityEvent onGridReady;

    private Cell[,] grid;
    private Cell[] fixedTrioCells = new Cell[3];
    private Coroutine shuffleCoroutine;
    private Coroutine respawnCoroutine;
    private GridLayoutGroup gridLayout;

    private Cell[] playerColumnCells = new Cell[3];

    private bool miniGameLocked = false;

void Awake()
{

    if (container == null)
    {
        container = GetComponent<RectTransform>();

        if (container == null)
        {
            var go = new GameObject("GridContainer", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            container = go.GetComponent<RectTransform>();
        }
    }


    if (cellPrefab == null)
    {
        // tenta achar prefab por tag
        var go = GameObject.FindGameObjectWithTag("CellPrefab");
        if (go != null) cellPrefab = go;

        // tenta carregar de Resources (Assets/Resources/Cell.prefab)
        if (cellPrefab == null)
            cellPrefab = Resources.Load<GameObject>("Cell");

        // último caso: cria um quadradinho simples
        if (cellPrefab == null)
        {
            cellPrefab = new GameObject("AutoCell", typeof(RectTransform), typeof(Image), typeof(Cell));
            var img = cellPrefab.GetComponent<Image>();
            img.color = Color.gray;
        }
    }
}



    void Start()
    {
        if (!Application.isPlaying) return;
        if (cellPrefab == null || container == null)
        {
            Debug.LogError("GridManager: atribua cellPrefab e container no Inspector.");
            enabled = false;
            return;
        }

        rows = Mathf.Max(1, rows);
        cols = Mathf.Max(3, cols);

        reservedLeftColumns = Mathf.Clamp(reservedLeftColumns, 0, Mathf.Max(0, cols - 3));
        fixedRow = Mathf.Clamp(fixedRow, 0, rows - 1);
        fixedColStart = Mathf.Clamp(fixedColStart, reservedLeftColumns, cols - 3);

        if (randomizeOnStart) PickRandomFixedTrio();

        PrepareLayout();
        GenerateGrid();

        if (showPlayerColumn)
            InitPlayerColumn();

        shuffleCoroutine = StartCoroutine(ShuffleRoutine());
        if (respawnFixedTrio)
            respawnCoroutine = StartCoroutine(RespawnTrioRoutine());
    }




    void OnDisable()
    {
        if (shuffleCoroutine != null) StopCoroutine(shuffleCoroutine);
        if (respawnCoroutine != null) StopCoroutine(respawnCoroutine);
    }

    void PrepareLayout()
    {
        gridLayout = container.GetComponent<GridLayoutGroup>();
        if (gridLayout == null) gridLayout = container.gameObject.AddComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(cellSize, cellSize);
        gridLayout.spacing = new Vector2(spacing, spacing);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = cols;
        gridLayout.childAlignment = TextAnchor.UpperLeft;
    }

    void GenerateGrid()
    {
        miniGameLocked = false;
        grid = new Cell[rows, cols];

        for (int i = container.childCount - 1; i >= 0; i--)
        {
            var child = container.GetChild(i).gameObject;
            if (Application.isPlaying) Destroy(child);
            else DestroyImmediate(child);
        }

        if (cellPrefab != null && cellPrefab.activeSelf)
            cellPrefab.SetActive(false);

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                GameObject go = Instantiate(cellPrefab, container);
                go.SetActive(false);

                go.name = $"Cell_{r}_{c}";
                var cell = go.GetComponent<Cell>();
                if (cell == null)
                {
                    Debug.LogError("cellPrefab precisa ter o componente Cell.");
                    if (Application.isPlaying) Destroy(go);
                    return;
                }

                cell.row = r;
                cell.col = c;

                if (r == fixedRow && c >= fixedColStart && c < fixedColStart + 3)
                {
                    cell.isStatic = true;
                    int idx = c - fixedColStart;
                    if (!string.IsNullOrEmpty(target) && target.Length > idx)
                        cell.SetChar(target[idx]);
                    else
                        cell.SetChar(RandomChar());
                    fixedTrioCells[idx] = cell;
                }
                else
                {
                    cell.isStatic = false;
                    cell.SetChar(RandomChar());
                }

                grid[r, c] = cell;
                go.SetActive(true);
            }
        }

        Canvas.ForceUpdateCanvases();
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(container);

        if (showPlayerColumn)
            InitPlayerColumn();

        // sincroniza a coluna do jogador com o trio fixo imediatamente
        if (showPlayerColumn)
            UpdatePlayerColumn(fixedRow, fixedColStart);

        onGridReady?.Invoke();
        Debug.Log("GridManager: Grid gerado e onGridReady invocado.");
    }

    IEnumerator ShuffleRoutine()
    {
        while (true)
        {
            if (!miniGameLocked)
            {
                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        var cell = grid[r, c];
                        if (cell == null) continue;
                        if (!cell.isStatic)
                            cell.SetChar(RandomChar());
                    }
                }
            }
            yield return new WaitForSeconds(shuffleInterval);
        }
    }

    IEnumerator RespawnTrioRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(respawnInterval);
            if (!miniGameLocked)
            {
                PickRandomFixedTrio();
                ApplyFixedTrioToGrid();
                Debug.Log("GridManager: RespawnFixedTrio aplicado.");
            }
        }
    }

    void PickRandomFixedTrio()
    {
        int minCol = Mathf.Clamp(reservedLeftColumns, 0, cols - 3);
        fixedRow = Random.Range(0, rows);
        fixedColStart = Random.Range(minCol, cols - 2);

        target = "";
        string charset = includeNumbers ? "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ" : "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        for (int i = 0; i < 3; i++)
        {
            int idx = Random.Range(0, charset.Length);
            target += charset[idx];
        }

        Debug.Log($"GridManager: PickRandomFixedTrio -> row {fixedRow} colStart {fixedColStart} target {target}");
    }

    void ApplyFixedTrioToGrid()
    {
        if (grid == null) return;

        fixedColStart = Mathf.Clamp(fixedColStart, Mathf.Clamp(reservedLeftColumns, 0, cols - 3), cols - 3);

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                var cell = grid[r, c];
                if (cell == null) continue;
                if (r == fixedRow && c >= fixedColStart && c < fixedColStart + 3)
                {
                    cell.isStatic = true;
                    int idx = c - fixedColStart;
                    cell.SetChar(target[idx]);
                    fixedTrioCells[idx] = cell;
                }
                else
                {
                    if (cell.isStatic)
                        cell.SetChar(RandomChar());
                    cell.isStatic = false;
                }
            }
        }

        onTrioChanged?.Invoke();

        Canvas.ForceUpdateCanvases();
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(container);
        Debug.Log($"GridManager: ApplyFixedTrioToGrid -> fixedRow {fixedRow} fixedColStart {fixedColStart} target {target}");
    }

    char RandomChar()
    {
        string charset = includeNumbers ? "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ" : "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        int i = Random.Range(0, charset.Length);
        return charset[i];
    }

    public void SetFixedTrio(int row, int colStart, string newTarget)
    {
        fixedRow = Mathf.Clamp(row, 0, rows - 1);
        int minCol = Mathf.Clamp(reservedLeftColumns, 0, cols - 3);
        fixedColStart = Mathf.Clamp(colStart, minCol, cols - 3);
        if (!string.IsNullOrEmpty(newTarget) && newTarget.Length == 3) target = newTarget;
        ApplyFixedTrioToGrid();
    }

    public string GetFixedTrioString() => target;

    public Cell GetCell(int row, int col)
    {
        if (grid == null) return null;
        if (row < 0 || row >= rows) return null;
        if (col < 0 || col >= cols) return null;
        return grid[row, col];
    }

    public Cell[] GetFixedTrioCells() => fixedTrioCells;

    public Vector3 GetCellWorldPosition(int row, int col)
    {
        col = Mathf.Clamp(col, 0, cols - 1);

        if (grid != null)
        {
            if (row >= 0 && row < rows)
            {
                var cell = grid[row, col];
                if (cell != null)
                    return cell.transform.position;
            }
            else if (row == -1)
            {
                var cell = grid[0, col];
                if (cell != null)
                {
                    Vector3 basePos = cell.transform.position;
                    float offset = (gridLayout != null) ? (gridLayout.cellSize.y + gridLayout.spacing.y) : (cellSize + spacing);
                    return basePos + Vector3.up * offset;
                }
            }
        }

        return container != null ? container.position : Vector3.zero;
    }

    public void StartShuffle()
    {
        if (shuffleCoroutine == null)
            shuffleCoroutine = StartCoroutine(ShuffleRoutine());
    }

    public void StopShuffle()
    {
        if (shuffleCoroutine != null)
        {
            StopCoroutine(shuffleCoroutine);
            shuffleCoroutine = null;
        }
    }

    public void ForceShuffleOnce()
    {
        if (grid == null) return;
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                var cell = grid[r, c];
                if (cell == null) continue;
                if (!cell.isStatic)
                    cell.SetChar(RandomChar());
            }
        }
    }

    // -------- player column methods --------
    public int GetRows() => rows;
    public int GetCols() => cols;

    void InitPlayerColumn()
    {
        if (playerColumnContainer == null || cellPrefab == null) return;
        // always recreate player column cells if container changed
        var existingGrid = playerColumnContainer.GetComponent<GridLayoutGroup>();
        var existingHor = playerColumnContainer.GetComponent<HorizontalLayoutGroup>();

        if (existingGrid != null)
        {
            existingGrid.cellSize = new Vector2(cellSize, cellSize);
            existingGrid.spacing = new Vector2(spacing, spacing);
            existingGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            existingGrid.constraintCount = 3;
            existingGrid.childAlignment = TextAnchor.MiddleCenter;
            existingGrid.startAxis = GridLayoutGroup.Axis.Horizontal;
        }
        else if (existingHor != null)
        {
            existingHor.spacing = spacing;
            existingHor.childAlignment = TextAnchor.MiddleCenter;
        }
        else
        {
            var grid = playerColumnContainer.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(cellSize, cellSize);
            grid.spacing = new Vector2(spacing, spacing);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;
            grid.childAlignment = TextAnchor.MiddleCenter;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        }

        for (int i = playerColumnContainer.childCount - 1; i >= 0; i--)
            Destroy(playerColumnContainer.GetChild(i).gameObject);

        for (int i = 0; i < 3; i++)
        {
            GameObject go = Instantiate(cellPrefab, playerColumnContainer);
            go.name = $"PlayerColumnCell_{i}";
            go.SetActive(true);
            playerColumnCells[i] = go.GetComponent<Cell>();
        }
    }

    public void UpdatePlayerColumn(int row, int colStart)
    {
        if (!showPlayerColumn) return;
        if (playerColumnCells[0] == null) InitPlayerColumn();
        if (playerColumnCells[0] == null) return;

        if (row < 0)
        {
            for (int i = 0; i < 3; i++)
            {
                var dst = playerColumnCells[i];
                if (dst != null)
                {
                    dst.Clear();
                    if (dst.label != null) dst.label.color = playerColumnTextColor;
                }
            }
            return;
        }

        colStart = Mathf.Clamp(colStart, 0, Mathf.Max(0, cols - 3));

        for (int i = 0; i < 3; i++)
        {
            var src = GetCell(row, colStart + i);
            var dst = playerColumnCells[i];
            if (dst == null) continue;
            if (src == null)
            {
                dst.Clear();
                if (dst.label != null) dst.label.color = playerColumnTextColor;
                continue;
            }

            dst.CopyFrom(src);

            if (dst.label != null)
                dst.label.color = playerColumnTextColor;
        }
    }

    // ---- mini-game end handling ----

    /// <summary>
    /// Sucesso: pinta o trio fixo e a coluna do jogador e bloqueia atualizações.
    /// </summary>
    public void MiniGameSuccess(Color successColor)
    {
        if (miniGameLocked) return;
        miniGameLocked = true;

        StopShuffle();
        StopRespawn();

        Debug.Log($"GridManager: MiniGameSuccess -> painting trio cells and player column with color {successColor}");

        for (int i = 0; i < 3; i++)
        {
            if (fixedTrioCells[i] != null && fixedTrioCells[i].background != null)
            {
                fixedTrioCells[i].background.color = successColor;
                Debug.Log($"  Success painted fixed cell [{fixedTrioCells[i].row},{fixedTrioCells[i].col}]");
            }

            if (playerColumnCells[i] != null && playerColumnCells[i].background != null)
            {
                playerColumnCells[i].background.color = successColor;
                Debug.Log($"  Success painted player column cell index {i}");
            }
        }
    }

    /// <summary>
    /// Fracasso: pinta apenas cellsToColor se fornecido. Caso contrário pinta todas.
    /// Bloqueia atualizações e reinicia após resetDelay.
    /// </summary>
    public void MiniGameFail(Color failureColor, float resetDelay, Cell[] cellsToColor = null)
    {
        if (miniGameLocked) return;
        miniGameLocked = true;

        StopShuffle();
        StopRespawn();

        if (cellsToColor != null && cellsToColor.Length > 0)
        {
            Debug.Log($"GridManager: MiniGameFail -> painting {cellsToColor.Length} provided cells with color {failureColor}");
            foreach (var cell in cellsToColor)
            {
                if (cell != null && cell.background != null)
                {
                    cell.background.color = failureColor;
                    Debug.Log($"  Failure painted cell [{cell.row},{cell.col}]");
                }
            }
        }
        else
        {
            Debug.Log($"GridManager: MiniGameFail -> painting ALL cells with color {failureColor}");
            if (grid != null)
            {
                for (int r = 0; r < rows; r++)
                    for (int c = 0; c < cols; c++)
                        if (grid[r, c] != null && grid[r, c].background != null)
                            grid[r, c].background.color = failureColor;
            }
        }

        StartCoroutine(FailureResetCoroutine(resetDelay));
    }

    IEnumerator FailureResetCoroutine(float delay)
    {
        Debug.Log($"GridManager: Failure reset in {delay} seconds.");
        yield return new WaitForSeconds(delay);
        ResetMiniGame();
    }

    /// <summary>
    /// Full reset: para coroutines, escolhe novo trio/posição, regenera todo o grid,
    /// re-inicializa a coluna do jogador e reinicia os loops (shuffle/respawn).
    /// Use este método quando quiser garantir um mini-game totalmente novo.
    /// </summary>
    public void ResetMiniGame()
    {
        Debug.Log("GridManager: Full ResetMiniGame starting...");

        // desbloqueia estado
        miniGameLocked = false;

        // para coroutines existentes
        if (shuffleCoroutine != null) { StopCoroutine(shuffleCoroutine); shuffleCoroutine = null; }
        if (respawnCoroutine != null) { StopCoroutine(respawnCoroutine); respawnCoroutine = null; }

        // limpa arrays e referências antigas
        fixedTrioCells = new Cell[3];
        playerColumnCells = new Cell[3];

        // remove filhos atuais do container (garante limpeza completa)
        if (container != null)
        {
            for (int i = container.childCount - 1; i >= 0; i--)
            {
                var child = container.GetChild(i).gameObject;
                if (Application.isPlaying) Destroy(child);
                else DestroyImmediate(child);
            }
        }

        // garante valores válidos e sorteia novo trio/posição
        rows = Mathf.Max(1, rows);
        cols = Mathf.Max(3, cols);
        reservedLeftColumns = Mathf.Clamp(reservedLeftColumns, 0, Mathf.Max(0, cols - 3));
        fixedRow = Mathf.Clamp(fixedRow, 0, rows - 1);

        // força nova escolha aleatória do trio e posição (sempre novo)
        PickRandomFixedTrio();

        // (re)gera o grid usando o novo fixedRow/fixedColStart/target
        GenerateGrid();

        // notifica listeners que o trio mudou
        onTrioChanged?.Invoke();

        // (re)inicia a coluna do jogador caso esteja visível e sincroniza com trio fixo
        if (showPlayerColumn)
        {
            InitPlayerColumn();
            UpdatePlayerColumn(fixedRow, fixedColStart);
        }

        // força rebuild visual
        Canvas.ForceUpdateCanvases();
        if (container != null) UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(container);

        // reinicia coroutines
        if (shuffleCoroutine == null) shuffleCoroutine = StartCoroutine(ShuffleRoutine());
        if (respawnFixedTrio && respawnCoroutine == null) respawnCoroutine = StartCoroutine(RespawnTrioRoutine());

        Debug.Log("GridManager: Full ResetMiniGame completed.");
    }

    void StopRespawn()
    {
        if (respawnCoroutine != null)
        {
            StopCoroutine(respawnCoroutine);
            respawnCoroutine = null;
        }
    }

    public void MiniGameFail(Color failureColor, float resetDelay)
    {
        MiniGameFail(failureColor, resetDelay, null);
    }
}
