using UnityEngine;
using System.Collections;

public class CameraShakeDeath : MonoBehaviour
{
    private Vector3 originalPos;

    void Start()
    {
        originalPos = transform.localPosition;
    }

    public void StartDeathShake(float duration, float intensity)
    {
        StopAllCoroutines();
        StartCoroutine(ShakeRoutine(duration, intensity));
    }

    IEnumerator ShakeRoutine(float duration, float intensity)
    {
        float timer = 0f;

        while (timer < duration)
        {
            transform.localPosition = originalPos + Random.insideUnitSphere * intensity;
            timer += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
    }
}
