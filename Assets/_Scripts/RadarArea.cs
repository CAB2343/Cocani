using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadarArea : MonoBehaviour
{
    public float sphereRadius = 5f;
    public LayerMask layerMask;

    void Update()
    {
        // Pega todos os colliders dentro da esfera
        Collider[] hits = Physics.OverlapSphere(transform.position, sphereRadius);

        bool detectou = false;

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("minigame"))
            {
                Debug.Log("Detectado: " + hit.name);
                detectou = true;
            }
        }

        if (!detectou)
        {
            Debug.Log("Nenhum objeto 'minigame' dentro da esfera");
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sphereRadius);
    }
}
