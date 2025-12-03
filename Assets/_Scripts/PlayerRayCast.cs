using UnityEngine;

public class PlayerRayCast : MonoBehaviour
{
    [Header("Configurações")]
    public float distanciaDoRaio = 5f;
    public float alturaDoRaio = 0.80f;
    public KeyCode interactKey = KeyCode.F;
    public string tagInteracao = "Interactable"; 

    private RestPoint ultimoPontoDescanso; // Memoriza a cadeira para desligar o texto depois

    void Update()
    {
        Vector3 origem = transform.position + Vector3.up * alturaDoRaio;
        Ray ray = new Ray(origem, transform.forward);
        RaycastHit hit;

        bool encontrouRestPoint = false;

        if (Physics.Raycast(ray, out hit, distanciaDoRaio))
        {
            if (hit.collider.CompareTag(tagInteracao))
            {
                // 1. Lógica de Interação (Funciona para tudo que for interagível)
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null && Input.GetKeyDown(interactKey))
                {
                    interactable.Interact();
                }

                // 2. Lógica Visual Específica (Só para o RestPoint)
                RestPoint pontoAtual = hit.collider.GetComponent<RestPoint>();
                
                if (pontoAtual != null)
                {
                    encontrouRestPoint = true;
                    
                    // Se mudou de objeto ou começou a olhar agora
                    if (ultimoPontoDescanso != pontoAtual)
                    {
                        if (ultimoPontoDescanso != null) ultimoPontoDescanso.ToggleTexto(false);
                        pontoAtual.ToggleTexto(true);
                        ultimoPontoDescanso = pontoAtual;
                    }
                }
            }
        }

        // Se parou de olhar para um RestPoint, desliga o texto dele
        if (!encontrouRestPoint && ultimoPontoDescanso != null)
        {
            ultimoPontoDescanso.ToggleTexto(false);
            ultimoPontoDescanso = null;
        }
    }
}