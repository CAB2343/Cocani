using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ObjectiveBarMulti.cs
// Atualizado para integrar com MiniGameActivator.
// Coloque este arquivo em Assets/Scripts/ObjectiveBarMulti.cs

public class ObjectiveBarMulti : MonoBehaviour
{
    [Header("UI Elements (configure no Inspector)")]
    public RectTransform barRect;                // o painel da barra (Image)
    public RectTransform markerPrefab;           // prefab UI (Image) para cada objetivo (filho da barra)
    public TMP_Text labelN;                      // filho "N"
    public TMP_Text labelL;                      // filho "L" (Leste)
    public TMP_Text labelS;                      // filho "S"
    public TMP_Text labelO;                      // filho "O" (Oeste)
    public TMP_Text headingLabel;                // mostra distância quando olhando para um objetivo, ou vazio caso contrário

    [Header("References")]
    public Camera referenceCamera;               // câmera que representa o olhar do jogador (recomendado)
    public Transform player;                     // fallback se camera null (se seu player tem tag "Player" o script tentará encontrá-lo)

    [Header("Detection (fallback se activator não estiver setado)")]
    public GameObject[] objectivePrefabs;
    public string objectiveTag = "";

    [Header("Behaviour")]
    public float smoothSpeed = 8f;
    public float detectInterval = 0.5f;
    public bool clampToBar = true;
    public Vector2 markerSize = Vector2.zero;

    [Header("Look & Highlight (simple scale)")]
    [Tooltip("Multiplicador aplicado à escala base do prefab quando o objetivo está sendo olhado.")]
    public float growthMultiplier = 1.4f;
    [Tooltip("Velocidade de suavização da escala (quanto maior, mais rápido cresce/volta).")]
    public float scaleSmoothSpeed = 12f;
    [Tooltip("Ângulo máximo (graus) entre camera.forward e direção ao objetivo para considerarmos que 'estamos olhando' para ele.")]
    public float lookAngleThreshold = 10f;

    [Header("Link to MiniGameActivator (opcional)")]
    [Tooltip("Se atribuído, a barra usará apenas a lista gerenciada pelo activator (recomendado).")]
    public MiniGameActivator activator;

    private Dictionary<Transform, MarkerData> markers = new Dictionary<Transform, MarkerData>();
    private float detectTimer = 0f;
    private Transform lookedTarget = null;

    class MarkerData
    {
        public RectTransform ui;
        public float curX;
        public Vector3 baseScale;      // escala inicial do prefab (usada como referência)
        public float curScaleFactor;   // fator multiplicador atual (1 = baseScale)
    }

    void Awake()
    {
        if (barRect == null) Debug.LogError("ObjectiveBarMulti: barRect não setado.");
        if (markerPrefab == null) Debug.LogError("ObjectiveBarMulti: markerPrefab não setado (crie um UI Image).");

        if (markerPrefab != null)
            markerPrefab.gameObject.SetActive(false);
    }

    void Start()
    {
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(barRect);
        UpdateCardinalLabels();

        if (activator != null)
        {
            // assina atualizações do activator
            activator.OnManagedListUpdated += OnActivatorUpdated;
            // faz uma detecção inicial a partir do activator
            PerformDetectionUsingActivator();
        }
    }

    void OnDestroy()
    {
        if (activator != null)
            activator.OnManagedListUpdated -= OnActivatorUpdated;
    }

    void Update()
    {
        detectTimer += Time.deltaTime;
        if (detectTimer >= detectInterval)
        {
            detectTimer = 0f;
            if (activator != null) PerformDetectionUsingActivator();
            else PerformDetection();
        }

        UpdateCardinalLabels();
        ComputeLookedTarget();
        UpdateHeadingLabel();
        UpdateMarkers();
    }

    // ---------- NOVO: usa lista do activator, caso exista ----------
    void OnActivatorUpdated() => PerformDetectionUsingActivator();

    void PerformDetectionUsingActivator()
    {
        if (activator == null) return;

        // obtém transforms gerenciados pelo activator (somente ativos por padrão)
        List<Transform> managed = activator.GetManagedTransforms(onlyActive: true);

        // opcional: respeitar enableCount (normalmente activator já faz isso)
        int limit = Mathf.Max(1, activator.enableCount);
        if (managed.Count > limit)
        {
            // se haver mais do que o limite, cortamos a lista (não deveria ocorrer se activator mantiver apenas enableCount ativos)
            managed.RemoveRange(limit, managed.Count - limit);
        }

        HashSet<Transform> found = new HashSet<Transform>(managed);

        foreach (Transform t in found)
            if (!markers.ContainsKey(t))
                CreateMarkerForTarget(t);

        List<Transform> toRemove = new List<Transform>();
        foreach (var kv in markers)
        {
            Transform target = kv.Key;
            if (target == null || !found.Contains(target))
            {
                if (kv.Value != null && kv.Value.ui != null)
                    Destroy(kv.Value.ui.gameObject);
                toRemove.Add(target);
            }
        }
        foreach (var r in toRemove) markers.Remove(r);
    }
    // -----------------------------------------------------------------

    // ----------------- EXISTENTE: varredura antiga (fallback) -----------------
    void PerformDetection()
    {
        HashSet<Transform> found = new HashSet<Transform>();

        if (!string.IsNullOrEmpty(objectiveTag))
        {
            GameObject[] objs = GameObject.FindGameObjectsWithTag(objectiveTag);
            foreach (GameObject go in objs) if (go != null && go.activeInHierarchy) found.Add(go.transform);
        }

        if (objectivePrefabs != null && objectivePrefabs.Length > 0)
        {
            Transform[] allTransforms = FindObjectsOfType<Transform>();
            foreach (GameObject prefab in objectivePrefabs)
            {
                if (prefab == null) continue;
                string baseName = prefab.name;
                foreach (Transform t in allTransforms)
                {
                    if (t == null || !t.gameObject.activeInHierarchy) continue;
                    if (t.name == baseName || t.name.StartsWith(baseName + " (") || t.name.StartsWith(baseName + "(") || t.name.StartsWith(baseName))
                    {
                        found.Add(t);
                    }
                }
            }
        }

        foreach (Transform t in found)
            if (!markers.ContainsKey(t))
                CreateMarkerForTarget(t);

        List<Transform> toRemove = new List<Transform>();
        foreach (var kv in markers)
        {
            Transform target = kv.Key;
            if (target == null || !target.gameObject.activeInHierarchy || !found.Contains(target))
            {
                if (kv.Value != null && kv.Value.ui != null)
                    Destroy(kv.Value.ui.gameObject);
                toRemove.Add(target);
            }
        }
        foreach (var r in toRemove) markers.Remove(r);
    }
    // -------------------------------------------------------------------------

    void CreateMarkerForTarget(Transform target)
    {
        if (markerPrefab == null || barRect == null) return;

        RectTransform inst = Instantiate(markerPrefab, barRect);
        inst.gameObject.SetActive(true);

        // anchors/pivot centralizados (mantemos a escala do prefab)
        inst.anchorMin = inst.anchorMax = new Vector2(0.5f, 0.5f);
        inst.pivot = new Vector2(0.5f, 0.5f);
        inst.anchoredPosition = Vector2.zero;
        inst.localEulerAngles = Vector3.zero;

        // Aplicar sizeDelta caso o usuário queira forçar um tamanho
        Vector2 defaultMarkerSize = new Vector2(24f, 24f);
        if (markerSize != Vector2.zero)
            inst.sizeDelta = markerSize;
        else
        {
            if (inst.sizeDelta.x <= 0f || inst.sizeDelta.y <= 0f || inst.sizeDelta.x > 200f || inst.sizeDelta.y > 200f)
                inst.sizeDelta = defaultMarkerSize;
        }

        inst.SetAsLastSibling();

        MarkerData d = new MarkerData
        {
            ui = inst,
            curX = inst.anchoredPosition.x,
            baseScale = inst.localScale,
            curScaleFactor = 1f
        };

        inst.localScale = d.baseScale * d.curScaleFactor;

        markers.Add(target, d);
    }

    // helper: retorna a transform de referência (camera/player atual) de forma robusta
    Transform GetReferenceTransform()
    {
        if (referenceCamera != null && referenceCamera.gameObject.activeInHierarchy)
            return referenceCamera.transform;

        if (player != null && player.gameObject.activeInHierarchy)
            return player;

        // tenta encontrar dinamicamente um GameObject com tag "Player"
        GameObject found = null;
        try
        {
            found = GameObject.FindWithTag("Player");
        }
        catch { found = null; }

        if (found != null)
        {
            player = found.transform; // atualiza cache
            return player;
        }

        // fallback para Camera.main se existir
        if (Camera.main != null)
            return Camera.main.transform;

        return null;
    }

    // ---------- UPDATED: ComputeLookedTarget (ignora eixo Y ao comparar direções) ----------
    void ComputeLookedTarget()
    {
        lookedTarget = null;
        Transform refT = GetReferenceTransform();
        if (refT == null || markers.Count == 0) return;

        Vector3 refPos = refT.position;

        // flatten the camera/player forward to XZ plane so vertical difference is ignored
        Vector3 refForwardFlat = Vector3.ProjectOnPlane(refT.forward, Vector3.up);
        if (refForwardFlat.sqrMagnitude < 0.0001f)
        {
            if (player != null) refForwardFlat = Vector3.ProjectOnPlane(player.forward, Vector3.up);
            if (refForwardFlat.sqrMagnitude < 0.0001f) refForwardFlat = Vector3.ProjectOnPlane(Vector3.forward, Vector3.up);
        }
        refForwardFlat.Normalize();

        float bestAngle = lookAngleThreshold;
        Transform best = null;

        foreach (Transform t in markers.Keys)
        {
            if (t == null || !t.gameObject.activeInHierarchy) continue;

            // direction to target flattened (ignore Y)
            Vector3 dir = t.position - refPos;
            Vector3 dirFlat = Vector3.ProjectOnPlane(dir, Vector3.up);
            if (dirFlat.sqrMagnitude < 0.0001f) continue;

            dirFlat.Normalize();

            // angle between flattened forward and flattened direction
            float angle = Vector3.Angle(refForwardFlat, dirFlat);

            if (angle <= bestAngle)
            {
                bestAngle = angle;
                best = t;
            }
        }

        lookedTarget = best;
    }
    // -------------------------------------------------------------------------------

    void UpdateMarkers()
    {
        if (barRect == null) return;
        float halfWidth = barRect.rect.width * 0.5f;
        Transform refT = GetReferenceTransform();
        Vector3 refPos = refT != null ? refT.position : Vector3.zero;
        float refYaw = refT != null ? refT.eulerAngles.y : 0f;

        List<Transform> died = new List<Transform>();

        foreach (var kv in markers)
        {
            Transform target = kv.Key;
            MarkerData md = kv.Value;

            if (target == null || !target.gameObject.activeInHierarchy)
            {
                died.Add(target);
                continue;
            }

            Vector3 dir = target.position - refPos;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f) continue;
            float worldAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            if (worldAngle < 0f) worldAngle += 360f;
            float relativeAngle = Mathf.DeltaAngle(refYaw, worldAngle);
            float normalized = (relativeAngle + 180f) / 360f;
            float targetLocalX = Mathf.Lerp(-halfWidth, halfWidth, normalized);
            if (clampToBar) targetLocalX = Mathf.Clamp(targetLocalX, -halfWidth, halfWidth);

            float tPos = 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime);
            md.curX = Mathf.Lerp(md.curX, targetLocalX, tPos);
            Vector2 anchored = md.ui.anchoredPosition;
            md.ui.anchoredPosition = new Vector2(md.curX, anchored.y);

            // escala simplificada: usa a baseScale do prefab e um multiplicador configurável
            float desiredFactor = (target == lookedTarget) ? growthMultiplier : 1f;
            float tScale = 1f - Mathf.Exp(-scaleSmoothSpeed * Time.deltaTime);
            md.curScaleFactor = Mathf.Lerp(md.curScaleFactor, desiredFactor, tScale);

            // aplica escala: baseScale * curScaleFactor
            md.ui.localScale = md.baseScale * md.curScaleFactor;
        }

        foreach (Transform r in died) markers.Remove(r);
    }

    void UpdateCardinalLabels()
    {
        if (barRect == null) return;
        TMP_Text[] labels = new TMP_Text[] { labelN, labelL, labelS, labelO };
        float[] cardAngles = new float[] { 0f, 90f, 180f, 270f };

        Transform refT = GetReferenceTransform();
        float refYaw = refT != null ? refT.eulerAngles.y : 0f;
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

    void UpdateHeadingLabel()
    {
        if (headingLabel == null) return;

        Transform refT = GetReferenceTransform();
        if (refT == null)
        {
            headingLabel.text = "";
            return;
        }

        if (lookedTarget != null)
        {
            // distância sempre em relação à posição atual do transform de referência
            float distance = Vector3.Distance(refT.position, lookedTarget.position);
            headingLabel.text = $"{distance:F1} m";
        }
        else
        {
            // quando não está olhando para um objetivo, NÃO mostramos letras — apenas vazio
            headingLabel.text = "";
        }
    }

    string YawToCompassString(float yaw)
    {
        yaw = (yaw % 360f + 360f) % 360f;
        string[] names = { "N", "NNE", "NE", "ENE", "L", "ESE", "SE", "SSE", "S", "SSW", "SW", "WSW", "O", "ONO", "NO", "NNO" };
        int idx = Mathf.RoundToInt(yaw / 22.5f) % 16;
        return names[idx];
    }

    public void RegisterTarget(Transform t)
    {
        if (t == null) return;
        if (!markers.ContainsKey(t))
            CreateMarkerForTarget(t);
    }

    public void UnregisterTarget(Transform t)
    {
        if (t == null) return;
        if (markers.TryGetValue(t, out MarkerData md))
        {
            if (md.ui != null) Destroy(md.ui.gameObject);
            markers.Remove(t);
        }
    }

    public void ClearAllTargets()
    {
        foreach (var kv in markers)
            if (kv.Value != null && kv.Value.ui != null) Destroy(kv.Value.ui.gameObject);
        markers.Clear();
    }
}
