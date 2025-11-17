using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Identificador : MonoBehaviour
{
    [Tooltip("Tag a ser ativada (edite pelo Inspector)")]
    public string tagToManage = "Enemy";

    [Tooltip("0 = sem limite (ativa todos os encontrados). >0 = ativa até este número por chamada de EnforceNow")]
    [Min(0)]
    public int maxToActivate = 0;

    [Tooltip("Se > 0, espaça a ativação entre objetos (em segundos) para evitar picos")]
    public float staggerInterval = 0f;

    [Header("Debug")]
    public bool enableDebug = true;

    // estado simples
    public int LastActivatedCount { get; private set; }

    private bool tagChecked = false;
    private bool tagExists = true;

    void Start()
    {
        EnforceNow();
    }

    void OnValidate()
    {
        if (maxToActivate < 0) maxToActivate = 0;
    }

    private void CheckTagOnce()
    {
        if (tagChecked) return;
        tagChecked = true;

        if (string.IsNullOrEmpty(tagToManage))
        {
            tagExists = false;
            if (enableDebug) Debug.LogWarning("[Identificador] tagToManage está vazia.");
            return;
        }

        try
        {
            gameObject.CompareTag(tagToManage);
            tagExists = true;
        }
        catch
        {
            tagExists = false;
            if (enableDebug) Debug.LogWarning($"[Identificador] A tag '{tagToManage}' não existe no projeto (Tags & Layers).");
        }
    }

    [ContextMenu("Enforce Now")]
    public void EnforceNow()
    {
        if (!tagChecked) CheckTagOnce();
        if (!tagExists) return;

        GameObject[] all = FindObjectsOfType<GameObject>(true);
        List<GameObject> toActivate = new List<GameObject>(32);

        for (int i = 0; i < all.Length; i++)
        {
            var go = all[i];
            if (go == null) continue;

            try
            {
                if (go.CompareTag(tagToManage) && !go.activeInHierarchy)
                    toActivate.Add(go);
            }
            catch (UnityException)
            {
                if (enableDebug) Debug.LogWarning($"[Identificador] Tag '{tagToManage}' não existe no projeto.");
                return;
            }
        }

        LastActivatedCount = 0;

        if (staggerInterval > 0f)
        {
            StartCoroutine(ActivateStaggered(toActivate));
            return;
        }

        int limit = (maxToActivate <= 0) ? int.MaxValue : maxToActivate;
        for (int i = 0; i < toActivate.Count && LastActivatedCount < limit; i++)
        {
            var go = toActivate[i];
            if (go == null) continue;
            EnsureParentsActive(go);
            go.SetActive(true);
            LastActivatedCount++;
            if (enableDebug) Debug.Log($"[Identificador] Ativado -> {go.name}");
        }

        if (enableDebug) Debug.Log($"[Identificador] EnforceNow: tentou ativar {toActivate.Count} encontrados, ativados agora={LastActivatedCount}");
    }

    private IEnumerator ActivateStaggered(List<GameObject> list)
    {
        int limit = (maxToActivate <= 0) ? int.MaxValue : maxToActivate;
        for (int i = 0; i < list.Count && LastActivatedCount < limit; i++)
        {
            var go = list[i];
            if (go == null) continue;
            EnsureParentsActive(go);
            go.SetActive(true);
            LastActivatedCount++;
            if (enableDebug) Debug.Log($"[Identificador] Ativado (staggered) -> {go.name}");
            yield return new WaitForSeconds(staggerInterval);
        }

        if (enableDebug) Debug.Log($"[Identificador] ActivateStaggered: total ativados={LastActivatedCount}");
    }

    // --- METHODS FOR COMPATIBILITY WITH GridManager ---

    // Desativa um GameObject específico (compatível com chamadas externas)
    public void DeactivateGameObject(GameObject go)
    {
        if (go == null) return;
        if (!tagChecked) CheckTagOnce();
        if (!tagExists) return;

        try
        {
            if (go.CompareTag(tagToManage))
            {
                if (go.activeInHierarchy)
                {
                    go.SetActive(false);
                    // Recalcula para manter consistência
                    EnforceNow();
                    if (enableDebug) Debug.Log($"[Identificador] Deactivated specific -> {go.name}");
                }
                else if (enableDebug)
                {
                    Debug.Log($"[Identificador] O objeto já estava inativo -> {go.name}");
                }
            }
            else if (enableDebug)
            {
                Debug.Log($"[Identificador] DeactivateGameObject: objeto não tem a tag '{tagToManage}' -> {go.name}");
            }
        }
        catch (UnityException)
        {
            if (enableDebug) Debug.LogWarning($"[Identificador] Tag '{tagToManage}' não existe no projeto.");
        }
    }

    // Coroutine compatível: ativa o próximo objeto inativo com a tag após delay
    public IEnumerator ActivateNextWithTagAfter(string tag, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (string.IsNullOrEmpty(tag))
        {
            if (enableDebug) Debug.LogWarning("[Identificador] ActivateNextWithTagAfter: tag vazia.");
            yield break;
        }

        try
        {
            GameObject[] all = FindObjectsOfType<GameObject>(true);
            for (int i = 0; i < all.Length; i++)
            {
                var go = all[i];
                if (go == null) continue;
                try
                {
                    if (go.CompareTag(tag) && !go.activeInHierarchy)
                    {
                        EnsureParentsActive(go);
                        go.SetActive(true);
                        LastActivatedCount++;
                        if (enableDebug) Debug.Log($"[Identificador] Ativado after delay -> {go.name}");
                        yield break;
                    }
                }
                catch (UnityException)
                {
                    if (enableDebug) Debug.LogWarning($"[Identificador] Tag '{tag}' não existe no projeto.");
                    yield break;
                }
            }

            // se não encontrou nenhum inativo, revalida o estado
            EnforceNow();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[Identificador] Erro em ActivateNextWithTagAfter: {ex.Message}");
        }
    }

    // --- helpers ---
    private void EnsureParentsActive(GameObject go)
    {
        Transform t = go.transform.parent;
        Stack<Transform> stack = null;
        while (t != null)
        {
            if (!t.gameObject.activeSelf)
            {
                if (stack == null) stack = new Stack<Transform>();
                stack.Push(t);
            }
            t = t.parent;
        }

        if (stack != null)
        {
            while (stack.Count > 0)
            {
                var p = stack.Pop();
                p.gameObject.SetActive(true);
            }
        }
    }

    // helper público se quiser chamar por outro script sem coroutine
    public void ActivateNextWithTagAfterDelayed(string tag, float delay)
    {
        StartCoroutine(ActivateNextWithTagAfter(tag, delay));
    }
}
