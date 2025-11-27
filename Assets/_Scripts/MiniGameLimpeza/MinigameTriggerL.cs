using UnityEngine;
using System.Collections;
using Cinemachine;

public class MinigameTrigger : MonoBehaviour, IInteractable
{
    [Header("Referências")]
    public MinigameManager minigameManager;

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

    // 🔥 INSCRIÇÃO NO EVENTO
    void OnEnable()
    {
        if (minigameManager != null)
            minigameManager.OnMinigameFinished += ExitMinigameView;
    }

    // 🔥 REMOÇÃO DA INSCRIÇÃO (EVITA ERROS DE MEMÓRIA)
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

        if (minigameCam != null) minigameCam.Priority = highPriority;

        if (mainCameraBrain != null)
        {
            yield return null; 
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

    // Esta função agora é chamada automaticamente pelo evento do Manager
    public void ExitMinigameView()
    {
        // Se o objeto já foi destruído ou desativado, não tenta rodar corrotina
        if (this.gameObject.activeInHierarchy)
            StartCoroutine(ExitSequence());
    }

    IEnumerator ExitSequence()
    {
        // Garante que o painel fecha
        minigameManager.StopMinigame(); 
        
        // Baixa a prioridade para a câmera principal assumir
        if (minigameCam != null)
        {
            minigameCam.Priority = lowPriority;
        }
        
        // Trava mouse novamente para FPS
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Espera a transição (opcional, apenas se quiser bloquear interações durante a volta)
        if (mainCameraBrain != null)
        {
            yield return null;
            while (mainCameraBrain.IsBlending) yield return null;
        }
    }
}