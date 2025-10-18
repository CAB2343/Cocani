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
    public GameObject dustParticlePrefab; // prefab deve ter Image (raycast target ON) — Button opcional
    public float gameDuration = 60f;
    public float particleLifetime = 2.5f;
    public float particleSpawnRate = 0.75f;
    public Vector2 particleSizeRange = new Vector2(25f, 45f);

    private int score = 0;
    private float currentTime = 0f;
    private bool isGameRunning = false;
    private List<GameObject> activeParticles = new List<GameObject>();

    [Header("Minigame Panel")]
    public GameObject minigamePanel;

    void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (startButton != null) startButton.onClick.AddListener(StartGame);
        if (restartButton != null) restartButton.onClick.AddListener(RestartGame);

        UpdateUI();
        if (minigamePanel != null) minigamePanel.SetActive(false);
    }

    void Update()
    {
        if (isGameRunning)
        {
            currentTime -= Time.unscaledDeltaTime;
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
        if (scoreText != null) scoreText.text = score.ToString();
        if (timerText != null) timerText.text = Mathf.CeilToInt(currentTime) + "s";
    }

    public void StartGame()
    {
        Debug.Log("GameManager: StartGame chamado!");
        score = 0;
        currentTime = gameDuration;
        isGameRunning = true;

        if (startButton != null) startButton.gameObject.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        ClearParticles();
        UpdateUI();

        PauseGame();
        if (minigamePanel != null) minigamePanel.SetActive(true);

        // Cursor liberado
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        StartCoroutine(SpawnParticlesRoutine());

        Canvas canvas = filterArea.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
        if (canvas.renderMode == RenderMode.WorldSpace)
        {
            if (Camera.main != null && Camera.main.GetComponent<PhysicsRaycaster>() == null)
                Camera.main.gameObject.AddComponent<PhysicsRaycaster>();
        }

        if (canvas.GetComponent<GraphicRaycaster>() == null)
            canvas.gameObject.AddComponent<GraphicRaycaster>();
         }
        else
        {
        Debug.LogError("Nenhum Canvas encontrado como pai de filterArea!");
        }

    }

    void EndGame()
    {
        Debug.Log("GameManager: EndGame chamado!");
        isGameRunning = false;
        StopAllCoroutines();
        ClearParticles();

        if (finalScoreText != null) finalScoreText.text = score.ToString();
        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        // Trava cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        UnpauseGame();
    }

    public void RestartGame()
    {
        Debug.Log("GameManager: RestartGame chamado!");
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (startButton != null) startButton.gameObject.SetActive(true);
        score = 0;
        currentTime = gameDuration;
        UpdateUI();
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        Debug.Log("Jogo principal pausado (Time.timeScale = 0)");
    }

    public void UnpauseGame()
    {
        Time.timeScale = 1f;
        Debug.Log("Jogo principal despausado (Time.timeScale = 1)");
    }

    IEnumerator SpawnParticlesRoutine()
    {
        while (isGameRunning)
        {
            SpawnParticle();
            yield return new WaitForSecondsRealtime(particleSpawnRate);
        }
    }

    void SpawnParticle()
    {
        if (!isGameRunning) return;
        if (filterArea == null)
        {
            Debug.LogError("[SpawnParticle] filterArea não atribuído!");
            return;
        }
        if (dustParticlePrefab == null)
        {
            Debug.LogError("[SpawnParticle] dustParticlePrefab não atribuído!");
            return;
        }

        // Instancia sem conectar (vai setar parent explicitamente para evitar problemas de escala)
        GameObject particleGO = Instantiate(dustParticlePrefab);
        particleGO.name = dustParticlePrefab.name + "_Instance";

        // Força parent no filterArea e mantém escala/posição local correta
        particleGO.transform.SetParent(filterArea, false);

        activeParticles.Add(particleGO);

        RectTransform rect = particleGO.GetComponent<RectTransform>();
        if (rect == null) rect = particleGO.AddComponent<RectTransform>();

        float size = Random.Range(particleSizeRange.x, particleSizeRange.y);
        rect.sizeDelta = new Vector2(size, size);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);

        float halfWidth = (filterArea.rect.width - size) / 2f;
        float halfHeight = (filterArea.rect.height - size) / 2f;
        rect.anchoredPosition = new Vector2(
            Random.Range(-halfWidth, halfWidth),
            Random.Range(-halfHeight, halfHeight)
        );

        // Garante Image com raycast ON
        Image img = particleGO.GetComponent<Image>();
        if (img == null) img = particleGO.AddComponent<Image>();
        img.raycastTarget = true;

        // Adiciona o ParticleClickHandler (ou pega se já existir)
        ParticleClickHandler handler = particleGO.GetComponent<ParticleClickHandler>();
        if (handler == null) handler = particleGO.AddComponent<ParticleClickHandler>();
        handler.gameManagerRef = this;
        handler.particleGO = particleGO;

        Debug.Log($"[SpawnParticle] Spawnou {particleGO.name} em {rect.anchoredPosition} (raycastTarget={img.raycastTarget})");

        StartCoroutine(RemoveParticleAfterDelay(particleGO, particleLifetime));
    }

    // Chamado pelo handler (IPointerClickHandler)
    public void OnParticleClickedFromHandler(GameObject particleGO)
    {
        // Este método é público para o handler chamar
        OnParticleClickedInternal(particleGO);
    }

    // Método internal que faz a remoção
    void OnParticleClickedInternal(GameObject particleGO)
    {
        if (!isGameRunning)
        {
            Debug.Log("[OnParticleClickedInternal] Ignorado — jogo não está rodando");
            return;
        }

        Debug.Log($"[GameManager] Clique processado em {particleGO.name}");

        score++;
        UpdateUI();

        Image img = particleGO.GetComponent<Image>();
        if (img != null)
            img.color = Color.green;

        if (activeParticles.Contains(particleGO))
            activeParticles.Remove(particleGO);

        Destroy(particleGO);
    }

    IEnumerator RemoveParticleAfterDelay(GameObject particleGO, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        if (particleGO != null && activeParticles.Contains(particleGO))
        {
            activeParticles.Remove(particleGO);
            Destroy(particleGO);
        }
    }

    void ClearParticles()
    {
        foreach (var p in activeParticles)
            if (p != null) Destroy(p);
        activeParticles.Clear();
    }

    void OnRectTransformDimensionsChange()
    {
        foreach (var p in activeParticles)
        {
            if (p == null) continue;
            RectTransform rect = p.GetComponent<RectTransform>();
            if (rect == null) continue;
            float size = rect.sizeDelta.x;
            float halfWidth = (filterArea.rect.width - size) / 2f;
            float halfHeight = (filterArea.rect.height - size) / 2f;
            float xPos = Mathf.Clamp(rect.anchoredPosition.x, -halfWidth, halfWidth);
            float yPos = Mathf.Clamp(rect.anchoredPosition.y, -halfHeight, halfHeight);
            rect.anchoredPosition = new Vector2(xPos, yPos);
        }
    }
}
