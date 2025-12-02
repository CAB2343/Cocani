using UnityEngine;
using System.Collections;
using Cinemachine;

public class MinigameTrigger : MonoBehaviour, IInteractable
{
    [Header("Referências")]
    public MinigameManager minigameManager;

    [Header("UI do Jogador")]
    // ARRASTE SEU CANVAS PRINCIPAL (VIDA, MIRA, ETC) PARA AQUI
    public GameObject playerHUD; 

    [Header("Cinemachine")]
    public CinemachineVirtualCamera minigameCam; 
    public int highPriority = 20;
    public int lowPriority = 0;

    private bool isTransitioning = false;
    private CinemachineBrain mainCameraBrain;

    void Start()
    {
        if (Camera.main != null)
            mainCameraBrain = Camera.main.GetComponent<CinemachineBrain>();
    }

    void OnEnable()
    {
        if (minigameManager != null)
            minigameManager.OnMinigameFinished += ExitMinigameView;
    }

    void OnDisable()
    {
        if (minigameManager != null)
            minigameManager.OnMinigameFinished -= ExitMinigameView;
    }

    public void Interact()
    {
        if (minigameManager != null && !minigameManager.isGameRunning && !isTransitioning)
        {
            StartCoroutine(StartSequence());
        }
    }

    IEnumerator StartSequence()
    {
        isTransitioning = true;

        // 🔥 NOVO: Esconde a UI do jogador assim que começa a transição
        if (playerHUD != null) playerHUD.SetActive(false);

        if (minigameCam != null) minigameCam.Priority = highPriority;

        if (mainCameraBrain != null)
        {
            // Espera o delay de segurança para o Cinemachine registrar a troca
            yield return new WaitForSeconds(0.2f); 
            while (mainCameraBrain.IsBlending) yield return null;
        }
        else
        {
            yield return new WaitForSeconds(2f);
        }

        minigameManager.StartMinigame();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        isTransitioning = false;
    }

    public void ExitMinigameView()
    {
        if (this.gameObject.activeInHierarchy)
            StartCoroutine(ExitSequence());
    }

    IEnumerator ExitSequence()
    {
        minigameManager.StopMinigame(); 
        
        if (minigameCam != null)
        {
            minigameCam.Priority = lowPriority;
        }
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (mainCameraBrain != null)
        {
            yield return new WaitForSeconds(0.1f);
            while (mainCameraBrain.IsBlending) yield return null;
        }

        // 🔥 NOVO: Reativa a UI do jogador APÓS a câmera voltar
        if (playerHUD != null) playerHUD.SetActive(true);
    }
}