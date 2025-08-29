using UnityEngine; 
using UnityEngine.UI; 
using UnityEngine.SceneManagement; 

public class GameOverManager : MonoBehaviour 
{ 
    [Header("UI Elements")] 
    public GameObject gameOverPanel; 
    public Button restartButton; 
    public Button mainMenuButton; 
    public Button quitButton; 

    [Header("Scene Management")] 
    public string mainMenuSceneName = "MainMenu"; 
    public string gameSceneName = "GameScene"; 

    void Start() 
    { 
        // Certifica-se de que o painel de Game Over está desativado no início 
        if (gameOverPanel != null) 
        { 
            gameOverPanel.SetActive(false); 
        } 

        // Adiciona listeners aos botões 
        if (restartButton != null) 
        { 
            restartButton.onClick.AddListener(RestartGame); 
        } 
        if (mainMenuButton != null) 
        { 
            mainMenuButton.onClick.AddListener(LoadMainMenu); 
        } 
        if (quitButton != null) 
        { 
            quitButton.onClick.AddListener(QuitGame); 
        } 
    } 

    // Chama este método para mostrar a tela de Game Over 
    public void ShowGameOverScreen() 
    { 
        if (gameOverPanel != null) 
        { 
            gameOverPanel.SetActive(true); 
            // Opcional: Pausar o jogo 
            Time.timeScale = 0f; 
        } 
    } 

    // Chama este método para esconder a tela de Game Over 
    public void HideGameOverScreen() 
    { 
        if (gameOverPanel != null) 
        { 
            gameOverPanel.SetActive(false); 
            // Opcional: Retomar o jogo 
            Time.timeScale = 1f; 
        } 
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
        // Para o editor Unity 
        #if UNITY_EDITOR 
        UnityEditor.EditorApplication.isPlaying = false; 
        #endif 
    } 
}

