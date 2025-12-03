using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;
    public GameObject pauseMenuUI;
    public PauseFade animacaoMenu;
    
    [Header("UI do Jogo")] // Organizador no Inspector
    public GameObject gpsCanvasUI; // <--- ADICIONADO: Arraste o GPSCanvas aqui

    private GameManager gameManager;

    void Start()
    {
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
        
        // Chama o fade out e espera terminar
        animacaoMenu.EsconderMenu(() => 
        {
            pauseMenuUI.SetActive(false);
            
            // Reativa o GPS quando o menu terminar de sumir
            if (gpsCanvasUI != null) gpsCanvasUI.SetActive(true); // <--- ADICIONADO

            Time.timeScale = 1f;
            GameIsPaused = false;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (gameManager != null)
                gameManager.UnpauseGame();
        });
    }

    public void Pause()
    {
        Debug.Log("[PauseMenu] Pausando o jogo...");
        
        // Esconde o GPS imediatamente ao pausar
        if (gpsCanvasUI != null) gpsCanvasUI.SetActive(false); // <--- ADICIONADO

        animacaoMenu.MostrarMenu();
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

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
        SceneManager.LoadScene("MenuPrincipal"); 
    }

    public void QuitGame()
    {
        Debug.Log("[PauseMenu] Saindo do jogo...");
        Application.Quit();
    }
}