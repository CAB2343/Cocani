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

    void Start()
    {
        gameOverPanel.SetActive(false);
        startButton.onClick.AddListener(StartGame);
        restartButton.onClick.AddListener(RestartGame);
        UpdateUI();
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
        score = 0;
        currentTime = gameDuration;
        isGameRunning = true;
        startButton.gameObject.SetActive(false);
        gameOverPanel.SetActive(false);
        ClearParticles();
        UpdateUI();

        StartCoroutine(SpawnParticlesRoutine());
    }

    void EndGame()
    {
        isGameRunning = false;
        StopAllCoroutines(); // Stop spawning particles
        ClearParticles();

        finalScoreText.text = score.ToString();
        gameOverPanel.SetActive(true);
    }

    public void RestartGame()
    {
        gameOverPanel.SetActive(false);
        startButton.gameObject.SetActive(true);
        score = 0;
        currentTime = gameDuration;
        UpdateUI();
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

        // Instantiate particle
        GameObject particleGO = Instantiate(dustParticlePrefab, filterArea);
        activeParticles.Add(particleGO);

        // Set random size
        float size = Random.Range(particleSizeRange.x, particleSizeRange.y);
        RectTransform particleRect = particleGO.GetComponent<RectTransform>();
        particleRect.sizeDelta = new Vector2(size, size);

        // Set random position within filterArea
        float xPos = Random.Range(0, filterArea.rect.width - size);
        float yPos = Random.Range(0, filterArea.rect.height - size);
        particleRect.anchoredPosition = new Vector2(xPos, yPos);

        // Add click listener
        Button particleButton = particleGO.GetComponent<Button>();
        if (particleButton == null)
        {
            particleButton = particleGO.AddComponent<Button>();
        }
        particleButton.onClick.RemoveAllListeners(); // Ensure no duplicate listeners
        particleButton.onClick.AddListener(() => OnParticleClicked(particleGO));

        // Auto-remove after lifetime
        StartCoroutine(RemoveParticleAfterDelay(particleGO, particleLifetime));
    }

    void OnParticleClicked(GameObject particleGO)
    {
        if (!isGameRunning) return;

        score++;
        UpdateUI();

        // Visual feedback (e.g., change color, scale up, then destroy)
        Image particleImage = particleGO.GetComponent<Image>();
        if (particleImage != null)
        {
            particleImage.color = Color.green; // Feedback color
        }
        // You might want to add an animation here before destroying
        Destroy(particleGO, 0.1f); // Destroy after a short delay for visual feedback
        activeParticles.Remove(particleGO);
    }

    IEnumerator RemoveParticleAfterDelay(GameObject particleGO, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (particleGO != null && activeParticles.Contains(particleGO))
        {
            activeParticles.Remove(particleGO);
            Destroy(particleGO);
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


