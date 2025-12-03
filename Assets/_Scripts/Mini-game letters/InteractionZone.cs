using UnityEngine;

// Implementa a interface IInteractable para ser detectado pelo Raycast
public class InteractionZone : MonoBehaviour, IInteractable
{
    [Header("Refs")]
    public MiniGameController miniGameController;

    // O PlayerRayCast chamará esta função automaticamente ao apertar 'F'
    public void Interact()
    {
        if (miniGameController != null)
        {
            miniGameController.Open();
        }
    }
}