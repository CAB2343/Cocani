using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    // ============================================================
    // ==                   REFERÊNCIAS UI                        ==
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
    // ==                   MINIGAME CONFIG                       ==
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

    void Update()
    {
        if (!isDead)
            FuelDecayLogic();

        // TESTE DE MORTE — K
        if (Input.GetKeyDown(KeyCode.K) && !isDead)
            StartCoroutine(DeathSequence());
    }


    // ============================================================
    // ==  FUEL LOGIC                                             ==
    // ============================================================
    void FuelDecayLogic()
    {
        currentFuel -= fuelDecayRate * Time.deltaTime;

        if (currentFuel <= 0 && !isDead)
            StartCoroutine(DeathSequence());
    }


    // ============================================================
    // ==  SEQUÊNCIA DE MORTE COMPLETA                            ==
    // ============================================================
    IEnumerator DeathSequence()
    {
        isDead = true;
        Debug.Log("[GameManager] Cutscene de morte iniciada.");

        // WARNING ON
        if (warningUI != null)
            warningUI.SetActive(true);

        // RED FADE — ATIVA, INICIALIZA E AUMENTA
        if (redFade != null)
        {
            redFade.gameObject.SetActive(true);
            yield return null;
            redFade.Initialize();
            StartCoroutine(redFade.FadeIn(0.5f));
        }

        // VIGNETTE — INCREASE
        if (vignetteController != null)
            StartCoroutine(vignetteController.IncreaseVignette(0.4f));

        // EMERGENCY LIGHT
        if (emergencyLight != null)
            emergencyLight.gameObject.SetActive(true);

        // ALARM SOUND
        if (alarmSource != null)
            alarmSource.Play();

        // SHAKE
        if (deathShake != null)
            deathShake.StartDeathShake(3f, 0.4f);

        // CAMERA FALL
        if (cameraFall != null)
            StartCoroutine(cameraFall.Fall(1f));

        // Aguarda cutscene
        yield return new WaitForSeconds(4f);

        // LIMPA OS EFEITOS (FADE OUT)
        ClearDeathEffects();

        // Aguarda efeitos sumirem
        yield return new WaitForSeconds(1f);

        // SHOW GAME OVER POR ÚLTIMO
        if (gameOverManager != null)
            gameOverManager.ShowGameOverScreen();
    }


    // ============================================================
    // ==  LIMPEZA DOS EFEITOS DA MORTE                           ==
    // ============================================================
    public void ClearDeathEffects()
    {
        // Warning desaparece
        if (warningUI != null)
            warningUI.SetActive(false);

        // Luz apaga
        if (emergencyLight != null)
            emergencyLight.gameObject.SetActive(false);

        // Alarme para
        if (alarmSource != null)
            alarmSource.Stop();

        // Shake para
        if (deathShake != null)
            deathShake.StopShake();

        // Fade out do vermelho
        if (redFade != null)
            StartCoroutine(redFade.FadeOut(0.7f));

        // Fade out do vignette
        if (vignetteController != null)
            StartCoroutine(vignetteController.DecreaseVignette(0.7f));
    }


    // ============================================================
    // ==  MINIGAME (SEM ALTERAÇÕES CRÍTICAS)                     ==
    // ============================================================
    public void StartGame()
    {
        if (filterArea == null || dustParticlePrefab == null)
        {
            Debug.LogError("[GameManager] Faltando referências.");
            return;
        }

        EnsureCanvasSetup();
        score = 0;
        UpdateScoreUI();

        isGameRunning = true;
        StartCoroutine(SpawnParticles());
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

        float halfWidth = (filterArea.rect.width - size) / 2f;
        float halfHeight = (filterArea.rect.height - size) / 2f;

        rect.anchoredPosition = new Vector2(
            Random.Range(-halfWidth, halfWidth),
            Random.Range(-halfHeight, halfHeight)
        );

        Image img = particleGO.GetComponent<Image>();
        if (img == null) img = particleGO.AddComponent<Image>();

        ParticleClickHandler handler = particleGO.AddComponent<ParticleClickHandler>();
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
    // ==  FUNÇÕES DE PAUSA NECESSÁRIAS PARA PauseMenu           ==
    // ============================================================
    public void PauseGame()
    {
        Time.timeScale = 0f;
    }

    public void UnpauseGame()
    {
        Time.timeScale = 1f;
    }


    // ============================================================
    // ==  CANVAS + EVENT SYSTEM                                  ==
    // ============================================================
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
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }
}