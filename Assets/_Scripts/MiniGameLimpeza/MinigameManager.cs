using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System; // NECESSÁRIO PARA O ACTION

public class MinigameManager : MonoBehaviour
{
    // EVENTO PARA AVISAR O TRIGGER QUE O JOGO ACABOU (PARA A CÂMERA VOLTAR)
    public Action OnMinigameFinished;

    [Header("Referências UI")]
    public GameObject minigamePanel;
    public Button startButton;
    public RectTransform filterArea;

    // Área específica para limitar onde as partículas nascem
    [Tooltip("Arraste aqui um RectTransform vazio (filho do filterArea) que define a área segura de spawn.")]
    public RectTransform spawnAreaContainer;

    public GameObject dustParticlePrefab;
    public Text scoreText;
    
    // Referência para o texto do tempo
    public Text timerText; 

    [Header("Configurações de Partículas")]
    public Vector2 particleSizeRange = new Vector2(25f, 45f);
    public float particleLifetime = 2.5f;
    public float spawnInterval = 1f;

    [Header("Configurações de Jogo")]
    public int initialDirtCount = 15;
    
    // Tempo Limite
    public float timeLimit = 10f; 
    private float currentTime;
    private int currentDirtCount;

    public bool isGameRunning = false;
    private List<GameObject> activeParticles = new List<GameObject>();

    void Start()
    {
        if (minigamePanel != null) minigamePanel.SetActive(false);
        if (startButton != null) startButton.onClick.AddListener(StartMinigame);
        EnsureEventSystem();
    }

    void Update()
    {
        // Lógica do Cronômetro
        if (isGameRunning)
        {
            currentTime -= Time.unscaledDeltaTime;
            
            if (currentTime <= 0)
            {
                currentTime = 0;
                FinishMinigameFailed(); // Tempo acabou
            }

            UpdateTimerUI();
        }
    }

    public void StartMinigame()
    {
        if (filterArea == null || dustParticlePrefab == null)
        {
            Debug.LogError("[MinigameManager] Faltando referências essenciais (FilterArea ou Prefab).");
            return;
        }

        // Fallback: Se esqueceu de arrastar o container, usa a área toda
        if (spawnAreaContainer == null)
        {
            Debug.LogWarning("[MinigameManager] spawnAreaContainer não definido. Usando filterArea como padrão.");
            spawnAreaContainer = filterArea;
        }

        if (minigamePanel != null) minigamePanel.SetActive(true);

        EnsureCanvasSetup();

        // Resetar Valores
        currentDirtCount = initialDirtCount;
        currentTime = timeLimit; // Reseta o tempo
        
        UpdateScoreUI();
        UpdateTimerUI();

        isGameRunning = true;
        StartCoroutine(SpawnParticles());
    }

    public void StopMinigame()
    {
        isGameRunning = false;
        StopAllCoroutines();

        for (int i = activeParticles.Count - 1; i >= 0; i--)
        {
            if (activeParticles[i] != null) Destroy(activeParticles[i]);
        }
        activeParticles.Clear();

        if (minigamePanel != null) minigamePanel.SetActive(false);
    }

    private void FinishMinigameSuccess()
    {
        Debug.Log("Venceu: Limpeza concluída!");
        StopMinigame();
        
        // AVISA O TRIGGER PARA RESETAR A CÂMERA
        OnMinigameFinished?.Invoke(); 
    }

    private void FinishMinigameFailed()
    {
        Debug.Log("Perdeu: O tempo acabou!");
        StopMinigame();
        
        // AVISA O TRIGGER PARA RESETAR A CÂMERA (Mesmo perdendo, a câmera tem que voltar)
        OnMinigameFinished?.Invoke();
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
        if (!isGameRunning) return;

        GameObject particleGO = Instantiate(dustParticlePrefab);
        
        // Define o pai como o container delimitador
        particleGO.transform.SetParent(spawnAreaContainer, false);
        activeParticles.Add(particleGO);

        RectTransform rect = particleGO.GetComponent<RectTransform>();
        if (rect == null) rect = particleGO.AddComponent<RectTransform>();

        float size = UnityEngine.Random.Range(particleSizeRange.x, particleSizeRange.y);
        rect.sizeDelta = new Vector2(size, size);

        // Calcula posição baseado no tamanho do container
        float containerWidth = spawnAreaContainer.rect.width;
        float containerHeight = spawnAreaContainer.rect.height;

        float halfWidth = (containerWidth - size) / 2f;
        float halfHeight = (containerHeight - size) / 2f;

        rect.anchoredPosition = new Vector2(
            UnityEngine.Random.Range(-halfWidth, halfWidth),
            UnityEngine.Random.Range(-halfHeight, halfHeight)
        );

        if (particleGO.GetComponent<Image>() == null)
            particleGO.AddComponent<Image>();

        ParticleClickHandler handler = particleGO.GetComponent<ParticleClickHandler>();
        if (handler == null) handler = particleGO.AddComponent<ParticleClickHandler>();
        
        handler.SetManager(this);

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

    public void OnParticleClicked(GameObject particle)
    {
        if (particle == null) return;
        if (activeParticles.Contains(particle)) activeParticles.Remove(particle);
        Destroy(particle);

        currentDirtCount--;
        if (currentDirtCount < 0) currentDirtCount = 0;

        UpdateScoreUI();

        if (currentDirtCount <= 0)
        {
            FinishMinigameSuccess();
        }
    }

    void UpdateScoreUI()
    {
        if (scoreText != null) scoreText.text = $"Sujeira: {currentDirtCount}";
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
            timerText.text = $"Tempo: {currentTime.ToString("F1")}s";
    }

    void EnsureCanvasSetup()
    {
        // Garante que o Raycast funcione na UI
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