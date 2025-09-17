using UnityEngine;

public class MiniGameController : MonoBehaviour
{
    [Header("UI / Gameplay")]
    public GameObject miniGameUI;      // painel que contém GridManager (desativar no Start)
    public GameObject playerRoot;      // (opcional) GameObject com scripts de movimento para desativar
    public bool disablePlayerRootOnOpen = true;

    void Start()
    {
        if (miniGameUI != null) miniGameUI.SetActive(false);
    }

    // chamável por InteractionZone
    public void Open()
    {
        if (miniGameUI != null) miniGameUI.SetActive(true);
        if (disablePlayerRootOnOpen && playerRoot != null) playerRoot.SetActive(false);
        // se quiser travar cursor ou bloquear input mais refinado, faça aqui
    }

    // botão UI ou lógica do mini-game chama para fechar
    public void Close()
    {
        if (miniGameUI != null) miniGameUI.SetActive(false);
        if (disablePlayerRootOnOpen && playerRoot != null) playerRoot.SetActive(true);
    }
}
