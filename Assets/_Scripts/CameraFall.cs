using UnityEngine;
using System.Collections;

public class CameraFall : MonoBehaviour
{
    public IEnumerator Fall(float speed)
    {
        float timer = 0f;
        Vector3 startRot = transform.localEulerAngles;

        while (timer < 2f)
        {
            transform.localEulerAngles = startRot + new Vector3(timer * 20f, 0, timer * 5f);
            timer += Time.deltaTime * speed;
            yield return null;
        }
    }
}
