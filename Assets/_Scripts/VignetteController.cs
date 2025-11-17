using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class VignetteController : MonoBehaviour
{
    private Vignette vignette;

    void Start()
    {
        Volume volume = GetComponent<Volume>();
        volume.profile.TryGet(out vignette);
    }

    public IEnumerator IncreaseVignette(float speed)
    {
        if (vignette == null) yield break;

        while (vignette.intensity.value < 0.6f)
        {
            vignette.intensity.value += Time.deltaTime * speed;
            yield return null;
        }
    }
}