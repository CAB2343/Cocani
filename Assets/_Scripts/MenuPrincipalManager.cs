using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MenuPrincipalManager : MonoBehaviour
{
    [SerializeField] private string nomeDoLevelDeJogo;
    [SerializeField] private GameObject painelMenuInicial;
    [SerializeField] private GameObject painelOpcoes;
    [SerializeField] private GameObject firstMenuButton; // Botão que será selecionado ao voltar para o menu principal

    private GameObject lastSelected;
    private bool usingController = false;

    void Update()
    {
        // Detecta se está usando controle
        if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
        {
            if (!usingController)
            {
                usingController = true;

                // Se não tiver nada selecionado, volta pro botão inicial
                if (EventSystem.current.currentSelectedGameObject == null)
                {
                    EventSystem.current.SetSelectedGameObject(firstMenuButton);
                }
            }
        }

        // Detecta se está usando mouse
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            usingController = false;
        }

        if (usingController)
        {
            // No controle: nunca deixar null
            if (EventSystem.current.currentSelectedGameObject == null && lastSelected != null)
            {
                EventSystem.current.SetSelectedGameObject(lastSelected);
            }
            else
            {
                lastSelected = EventSystem.current.currentSelectedGameObject;
            }
        }
        else
        {
            // No mouse: pode deselecionar normalmente
            lastSelected = EventSystem.current.currentSelectedGameObject;
        }
    }

    public void Jogar()
    {
        SceneManager.LoadScene(nomeDoLevelDeJogo);
    }

    public void AbrirOpcoes()
    {
        painelMenuInicial.SetActive(false);
        painelOpcoes.SetActive(true);
        // O painel Opcoes já tem script para selecionar o slider inicial (OptionsMenu.cs)
    }

    public void FecharOpcoes()
    {
        painelMenuInicial.SetActive(true);
        painelOpcoes.SetActive(false);

        // Força foco de volta no botão inicial do Menu Principal, mas só se estiver usando controle
        if (usingController)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstMenuButton);
        }
    }

    public void SairJogo()
    {
        Debug.Log("Sair do Jogo");
        Application.Quit();
    }
}
