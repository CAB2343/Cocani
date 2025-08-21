using UnityEngine;
using UnityEngine.EventSystems;

public class OptionsMenu : MonoBehaviour
{
    [Header("Primeiro selecionado ao abrir o menu")]
    public GameObject firstSelected; // arraste aqui o Slider de Volume no Inspector

    void OnEnable()
    {
        // Garante que o EventSystem sabe o que selecionar
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSelected);
    }
}
