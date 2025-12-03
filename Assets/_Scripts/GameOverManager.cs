using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections; // Necessário para Coroutines

public class GameOverManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject gameOverPanel;
    public CanvasGroup panelCanvasGroup; // <--- ARRASTE O COMPONENTE CANVAS GROUP AQUI
    public float fadeDuration = 1.0f;    // Tempo do fade

    [Header("Buttons")]
    public Button restartButton;
    public Button mainMenuButton;
    public Button quitButton;

    [Header("Scene Management")]
    public string mainMenuSceneName = "MainMenu";
    public string gameSceneName = "GameScene";

    [Header("Controles do Jogador")]
    public MonoBehaviour[] scriptsToDisable; 

    void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Se esqueceu de arrastar o CanvasGroup, tenta pegar automático se estiver no painel
        if (panelCanvasGroup == null && gameOverPanel != null)
            panelCanvasGroup = gameOverPanel.GetComponent<CanvasGroup>();

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(LoadMainMenu);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
    }

    public void ShowGameOverScreen()
    {
        // 1. Ativa o objeto, mas deixa transparente (Alpha 0)
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if(panelCanvasGroup != null) 
                panelCanvasGroup.alpha = 0f;
        }

        // 2. Inicia o Fade In (usando tempo real para ignorar o pause)
        StartCoroutine(FadeInSequence());
    }

    IEnumerator FadeInSequence()
    {
        // === PASSO A: Configurações Iniciais ===
        
        // Para o tempo do jogo (física e inimigos param)
        Time.timeScale = 0f;

        // Desativa scripts de controle (para a câmera não mexer)
        foreach (MonoBehaviour script in scriptsToDisable)
        {
            if (script != null) script.enabled = false;
        }

        // Destrava o mouse
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // === PASSO B: Animação de Fade ===
        // Se não tiver CanvasGroup, ignora o fade
        if (panelCanvasGroup != null)
        {
            float timer = 0f;
            while (timer < fadeDuration)
            {
                // Usamos unscaledDeltaTime porque o timeScale está 0
                timer += Time.unscaledDeltaTime; 
                panelCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
                yield return null;
            }
            panelCanvasGroup.alpha = 1f;
        }
    }

    public void HideGameOverScreen()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        Time.timeScale = 1f;
    }

    void RestartGame()
    {
        HideGameOverScreen();
        SceneManager.LoadScene(gameSceneName);
    }

    void LoadMainMenu()
    {
        HideGameOverScreen();
        SceneManager.LoadScene(mainMenuSceneName);
    }

    void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}