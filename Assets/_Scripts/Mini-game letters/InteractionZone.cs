using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider))]
public class InteractionZone : MonoBehaviour
{
    [Header("Refs")]
    public string playerTag = "Player";
    public KeyCode interactKey = KeyCode.F;
    public GameObject promptUI;        // painel/Text que aparece: ex: "Aperte F para Interagir"
    public MiniGameController miniGameController; // referência ao controller do mini-game

    bool playerInRange = false;

    void Start()
    {
        if (promptUI != null) promptUI.SetActive(false);
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            if (miniGameController != null)
                miniGameController.Open();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInRange = true;
        if (promptUI != null) promptUI.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInRange = false;
        if (promptUI != null) promptUI.SetActive(false);
    }

    // also support 2D colliders if needed
    void OnTriggerEnter2D(Collider2D other2D)
    {
        if (!other2D.CompareTag(playerTag)) return;
        playerInRange = true;
        if (promptUI != null) promptUI.SetActive(true);
    }

    void OnTriggerExit2D(Collider2D other2D)
    {
        if (!other2D.CompareTag(playerTag)) return;
        playerInRange = false;
        if (promptUI != null) promptUI.SetActive(false);
    }
}
