using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Text scoreText;
    public Text timerText;
    public Button startButton;
    public RectTransform filterArea;
    public GameObject gameOverPanel;
    public Text finalScoreText;
    public Button restartButton;

    [Header("Game Settings")]
    public GameObject dustParticlePrefab;
    public float gameDuration = 60f; // Seconds
    public float particleLifetime = 2.5f; // Seconds
    public float particleSpawnRate = 0.75f; // Seconds between spawns
    public Vector2 particleSizeRange = new Vector2(25f, 45f); // Min and Max size

    private int score = 0;
    private float currentTime = 0f;
    private bool isGameRunning = false;
    private List<GameObject> activeParticles = new List<GameObject>();

    [Header("Minigame Panel")]
    public GameObject minigamePanel; // Referência ao painel principal do minigame

    void Start()
    {
        gameOverPanel.SetActive(false);
        startButton.onClick.AddListener(StartGame);
        restartButton.onClick.AddListener(RestartGame);
        UpdateUI();
        // UnpauseGame() não é mais chamado aqui, será controlado externamente
        minigamePanel.SetActive(false); // Garante que o minigame esteja desativado no início
    }

    void Update()
    {
        if (isGameRunning)
        {
            currentTime -= Time.deltaTime;
            if (currentTime <= 0)
            {
                currentTime = 0;
                EndGame();
            }
            UpdateUI();
        }
    }

    void UpdateUI()
    {
        scoreText.text = score.ToString();
        timerText.text = Mathf.CeilToInt(currentTime).ToString() + "s";
    }

    public void StartGame()
    {
        Debug.Log("GameManager: StartGame chamado!");
        score = 0;
        currentTime = gameDuration;
        isGameRunning = true;
        startButton.gameObject.SetActive(false);
        gameOverPanel.SetActive(false);
        ClearParticles();
        UpdateUI();
        PauseGame(); // Pause the main game
        minigamePanel.SetActive(true); // Ativa o painel do minigame
        Debug.Log("GameManager: MinigamePanel ativado!");

        StartCoroutine(SpawnParticlesRoutine());
    }

    void EndGame()
    {
        Debug.Log("GameManager: EndGame chamado!");
        isGameRunning = false;
        StopAllCoroutines(); // Stop spawning particles
        ClearParticles();

        finalScoreText.text = score.ToString();
        gameOverPanel.SetActive(true);
        UnpauseGame(); // Unpause the main game
    }

    public void RestartGame()
    {
        Debug.Log("GameManager: RestartGame chamado!");
        gameOverPanel.SetActive(false);
        startButton.gameObject.SetActive(true);
        score = 0;
        currentTime = gameDuration;
        UpdateUI();
        UnpauseGame(); // Ensure game is unpaused when returning to start screen
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        Debug.Log("Game Paused (Time.timeScale = 0)");
    }

    public void UnpauseGame()
    {
        Time.timeScale = 1f;
        Debug.Log("Game Unpaused (Time.timeScale = 1)");
    }

    IEnumerator SpawnParticlesRoutine()
    {
        while (isGameRunning)
        {
            SpawnParticle();
            yield return new WaitForSeconds(particleSpawnRate);
        }
    }

    void SpawnParticle()
    {
        if (!isGameRunning) return;
        Debug.Log("GameManager: Tentando spawnar partícula.");

        // Instantiate particle
        GameObject particleGO = Instantiate(dustParticlePrefab, filterArea);
        activeParticles.Add(particleGO);
        Debug.Log($"GameManager: Partícula {particleGO.name} spawnada em {particleGO.transform.position}.");

        // Set random size
        float size = Random.Range(particleSizeRange.x, particleSizeRange.y);
        RectTransform particleRect = particleGO.GetComponent<RectTransform>();
        particleRect.sizeDelta = new Vector2(size, size);

        // Set random position within filterArea
        float xPos = Random.Range(0, filterArea.rect.width - size);
        float yPos = Random.Range(0, filterArea.rect.height - size);
        particleRect.anchoredPosition = new Vector2(xPos, yPos);

        // Add click listener
        Image particleImage = particleGO.GetComponent<Image>();
        if (particleImage == null)
        {
            particleImage = particleGO.AddComponent<Image>();
        }
        particleImage.raycastTarget = true; // Ensure image can receive raycasts

        Button particleButton = particleGO.GetComponent<Button>();
        if (particleButton == null)
        {
            particleButton = particleGO.AddComponent<Button>();
        }
        particleButton.targetGraphic = particleImage; // Set the image as the button's target graphic
        particleButton.onClick.RemoveAllListeners(); // Ensure no duplicate listeners
        particleButton.onClick.AddListener(() => OnParticleClicked(particleGO));
        Debug.Log($"GameManager: Listener de clique adicionado à partícula {particleGO.name}.");

        // Auto-remove after lifetime
        StartCoroutine(RemoveParticleAfterDelay(particleGO, particleLifetime));
    }

    void OnParticleClicked(GameObject particleGO)
    {
        Debug.Log($"GameManager: OnParticleClicked chamado para {particleGO.name}.");
        if (!isGameRunning) return;

        score++;
        UpdateUI();
        Debug.Log($"GameManager: Score incrementado para {score}.");

        // Visual feedback (e.g., change color, scale up, then destroy)
        Image particleImage = particleGO.GetComponent<Image>();
        if (particleImage != null)
        {
            particleImage.color = Color.green; // Feedback color
        }
        // You might want to add an animation here before destroying
        Debug.Log($"GameManager: Destruindo partícula {particleGO.name}.");
        Destroy(particleGO, 0.1f); // Destroy after a short delay for visual feedback
        activeParticles.Remove(particleGO);
    }

    IEnumerator RemoveParticleAfterDelay(GameObject particleGO, float delay)
    {
        Debug.Log($"GameManager: Coroutine RemoveParticleAfterDelay iniciada para {particleGO.name}.");
        yield return new WaitForSeconds(delay);
        if (particleGO != null && activeParticles.Contains(particleGO))
        {
            Debug.Log($"GameManager: Partícula {particleGO.name} removida por tempo.");
            activeParticles.Remove(particleGO);
            Destroy(particleGO);
        }
        else if (particleGO != null) {
            Debug.Log($"GameManager: Partícula {particleGO.name} já foi removida ou não está ativa.");
        }
    }

    void ClearParticles()
    {
        foreach (GameObject particle in activeParticles)
        {
            if (particle != null)
            {
                Destroy(particle);
            }
        }
        activeParticles.Clear();
    }
}


