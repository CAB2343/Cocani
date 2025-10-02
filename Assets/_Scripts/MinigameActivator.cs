using UnityEngine;
using UnityEngine.UI;

public class MinigameActivator : MonoBehaviour
{
    public GameManager gameManager; // Referência ao GameManager
    public GameObject minigameUI; // Referência ao painel principal do minigame (MinigamePanel)
    public string playerTag = "Player"; // Tag do seu jogador

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
            // Opcional: Mostrar um prompt de UI para o jogador (ex: "Pressione E para interagir")
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;
            Debug.Log("MinigameActivator: Jogador saiu do alcance de interação.");
            // Opcional: Esconder o prompt de UI
        }
    }

    // Este método será chamado quando o jogador interagir com este objeto
    public void ActivateMinigame()
    {
        if (gameManager == null)
        {
            Debug.LogError("MinigameActivator: GameManager não atribuído!");
            return;
        }
        if (minigameUI == null)
        {
            Debug.LogError("MinigameActivator: MinigameUI (painel) não atribuído!");
            return;
        }

        minigameUI.SetActive(true); // Ativa o painel do minigame
        gameManager.StartGame(); // Inicia o minigame (que já pausa o jogo principal)

        // Opcional: Desativar o movimento do jogador ou outros controles
        // Ex: FindObjectOfType<PlayerMovement>().enabled = false;
    }

    // Este método pode ser chamado para fechar o minigame sem jogar (ex: botão de fechar)
    public void DeactivateMinigame()
    {
        if (gameManager == null)
        {
            Debug.LogError("MinigameActivator: GameManager não atribuído!");
            return;
        }
        if (minigameUI == null)
        {
            Debug.LogError("MinigameActivator: MinigameUI (painel) não atribuído!");
            return;
        }

        minigameUI.SetActive(false); // Desativa o painel do minigame
        gameManager.UnpauseGame(); // Despausa o jogo principal

        // Opcional: Reativar o movimento do jogador ou outros controles
        // Ex: FindObjectOfType<PlayerMovement>().enabled = true;
    }
}


