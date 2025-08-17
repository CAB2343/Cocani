using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Events;

public class StressManager : MonoBehaviour
{
    [Header("Stress Settings")]
    public int fullStress = 100;
    public int difficulty = 1;
    public float loseInterval = 0.1f; // tempo entre cada perda de stress

    [Header("References")]
    public Slider stressSlider;
    public PostProcessProfile profile;

    [Tooltip("Se verdadeiro, o vignette acompanha o stress.")]
    public bool useVignette = true;

    public UnityEvent onInsane;

    private Vignette vignette;
    private float currentStressPercent;
    private bool hasShakenAtHalf = false; // Nova flag para controlar o tremor na metade

    void Start()
    {
        if (profile != null)
            profile.TryGetSettings(out vignette);

        if (stressSlider != null)
        {
            stressSlider.maxValue = fullStress;
            stressSlider.value = fullStress;
        }

        if (vignette != null)
            vignette.intensity.value = 0;

        StartCoroutine(LoseStress());

    }

    IEnumerator LoseStress()
    {
        while (stressSlider.value > 0)
        {
            stressSlider.value -= 2f * difficulty;

            // Calcula a porcentagem de stress atual (0 a 1, onde 1 é stress total)
            // É importante que seja (maxValue - currentValue) para que 0% de stress seja 0 e 100% de stress seja 1
            currentStressPercent = (fullStress - stressSlider.value) / fullStress;

            if (useVignette && vignette != null)
                vignette.intensity.value = currentStressPercent; // Vignette aumenta com o stress

            // Verifica se o stress atingiu a metade e ainda não tremeu
            if (stressSlider.value <= fullStress / 2f && !hasShakenAtHalf)
            {
                CinemachineShake.Instance.ShakeCamera(0.5f, 0.5f); // Intensidade e duração do tremor
                hasShakenAtHalf = true; // Garante que trema apenas uma vez
            }

            yield return new WaitForSeconds(loseInterval);
        }

        onInsane.Invoke();
    }
}


