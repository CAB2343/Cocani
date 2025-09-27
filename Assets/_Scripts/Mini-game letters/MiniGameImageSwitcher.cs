using System.Collections;
using UnityEngine;

/// <summary>
/// Switcher com 3 backgrounds distintos e métodos immediate/with-delay.
/// </summary>
public class MiniGameImageSwitcher : MonoBehaviour
{
    [Header("Backgrounds (assign UNIQUE objects)")]
    public GameObject inGameBackground;
    public GameObject victoryBackground;
    public GameObject defeatBackground;

    [Header("Delays (used only if you call OnSuccess/OnFail without Immediate)")]
    public float successDelay = 0f;
    public float failDelay = 0f;

    [Header("Auto-restore")]
    public GridManager gridManager;

    bool origInGameActive;
    bool origVictoryActive;
    bool origDefeatActive;

    void Awake()
    {
        origInGameActive = inGameBackground ? inGameBackground.activeSelf : false;
        origVictoryActive = victoryBackground ? victoryBackground.activeSelf : false;
        origDefeatActive = defeatBackground ? defeatBackground.activeSelf : false;
    }

    void OnEnable()
    {
        if (gridManager != null && gridManager.onGridReady != null)
            gridManager.onGridReady.AddListener(OnGridReady);
    }

    void OnDisable()
    {
        if (gridManager != null && gridManager.onGridReady != null)
            gridManager.onGridReady.RemoveListener(OnGridReady);
    }

    void OnGridReady()
    {
        RestoreOriginalState();
        OnStartImmediate();
        Debug.Log("MiniGameImageSwitcher: OnGridReady -> restored original and set in-game.");
    }

    public void OnStartImmediate()
    {
        SetOnly(inGameBackground);
    }

    public void OnStart()
    {
        OnStartImmediate();
        Debug.Log("MiniGameImageSwitcher: OnStart called.");
    }

    public void OnSuccess()
    {
        StopAllCoroutines();
        StartCoroutine(DoSwitchAfterDelay(successDelay, victoryBackground));
        Debug.Log($"MiniGameImageSwitcher: OnSuccess scheduled in {successDelay} s.");
    }

    public void OnFail()
    {
        StopAllCoroutines();
        StartCoroutine(DoSwitchAfterDelay(failDelay, defeatBackground));
        Debug.Log($"MiniGameImageSwitcher: OnFail scheduled in {failDelay} s.");
    }

    // métodos imediatos para aplicar visuals mesmo se a UI vai ser fechada depois
    public void OnSuccessImmediate()
    {
        StopAllCoroutines();
        SetOnly(victoryBackground);
        Debug.Log("MiniGameImageSwitcher: OnSuccessImmediate executed.");
    }

    public void OnFailImmediate()
    {
        StopAllCoroutines();
        SetOnly(defeatBackground);
        Debug.Log("MiniGameImageSwitcher: OnFailImmediate executed.");
    }

    IEnumerator DoSwitchAfterDelay(float delay, GameObject activate)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        SetOnly(activate);
        Debug.Log($"MiniGameImageSwitcher: DoSwitch executed -> activated {(activate!=null?activate.name:"null")}");
    }

    public void RestoreOriginalState()
    {
        if (inGameBackground != null) inGameBackground.SetActive(origInGameActive);
        if (victoryBackground != null) victoryBackground.SetActive(origVictoryActive);
        if (defeatBackground != null) defeatBackground.SetActive(origDefeatActive);
        Debug.Log("MiniGameImageSwitcher: RestoreOriginalState executed.");
    }

    void SetOnly(GameObject single)
    {
        if (inGameBackground != null) inGameBackground.SetActive(inGameBackground == single);
        if (victoryBackground != null) victoryBackground.SetActive(victoryBackground == single);
        if (defeatBackground != null) defeatBackground.SetActive(defeatBackground == single);
    }
}
