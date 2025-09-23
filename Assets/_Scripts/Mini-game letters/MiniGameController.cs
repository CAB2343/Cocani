using UnityEngine;

public class MiniGameController : MonoBehaviour
{
    [Header("UI / Gameplay")]
    public GameObject miniGameUI;
    public PlayerController1 playerRoot;     
    public bool disablePlayerRootOnOpen = true; 

    void Start()
    {
        if (miniGameUI != null)
            miniGameUI.SetActive(false);
    }

    // Chamado pelo InteractionZone ou Raycast
    public void Open()
    {
        if (miniGameUI != null)
            miniGameUI.SetActive(true);

        if (disablePlayerRootOnOpen && playerRoot != null)
            playerRoot.enabled = false; 
    }


    public void Close()
    {
        if (miniGameUI != null)
            miniGameUI.SetActive(false);

        if (disablePlayerRootOnOpen && playerRoot != null)
            playerRoot.enabled = true; 
    }
}
