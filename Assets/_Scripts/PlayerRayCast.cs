using UnityEngine;

public class PlayerRayCast : MonoBehaviour
{
    [Header("Raycast Settings")]
    public float distanciaDoRaio = 5f;
    public float alturaDoRaio = 0.80f;
    public KeyCode interactKey = KeyCode.F;

    [Header("UI")]
    public GameObject promptUI; 

    private InteractionZone objetoAtual;

    void Start()
    {
        if (promptUI != null)
            promptUI.SetActive(false);
    }

    void Update()
    {
        Vector3 origem = transform.position + Vector3.up * alturaDoRaio;
        Ray ray = new Ray(origem, transform.forward);
        RaycastHit hit;

        Debug.DrawRay(origem, transform.forward * distanciaDoRaio, Color.red);

        if (Physics.Raycast(ray, out hit, distanciaDoRaio))
        {
            InteractionZone zone = hit.collider.GetComponent<InteractionZone>();

            if (zone != null)
            {

                if (promptUI != null && objetoAtual != zone)
                    promptUI.SetActive(true);

                objetoAtual = zone;


                if (Input.GetKeyDown(interactKey))
                {
                    zone.Interact();
                }

                return;
            }
        }

        // Se não está olhando para nada interagível
        if (promptUI != null)
            promptUI.SetActive(false);

        objetoAtual = null;
    }
}
