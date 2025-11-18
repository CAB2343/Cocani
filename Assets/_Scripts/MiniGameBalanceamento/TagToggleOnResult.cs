using System.Collections;
using UnityEngine;

/// <summary>
/// Ouve os eventos de mini-game (vitória/derrota) e desativa/reativa objetos por tag.
/// Coloque esse componente em um GameObject controlador (ex: GameManagers) ou em prefabs que devem gerenciar a própria visibilidade.
/// </summary>
public class TagToggleOnResult : MonoBehaviour
{
    [Tooltip("Tag dos objetos que serão desativados/reativados.")]
    public string targetTag = "MinigameTarget";

    [Tooltip("Tempo em segundos até o(s) objeto(s) reaparecer(em).")]
    public float respawnDelay = 5f;

    [Tooltip("Se verdadeiro, reage apenas à vitória.")]
    public bool reactToVictory = true;

    [Tooltip("Se verdadeiro, reage apenas à derrota.")]
    public bool reactToDefeat = true;

    [Tooltip("Se verdadeiro, gerencia todos os objetos com a tag. Se falso, gerencia apenas este GameObject.")]
    public bool manageAllWithTag = true;

    void OnEnable()
    {
        MiniGameResultEvents.OnVictory += HandleVictory;
        MiniGameResultEvents.OnDefeat += HandleDefeat;
    }

    void OnDisable()
    {
        MiniGameResultEvents.OnVictory -= HandleVictory;
        MiniGameResultEvents.OnDefeat -= HandleDefeat;
    }

    private void HandleVictory()
    {
        if (!reactToVictory) return;
        TriggerToggle();
    }

    private void HandleDefeat()
    {
        if (!reactToDefeat) return;
        TriggerToggle();
    }

    private void TriggerToggle()
    {
        if (manageAllWithTag)
        {
            GameObject[] objs = GameObject.FindGameObjectsWithTag(targetTag);
            foreach (var obj in objs)
            {
                StartCoroutine(DeactivateThenReactivate(obj));
            }
        }
        else
        {
            StartCoroutine(DeactivateThenReactivate(this.gameObject));
        }
    }

    private IEnumerator DeactivateThenReactivate(GameObject obj)
    {
        if (obj == null) yield break;
        if (!obj.activeInHierarchy) yield break;

        obj.SetActive(false);

        float timer = 0f;
        while (timer < respawnDelay)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        // se o objeto ainda existir, reativa
        if (obj != null)
            obj.SetActive(true);
    }
}
