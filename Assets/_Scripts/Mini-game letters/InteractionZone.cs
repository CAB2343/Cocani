using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class InteractionZone : MonoBehaviour
{
    [Header("Who can interact")]
    public LayerMask allowedLayers = ~0;
    public string[] allowedTags = new string[0]; // empty = ignore tag

    [Header("Input")]
    public KeyCode interactKey = KeyCode.F;
    public bool requireHold = false;
    public float holdDuration = 0.8f;

    [Header("UI Prompt")]
    public GameObject promptRoot;      // painel (setActive true/false)
    public TMP_Text promptText;        // "Aperte F para interagir"
    public Image progressFill;         // optional circular/linear fill for hold

    [Header("Behaviour")]
    public bool singleUse = false;
    public float cooldown = 0f;

    [Header("Events")]
    public UnityEvent OnEnter;
    public UnityEvent OnExit;
    public UnityEvent OnInteract;

    bool playerInRange = false;
    bool isInteracting = false;
    bool isUsed = false;
    float lastUsedTime = -999f;
    Coroutine holdCoroutine;

    void Reset() { // helper default
        allowedTags = new string[] { "Player" };
    }

    void Start()
    {
        if (promptRoot != null) promptRoot.SetActive(false);
        if (progressFill != null) progressFill.fillAmount = 0f;
    }

    void Update()
    {
        if (!playerInRange || isInteracting || isUsed) return;
        if (Time.time - lastUsedTime < cooldown) return;

        if (!requireHold)
        {
            if (Input.GetKeyDown(interactKey))
                TriggerInteract();
        }
        else
        {
            if (Input.GetKeyDown(interactKey))
                holdCoroutine = StartCoroutine(HoldRoutine());
            if (Input.GetKeyUp(interactKey))
            {
                if (holdCoroutine != null) StopCoroutine(holdCoroutine);
                ResetProgress();
            }
        }
    }

    IEnumerator HoldRoutine()
    {
        isInteracting = true;
        float t = 0f;
        while (t < holdDuration)
        {
            t += Time.deltaTime;
            if (progressFill != null) progressFill.fillAmount = Mathf.Clamp01(t / holdDuration);
            yield return null;
        }
        ResetProgress();
        isInteracting = false;
        TriggerInteract();
    }

    void ResetProgress()
    {
        isInteracting = false;
        if (progressFill != null) progressFill.fillAmount = 0f;
    }

    void TriggerInteract()
    {
        isInteracting = true;
        OnInteract?.Invoke();
        lastUsedTime = Time.time;
        if (singleUse) isUsed = true;
        // small unlock to allow replayed interactions after event handlers finish
        StartCoroutine(EndInteractionNextFrame());
    }

    IEnumerator EndInteractionNextFrame()
    {
        yield return null;
        isInteracting = false;
    }

    // External hookup for new Input System or other scripts
    public void ExternalInteract() => TriggerInteract();

    #region Trigger detection (3D + 2D)
    void OnTriggerEnter(Collider other) => HandleEnter(other.gameObject);
    void OnTriggerExit(Collider other) => HandleExit(other.gameObject);
    void OnTriggerEnter2D(Collider2D other) => HandleEnter(other.gameObject);
    void OnTriggerExit2D(Collider2D other) => HandleExit(other.gameObject);

    void HandleEnter(GameObject go)
    {
        if (!IsAllowed(go)) return;
        playerInRange = true;
        if (promptRoot != null) promptRoot.SetActive(true);
        OnEnter?.Invoke();
    }

    void HandleExit(GameObject go)
    {
        if (!IsAllowed(go)) return;
        playerInRange = false;
        if (promptRoot != null) promptRoot.SetActive(false);
        ResetProgress();
        OnExit?.Invoke();
    }

    bool IsAllowed(GameObject go)
    {
        if (((1 << go.layer) & allowedLayers) == 0) return false;
        if (allowedTags.Length == 0) return true;
        foreach (var t in allowedTags) if (!string.IsNullOrEmpty(t) && go.CompareTag(t)) return true;
        return false;
    }
    #endregion

    void OnDrawGizmosSelected()
    {
        var col = GetComponent<Collider>();
        if (col == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.matrix = transform.localToWorldMatrix;
        if (col is BoxCollider b) Gizmos.DrawWireCube(b.center, b.size);
        else Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }
}
