using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RedFade : MonoBehaviour
{
    private Image img;

    [Header("Opacidade Máxima do Vermelho (0 a 1)")]
    [Range(0f, 1f)]
    public float maxAlpha = 0.35f;   // valor ideal para ver o ambiente

    public void Initialize()
    {
        img = GetComponent<Image>();
    }

    public IEnumerator FadeIn(float speed)
    {
        if (img == null)
            yield break;

        Color c = img.color;

        // Fade até o maxAlpha configurado
        while (c.a < maxAlpha)
        {
            c.a += Time.deltaTime * speed;
            img.color = c;
            yield return null;
        }

        c.a = maxAlpha;
        img.color = c;
    }

    public IEnumerator FadeOut(float speed)
    {
        if (img == null)
            yield break;

        Color c = img.color;

        while (c.a > 0f)
        {
            c.a -= Time.deltaTime * speed;
            img.color = c;
            yield return null;
        }

        c.a = 0f;
        img.color = c;

        // Depois que some, desativa o objeto
        gameObject.SetActive(false);
    }
}