using UnityEngine;

public class MinimapToggle : MonoBehaviour
{
    public GameObject minimapUI; // Arraste o MinimapViewport aqui no Inspector

    void Start()
    {
        minimapUI.SetActive(false); // garante que começa desligado
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            minimapUI.SetActive(!minimapUI.activeSelf);
        }
    }
}
