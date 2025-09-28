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
    public MiniGameController miniGameController; // ASSIGN no Inspector (opcional)

    [Header("Fechamento automático")]
    public float successCloseDelay = 3f; // tempo em segundos para fechar após sucesso
    public float failCloseDelay = 3f;    // tempo em segundos para fechar após falha

    [Header("Cores do jogador (Inspector)")]
    public Color playerBackgroundColor = new Color(1f, 1f, 1f, 1f); // alpha 1 = 255
    public Color playerTextColor = Color.black;

    [Header("Resultado visual")]
    public Color successBackgroundColor = Color.green;
    public Color failureBackgroundColor = Color.red;

    [Header("Tempo")]
    public float timeLimitSeconds = 30f;

    [Header("Config")]
    public int startRow = -1;
    public int startCol = 0;
    public bool wrapHorizontally = false;
    public bool overlayMode = true;

    [Header("Sync")]
    public bool syncHeldWithFixedTrioOnStart = true;
    public bool autoSyncHeldWithGridTrio = false;

    // estado
    [HideInInspector] public int curRow;
    [HideInInspector] public int curColStart;
    private char[] heldChars = new char[3];

    // internals
    private GameObject[] playerSlots = new GameObject[3];
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

        if (gridManager.onGridReady != null)
            gridManager.onGridReady.AddListener(OnGridReady);

        ApplyOverlayAt(curRow, curColStart);
        if (gridManager != null) gridManager.UpdatePlayerColumn(curRow, curColStart);
        RefreshOverlayVisuals();

        Debug.Log("PlayerTrioController: Initialized. Timer NOT started. Call StartMiniGame() to begin.");
    }

    // called when GridManager regenerates the grid (reset)
    void OnGridReady()
    {
        Debug.Log("PlayerTrioController: OnGridReady -> reapplying overlay to current pos.");
        // re-fetch cells and reapply overlay visually
        ApplyOverlayAt(curRow, curColStart);
        if (gridManager != null) gridManager.UpdatePlayerColumn(curRow, curColStart);
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
            OnTimeExpired();
        }
    }

    void OnTimeExpired()
    {
        Debug.Log("PlayerTrioController: OnTimeExpired triggered.");

        // close UI first so player can't continue moving
        if (miniGameController != null)
        {
            Debug.Log($"PlayerTrioController: FAIL -> will close mini-game after {failCloseDelay:F2}s");
            StartCoroutine(CloseAfterDelay(failCloseDelay));
        }

        // monta array de células controladas (se houver)
        Cell[] controlled = null;
        if (curRow >= 0)
        {
            controlled = new Cell[3];
            for (int i = 0; i < 3; i++)
            {
                controlled[i] = gridManager.GetCell(curRow, curColStart + i);
                Debug.Log($"  Controlled cell {i} = {(controlled[i] != null ? $"[{controlled[i].row},{controlled[i].col}]" : "null")}");
            }
        }
        else
        {
            Debug.Log("  curRow == -1 -> nenhum cell do grid está controlado no momento.");
        }

        // tell GridManager to paint only controlled cells and reset after delay
        if (gridManager != null)
            gridManager.MiniGameFail(failureBackgroundColor, gridManager.failureResetDelay, controlled);

        // pinta também os slots do jogador em vermelho (feedback curto)
        for (int i = 0; i < 3; i++)
        {
            var slot = playerSlots[i];
            if (slot == null) continue;
            var c = slot.GetComponent<Cell>();
            if (c != null && c.background != null)
            {
                c.background.color = failureBackgroundColor;
                Debug.Log($"  Player slot {i} painted failure color.");
            }
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
            if (hc == '\0' || hc != fixedStr[i])
            {
                Debug.Log($"PlayerTrioController: CheckForSuccess failed at index {i}. held='{hc}' expected='{fixedStr[i]}'");
                return;
            }
        }

        // sucesso
        miniGameRunning = false;
        if (timerCoroutine != null) { StopCoroutine(timerCoroutine); timerCoroutine = null; }

        Debug.Log($"PlayerTrioController: SUCCESS! held='{GetHeldString()}' matched target='{fixedStr}' at row={curRow} colStart={curColStart}. Time remaining={timeRemaining:F2}s");

        if (gridManager != null)
            gridManager.MiniGameSuccess(successBackgroundColor);

        // pinta os 3 cells que o jogador estava controlando
        for (int i = 0; i < 3; i++)
        {
            var cell = gridManager.GetCell(curRow, curColStart + i);
            if (cell != null && cell.background != null)
                cell.background.color = successBackgroundColor;
        }

        // pinta os slots do jogador
        for (int i = 0; i < 3; i++)
        {
            var slot = playerSlots[i];
            if (slot == null) continue;
            var c = slot.GetComponent<Cell>();
            if (c != null && c.background != null)
                c.background.color = successBackgroundColor;
        }

        // close UI now that player succeeded (with delay)
        if (miniGameController != null)
        {
            Debug.Log($"PlayerTrioController: SUCCESS -> will close mini-game after {successCloseDelay:F2}s");
            StartCoroutine(CloseAfterDelay(successCloseDelay));
        }

    }

        IEnumerator CloseAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (miniGameController != null)
            {
                miniGameController.Close();
                Debug.Log("PlayerTrioController: mini-game closed after delay.");
            }
        }

    public void SetHeldChars(string s)
    {
        if (string.IsNullOrEmpty(s)) s = "   ";
        for (int i = 0; i < 3; i++) heldChars[i] = (i < s.Length) ? s[i] : '\0';
        RefreshOverlayVisuals();
        Debug.Log($"PlayerTrioController: SetHeldChars -> '{GetHeldString()}'");
        CheckForSuccess();
    }

    // ------ public controls for starting/stopping the mini-game timer ------

    public void StartMiniGame()
    {
        if (miniGameRunning)
        {
            Debug.Log("PlayerTrioController: StartMiniGame called but already running.");
            return;
        }

        timeRemaining = Mathf.Max(0f, timeLimitSeconds);
        miniGameRunning = true;
        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        timerCoroutine = StartCoroutine(TimerRoutine());
        Debug.Log($"PlayerTrioController: StartMiniGame -> timer started {timeRemaining:F2}s");
    }

    public void StopMiniGame()
    {
        if (!miniGameRunning)
        {
            Debug.Log("PlayerTrioController: StopMiniGame called but not running.");
            return;
        }
        miniGameRunning = false;
        if (timerCoroutine != null) { StopCoroutine(timerCoroutine); timerCoroutine = null; }
        Debug.Log("PlayerTrioController: StopMiniGame -> timer stopped and input disabled.");
    }

    public void ResetMiniGameTimer()
    {
        if (timerCoroutine != null) { StopCoroutine(timerCoroutine); timerCoroutine = null; }
        miniGameRunning = false;
        timeRemaining = timeLimitSeconds;
        Debug.Log("PlayerTrioController: ResetMiniGameTimer -> timer reset and not running.");
    }

    // rest of overlay helpers (unchanged)
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
        CheckForSuccess();
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

    string GetHeldString() => new string(heldChars);
}
