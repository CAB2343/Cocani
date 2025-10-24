using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadarArea : MonoBehaviour
{
    public float sphereRadius = 5f;
    public LayerMask layerMask;

    void Update()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, sphereRadius);

        bool detectou = false;

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("minigame"))
            {
                Debug.Log("(づ￣ 3￣)づ");
                detectou = true;
            }
        }

        if (!detectou)
        {
            Debug.Log("não");
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sphereRadius);
    }
}
