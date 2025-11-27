using System.Collections;
using UnityEngine;

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
            FuelDecayLogic();

        if (Input.GetKeyDown(KeyCode.K) && !isDead)
            StartCoroutine(DeathSequence());
    }

    void FuelDecayLogic()
    {
        currentFuel -= fuelDecayRate * Time.deltaTime;

        if (currentFuel <= 0 && !isDead)
            StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        isDead = true;
        Debug.Log("[GameManager] Cutscene de morte iniciada.");

        if (warningUI != null) warningUI.SetActive(true);
        if (emergencyLight != null) emergencyLight.gameObject.SetActive(true);
        if (alarmSource != null) alarmSource.Play();
        
        if (redFade != null)
        {
            redFade.gameObject.SetActive(true);
            yield return null;
            redFade.Initialize();
            StartCoroutine(redFade.FadeIn(0.5f));
        }

        if (vignetteController != null) StartCoroutine(vignetteController.IncreaseVignette(0.4f));
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
    }

    // ============================================================
    // ==  FUNÇÕES GLOBAIS DE PAUSA                              ==
    // ============================================================
    public void PauseGame() => Time.timeScale = 0f;
    public void UnpauseGame() => Time.timeScale = 1f;
}