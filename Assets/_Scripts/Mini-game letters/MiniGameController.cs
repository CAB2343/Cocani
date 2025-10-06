using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class MiniGameController : MonoBehaviour
{
    [Header("UI")]
    public GameObject miniGameUI;

    [Header("Player Root")]
    public PlayerController1 playerRoot;
    public bool disablePlayerRootOnOpen = true;

    [Header("Movement scripts to disable while in mini-game")]
    public MonoBehaviour[] movementScripts;

    [Header("Cinemachine")]
    public CinemachineVirtualCamera[] virtualCameras;
    public CinemachineBrain cinemachineBrain;

    [Header("Mini-game hooks")]
    public PlayerTrioController playerTrio; // controla timer/start/stop
    public GridManager gridManager;         // reset do grid
    public bool resetGridOnOpen = true;

    [Header("Debug")]
    public bool exitWithEscForDebug = true;

    // runtime state
    bool prevPlayerRootEnabled = false;
    bool prevPlayerRootStored = false;
    Dictionary<MonoBehaviour, bool> movementPrevMap;
    bool[] vcamPrev;
    bool prevBrainEnabled;
    bool isOpen = false;

    void Awake()
    {
        // === AUTO REFERENCES ===
        if (miniGameUI == null)
            miniGameUI = GameObject.FindGameObjectWithTag("MiniGameUI");

        if (playerRoot == null)
            playerRoot = FindObjectOfType<PlayerController1>();

        if ((movementScripts == null || movementScripts.Length == 0) && playerRoot != null)
            movementScripts = playerRoot.GetComponents<MonoBehaviour>();

        if (virtualCameras == null || virtualCameras.Length == 0)
            virtualCameras = FindObjectsOfType<CinemachineVirtualCamera>();

        if (cinemachineBrain == null)
            cinemachineBrain = FindObjectOfType<CinemachineBrain>();

        if (playerTrio == null)
            playerTrio = FindObjectOfType<PlayerTrioController>();

        if (gridManager == null)
            gridManager = FindObjectOfType<GridManager>();
    }

    void Start()
    {
        if (miniGameUI != null)
            miniGameUI.SetActive(false);
    }

    void Update()
    {
        if (!exitWithEscForDebug) return;
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
            Close();
    }

    public void Open()
    {
        if (miniGameUI != null)
            miniGameUI.SetActive(true);

        if (isOpen) return;

        Debug.Log("MiniGameController: Open() called.");

        if (resetGridOnOpen && gridManager != null)
        {
            Debug.Log("MiniGameController: Resetting grid before opening mini-game.");
            gridManager.ResetMiniGame();
        }

        if (playerTrio != null)
        {
            playerTrio.ResetMiniGameTimer();
            playerTrio.StartMiniGame();
            Debug.Log("MiniGameController: PlayerTrio timer reset and started.");
        }

        isOpen = true;

        // playerRoot
        if (playerRoot != null)
        {
            prevPlayerRootEnabled = playerRoot.enabled;
            prevPlayerRootStored = true;
            if (disablePlayerRootOnOpen) playerRoot.enabled = false;
        }

        // movement scripts
        if (movementScripts != null && movementScripts.Length > 0)
        {
            if (movementPrevMap == null) movementPrevMap = new Dictionary<MonoBehaviour, bool>();
            for (int i = 0; i < movementScripts.Length; i++)
            {
                var mb = movementScripts[i];
                if (mb == null) continue;
                if (!movementPrevMap.ContainsKey(mb))
                    movementPrevMap[mb] = mb.enabled;
                mb.enabled = false;
            }
        }

        // virtual cameras
        if (virtualCameras != null && virtualCameras.Length > 0)
        {
            vcamPrev = new bool[virtualCameras.Length];
            for (int i = 0; i < virtualCameras.Length; i++)
            {
                var v = virtualCameras[i];
                if (v == null) continue;
                vcamPrev[i] = v.enabled;
                v.enabled = false;
            }
        }

        // cinemachine brain
        if (cinemachineBrain != null)
        {
            prevBrainEnabled = cinemachineBrain.enabled;
            cinemachineBrain.enabled = false;
        }
    }

    public void Close()
    {
        if (miniGameUI != null)
            miniGameUI.SetActive(false);

        if (!isOpen) return;

        Debug.Log("MiniGameController: Close() called.");

        if (playerTrio != null)
        {
            playerTrio.StopMiniGame();
            playerTrio.ResetMiniGameTimer();
            Debug.Log("MiniGameController: PlayerTrio timer stopped and reset.");
        }

        isOpen = false;

        // restore playerRoot
        if (playerRoot != null && prevPlayerRootStored)
            playerRoot.enabled = prevPlayerRootEnabled;

        // restore movement scripts
        if (movementPrevMap != null)
        {
            var keys = new List<MonoBehaviour>(movementPrevMap.Keys);
            foreach (var mb in keys)
            {
                if (mb == null) continue;
                if (movementPrevMap.TryGetValue(mb, out bool wasEnabled))
                    mb.enabled = wasEnabled;
            }
            movementPrevMap.Clear();
        }

        // restore virtual cameras
        if (virtualCameras != null && vcamPrev != null)
        {
            for (int i = 0; i < virtualCameras.Length; i++)
            {
                var v = virtualCameras[i];
                if (v == null) continue;
                v.enabled = vcamPrev.Length > i ? vcamPrev[i] : true;
            }
            vcamPrev = null;
        }

        // restore brain
        if (cinemachineBrain != null)
            cinemachineBrain.enabled = prevBrainEnabled;

        prevPlayerRootStored = false;
    }
}
