using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using TMPro;

public class LevelLoader : MonoBehaviour
{
    [Header("Configurações")]
    public VideoPlayer videoPlayer;
    public string nomeDaProximaCena;
    [Range(0.1f, 2f)] public float velocidadeDoTexto = 0.5f;

    [Header("UI de Loading")]
    public GameObject telaDeLoading;
    public CanvasGroup canvasGroupLoading; // ARRASTE O OBJETO COM CANVAS GROUP AQUI
    public TextMeshProUGUI textoPorcentagem;

    void Start()
    {
        // Garante que a tela comece desligada e invisível
        if (telaDeLoading != null) telaDeLoading.SetActive(false);
        if (canvasGroupLoading != null) canvasGroupLoading.alpha = 0;

        // Configuração manual do vídeo
        videoPlayer.playOnAwake = false;
        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.loopPointReached += OnVideoEnd;
        videoPlayer.Prepare();
    }

    void OnVideoPrepared(VideoPlayer vp)
    {
        vp.Play();
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        vp.loopPointReached -= OnVideoEnd;
        StartCoroutine(CarregarCenaSuave());
    }

    IEnumerator CarregarCenaSuave()
    {
        // 1. Configurações iniciais
        videoPlayer.targetCameraAlpha = 0; // Oculta vídeo
        telaDeLoading.SetActive(true);     // Ativa objeto (mas ainda está invisível pelo Alpha 0)
        
        if(textoPorcentagem != null) textoPorcentagem.text = "0%";

        // 2. ANIMAÇÃO DE FADE IN (Duração: aprox. 0.5 segundos)
        if (canvasGroupLoading != null)
        {
            canvasGroupLoading.alpha = 0f;
            while (canvasGroupLoading.alpha < 1f)
            {
                canvasGroupLoading.alpha += Time.deltaTime * 2f; // Multiplique por 2 para ser rápido
                yield return null;
            }
            canvasGroupLoading.alpha = 1f; // Garante que fique 100% visível
        }

        // 3. Inicia o carregamento da cena
        float progressoVisual = 0f;
        AsyncOperation operacao = SceneManager.LoadSceneAsync(nomeDaProximaCena);
        operacao.allowSceneActivation = false;

        while (progressoVisual < 1f)
        {
            float progressoReal = Mathf.Clamp01(operacao.progress / 0.9f);
            progressoVisual = Mathf.MoveTowards(progressoVisual, progressoReal, Time.deltaTime * velocidadeDoTexto);

            if(textoPorcentagem != null)
                textoPorcentagem.text = (progressoVisual * 100).ToString("F0") + "%";

            if (progressoVisual >= 0.99f && operacao.progress >= 0.9f)
            {
                if(textoPorcentagem != null) textoPorcentagem.text = "100%";
                yield return new WaitForSeconds(0.5f);
                operacao.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}