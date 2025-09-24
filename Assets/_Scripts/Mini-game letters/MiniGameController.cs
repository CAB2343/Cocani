using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class MiniGameController : MonoBehaviour
{
    public GameObject miniGameUI;
    public PlayerController1 playerRoot;
    public bool disablePlayerRootOnOpen = true;

    [Header("Movement scripts to disable while in mini-game")]
    public MonoBehaviour[] movementScripts;

    [Header("Cinemachine")]
    public CinemachineVirtualCamera[] virtualCameras;
    public CinemachineBrain cinemachineBrain;

    [Header("Debug")]
    public bool exitWithEscForDebug = true;

    // runtime state
    bool prevPlayerRootEnabled = false;
    bool prevPlayerRootStored = false;
    Dictionary<MonoBehaviour, bool> movementPrevMap;
    bool[] vcamPrev;
    bool prevBrainEnabled;
    bool isOpen = false;

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

        if (isOpen) return; // evita duplo open que sobrescreva estados

        isOpen = true;

        // playerRoot
        if (playerRoot != null)
        {
            prevPlayerRootEnabled = playerRoot.enabled;
            prevPlayerRootStored = true;
            if (disablePlayerRootOnOpen) playerRoot.enabled = false;
        }

        // movement scripts: salve por componente (não sobrescrever se já salvo)
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

        isOpen = false;

        // restore playerRoot
        if (playerRoot != null && prevPlayerRootStored)
            playerRoot.enabled = prevPlayerRootEnabled;

        // restore movement scripts from map
        if (movementPrevMap != null)
        {
            // snapshot keys to avoid collection-mod during iteration
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
