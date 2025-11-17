using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RedFade : MonoBehaviour
{
    private Image img;

    public void Initialize()
    {
        img = GetComponent<Image>();
        Debug.Log("[RedFade] Initialize → achou Image? " + (img != null));
    }

    public IEnumerator FadeIn(float speed)
    {
        Debug.Log("FadeIn START — ativo? " + gameObject.activeInHierarchy);

        Color c = img.color;

        while (c.a < 0.8f)
        {
            if (img == null)
            {
                Debug.LogError("IMG VIROU NULL NO MEIO DO FADE!");
                yield break;
            }

            c.a += Time.deltaTime * speed;
            img.color = c;
            yield return null;
        }
    }
}