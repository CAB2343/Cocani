using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;
    public GameObject pauseMenuUI;

    private GameManager gameManager; // referência opcional para integrar com o GameManager

    void Start()
    {
        // Tenta achar automaticamente o GameManager na cena
        gameManager = FindObjectOfType<GameManager>();

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("ESC pressed");
            if (GameIsPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Resume()
    {
        Debug.Log("[PauseMenu] Resumindo o jogo...");
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Retoma o GameManager se existir
        if (gameManager != null)
            gameManager.UnpauseGame();
    }

    public void Pause()
    {
        Debug.Log("[PauseMenu] Pausando o jogo...");
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Pausa o GameManager se existir
        if (gameManager != null)
            gameManager.PauseGame();
    }

    public void RestartLevel()
    {
        Debug.Log("[PauseMenu] Reiniciando cena...");
        Time.timeScale = 1f;
        GameIsPaused = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMenu()
    {
        Debug.Log("[PauseMenu] Carregando menu...");
        Time.timeScale = 1f;
        GameIsPaused = false;
        SceneManager.LoadScene("Menu"); // Altere para o nome correto da cena do menu
    }

    public void QuitGame()
    {
        Debug.Log("[PauseMenu] Saindo do jogo...");
        Application.Quit();
    }
}
