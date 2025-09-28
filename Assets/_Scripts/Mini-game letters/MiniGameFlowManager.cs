using System.Collections;
using UnityEngine;

/// <summary>
/// Orquestra Start, Success, Fail, Close, Reset.
/// Mostra derrota visível por 'failVisibleDelay' antes de fechar e resetar.
/// </summary>
public class MiniGameFlowManager : MonoBehaviour
{
    [Header("Refs")]
    public GridManager gridManager;
    public MiniGameController miniGameController;
    public PlayerTrioController playerTrio;
    public MiniGameImageSwitcher imageSwitcher;

    [Header("Delays")]
    public float successCloseDelay = 3f;
    public float failCloseDelay = 3f;
    [Tooltip("Quanto tempo a derrota fica visível antes de fechar a UI.")]
    public float failVisibleDelay = 2f;

    [Header("Result colors")]
    public Color successColor = Color.green;
    public Color failColor = Color.red;

    [Header("Fail behavior")]
    [Tooltip("Se true, pinta apenas as células controladas pelo jogador ao falhar. Caso contrário pinta todas.")]
    public bool failPaintOnlyControlled = true;

    bool flowLocked = false;

    public void StartMiniGame()
    {
        if (flowLocked) return;
        Debug.Log("MiniGameFlowManager: StartMiniGame");
        if (gridManager != null) gridManager.ResetMiniGame();

        if (imageSwitcher != null) imageSwitcher.OnStartImmediate();

        if (playerTrio != null) playerTrio.ResetMiniGameTimer();
        if (playerTrio != null) playerTrio.StartMiniGame();
    }

    public void OnPlayerSuccess()
    {
        if (flowLocked) return;
        flowLocked = true;
        Debug.Log("MiniGameFlowManager: OnPlayerSuccess");

        if (gridManager != null) gridManager.MiniGameSuccess(successColor);

        if (imageSwitcher != null) imageSwitcher.OnSuccessImmediate();

        PaintPlayerControlledCells(successColor);
        StartCoroutine(SuccessEndSequence());
    }

    public void OnPlayerFail()
    {
        if (flowLocked) return;
        flowLocked = true;
        Debug.Log("MiniGameFlowManager: OnPlayerFail");

        // show defeat visual immediately
        if (imageSwitcher != null)
        {
            imageSwitcher.OnFailImmediate();
            Debug.Log("MiniGameFlowManager: imageSwitcher.OnFailImmediate() called.");
        }

        // determine controlled cells
        Cell[] controlled = null;
        if (playerTrio != null && playerTrio.curRow >= 0)
        {
            controlled = new Cell[3];
            for (int i = 0; i < 3; i++)
                controlled[i] = gridManager?.GetCell(playerTrio.curRow, playerTrio.curColStart + i);
        }

        // ensure grid's resetDelay is at least as long as the visible delay
        float targetResetDelay = (gridManager != null) ? Mathf.Max(gridManager.failureResetDelay, failVisibleDelay + 0.1f) : failVisibleDelay + 0.1f;

        if (gridManager != null)
        {
            if (failPaintOnlyControlled)
                gridManager.MiniGameFail(failColor, targetResetDelay, controlled);
            else
                gridManager.MiniGameFail(failColor, targetResetDelay, null);
        }

        // immediate feedback on player slots
        PaintPlayerSlots(failColor);

        // wait visible delay, then close UI, then unlock after reset completes
        StartCoroutine(FailSequenceVisible(targetResetDelay));
    }

    IEnumerator FailSequenceVisible(float gridResetDelay)
    {
        // show defeat for this time
        yield return new WaitForSeconds(failVisibleDelay);

        if (miniGameController != null)
        {
            miniGameController.Close();
            Debug.Log("MiniGameFlowManager: miniGameController.Close() called after visible fail delay.");
        }

        // unlock after grid reset finishes
        yield return new WaitForSeconds(gridResetDelay + 0.05f - failVisibleDelay);
        flowLocked = false;
        Debug.Log("MiniGameFlowManager: flow unlocked after fail/reset.");
    }

    IEnumerator SuccessEndSequence()
    {
        yield return new WaitForSeconds(successCloseDelay);

        if (miniGameController != null) miniGameController.Close();
        Debug.Log("MiniGameFlowManager: Closed UI after success delay.");

        if (gridManager != null)
        {
            gridManager.ResetMiniGame();
            Debug.Log("MiniGameFlowManager: Grid reset after success.");
        }

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
