using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class LevelLoader : MonoBehaviour
{
    [Header("Configurações")]
    public VideoPlayer videoPlayer;
    public string nomeDaProximaCena;

    [Header("UI de Loading")]
    public GameObject telaDeLoading; // O Painel que você criou
    public Slider barraDeProgresso;

    void Start()
    {
        // Avisa o script para rodar a função quando o vídeo acabar
        videoPlayer.loopPointReached += OnVideoEnd;
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        StartCoroutine(CarregarCenaAsync());
    }

    IEnumerator CarregarCenaAsync()
    {
        // 1. Oculta o vídeo para não travar na última imagem
        videoPlayer.targetCameraAlpha = 0; // Deixa o vídeo transparente
        // ou use: videoPlayer.gameObject.SetActive(false); 

        // 2. Ativa a tela de loading
        telaDeLoading.SetActive(true);
        barraDeProgresso.value = 0;

        // 3. Inicia o carregamento, mas IMPEDE a troca automática de cena
        AsyncOperation operacao = SceneManager.LoadSceneAsync(nomeDaProximaCena);
        operacao.allowSceneActivation = false; // Segura a cena nova na memória

        // 4. Loop de carregamento
        while (!operacao.isDone)
        {
            // O progresso vai de 0 a 0.9 enquanto carrega
            float progressoReal = Mathf.Clamp01(operacao.progress / 0.9f);

            // Aumenta a barra visualmente (opcional: lerp para suavizar)
            barraDeProgresso.value = progressoReal;

            // Se o carregamento técnico terminou (chegou a 0.9)
            if (operacao.progress >= 0.9f)
            {
                barraDeProgresso.value = 1f; // Enche a barra visualmente
                
                // Dica: Adicione um pequeno delay estético para o jogador ver o 100%
                yield return new WaitForSeconds(1.0f); 

                // Libera a troca de cena
                operacao.allowSceneActivation = true;
            }

            yield return null;
        }
    }

    
}