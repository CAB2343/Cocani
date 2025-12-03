using UnityEngine;
using System.Collections;

public class RestPoint : MonoBehaviour, IInteractable
{
    [Header("Conexões")]
    public GameManager gameManager;
    public CanvasGroup blackScreenCanvasGroup;
    
    [Header("Visual")]
    public GameObject textoInteracao; // Arraste o texto "Pressione F" aqui
    public float fadeDuration = 1.0f;
    public float waitTime = 1.0f;

    private bool isInteracting = false;

    void Start()
    {
        // Garante que o texto comece apagado
        if (textoInteracao != null) textoInteracao.SetActive(false);
    }

    // Função que o Raycast vai chamar diretamente
    public void ToggleTexto(bool ligar)
    {
        if (textoInteracao != null && !isInteracting)
        {
            textoInteracao.SetActive(ligar);
        }
        else if (isInteracting && textoInteracao != null)
        {
            // Se estiver interagindo, esconde o texto obrigatoriamente
            textoInteracao.SetActive(false);
        }
    }

    public void Interact()
    {
        if (!isInteracting)
        {
            // Garante que o texto suma ao começar
            ToggleTexto(false);
            StartCoroutine(RestSequence());
        }
    }

    IEnumerator RestSequence()
    {
        isInteracting = true;

        // Fade In
        yield return StartCoroutine(Fade(0f, 1f));

        // Descanso
        yield return new WaitForSeconds(waitTime);

        // Cura
        if (gameManager != null) gameManager.RestaurarSanidade(100f);

        // Fade Out
        yield return StartCoroutine(Fade(1f, 0f));

        isInteracting = false;
    }

    IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float time = 0f;
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            if (blackScreenCanvasGroup != null)
                blackScreenCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, time / fadeDuration);
            yield return null;
        }
        if (blackScreenCanvasGroup != null) blackScreenCanvasGroup.alpha = endAlpha;
    }
}