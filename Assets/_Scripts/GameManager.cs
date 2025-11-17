using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    // ============================================================
    // ==                   REFERÊNCIAS DE UI                    ==
    // ============================================================
    [Header("Referências UI")]
    public GameObject minigamePanel;
    public Button startButton;
    public RectTransform filterArea;
    public GameObject dustParticlePrefab;
    public Text scoreText;

    [Header("Efeitos Visuais da Cutscene")]
    public RedFade redFade;
    public VignetteController vignetteController;
    public GameObject warningUI;

    // ============================================================
    // ==                   MINIGAME CONFIG                      ==
    // ============================================================
    [Header("Configurações de Partículas")]
    public Vector2 particleSizeRange = new Vector2(25f, 45f);
    public float particleLifetime = 2.5f;
    public float spawnInterval = 1f;

    private int score = 0;
    private bool isGameRunning = false;
    private List<GameObject> activeParticles = new List<GameObject>();

    // ============================================================
    // ==                SISTEMA DE COMBUSTÍVEL / MORTE          ==
    // ============================================================
    [Header("Combustível / Morte")]
    public float currentFuel = 100f;
    public float fuelDecayRate = 2f;
    public bool isDead = false;

    // ============================================================
    // ==                 COMPONENTES DA CUTSCENE                 ==
    // ============================================================
    [Header("Cutscene de Morte")]
    public CameraShakeDeath deathShake;
    public Light emergencyLight;
    public AudioSource alarmSource;
    public GameOverManager gameOverManager;
    public CameraFall cameraFall;

    // ============================================================
    // ==                         START                           ==
    // ============================================================
    void Start()
    {
        if (minigamePanel != null)
        minigamePanel.SetActive(false);

        if (startButton != null)
        {
            startButton.onClick.AddListener(() =>
            {
                StartGame();
                if (minigamePanel != null)
                    minigamePanel.SetActive(false);
            });
        }

        EnsureEventSystem();
    }

    // ============================================================
    // ==                         UPDATE                          ==
    // ============================================================
    void Update()
    {
        if (!isDead)
            FuelDecayLogic();

        // TESTE DE MORTE: apertar K para ver a cutscene
        if (Input.GetKeyDown(KeyCode.K) && !isDead)
        {
            StartCoroutine(DeathSequence());
        }
    }

    // ============================================================
    // ==     DIMINUIÇÃO DE COMBUSTÍVEL / DISPARO DE MORTE       ==
    // ============================================================
    void FuelDecayLogic()
    {
        currentFuel -= fuelDecayRate * Time.deltaTime;

        if (currentFuel <= 0 && !isDead)
        {
            StartCoroutine(DeathSequence());
        }
    }

    // ============================================================
    // ==                  CUTSCENE COMPLETA DE MORTE             ==
    // ============================================================
    IEnumerator DeathSequence()
    {
        isDead = true;
        Debug.Log("[GameManager] Cutscene de morte iniciada.");

        Debug.Log("=== DEBUG REDFADE ===");
        Debug.Log("Objeto atribuído ao redFade: " + redFade.gameObject.name);
        Debug.Log("Tem componente Image? " + (redFade.GetComponent<Image>() != null));


        // WARNING ON
        if (warningUI != null)
            warningUI.SetActive(true);

        // TELA VERMELHA FADE
        // ATIVA O OBJETO PAI QUE TEM O REDOVERLAY
        if (redFade != null)
        {
            GameObject overlay = redFade.gameObject;

            Debug.Log("Ativando RedOverlay: " + overlay.name);

            overlay.SetActive(true);   // <-- ATIVA AGORA
            yield return null;         // <-- espera 1 frame

            redFade.Initialize();      // <-- reconhece o Image agora
            StartCoroutine(redFade.FadeIn(0.5f));
        }



        // VIGNETTE
        if (vignetteController != null)
            StartCoroutine(vignetteController.IncreaseVignette(0.3f));

        // LUZ DE EMERGÊNCIA
        if (emergencyLight != null)
            emergencyLight.gameObject.SetActive(true);

        // SOM DE ALARME
        if (alarmSource != null)
            alarmSource.Play();

        // TREPIDAÇÃO DA MORTE
        if (deathShake != null)
            deathShake.StartDeathShake(3f, 0.4f);

        // CÂMERA CAINDO
        if (cameraFall != null)
            StartCoroutine(cameraFall.Fall(1f));

        // Espera a cutscene inteira acontecer
        yield return new WaitForSeconds(4f);

        // GAME OVER NO FINAL (IMPORTANTÍSSIMO)
        if (gameOverManager != null)
            gameOverManager.ShowGameOverScreen();
    }

    // ============================================================
    // ==                       MINIGAME                          ==
    // ============================================================
    public void StartGame()
    {
        if (filterArea == null || dustParticlePrefab == null)
        {
            Debug.LogError("[GameManager] Faltando referências (FilterArea ou DustParticlePrefab).");
            return;
        }

        EnsureCanvasSetup();

        score = 0;
        UpdateScoreUI();

        isGameRunning = true;
        StartCoroutine(SpawnParticles());

        Debug.Log("[GameManager] Minigame iniciado!");
    }

    IEnumerator SpawnParticles()
    {
        while (isGameRunning)
        {
            SpawnParticle();
            yield return new WaitForSecondsRealtime(spawnInterval);
        }
    }

    void SpawnParticle()
    {
        if (!isGameRunning || filterArea == null || dustParticlePrefab == null)
            return;

        GameObject particleGO = Instantiate(dustParticlePrefab);
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

        Image img = particleGO.GetComponent<Image>();
        if (img == null) img = particleGO.AddComponent<Image>();
        img.raycastTarget = true;

        ParticleClickHandler handler = particleGO.GetComponent<ParticleClickHandler>();
        if (handler == null) handler = particleGO.AddComponent<ParticleClickHandler>();
        handler.gameManagerRef = this;
        handler.particleGO = particleGO;

        StartCoroutine(RemoveParticleAfterDelay(particleGO, particleLifetime));
    }

    IEnumerator RemoveParticleAfterDelay(GameObject particle, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        if (particle != null)
        {
            activeParticles.Remove(particle);
            Destroy(particle);
        }
    }

    public void OnParticleClickedFromHandler(GameObject particle)
    {
        if (particle == null) return;

        if (activeParticles.Contains(particle))
            activeParticles.Remove(particle);

        Destroy(particle);
        score++;
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = $"Poeira limpa: {score}";
    }

    // ============================================================
    // ==                    CANVAS / EVENTOS                     ==
    // ============================================================
    public void PauseGame()
    {
        isGameRunning = false;
    }

    public void UnpauseGame()
    {
        if (!isGameRunning)
        {
            isGameRunning = true;
            StartCoroutine(SpawnParticles());
        }
    }

    void EnsureCanvasSetup()
    {
        Canvas canvas = filterArea.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            if (canvas.renderMode == RenderMode.WorldSpace && canvas.worldCamera == null && Camera.main != null)
                canvas.worldCamera = Camera.main;

            if (canvas.GetComponent<GraphicRaycaster>() == null)
                canvas.gameObject.AddComponent<GraphicRaycaster>();
        }
    }

    void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }
    }
}