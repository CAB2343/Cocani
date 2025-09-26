using System.Collections;
using UnityEngine;

/// <summary>
/// Orquestra o fluxo do mini-game: Start, Success, Fail, Close, Reset.
/// Mantém delays e chama GridManager / MiniGameController / PlayerTrioController.
/// </summary>
public class MiniGameFlowManager : MonoBehaviour
{
    [Header("Refs")]
    public GridManager gridManager;
    public MiniGameController miniGameController; // controla UI open/close
    public PlayerTrioController playerTrio;       // pra obter estado e slots

    [Header("Delays")]
    public float successCloseDelay = 3f;
    public float failCloseDelay = 3f;

    [Header("Result colors")]
    public Color successColor = Color.green;
    public Color failColor = Color.red;

    [Header("Fail behavior")]
    [Tooltip("Se true, pinta apenas as células controladas pelo jogador ao falhar. Caso contrário pinta todas.")]
    public bool failPaintOnlyControlled = true;

    bool flowLocked = false;

    // chama quando abrir mini-game
    public void StartMiniGame()
    {
        if (flowLocked) return;
        Debug.Log("MiniGameFlowManager: StartMiniGame");
        if (gridManager != null) gridManager.ResetMiniGame(); // garante grid limpo
        if (playerTrio != null) playerTrio.ResetMiniGameTimer();
        if (playerTrio != null) playerTrio.StartMiniGame();
    }

    // PlayerTrio chama isso ao detectar sucesso
    public void OnPlayerSuccess()
    {
        if (flowLocked) return;
        flowLocked = true;
        Debug.Log("MiniGameFlowManager: OnPlayerSuccess");
        // pinta trio fixo + coluna do jogador via GridManager (keeps centralized)
        if (gridManager != null) gridManager.MiniGameSuccess(successColor);

        // também pinta controlled cells and slots for immediate visual
        PaintPlayerControlledCells(successColor);

        StartCoroutine(SuccessEndSequence());
    }

    // PlayerTrio calls this on timeout/fail
    public void OnPlayerFail()
    {
        if (flowLocked) return;
        flowLocked = true;
        Debug.Log("MiniGameFlowManager: OnPlayerFail");

        // determine which cells to color
        Cell[] controlled = null;
        if (playerTrio != null && playerTrio.curRow >= 0)
        {
            controlled = new Cell[3];
            for (int i = 0; i < 3; i++)
                controlled[i] = gridManager?.GetCell(playerTrio.curRow, playerTrio.curColStart + i);
        }

        if (gridManager != null)
        {
            if (failPaintOnlyControlled)
                gridManager.MiniGameFail(failColor, gridManager.failureResetDelay, controlled);
            else
                gridManager.MiniGameFail(failColor, gridManager.failureResetDelay, null);
        }

        // immediate feedback on player slots
        PaintPlayerSlots(failColor);

        StartCoroutine(FailEndSequence());
    }

    IEnumerator SuccessEndSequence()
    {
        yield return new WaitForSeconds(successCloseDelay);

        // depois do delay fecha UI e força reset para novo mini-game
        if (miniGameController != null) miniGameController.Close();
        Debug.Log("MiniGameFlowManager: Closed UI after success delay.");

        if (gridManager != null)
        {
            gridManager.ResetMiniGame();
            Debug.Log("MiniGameFlowManager: Grid reset after success.");
        }

        flowLocked = false;
    }

    IEnumerator FailEndSequence()
    {
        yield return new WaitForSeconds(failCloseDelay);

        if (miniGameController != null) miniGameController.Close();
        Debug.Log("MiniGameFlowManager: Closed UI after fail delay.");

        // GridManager already schedules reset after failureResetDelay.
        // Optionally force immediate reset:
        // if (gridManager != null) gridManager.ResetMiniGame();

        flowLocked = false;
    }

    void PaintPlayerControlledCells(Color color)
    {
        if (playerTrio == null || gridManager == null) return;
        int r = playerTrio.curRow;
        int c0 = playerTrio.curColStart;
        if (r < 0) return;
        for (int i = 0; i < 3; i++)
        {
            var cell = gridManager.GetCell(r, c0 + i);
            if (cell != null && cell.background != null) cell.background.color = color;
        }
    }

    void PaintPlayerSlots(Color color)
    {
        if (playerTrio == null) return;
        for (int i = 0; i < 3; i++)
        {
            var go = playerTrio.playerSlots[i];
            if (go == null) continue;
            var c = go.GetComponent<Cell>();
            if (c != null && c.background != null) c.background.color = color;
        }
    }
}
