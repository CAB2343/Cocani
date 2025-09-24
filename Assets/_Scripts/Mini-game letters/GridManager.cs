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

    [Header("Grid")]
    public int rows = 10;
    public int cols = 15;
    public float cellSize = 40f;
    public float spacing = 2f;

    [Header("Shuffle")]
    public float shuffleInterval = 0.15f;

    [Header("Fixed trio")]
    [Tooltip("Se true, posição e chars do trio serão escolhidos aleatoriamente no Start")]
    public bool randomizeOnStart = true;
    [Tooltip("Permite usar dígitos 0-9 além das letras A-Z")]
    public bool includeNumbers = true;
    public string target = "ABC";
    public int fixedRow = 4;
    public int fixedColStart = 5;

    [Header("Runtime dynamics")]
    [Tooltip("Se true, o trio será re-randomizado a cada respawnInterval")]
    public bool respawnFixedTrio = false;
    public float respawnInterval = 8f;

    public UnityEvent onTrioChanged;

    private Cell[,] grid;
    private Cell[] fixedTrioCells = new Cell[3]; // <<<< NOVO
    private Coroutine shuffleCoroutine;
    private Coroutine respawnCoroutine;
    private GridLayoutGroup gridLayout;

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

        fixedRow = Mathf.Clamp(fixedRow, 0, rows - 1);
        fixedColStart = Mathf.Clamp(fixedColStart, 0, cols - 3);

        if (randomizeOnStart) PickRandomFixedTrio();

        PrepareLayout();
        GenerateGrid();

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
                    cell.SetChar(target[idx]);
                    fixedTrioCells[idx] = cell; // <<<< MARCA O TRIO
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
    }

    IEnumerator ShuffleRoutine()
    {
        while (true)
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
            yield return new WaitForSeconds(shuffleInterval);
        }
    }

    IEnumerator RespawnTrioRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(respawnInterval);
            PickRandomFixedTrio();
            ApplyFixedTrioToGrid();
        }
    }

    void PickRandomFixedTrio()
    {
        fixedRow = Random.Range(0, rows);
        fixedColStart = Random.Range(0, cols - 2);

        target = "";
        string charset = includeNumbers ? "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ" : "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        for (int i = 0; i < 3; i++)
        {
            int idx = Random.Range(0, charset.Length);
            target += charset[idx];
        }
    }

    void ApplyFixedTrioToGrid()
    {
        if (grid == null) return;

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
                    fixedTrioCells[idx] = cell; // <<<< ATUALIZA TRIO
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
        fixedColStart = Mathf.Clamp(colStart, 0, cols - 3);
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

    public Cell[] GetFixedTrioCells() => fixedTrioCells; // <<<< NOVO

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

    public int GetRows() => rows;
    public int GetCols() => cols;
}
