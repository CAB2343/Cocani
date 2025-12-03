using UnityEngine;
using System.Collections;
using System; // Necessário para Action

public class PauseFade : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public float duracaoFade = 0.5f;

    public void MostrarMenu()
    {
        canvasGroup.alpha = 0;
        gameObject.SetActive(true);
        StartCoroutine(FadeCanvas(0, 1, null));
    }

    // Agora aceita uma "Action" opcional
    public void EsconderMenu(Action aoTerminar = null)
    {
        StartCoroutine(FadeCanvas(1, 0, aoTerminar));
    }

    IEnumerator FadeCanvas(float inicio, float fim, Action aoTerminar)
    {
        float tempoPassado = 0f;
        while (tempoPassado < duracaoFade)
        {
            tempoPassado += Time.unscaledDeltaTime; // Usa tempo real (ignora pause)
            canvasGroup.alpha = Mathf.Lerp(inicio, fim, tempoPassado / duracaoFade);
            yield return null;
        }
        canvasGroup.alpha = fim;

        // Executa o código de limpeza (desativar objeto, voltar tempo) aqui
        if (aoTerminar != null) aoTerminar.Invoke();
    }
}