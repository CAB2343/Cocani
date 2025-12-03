using System.Collections;
using UnityEngine;
using UnityEngine.UI; 

public class GameManager : MonoBehaviour
{
    // ============================================================
    // ==                   SISTEMA DE COMBUSTÍVEL / MORTE       ==
    // ============================================================
    [Header("Combustível / Morte")]
    public float currentFuel = 100f;
    public float fuelDecayRate = 2f;
    public bool isDead = false;

    // ============================================================
    // ==                   SISTEMA DE SANIDADE                  ==
    // ============================================================
    [Header("Sanidade")]
    public float currentSanity = 100f;
    public float sanityDecayRate = 1.5f; 
    public Image sanityImage;            
    public Gradient sanityGradient;      

    [Header("UI Sanidade")]
    public GameObject sanityTextUI; // Texto de aviso (Aparece nos 30)
    private bool sanityWarningShown = false; // Trava para mostrar o aviso apenas uma vez

    // Variável interna para avisos
    private bool lowFuelWarningActive = false;

    // ============================================================
    // ==                 COMPONENTES DA CUTSCENE                ==
    // ============================================================
    [Header("Referências da Cutscene")]
    public GameObject warningUI;
    public RedFade redFade;
    public VignetteController vignetteController;
    public CameraShakeDeath deathShake;
    public Light emergencyLight;
    public AudioSource alarmSource;
    public GameOverManager gameOverManager;
    public CameraFall cameraFall;

    void Update()
    {
        if (!isDead)
        {
            FuelDecayLogic();
            SanityLogic(); 
        }

        // Atalho de debug
        if (Input.GetKeyDown(KeyCode.K) && !isDead)
            StartCoroutine(FuelDeathSequence());
    }

    // ============================================================
    // ==            LÓGICA DE SANIDADE E DESCANSO               ==
    // ============================================================
    
    void SanityLogic()
    {
        currentSanity -= sanityDecayRate * Time.deltaTime;
        currentSanity = Mathf.Clamp(currentSanity, 0f, 100f);

        if (sanityImage != null)
            sanityImage.color = sanityGradient.Evaluate(currentSanity / 100f);

        // ========================================================
        // AVISO DE LOUCURA (30%): Apenas Texto (3s)
        // ========================================================
        if (currentSanity <= 30f && !sanityWarningShown)
        {
            StartCoroutine(ShowSanityWarningRoutine());
        }
        // Se a sanidade subir, reseta a trava para avisar novamente se cair de novo
        else if (currentSanity > 30f)
        {
            sanityWarningShown = false;
            if (sanityTextUI != null) sanityTextUI.SetActive(false);
        }

        // MORTE POR SANIDADE (0%)
        if (currentSanity <= 0 && !isDead)
        {
            StartCoroutine(SanityDeathSequence());
        }
    }

    // COROUTINE: Controla apenas o tempo do texto
    IEnumerator ShowSanityWarningRoutine()
    {
        sanityWarningShown = true; // Trava para não repetir

        // 1. Ativa o Texto
        if (sanityTextUI != null) sanityTextUI.SetActive(true);

        // 2. Espera 3 segundos
        yield return new WaitForSeconds(3f);

        // 3. Desativa o Texto
        if (sanityTextUI != null) sanityTextUI.SetActive(false);
    }

    public void RestaurarSanidade(float quantidade)
    {
        if (isDead) return;

        currentSanity += quantidade;
        currentSanity = Mathf.Clamp(currentSanity, 0f, 100f);
        
        Debug.Log($"[GameManager] Descansou. Sanidade recuperada para: {currentSanity}");
    }

    IEnumerator SanityDeathSequence()
    {
        isDead = true;
        Debug.Log("[GameManager] Morreu de Insanidade.");

        // Força o texto aparecer de novo na hora da morte
        if (sanityTextUI != null) sanityTextUI.SetActive(true);

        // Efeitos de desmaio
        if (cameraFall != null) StartCoroutine(cameraFall.Fall(1f));
        if (vignetteController != null) StartCoroutine(vignetteController.IncreaseVignette(0.9f)); 

        yield return new WaitForSeconds(4f);

        if (gameOverManager != null) gameOverManager.ShowGameOverScreen();
    }

    // ============================================================
    // ==            LÓGICA DE COMBUSTÍVEL E MORTE               ==
    // ============================================================

    void FuelDecayLogic()
    {
        currentFuel -= fuelDecayRate * Time.deltaTime;

        if (currentFuel <= 30f && !lowFuelWarningActive && !isDead)
        {
            ActivateLowFuelWarning();
        }

        if (currentFuel <= 0 && !isDead)
        {
            StartCoroutine(FuelDeathSequence());
        }
    }

    void ActivateLowFuelWarning()
    {
        lowFuelWarningActive = true;
        Debug.Log("[GameManager] Combustível Crítico.");

        if (warningUI != null) warningUI.SetActive(true);
        if (emergencyLight != null) emergencyLight.gameObject.SetActive(true);
        if (alarmSource != null) alarmSource.Play();
        if (redFade != null)
        {
            redFade.gameObject.SetActive(true);
            redFade.Initialize();
            StartCoroutine(redFade.FadeIn(2.0f));
        }
        if (vignetteController != null) StartCoroutine(vignetteController.IncreaseVignette(0.25f));
    }

    IEnumerator FuelDeathSequence()
    {
        isDead = true;
        Debug.Log("[GameManager] Morreu sem Combustível.");

        if (!lowFuelWarningActive) ActivateLowFuelWarning();

        if (vignetteController != null) StartCoroutine(vignetteController.IncreaseVignette(0.5f));
        if (deathShake != null) deathShake.StartDeathShake(3f, 0.4f);
        if (cameraFall != null) StartCoroutine(cameraFall.Fall(1f));

        yield return new WaitForSeconds(4f);
        ClearDeathEffects();
        yield return new WaitForSeconds(1f);

        if (gameOverManager != null) gameOverManager.ShowGameOverScreen();
    }

    public void ClearDeathEffects()
    {
        if (warningUI != null) warningUI.SetActive(false);
        if (emergencyLight != null) emergencyLight.gameObject.SetActive(false);
        if (alarmSource != null) alarmSource.Stop();
        if (deathShake != null) deathShake.StopShake();
        if (redFade != null) StartCoroutine(redFade.FadeOut(0.7f));
        if (vignetteController != null) StartCoroutine(vignetteController.DecreaseVignette(0.7f));
        
        // Esconde o texto de sanidade ao reiniciar
        if (sanityTextUI != null) sanityTextUI.SetActive(false);
        sanityWarningShown = false;
        
        lowFuelWarningActive = false;
    }

    public void PauseGame() => Time.timeScale = 0f;
    public void UnpauseGame() => Time.timeScale = 1f;
}