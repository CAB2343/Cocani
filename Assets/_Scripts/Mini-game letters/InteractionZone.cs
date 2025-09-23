using UnityEngine;

public class InteractionZone : MonoBehaviour
{
    [Header("Refs")]
    public string playerTag = "Player";
    public MiniGameController miniGameController;
    public void Interact()
    {
        if (miniGameController != null)
            miniGameController.Open();
    }
}
