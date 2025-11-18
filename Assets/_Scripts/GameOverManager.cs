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
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(LoadMainMenu);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
    }

    public void ShowGameOverScreen()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // MOSTRA O MOUSE
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void HideGameOverScreen()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // ESCONDE E TRAVA O MOUSE DE NOVO
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

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