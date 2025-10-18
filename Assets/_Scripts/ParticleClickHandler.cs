using UnityEngine;
using UnityEngine.EventSystems;

public class ParticleClickHandler : MonoBehaviour, IPointerClickHandler
{
    // Pode ser atribuído pelo GameManager no instante do spawn
    [HideInInspector] public GameManager gameManagerRef;
    [HideInInspector] public GameObject particleGO;

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"[ParticleClickHandler] OnPointerClick recebido em {gameObject.name} (button: {eventData.button})");
        if (gameManagerRef != null && particleGO != null)
        {
            gameManagerRef.OnParticleClickedFromHandler(particleGO);
        }
        else
        {
            Debug.LogWarning("[ParticleClickHandler] gameManagerRef ou particleGO não atribuídos!");
        }
    }
}
