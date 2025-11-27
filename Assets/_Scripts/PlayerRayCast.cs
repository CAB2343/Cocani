using UnityEngine;

public class PlayerRayCast : MonoBehaviour
{
    [Header("Raycast Settings")]
    public float distanciaDoRaio = 5f;
    public float alturaDoRaio = 0.80f;
    public KeyCode interactKey = KeyCode.F;
    
    // Nome da Tag que você deve colocar no objeto na Unity
    public string tagInteracao = "Interactable"; 

    [Header("UI")]
    public GameObject promptUI; 

    private IInteractable objetoAtual; // Agora é genérico

    void Start()
    {
        if (promptUI != null) promptUI.SetActive(false);
    }

    void Update()
    {
        Vector3 origem = transform.position + Vector3.up * alturaDoRaio;
        Ray ray = new Ray(origem, transform.forward);
        RaycastHit hit;

        Debug.DrawRay(origem, transform.forward * distanciaDoRaio, Color.red);

        if (Physics.Raycast(ray, out hit, distanciaDoRaio))
        {
            // 1. Verifica se a TAG está correta
            if (hit.collider.CompareTag(tagInteracao))
            {
                // 2. Tenta pegar qualquer script que tenha a interface IInteractable
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();

                if (interactable != null)
                {
                    if (promptUI != null && objetoAtual != interactable)
                        promptUI.SetActive(true);

                    objetoAtual = interactable;

                    if (Input.GetKeyDown(interactKey))
                    {
                        interactable.Interact();
                    }
                    return;
                }
            }
        }

        if (promptUI != null) promptUI.SetActive(false);
        objetoAtual = null;
    }
}