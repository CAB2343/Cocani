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
    public MiniGameFlowManager flowManager; // opcional: assign no Inspector
    public float GetTimeRemaining() => timeRemaining;
    public float GetTimeLimit() => timeLimitSeconds;
    public bool IsMiniGameRunning() => miniGameRunning;

    [Header("Cores/Visual")]
    public Color playerBackgroundColor = new Color(1f,1f,1f,1f);
    public Color playerTextColor = Color.black;

    [Header("Tempo")]
    public float timeLimitSeconds = 30f;

    [Header("Config")]
    public int startRow = -1;
    public int startCol = 0;
    public bool wrapHorizontally = false;
    public bool overlayMode = true;

    // estado público para o manager usar
    [HideInInspector] public int curRow;
    [HideInInspector] public int curColStart;
    [HideInInspector] public GameObject[] playerSlots = new GameObject[3];

    private char[] heldChars = new char[3];
    private Cell[] lastOverlayCells = new Cell[3];
    private Color[] savedBgColors = new Color[3];
    private Color[] savedTextColors = new Color[3];

    // timer
    private float timeRemaining;
    private Coroutine timerCoroutine;
    private bool miniGameRunning = false;

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
        if (gridManager != null)
        {
            if (gridManager.onTrioChanged != null) gridManager.onTrioChanged.RemoveListener(OnGridTrioChanged);
            if (gridManager.onGridReady != null) gridManager.onGridReady.RemoveListener(OnGridReady);
        }
        RestoreLastBackgrounds();
        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
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

        // garantir que os slots do jogador sigam o trio fixo sempre que o GridManager disparar evento
        if (gridManager.onTrioChanged != null) gridManager.onTrioChanged.AddListener(OnGridTrioChanged);
        if (gridManager.onGridReady != null) gridManager.onGridReady.AddListener(OnGridReady);

        // inicializa held chars com o trio atual do grid (se houver)
        string fixedStr = gridManager.GetFixedTrioString();
        if (!string.IsNullOrEmpty(fixedStr) && fixedStr.Length >= 3)
            SetHeldChars(fixedStr);
        else
            SetHeldChars("   ");

        ApplyOverlayAt(curRow, curColStart);
        if (gridManager != null) gridManager.UpdatePlayerColumn(curRow, curColStart);
        RefreshOverlayVisuals();

        Debug.Log("PlayerTrioController: Initialized. Timer NOT started. Use flowManager.StartMiniGame() or StartMiniGame().");
    }

    // quando o GridManager (re)define o trio, atualizamos os held chars do jogador para o mesmo trio
    void OnGridTrioChanged()
    {
        string fixedStr = gridManager != null ? gridManager.GetFixedTrioString() : null;
        if (!string.IsNullOrEmpty(fixedStr) && fixedStr.Length >= 3)
        {
            SetHeldChars(fixedStr);
            Debug.Log($"PlayerTrioController: OnGridTrioChanged -> synced held chars to '{fixedStr}'");
        }
        else
        {
            SetHeldChars("   ");
            Debug.Log("PlayerTrioController: OnGridTrioChanged -> no valid fixed trio, cleared held chars");
        }

        // atualiza preview e overlay para refletir o novo trio
        ApplyOverlayAt(curRow, curColStart);
        if (gridManager != null) gridManager.UpdatePlayerColumn(curRow, curColStart);
    }

    // chamado quando o grid foi (re)gerado; reaplica overlay e também sincroniza held chars
    void OnGridReady()
    {
        OnGridTrioChanged();
        Debug.Log("PlayerTrioController: OnGridReady -> reapplied overlay and synced held chars.");
    }

    void Update()
    {
        if (!miniGameRunning) return;
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
            CheckForSuccess();
        }
    }

    IEnumerator TimerRoutine()
    {
        Debug.Log($"PlayerTrioController: TimerRoutine started: {timeRemaining:F2}s");
        while (timeRemaining > 0f && miniGameRunning)
        {
            yield return null;
            timeRemaining -= Time.deltaTime;
        }

        if (!miniGameRunning) yield break;

        if (timeRemaining <= 0f)
        {
            miniGameRunning = false;
            Debug.Log($"PlayerTrioController: Timer expired. curRow={curRow} curColStart={curColStart} held='{GetHeldString()}'");
            if (flowManager != null) flowManager.OnPlayerFail();
        }
    }

    void CheckForSuccess()
    {
        if (!miniGameRunning) return;
        if (gridManager == null) return;

        if (curRow != gridManager.fixedRow) return;
        if (curColStart != gridManager.fixedColStart) return;

        string fixedStr = gridManager.GetFixedTrioString();
        if (string.IsNullOrEmpty(fixedStr) || fixedStr.Length < 3) return;

        for (int i = 0; i < 3; i++)
        {
            char hc = heldChars[i];
            if (hc == '\0' || hc != fixedStr[i]) return;
        }

        // success -> delegate to flow manager
        miniGameRunning = false;
        if (timerCoroutine != null) { StopCoroutine(timerCoroutine); timerCoroutine = null; }
        Debug.Log("PlayerTrioController: success detected -> notifying flow manager.");
        if (flowManager != null) flowManager.OnPlayerSuccess();
    }

    public void SetHeldChars(string s)
    {
        if (string.IsNullOrEmpty(s)) s = "   ";
        for (int i = 0; i < 3; i++) heldChars[i] = (i < s.Length) ? s[i] : '\0';
        RefreshOverlayVisuals();
    }

    // public controls used by flow manager
    public void StartMiniGame()
    {
        if (miniGameRunning) return;
        timeRemaining = Mathf.Max(0f, timeLimitSeconds);
        miniGameRunning = true;
        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        timerCoroutine = StartCoroutine(TimerRoutine());
    }

    public void StopMiniGame()
    {
        if (!miniGameRunning) return;
        miniGameRunning = false;
        if (timerCoroutine != null) { StopCoroutine(timerCoroutine); timerCoroutine = null; }
    }

    public void ResetMiniGameTimer()
    {
        if (timerCoroutine != null) { StopCoroutine(timerCoroutine); timerCoroutine = null; }
        miniGameRunning = false;
        timeRemaining = timeLimitSeconds;
    }

    // overlay helpers
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

            if (cell.background != null)
            {
                savedBgColors[i] = cell.background.color;
                cell.background.color = playerBackgroundColor;
            }

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

            if (slotCell.background != null)
                slotCell.background.color = playerBackgroundColor;

            if (slotCell.label != null)
            {
                slotCell.label.color = playerTextColor;
                slotCell.SetOverlayChar(heldChars[i]);
            }
        }
    }

    void OnGridTrioChangedLegacy() { /* kept for compatibility if needed */ }

    string GetHeldString() => new string(heldChars);
}
