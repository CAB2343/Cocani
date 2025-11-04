using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// MiniGameActivator.cs
// Coloque este arquivo em Assets/Scripts/MiniGameActivator.cs
// Versão atualizada: expõe a lista gerenciada, notifica listeners quando a lista muda
// e garante integração fácil com a sua ObjectiveBarMulti.

[DisallowMultipleComponent]
public class MiniGameActivator : MonoBehaviour
{
    public enum DetectionMode { ByPrefabList, ByTag }

    [Header("Detecção")]
    public DetectionMode detectionMode = DetectionMode.ByTag;

    [Tooltip("Usado quando DetectionMode == ByPrefabList: arraste os prefabs (assets) que você quer controlar.")]
    public List<GameObject> targetPrefabs = new List<GameObject>();

    [Tooltip("Usado quando DetectionMode == ByTag: escolha a tag que identifica todos os mini-games (ex: 'minigame').")]
    public string tagName = "minigame";

    [Header("Ativação")]
    [Min(0)] public int enableCount = 5;
    public bool randomizeSelection = true;

    [Header("Timing")]
    [Tooltip("Delay inicial (segundos) antes de procurar as instâncias. Útil para esperar a geração procedural terminar.")]
    public float delayAfterGeneration = 0.15f; // espera inicial para geração procedural
    [Tooltip("Tempo (segundos) entre terminar um mini-game e ativar outro para repor o número de ativos.")]
    public float respawnDelay = 2f; // espera entre terminar e ativar outro

    [Header("Opções")]
    public bool forceDisableOthers = true; // desativa todas as instâncias candidatas antes de ativar selecionadas
    public bool avoidReactivatingCompletedInstances = true;

    // runtime
    // candidatos encontrados na cena (instâncias de prefabs ou objetos com tag)
    private List<GameObject> candidates = new List<GameObject>();
    private HashSet<int> completedInstanceIDs = new HashSet<int>();
    private System.Random rng = new System.Random();

    public static MiniGameActivator Instance { get; private set; }

    // Evento que a UI (ObjectiveBarMulti) pode assinar para saber quando a lista mudou
    public Action OnManagedListUpdated;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(InitialActivateRoutine());
    }

    private IEnumerator InitialActivateRoutine()
    {
        if (delayAfterGeneration > 0f)
            yield return new WaitForSeconds(delayAfterGeneration);

        RefreshCandidates();

        if (candidates.Count == 0)
        {
            Debug.LogWarning("[MiniGameActivator] Nenhum candidato encontrado. Verifique modo de detecção e tags/prefabs.");
            // notifica mesmo que esteja vazio para que a UI atualize
            OnManagedListUpdated?.Invoke();
            yield break;
        }

        if (forceDisableOthers)
        {
            foreach (var g in candidates)
                if (g != null)
                    g.SetActive(false);
        }

        if (randomizeSelection)
            ShuffleList(candidates);
        else
            candidates.Sort((a, b) => string.Compare(a.name, b.name));

        int toEnable = Mathf.Min(enableCount, candidates.Count);
        int enabled = 0;
        for (int i = 0; i < candidates.Count && enabled < toEnable; i++)
        {
            var g = candidates[i];
            if (g == null) continue;
            if (avoidReactivatingCompletedInstances && completedInstanceIDs.Contains(g.GetInstanceID())) continue;
            g.SetActive(true);
            enabled++;
        }

        Debug.Log($"[MiniGameActivator] Inicial: {enabled} ativados de {candidates.Count} candidatos.");

        // notifica UI/assinantes que a lista gerenciada foi atualizada
        OnManagedListUpdated?.Invoke();
    }

    /// <summary>
    /// Recalcula a lista de candidatos na cena com base no modo de detecção.
    /// Invoca OnManagedListUpdated ao final.
    /// </summary>
    private void RefreshCandidates()
    {
        candidates.Clear();

        if (detectionMode == DetectionMode.ByTag)
        {
            // Find inactive objects with tag: Resources.FindObjectsOfTypeAll + filter cenas
            GameObject[] all = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var go in all)
            {
                if (go == null) continue;
                // filtra assets e objetos de cenas inválidas
                if (!go.scene.IsValid()) continue;
                // assegura que o objeto pertence a cena carregada (ignora assets/prefabs em Project)
                // CompareTag pode lançar se a tag não existe — proteja com try/catch
                try
                {
                    if (!string.IsNullOrEmpty(tagName) && go.CompareTag(tagName))
                        candidates.Add(go);
                }
                catch (UnityException)
                {
                    // tag não definida no projeto; ignora
                }
            }
        }
        else // ByPrefabList
        {
            GameObject[] all = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var go in all)
            {
                if (go == null) continue;
                if (!go.scene.IsValid()) continue;
                foreach (var pf in targetPrefabs)
                {
                    if (pf == null) continue;
                    // comparações por nome (fallback robusto) — remove o sufixo "(Clone)" e variações
                    if (go.name.StartsWith(pf.name, StringComparison.Ordinal))
                    {
                        candidates.Add(go);
                        break;
                    }
                }
            }
        }

        // torna a lista única e consistente
        var uniq = new List<GameObject>();
        var seen = new HashSet<int>();
        foreach (var g in candidates)
        {
            if (g == null) continue;
            int id = g.GetInstanceID();
            if (!seen.Contains(id)) { seen.Add(id); uniq.Add(g); }
        }
        candidates = uniq;

        // notifica assinantes
        OnManagedListUpdated?.Invoke();
    }

    /// <summary>
    /// Chamada pelos mini-games quando terminam. Mantém o contador de completados e tenta ativar outro após respawnDelay.
    /// </summary>
    public void NotifyMiniGameCompleted(GameObject instance)
    {
        if (instance == null) return;
        int id = instance.GetInstanceID();
        completedInstanceIDs.Add(id);
        instance.SetActive(false);

        // recalc candidatos e tenta manter o número ativo
        RefreshCandidates();
        int activeCount = 0;
        foreach (var g in candidates)
            if (g != null && g.activeInHierarchy) activeCount++;

        // notifica imediatamente (lista mudou)
        OnManagedListUpdated?.Invoke();

        if (activeCount < enableCount)
            StartCoroutine(DelayedActivateNext(respawnDelay));
    }

    private IEnumerator DelayedActivateNext(float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);

        RefreshCandidates();

        List<GameObject> options = new List<GameObject>();
        foreach (var g in candidates)
        {
            if (g == null) continue;
            if (g.activeInHierarchy) continue;
            if (avoidReactivatingCompletedInstances && completedInstanceIDs.Contains(g.GetInstanceID())) continue;
            options.Add(g);
        }

        if (options.Count == 0)
        {
            Debug.Log("[MiniGameActivator] Nenhum candidato disponível para reativar.");
            yield break;
        }

        if (randomizeSelection)
            ShuffleList(options);
        else
            options.Sort((a, b) => string.Compare(a.name, b.name));

        options[0].SetActive(true);

        // após reativar, atualiza a lista e notifica
        RefreshCandidates();
        OnManagedListUpdated?.Invoke();

        Debug.Log("[MiniGameActivator] Reativado 1 mini-game (após conclusão de outro).");
    }

    // util: embaralhamento Fisher-Yates
    private void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        for (int i = n - 1; i > 0; i--)
        {
            int k = rng.Next(i + 1);
            T tmp = list[i];
            list[i] = list[k];
            list[k] = tmp;
        }
    }

    // ----------------- API pública para UI / outras partes do jogo -----------------

    /// <summary>
    /// Retorna as transforms que este ativador gerencia (opcionalmente apenas as ativas).
    /// </summary>
    public List<Transform> GetManagedTransforms(bool onlyActive = false)
    {
        var list = new List<Transform>();
        foreach (var g in candidates)
        {
            if (g == null) continue;
            if (!g.scene.IsValid()) continue;
            if (onlyActive && !g.activeInHierarchy) continue;
            list.Add(g.transform);
        }
        return list;
    }

    /// <summary>
    /// Retorna os GameObjects gerenciados (útil para a UI que quer ativar/desativar ou ler estado diretamente).
    /// </summary>
    public List<GameObject> GetManagedGameObjects(bool onlyActive = false)
    {
        var list = new List<GameObject>();
        foreach (var g in candidates)
        {
            if (g == null) continue;
            if (!g.scene.IsValid()) continue;
            if (onlyActive && !g.activeInHierarchy) continue;
            list.Add(g);
        }
        return list;
    }

    /// <summary>
    /// Força uma atualização da lista (útil para editor ou para chamar quando sua geração procedural terminar).
    /// </summary>
    public void TriggerRefresh()
    {
        RefreshCandidates();
        OnManagedListUpdated?.Invoke();
    }
}

/*
USO (resumo):
- Coloque este script num GameObject da cena (GameManager).
- No Inspector, defina DetectionMode = ByTag e tagName = "minigame" (ou a tag que você usa).
- Garanta que os prefabs de mini-game tenham essa tag (prefab asset ou instâncias herdaremão a tag).
- Defina enableCount = 5 e deixe as instâncias desativadas por padrão se quiser. O script buscará objetos inativos também.
- Quando um mini-game termina, chame MiniGameActivator.Instance.NotifyMiniGameCompleted(this.gameObject);
- Para integrar com a ObjectiveBarMulti, arraste o componente MiniGameActivator para o campo "activator" da UI.
*/
