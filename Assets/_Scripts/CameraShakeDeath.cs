using UnityEngine;

public class CameraShakeDeath : MonoBehaviour
{
    private bool shaking = false;
    private float shakeDuration;
    private float shakeIntensity;
    private Vector3 originalPos;

    void Start()
    {
        originalPos = transform.localPosition;
    }

    public void StartDeathShake(float duration, float intensity)
    {
        shakeDuration = duration;
        shakeIntensity = intensity;
        shaking = true;
    }

    void Update()
    {
        if (shaking)
        {
            if (shakeDuration > 0)
            {
                transform.localPosition = originalPos + Random.insideUnitSphere * shakeIntensity;
                shakeDuration -= Time.deltaTime;
            }
            else
            {
                shaking = false;
                transform.localPosition = originalPos;
            }
        }
    }

    // ============================================================
    // ==      FUNÇÃO NOVA — PARA O SHAKE IMEDIATAMENTE         ==
    // ============================================================
    public void StopShake()
    {
        shaking = false;
        transform.localPosition = originalPos;
    }
}