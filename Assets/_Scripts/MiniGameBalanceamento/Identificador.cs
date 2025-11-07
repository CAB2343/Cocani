using System.Collections.Generic;
using UnityEngine;
using System.Collections; // Adicionado para suportar IEnumerator

public class Identificador : MonoBehaviour
{
    [Tooltip("Tag a ser gerenciada (edite pelo Inspector)")]
    public string tagToManage = "Enemy";

    [Tooltip("Número máximo de objetos com essa tag que podem estar ativos ao mesmo tempo")]
    [Range(1, 100)]
    public int maxActive = 5;

    [Tooltip("Intervalo (s) para checar e aplicar o limite. 0 = só em Start e quando EnforceNow() for chamado")]
    public float updateInterval = 0.5f;

    [Header("Comportamento")]
    [Tooltip("Se true, garante hard-limit: se existirem mais ativos que maxActive, desativa os excedentes")]
    public bool enforceHardLimit = true;

    [Header("Debug")]
    public bool enableDebug = true;
    [Tooltip("Se true, só loga quando a contagem ativa muda")]
    public bool logOnlyWhenChanged = true;

    // estado
    public int CurrentActiveCount { get; private set; }

    private int lastLoggedCount = int.MinValue;

    void Start()
    {
        // validar
        if (maxActive < 1) maxActive = 1;

        if (updateInterval > 0f)
            InvokeRepeating(nameof(EnforceNow), 0f, updateInterval);
        else
            EnforceNow();
    }

    void OnValidate()
    {
        if (maxActive < 1) maxActive = 1;
        // se alterar pelo Inspector durante Play e updateInterval == 0, aplica imediatamente
        if (Application.isPlaying && updateInterval <= 0f)
            EnforceNow();
    }

    /// <summary>
    /// Enforce agora: busca todos os GameObjects (incluindo inativos),
    /// filtra pela tag e aplica o limite ativando/desativando conforme necessário.
    /// </summary>
    [ContextMenu("Enforce Now")]
    public void EnforceNow()
    {
        if (string.IsNullOrEmpty(tagToManage))
        {
            CurrentActiveCount = 0;
            if (enableDebug) Debug.Log("[TagActivatorWithLimit] tagToManage está vazia.");
            return;
        }

        try
        {
            // FindObjectsOfType<GameObject>(true) retorna objetos incluindo inativos (Unity 2020+)
            GameObject[] all = FindObjectsOfType<GameObject>(true);

            List<GameObject> tagged = new List<GameObject>(16);
            for (int i = 0; i < all.Length; i++)
            {
                GameObject go = all[i];
                // CompareTag é mais rápido e evita exceção quando tag não existe?
                // Note: se a tag não existir, CompareTag lança exceção, então protegemos abaixo.
                try
                {
                    if (go.CompareTag(tagToManage))
                        tagged.Add(go);
                }
                catch (UnityException)
                {
                    // tag não existe no projeto
                    if (enableDebug) Debug.LogWarning($"[TagActivatorWithLimit] A tag '{tagToManage}' não existe no projeto (verifique Tags & Layers).");
                    CurrentActiveCount = 0;
                    return;
                }
            }

            // conta ativos
            int activeCount = 0;
            for (int i = 0; i < tagged.Count; i++)
                if (tagged[i].activeInHierarchy) activeCount++;

            // se houver mais ativos do que o permitido -> desativar extras (se enforceHardLimit)
            if (enforceHardLimit && activeCount > maxActive)
            {
                // desativa excedentes — começa do fim da lista para não afetar índices iniciais
                for (int i = tagged.Count - 1; i >= 0 && activeCount > maxActive; i--)
                {
                    GameObject go = tagged[i];
                    if (go.activeInHierarchy)
                    {
                        go.SetActive(false);
                        activeCount--;
                        if (enableDebug) Debug.Log($"[TagActivatorWithLimit] Desativado (excedente) -> {go.name}");
                    }
                }
            }

            // se houver menos ativos do que o permitido -> ativar inativos até o limite
            if (activeCount < maxActive)
            {
                for (int i = 0; i < tagged.Count && activeCount < maxActive; i++)
                {
                    GameObject go = tagged[i];
                    if (!go.activeInHierarchy)
                    {
                        go.SetActive(true);
                        activeCount++;
                        if (enableDebug) Debug.Log($"[TagActivatorWithLimit] Ativado -> {go.name}");
                    }
                }
            }

            CurrentActiveCount = activeCount;

            if (enableDebug)
            {
                if (!logOnlyWhenChanged || CurrentActiveCount != lastLoggedCount)
                {
                    Debug.Log($"[TagActivatorWithLimit] Tag='{tagToManage}' TotalEncontrados={tagged.Count}, AtivosAgora={CurrentActiveCount} (maxActive={maxActive})");
                    lastLoggedCount = CurrentActiveCount;
                }
            }
        }
        catch (System.Exception ex)
        {
            CurrentActiveCount = 0;
            Debug.LogError($"[TagActivatorWithLimit] Exceção ao processar tag '{tagToManage}': {ex.Message}\n{ex.StackTrace}");
        }
    }

    // --- NOVOS MÉTODOS ADICIONADOS ---

    // Desativa um GameObject específico (se ele tiver a tag gerenciada)
    public void DeactivateGameObject(GameObject go)
    {
        if (go == null) return;

        try
        {
            if (!string.IsNullOrEmpty(tagToManage) && go.CompareTag(tagToManage))
            {
                if (go.activeInHierarchy)
                {
                    go.SetActive(false);
                    // atualiza contador (não confie só no EnforceNow imediato)
                    CurrentActiveCount = Mathf.Max(0, CurrentActiveCount - 1);
                    if (enableDebug) Debug.Log($"[TagActivatorWithLimit] Deactivated specific -> {go.name}");
                }
                else if (enableDebug)
                {
                    Debug.Log($"[TagActivatorWithLimit] O objeto já estava inativo -> {go.name}");
                }
            }
            else if (enableDebug)
            {
                Debug.Log($"[TagActivatorWithLimit] DeactivateGameObject: objeto não tem a tag '{tagToManage}' -> {go.name}");
            }
        }
        catch (UnityException)
        {
            if (enableDebug) Debug.LogWarning($"[TagActivatorWithLimit] Tag '{tagToManage}' não existe no projeto.");
        }
    }

    // Ativa o próximo GameObject com a mesma tag após delay (procura um inativo e ativa)
    public IEnumerator ActivateNextWithTagAfter(string tag, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (string.IsNullOrEmpty(tag))
        {
            if (enableDebug) Debug.LogWarning("[TagActivatorWithLimit] ActivateNextWithTagAfter: tag vazia.");
            yield break;
        }

        try
        {
            GameObject[] all = FindObjectsOfType<GameObject>(true);
            for (int i = 0; i < all.Length; i++)
            {
                var go = all[i];
                try
                {
                    if (go.CompareTag(tag) && !go.activeInHierarchy)
                    {
                        go.SetActive(true);
                        CurrentActiveCount++;
                        if (enableDebug) Debug.Log($"[TagActivatorWithLimit] Ativado after delay -> {go.name}");
                        yield break;
                    }
                }
                catch (UnityException)
                {
                    if (enableDebug) Debug.LogWarning($"[TagActivatorWithLimit] Tag '{tag}' não existe no projeto.");
                    yield break;
                }
            }

            // se não encontrou nenhum inativo, chama EnforceNow para garantir consistência
            EnforceNow();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[TagActivatorWithLimit] Erro em ActivateNextWithTagAfter: {ex.Message}");
        }
    }
}