using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ParticleClickHandler : MonoBehaviour, IPointerClickHandler
{
    // 🔥 Referência alterada para o novo Manager específico
    [HideInInspector] public MinigameManager minigameManagerRef;

    private Image img;

    void Awake()
    {
        img = GetComponent<Image>();
        if (img == null)
        {
            img = gameObject.AddComponent<Image>();
        }

        // Garante que a imagem receba cliques do mouse
        img.raycastTarget = true;
    }

    // Método auxiliar para o MinigameManager configurar a referência ao spawnar
    public void SetManager(MinigameManager manager)
    {
        minigameManagerRef = manager;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Verifica se o jogo está rodando através do novo Manager
        if (minigameManagerRef != null && !minigameManagerRef.isGameRunning)
            return;

        Debug.Log($"[ParticleClickHandler] Clique detectado em {gameObject.name}");

        if (minigameManagerRef != null)
        {
            // Chama a função de pontuação no novo Manager
            minigameManagerRef.OnParticleClicked(gameObject);
        }
        else
        {
            // Fallback de segurança caso perca a referência
            Destroy(gameObject);
        }
    }
}