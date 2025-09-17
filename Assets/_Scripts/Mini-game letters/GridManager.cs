using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

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
    public string target = "ABC";    // será sobrescrito se randomizeOnStart == true
    public int fixedRow = 4;
    public int fixedColStart = 5;

    [Header("Runtime dynamics")]
    [Tooltip("Se true, o trio será re-randomizado a cada respawnInterval")]
    public bool respawnFixedTrio = false;
    public float respawnInterval = 8f;

    private Cell[,] grid;
    private Coroutine shuffleCoroutine;
    private Coroutine respawnCoroutine;

    void Start()
    {
        if (Application.isPlaying == false) return;

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
        var layout = container.GetComponent<GridLayoutGroup>();
        if (layout == null) layout = container.gameObject.AddComponent<GridLayoutGroup>();
        layout.cellSize = new Vector2(cellSize, cellSize);
        layout.spacing = new Vector2(spacing, spacing);
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = cols;
        layout.childAlignment = TextAnchor.UpperLeft;
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

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                var go = Instantiate(cellPrefab, container);
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
                }
                else
                {
                    cell.isStatic = false;
                    cell.SetChar(RandomChar());
                }

                grid[r, c] = cell;
            }
        }
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

    // escolhe posição e chars aleatórias
    void PickRandomFixedTrio()
    {
        fixedRow = Random.Range(0, rows);
        // fixedColStart deve permitir trio de 3 colunas: 0..cols-3 inclusive
        fixedColStart = Random.Range(0, cols - 2); // upper exclusive => max cols-3

        target = "";
        string charset = includeNumbers ? "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ" : "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        for (int i = 0; i < 3; i++)
        {
            int idx = Random.Range(0, charset.Length);
            target += charset[idx];
        }
    }

    // aplica a configuração atual do trio ao grid já gerado
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
                    cell.SetChar(target[c - fixedColStart]);
                }
                else
                {
                    // se era estática antes, agora libera e sorteia novo char
                    if (cell.isStatic)
                        cell.SetChar(RandomChar());
                    cell.isStatic = false;
                }
            }
        }
    }

    char RandomChar()
    {
        string charset = includeNumbers ? "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ" : "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        int i = Random.Range(0, charset.Length);
        return charset[i];
    }

    // método público para forçar manualmente um trio (pos ou target)
    public void SetFixedTrio(int row, int colStart, string newTarget)
    {
        fixedRow = Mathf.Clamp(row, 0, rows - 1);
        fixedColStart = Mathf.Clamp(colStart, 0, cols - 3);
        if (!string.IsNullOrEmpty(newTarget) && newTarget.Length == 3) target = newTarget;
        ApplyFixedTrioToGrid();
    }
}
