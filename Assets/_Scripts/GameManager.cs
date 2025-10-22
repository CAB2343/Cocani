using UnityEngine;
using UnityEngine.UI;
// using UnityEngine.EventSystems; // removido para evitar CS0246
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

    [Header("UI Components")]
    public Canvas mainCanvas;
    public GameObject eventSystemGO; // substitui EventSystem tipado
    private Component eventSystemComponent; // referência ao componente EventSystem via string

    [Header("Raycaster Settings")]
    public Camera raycastCamera;

    private Component physicsRaycaster;
    private GraphicRaycaster graphicRaycaster;

    void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (startButton != null) startButton.onClick.AddListener(StartGame);
        if (restartButton != null) restartButton.onClick.AddListener(RestartGame);

        UpdateUI();
        if (minigamePanel != null) minigamePanel.SetActive(false);

        InitializeRaycasters();
    }

    void InitializeRaycasters()
    {
        if (raycastCamera != null)
        {
            physicsRaycaster = raycastCamera.GetComponent("PhysicsRaycaster");
            if (physicsRaycaster == null)
            {
                physicsRaycaster = raycastCamera.gameObject.AddComponent("PhysicsRaycaster");
            }
        }

        if (mainCanvas != null)
        {
            graphicRaycaster = mainCanvas.GetComponent<GraphicRaycaster>();
            if (graphicRaycaster == null)
            {
                graphicRaycaster = mainCanvas.gameObject.AddComponent<GraphicRaycaster>();
            }
        }

        // Garantir EventSystem sem usar tipos explícitos
        if (eventSystemGO == null)
        {
            eventSystemGO = GameObject.Find("EventSystem");
            if (eventSystemGO == null)
            {
                eventSystemGO = new GameObject("EventSystem");
            }
        }
        eventSystemComponent = eventSystemGO.GetComponent("EventSystem");
        if (eventSystemComponent == null)
        {
            eventSystemComponent = eventSystemGO.AddComponent("EventSystem");
        }
        if (eventSystemGO.GetComponent("StandaloneInputModule") == null)
        {
            eventSystemGO.AddComponent("StandaloneInputModule");
        }
    }

    // Método para fazer raycast físico
    public bool PhysicsRaycast(Vector3 origin, Vector3 direction, out RaycastHit hit, float maxDistance = Mathf.Infinity)
    {
        return Physics.Raycast(origin, direction, out hit, maxDistance);
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

        if (physicsRaycaster == null && raycastCamera != null)
        {
            physicsRaycaster = raycastCamera.GetComponent("PhysicsRaycaster");
        }

        if (graphicRaycaster == null && mainCanvas != null)
        {
            graphicRaycaster = mainCanvas.GetComponent<GraphicRaycaster>();
        }

        // mantém o EventSystem caso seja removido
        if (eventSystemGO != null)
        {
            if (eventSystemGO.GetComponent("EventSystem") == null)
                eventSystemGO.AddComponent("EventSystem");
            if (eventSystemGO.GetComponent("StandaloneInputModule") == null)
                eventSystemGO.AddComponent("StandaloneInputModule");
        }
    }

    // Método para obter o PhysicsRaycaster atual
    public Component GetPhysicsRaycaster()
    {
        return physicsRaycaster;
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

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        StartCoroutine(SpawnParticlesRoutine());

        Canvas canvas = filterArea.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            if (canvas.renderMode == RenderMode.WorldSpace)
            {
                if (canvas.worldCamera == null && Camera.main != null)
                {
                    canvas.worldCamera = Camera.main;
                }
                if (Camera.main != null && Camera.main.GetComponent("PhysicsRaycaster") == null)
                    Camera.main.gameObject.AddComponent("PhysicsRaycaster");
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

        GameObject particleGO = Instantiate(dustParticlePrefab);
        particleGO.name = dustParticlePrefab.name + "_Instance";
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

        // Garantir Button para clique sem depender de IPointerClickHandler
        Button btn = particleGO.GetComponent<Button>();
        if (btn == null) btn = particleGO.AddComponent<Button>();
        btn.transition = Selectable.Transition.None;
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => {
            OnParticleClickedFromHandler(particleGO);
        });

        // Removido handler antigo para evitar dependência de EventSystems
        var oldHandler = particleGO.GetComponent<ParticleClickHandler>();
        if (oldHandler != null)
        {
            Destroy(oldHandler);
        }

        ParticleClickHandler handler = particleGO.GetComponent<ParticleClickHandler>();
        if (handler == null) handler = particleGO.AddComponent<ParticleClickHandler>();
        handler.gameManagerRef = this;
        handler.particleGO = particleGO;

        Debug.Log($"[SpawnParticle] Spawnou {particleGO.name} em {rect.anchoredPosition} (raycastTarget={img.raycastTarget})");

        StartCoroutine(RemoveParticleAfterDelay(particleGO, particleLifetime));
    }

    public void OnParticleClickedFromHandler(GameObject particleGO)
    {
        OnParticleClickedInternal(particleGO);
    }

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
