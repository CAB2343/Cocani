using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeAndDisable : MonoBehaviour
{
    [Header("Configurações")]
    [Range(0.1f, 5f)]
    public float duracaoDoFade = 1.0f; // Tempo em segundos
    public bool iniciarAutomaticamente = false; // Se quiser que suma assim que aparecer

    private CanvasGroup canvasGroup;

    void Awake()
    {
        // Tenta pegar o CanvasGroup automaticamente
        canvasGroup = GetComponent<CanvasGroup>();
        
        // Se esqueceu de colocar no Inspector, adiciona via código pra não dar erro
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    void Start()
    {
        if (iniciarAutomaticamente)
        {
            IniciarFadeOut();
        }
    }

    // Chame esta função para começar o efeito (pode chamar via botão ou outro script)
    public void IniciarFadeOut()
    {
        // Garante que o objeto esteja ativo antes de começar
        gameObject.SetActive(true); 
        canvasGroup.alpha = 1f;
        
        StartCoroutine(ProcessoDeFade());
    }

    IEnumerator ProcessoDeFade()
    {
        float tempoPassado = 0f;
        float alphaInicial = canvasGroup.alpha;

        while (tempoPassado < duracaoDoFade)
        {
            tempoPassado += Time.deltaTime;
            
            // Lerp faz a transição suave de 1 (alphaInicial) até 0
            canvasGroup.alpha = Mathf.Lerp(alphaInicial, 0f, tempoPassado / duracaoDoFade);
            
            yield return null;
        }

        // Garante que fique totalmente invisível
        canvasGroup.alpha = 0f;
        
        // DESATIVA O OBJETO
        gameObject.SetActive(false);
    }
}