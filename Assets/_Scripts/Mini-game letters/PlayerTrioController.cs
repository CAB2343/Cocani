// PlayerTrioController.cs
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerTrioController : MonoBehaviour
{
    [Header("Refs")]
    public GridManager gridManager;
    public RectTransform playerParent;
    public GameObject cellPrefab;

    [Header("Cores do jogador (Inspector)")]
    public Color playerBackgroundColor = new Color(1f, 1f, 1f, 1f); // alpha 1 = 255
    public Color playerTextColor = Color.black;

    [Header("Config")]
    public int startRow = -1;
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
    private Cell[] lastOverlayCells = new Cell[3];
    private Color[] savedBgColors = new Color[3];
    private Color[] savedTextColors = new Color[3];

    IEnumerator Start()
    {
        if (gridManager == null || playerParent == null || cellPrefab == null)
        {
            Debug.LogError("PlayerTrioController: atribua GridManager, playerParent e cellPrefab no inspector.");
            enabled = false;
            yield break;
        }

        float t = 0f;
        while (gridManager.GetCell(0, 0) == null && t < 2f)
        {
            t += Time.deltaTime;
            yield return null;
        }

        Init();
    }

    void OnDestroy()
    {
        if (gridManager != null && gridManager.onTrioChanged != null)
            gridManager.onTrioChanged.RemoveListener(OnGridTrioChanged);
        RestoreLastBackgrounds();
        if (gridManager != null) gridManager.UpdatePlayerColumn(-1, 0);
    }

    void Init()
    {
        var gridLayout = playerParent.GetComponent<UnityEngine.UI.GridLayoutGroup>();
        if (gridLayout == null) gridLayout = playerParent.gameObject.AddComponent<UnityEngine.UI.GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(gridManager.cellSize, gridManager.cellSize);
        gridLayout.spacing = new Vector2(gridManager.spacing, gridManager.spacing);
        gridLayout.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = 3;

        curRow = Mathf.Clamp(startRow, -1, gridManager.GetRows() - 1);
        curColStart = Mathf.Clamp(startCol, 0, Mathf.Max(0, gridManager.GetCols() - 3));

        for (int i = 0; i < 3; i++)
        {
            var go = Instantiate(cellPrefab, playerParent);
            go.name = $"PlayerSlot_{i}";
            playerSlots[i] = go;
        }

        if (syncHeldWithFixedTrioOnStart)
        {
            string fixedStr = gridManager.GetFixedTrioString();
            if (!string.IsNullOrEmpty(fixedStr) && fixedStr.Length >= 3)
                SetHeldChars(fixedStr);
            else
                SetHeldChars("   ");
        }
        else SetHeldChars("   ");

        if (gridManager.onTrioChanged != null)
            gridManager.onTrioChanged.AddListener(OnGridTrioChanged);

        ApplyOverlayAt(curRow, curColStart);
        // atualiza coluna visual do GridManager
        if (gridManager != null) gridManager.UpdatePlayerColumn(curRow, curColStart);
        RefreshOverlayVisuals();
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

        if (Input.GetKeyDown(KeyCode.A)) { newCol--; moved = true; }
        else if (Input.GetKeyDown(KeyCode.D)) { newCol++; moved = true; }
        else if (Input.GetKeyDown(KeyCode.W)) { newRow--; moved = true; }
        else if (Input.GetKeyDown(KeyCode.S)) { newRow++; moved = true; }

        if (!moved) return;

        newRow = Mathf.Clamp(newRow, -1, gridManager.GetRows() - 1);
        int maxColStart = Mathf.Max(0, gridManager.GetCols() - 3);
        if (wrapHorizontally)
        {
            if (newCol < 0) newCol = maxColStart;
            else if (newCol > maxColStart) newCol = 0;
        }
        else newCol = Mathf.Clamp(newCol, 0, maxColStart);

        if (newRow != curRow || newCol != curColStart)
        {
            ClearOverlayAt(curRow, curColStart);
            curRow = newRow;
            curColStart = newCol;
            ApplyOverlayAt(curRow, curColStart);
            if (gridManager != null) gridManager.UpdatePlayerColumn(curRow, curColStart);
        }
    }

    public void SetHeldChars(string s)
    {
        if (string.IsNullOrEmpty(s)) s = "   ";
        for (int i = 0; i < 3; i++) heldChars[i] = (i < s.Length) ? s[i] : '\0';
        RefreshOverlayVisuals();
    }

    void ApplyOverlayAt(int row, int colStart)
    {
        RestoreLastBackgrounds();
        ClearLastOverlayCache();

        if (row == -1)
        {
            RefreshOverlayVisuals();
            if (gridManager != null) gridManager.UpdatePlayerColumn(-1, 0);
            return;
        }

        for (int i = 0; i < 3; i++)
        {
            int c = colStart + i;
            Cell cell = gridManager.GetCell(row, c);
            lastOverlayCells[i] = cell;
            if (cell == null) continue;

            // salva cor atual do background
            if (cell.background != null)
            {
                savedBgColors[i] = cell.background.color;
                cell.background.color = playerBackgroundColor; // aplica cor do inspector (alpha já em 1)
            }

            // salva e aplica cor do texto via label
            if (cell.label != null)
            {
                savedTextColors[i] = cell.label.color;
                cell.label.color = playerTextColor;
            }

            if (overlayMode)
                cell.SetOverlayChar(heldChars[i]);
        }

        RefreshOverlayVisuals();
        if (gridManager != null) gridManager.UpdatePlayerColumn(row, colStart);
    }

    void ClearOverlayAt(int row, int colStart)
    {
        for (int i = 0; i < 3; i++)
        {
            var cell = lastOverlayCells[i];
            if (cell != null)
            {
                if (overlayMode) cell.ClearOverlay();

                if (cell.background != null)
                {
                    cell.background.color = savedBgColors[i];
                    savedBgColors[i] = default(Color);
                }

                if (cell.label != null)
                {
                    cell.label.color = savedTextColors[i];
                    savedTextColors[i] = default(Color);
                }

                lastOverlayCells[i] = null;
            }
        }

        if (gridManager != null) gridManager.UpdatePlayerColumn(-1, 0);
    }

    void RestoreLastBackgrounds()
    {
        for (int i = 0; i < 3; i++)
        {
            var cell = lastOverlayCells[i];
            if (cell != null)
            {
                if (cell.background != null)
                {
                    cell.background.color = savedBgColors[i];
                    savedBgColors[i] = default(Color);
                }

                if (cell.label != null)
                {
                    cell.label.color = savedTextColors[i];
                    savedTextColors[i] = default(Color);
                }

                lastOverlayCells[i] = null;
            }
        }
    }

    void ClearLastOverlayCache()
    {
        for (int i = 0; i < 3; i++) lastOverlayCells[i] = null;
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

            // copia background do trio fixo para os slots, mas depois aplica a cor do jogador
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

            // aplica cor do jogador nos slots (preview)
            if (slotCell.background != null)
                slotCell.background.color = playerBackgroundColor;

            if (slotCell.label != null)
            {
                slotCell.label.color = playerTextColor;
                // exibe held char no slot (preview)
                slotCell.SetOverlayChar(heldChars[i]);
            }
        }
    }

    void OnGridTrioChanged()
    {
        if (!autoSyncHeldWithGridTrio || gridManager == null) return;
        string fixedStr = gridManager.GetFixedTrioString();
        if (!string.IsNullOrEmpty(fixedStr) && fixedStr.Length >= 3)
        {
            SetHeldChars(fixedStr);
            ApplyOverlayAt(curRow, curColStart);
            if (gridManager != null) gridManager.UpdatePlayerColumn(curRow, curColStart);
        }
    }
}
