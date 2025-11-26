using UnityEngine;

public class MinigameLimpeza : MonoBehaviour
{
    [Header("Referências")]
    public GameManager gameManager;          // Referência ao GameManager
    public GameObject minigameUI;            // Painel principal do minigame
    
    [Header("Configuração Raycast")]
    public float interactionDistance = 3f;   // Distância máxima para interagir

    void Update()
    {
        // Verifica o input F primeiro para economizar processamento
        if (Input.GetKeyDown(KeyCode.F))
        {
            CheckInteraction();
        }
    }

    private void CheckInteraction()
    {
        // Cria um raio saindo do centro da tela (Câmera Principal)
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        // Se o raio bater em algo dentro da distância limite
        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            // Verifica se o objeto atingido é ESTE objeto
            if (hit.transform == transform)
            {
                ActivateMinigame();
            }
        }
    }

    public void ActivateMinigame()
    {
        if (gameManager == null || minigameUI == null)
        {
            Debug.LogError("MinigameLimpeza: Referências (GameManager ou UI) faltando!");
            return;
        }

        minigameUI.SetActive(true);
        gameManager.StartGame();

        // Libera o cursor para o minigame
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("Minigame ativado via Raycast!");
    }

    public void DeactivateMinigame()
    {
        if (gameManager == null || minigameUI == null) return;

        minigameUI.SetActive(false);
        gameManager.UnpauseGame();

        // Trava o cursor novamente para o jogo principal
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("Minigame desativado!");
    }
}