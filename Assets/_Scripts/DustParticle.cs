using UnityEngine;

public class DustParticle : MonoBehaviour
{
    // Este script pode ser usado para adicionar comportamentos específicos à partícula,
    // como animações de fade-out ou efeitos sonoros ao ser clicada.
    // Por enquanto, o GameManager lida com a maior parte da lógica.
    // Pode ser expandido para gerenciar a vida útil da partícula ou interações mais complexas.

    // Exemplo: Se você quiser que a partícula tenha um ID único ou um tipo específico
    public string particleType = "Normal";

    // Exemplo: Método para ser chamado quando a partícula é clicada
    public void OnClicked()
    {
        // Debug.Log($"Partícula {particleType} clicada!");
        // O GameManager já lida com a pontuação e destruição.
        // Aqui você poderia adicionar um efeito visual ou sonoro específico para esta partícula.
    }
}


