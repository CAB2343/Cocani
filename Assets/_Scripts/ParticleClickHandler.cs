using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ParticleClickHandler : MonoBehaviour, IPointerClickHandler
{
    [HideInInspector] public GameManager gameManagerRef;
    [HideInInspector] public GameObject particleGO;

    private Image img;

    void Awake()
    {
        img = GetComponent<Image>();
        if (img == null)
        {
            img = gameObject.AddComponent<Image>();
        }
        img.raycastTarget = true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"[ParticleClickHandler] Clique detectado em {gameObject.name}");

        if (gameManagerRef != null)
            gameManagerRef.OnParticleClickedFromHandler(gameObject);
        else
            Destroy(gameObject); // fallback
    }
}
