using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.UI.Extensions; // opcional, remova se não usar

// Se você usa TextMeshPro, troque Text por TMPro.TMP_Text e importe TMPro.
public class ObjectiveBarController_PlayerRelative : MonoBehaviour
{
    public static ObjectiveBarController_PlayerRelative Instance { get; private set; }

    [Header("UI Elements")]
    public RectTransform barRect;      // painel da barra
    public RectTransform pointer;      // marker que se move (filho de barRect)
    public Text labelN;
    public Text labelE;
    public Text labelS;
    public Text labelW;

    [Header("References")]
    [Tooltip("Use a câmera que representa onde o jogador 'olha'. Se vazio, usa o transform 'player'.")]
    public Camera referenceCamera;     // preferível: a câmera do jogador
    public Transform player;           // fallback (ex.: player root)

    [Header("Behaviour")]
    [Tooltip("Velocidade de suavização (quanto maior, mais rápido segue).")]
    public float smoothSpeed = 8f;

    [Tooltip("Habilite para que o pointer também gire visualmente como uma seta.")]
    public bool rotatePointer = false;

    [Tooltip("Se true, o pointer é limitado aos limites da barra (não sai das bordas).")]
    public bool clampToBar = true;

    // estado
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
        // força recriar layout para garantir que barRect.rect tenha valores corretos
        Canvas.ForceUpdateCanvases();
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(barRect);

        // posiciona inicial das labels
        UpdateCardinalLabels();
    }

    void Update()
    {
        // atualiza labels (movem-se conforme a rotação do jogador)
        UpdateCardinalLabels();

        if (currentTarget == null)
        {
            if (pointer.gameObject.activeSelf) pointer.gameObject.SetActive(false);
            return;
        }

        if (!pointer.gameObject.activeSelf) pointer.gameObject.SetActive(true);

        // 1) direção do alvo no plano XZ
        Vector3 refPos = referenceCamera != null ? referenceCamera.transform.position : (player != null ? player.position : Vector3.zero);
        Vector3 dir = currentTarget.position - refPos;
        dir.y = 0f; // ignoramos altura para bearing horizontal

        if (dir.sqrMagnitude < 0.0001f) return;

        // 2) ângulo do alvo no mundo (0 = world north Z+)
        float worldAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg; // 0..360 (ou -180..180)
        if (worldAngle < 0f) worldAngle += 360f;

        // 3) ângulo de referência (yaw) a partir da câmera ou player — representa "frente do jogador"
        float refYaw = 0f;
        if (referenceCamera != null) refYaw = referenceCamera.transform.eulerAngles.y;
        else if (player != null) refYaw = player.eulerAngles.y;

        // 4) ângulo relativo: quanto o target está à direita (+) / esquerda (-) da frente do jogador
        // Mathf.DeltaAngle(refYaw, worldAngle) retorna -180..180
        float relativeAngle = Mathf.DeltaAngle(refYaw, worldAngle); // -180..180

        // 5) normaliza para 0..1 onde 0.5 = 0° (frente). Mapeamos -180..180 -> 0..1
        float normalized = (relativeAngle + 180f) / 360f; // 0..1
        // target X na barra (anchored)
        float halfWidth = barRect.rect.width * 0.5f;
        float targetLocalX = Mathf.Lerp(-halfWidth, halfWidth, normalized);

        // opcionalmente clamp para não sair da barra
        if (clampToBar)
        {
            targetLocalX = Mathf.Clamp(targetLocalX, -halfWidth, halfWidth);
        }

        // 6) suaviza a movimentação do pointer (frame-rate independent)
        Vector2 cur = pointer.anchoredPosition;
        float t = 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime);
        float newX = Mathf.Lerp(cur.x, targetLocalX, t);
        pointer.anchoredPosition = new Vector2(newX, cur.y);

        // 7) rotaciona o pointer visualmente para apontar (opcional)
        if (rotatePointer)
        {
            // o pointer apontará conforme o relativeAngle: -180..180 -> z-rotation (inverter se sprite apontar pra cima)
            pointer.localEulerAngles = new Vector3(0f, 0f, -relativeAngle);
        }
    }

    // atualiza posição das labels N/E/S/W para que mostrem onde estas direções estão RELATIVAS à frente do jogador
    void UpdateCardinalLabels()
    {
        if (barRect == null) return;

        // ângulos do mundo para N(0), E(90), S(180), W(270)
        float[] cardAngles = new float[] { 0f, 90f, 180f, 270f };
        Text[] labels = new Text[] { labelN, labelE, labelS, labelW };

        float refYaw = referenceCamera != null ? referenceCamera.transform.eulerAngles.y : (player != null ? player.eulerAngles.y : 0f);
        float halfWidth = barRect.rect.width * 0.5f;

        for (int i = 0; i < labels.Length; i++)
        {
            if (labels[i] == null) continue;
            float worldCard = cardAngles[i];
            float rel = Mathf.DeltaAngle(refYaw, worldCard); // -180..180
            float normalized = (rel + 180f) / 360f; // 0..1
            float x = Mathf.Lerp(-halfWidth, halfWidth, normalized);

            RectTransform rt = labels[i].GetComponent<RectTransform>();
            if (rt != null)
                rt.anchoredPosition = new Vector2(x, rt.anchoredPosition.y);
        }
    }

    // --- API público ---
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
