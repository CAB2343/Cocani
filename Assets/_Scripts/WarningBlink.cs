using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WarningBlink : MonoBehaviour
{
    public Image warningImage;
    public float speed = 5f;
    public float maxAlpha = 1f;
    public float minAlpha = 0.2f;

    void Update()
    {
        Color c = warningImage.color;
        c.a = Mathf.Lerp(minAlpha, maxAlpha, Mathf.Abs(Mathf.Sin(Time.time * speed)));
        warningImage.color = c;
    }
}