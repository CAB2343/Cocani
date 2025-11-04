using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
// using UnityEngine.UI.Extensions; // ❌ REMOVIDO — não necessário

public class ObjectiveBarController_PlayerRelative : MonoBehaviour
{
    public static ObjectiveBarController_PlayerRelative Instance { get; private set; }

    [Header("UI Elements")]
    public RectTransform barRect;
    public RectTransform pointer;
    public Text labelN;
    public Text labelE;
    public Text labelS;
    public Text labelW;

    [Header("References")]
    [Tooltip("Use a câmera que representa onde o jogador 'olha'. Se vazio, usa o transform 'player'.")]
    public Camera referenceCamera;
    public Transform player;

    [Header("Behaviour")]
    [Tooltip("Velocidade de suavização (quanto maior, mais rápido segue).")]
    public float smoothSpeed = 8f;

    [Tooltip("Habilite para que o pointer também gire visualmente como uma seta.")]
    public bool rotatePointer = false;

    [Tooltip("Se true, o pointer é limitado aos limites da barra (não sai das bordas).")]
    public bool clampToBar = true;

    private Transform currentTarget;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        Instance = this;

        if (barRect == null) Debug.LogError("ObjectiveBarController: barRect não setado.");
        if (pointer == null) Debug.LogError("ObjectiveBarController: pointer não setado.");

        if (pointer != null) pointer.gameObject.SetActive(false);
    }

    void Start()
    {
        Canvas.ForceUpdateCanvases();
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(barRect);
        UpdateCardinalLabels();
    }

    void Update()
    {
        UpdateCardinalLabels();

        if (currentTarget == null)
        {
            if (pointer.gameObject.activeSelf) pointer.gameObject.SetActive(false);
            return;
        }

        if (!pointer.gameObject.activeSelf) pointer.gameObject.SetActive(true);

        Vector3 refPos = referenceCamera != null ? referenceCamera.transform.position : (player != null ? player.position : Vector3.zero);
        Vector3 dir = currentTarget.position - refPos;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f) return;

        float worldAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        if (worldAngle < 0f) worldAngle += 360f;

        float refYaw = referenceCamera != null ? referenceCamera.transform.eulerAngles.y : (player != null ? player.eulerAngles.y : 0f);
        float relativeAngle = Mathf.DeltaAngle(refYaw, worldAngle);
        float normalized = (relativeAngle + 180f) / 360f;
        float halfWidth = barRect.rect.width * 0.5f;
        float targetLocalX = Mathf.Lerp(-halfWidth, halfWidth, normalized);

        if (clampToBar)
            targetLocalX = Mathf.Clamp(targetLocalX, -halfWidth, halfWidth);

        Vector2 cur = pointer.anchoredPosition;
        float t = 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime);
        float newX = Mathf.Lerp(cur.x, targetLocalX, t);
        pointer.anchoredPosition = new Vector2(newX, cur.y);

        if (rotatePointer)
            pointer.localEulerAngles = new Vector3(0f, 0f, -relativeAngle);
    }

    void UpdateCardinalLabels()
    {
        if (barRect == null) return;

        float[] cardAngles = new float[] { 0f, 90f, 180f, 270f };
        Text[] labels = new Text[] { labelN, labelE, labelS, labelW };

        float refYaw = referenceCamera != null ? referenceCamera.transform.eulerAngles.y : (player != null ? player.eulerAngles.y : 0f);
        float halfWidth = barRect.rect.width * 0.5f;

        for (int i = 0; i < labels.Length; i++)
        {
            if (labels[i] == null) continue;
            float worldCard = cardAngles[i];
            float rel = Mathf.DeltaAngle(refYaw, worldCard);
            float normalized = (rel + 180f) / 360f;
            float x = Mathf.Lerp(-halfWidth, halfWidth, normalized);

            RectTransform rt = labels[i].GetComponent<RectTransform>();
            if (rt != null)
                rt.anchoredPosition = new Vector2(x, rt.anchoredPosition.y);
        }
    }

    public void RegisterTarget(Transform t)
    {
        currentTarget = t;
        if (pointer != null) pointer.gameObject.SetActive(t != null);
    }

    public void ClearTarget()
    {
        currentTarget = null;
        if (pointer != null) pointer.gameObject.SetActive(false);
    }
}
