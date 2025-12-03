using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing; // Mantive caso use em outro lugar

public class StressManager : MonoBehaviour
{
    [Header("Conexões")]
    public GameManager gameManager; // Arraste o seu GameManager aqui
    public Slider stressSlider;
    public CameraShake cameraShake;

    // Removemos o 'sliderSpeed' pois quem controla a velocidade agora é o 'fuelDecayRate' no GameManager

    void OnEnable()
    {
        // Garante que o slider atualize instantaneamente ao abrir o painel
        AtualizarValores();
    }

    void Update()
    {
        AtualizarValores();
    }

    void AtualizarValores()
    {
        if (gameManager == null) return;

        // 1. O Slider apenas ESPELHA o valor real do combustível
        stressSlider.value = gameManager.currentFuel;

        // 2. Verifica se precisa tremer a câmera baseado no combustível real
        // Nota: alterei para verificar gameManager.currentFuel para ser mais preciso
        if(cameraShake != null && cameraShake.isShaking == false && gameManager.currentFuel <= 50f)
        {
            cameraShake.isShaking = true;
        }
    }
}