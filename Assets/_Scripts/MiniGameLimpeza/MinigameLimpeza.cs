using UnityEngine;

public class MinigameLimpeza : MonoBehaviour
{
    [Header("Referências")]
    public GameManager gameManager;          // Referência ao GameManager
    public GameObject minigameUI;            // Painel principal do minigame
    public string playerTag = "Player";      // Tag do jogador

    private bool playerInRange = false;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            ActivateMinigame();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = true;
            Debug.Log("MinigameActivator: Jogador entrou no alcance de interação.");
            // Aqui você pode mostrar um prompt tipo "Pressione E para interagir"
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;
            Debug.Log("MinigameActivator: Jogador saiu do alcance de interação.");
            // Aqui você pode esconder o prompt
        }
    }

    public void ActivateMinigame()
    {
        if (gameManager == null)
        {
            Debug.LogError("MinigameActivator: GameManager não atribuído!");
            return;
        }
        if (minigameUI == null)
        {
            Debug.LogError("MinigameActivator: MinigameUI não atribuído!");
            return;
        }

        minigameUI.SetActive(true);
        gameManager.StartGame();

        // Libera o cursor para o minigame
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("Minigame ativado!");
    }

    public void DeactivateMinigame()
    {
        if (gameManager == null || minigameUI == null)
        {
            Debug.LogError("MinigameActivator: Referências faltando!");
            return;
        }

        minigameUI.SetActive(false);
        gameManager.UnpauseGame();

        // Trava o cursor novamente para o jogo principal
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("Minigame desativado!");
    }
}