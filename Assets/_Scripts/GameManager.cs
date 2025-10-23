using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    [Header("Referências UI")]
    public GameObject minigamePanel;         // Painel do minigame (com o botão e área de filtro)
    public Button startButton;               // Botão “Iniciar Limpeza”
    public RectTransform filterArea;         // Área onde as partículas aparecem
    public GameObject dustParticlePrefab;    // Prefab das partículas
    public Text scoreText;                   // Texto de pontuação

    [Header("Configurações de Partículas")]
    public Vector2 particleSizeRange = new Vector2(25f, 45f);
    public float particleLifetime = 2.5f;
    public float spawnInterval = 1f;

    private int score = 0;
    private bool isGameRunning = false;
    private List<GameObject> activeParticles = new List<GameObject>();

    void Start()
    {
        // Garante que o painel do minigame esteja visível no início
        if (minigamePanel != null)
            minigamePanel.SetActive(true);

        // Configura o botão "Iniciar Limpeza"
        if (startButton != null)
        {
            startButton.onClick.AddListener(() =>
            {
                StartGame();
                if (minigamePanel != null)
                    minigamePanel.SetActive(false); // Oculta o painel ao iniciar
            });
        }

        EnsureEventSystem();
    }

    // =============================================================
    // ==            INÍCIO E LÓGICA DO MINIGAME                  ==
    // =============================================================
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

    // =============================================================
    // ==                   SPAWN DE PARTÍCULAS                   ==
    // =============================================================
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

        // Adiciona handler de clique
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

    // =============================================================
    // ==                   SUPORTE AO PAUSE                      ==
    // =============================================================
    public void PauseGame()
    {
        isGameRunning = false;
        Debug.Log("[GameManager] Jogo pausado.");
    }

    public void UnpauseGame()
    {
        if (!isGameRunning)
        {
            isGameRunning = true;
            StartCoroutine(SpawnParticles());
            Debug.Log("[GameManager] Jogo retomado.");
        }
    }

    // =============================================================
    // ==                 AJUSTES DE CANVAS E EVENTOS              ==
    // =============================================================
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
            Debug.Log("[GameManager] EventSystem criado automaticamente.");
        }
    }
}
