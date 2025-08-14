using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRayCast : MonoBehaviour
{

    public float distanciaDoRaio = 5f;
    public float LugarDoRaio = 0.80f;

    void Start()
    {

    }
    void Update()
    {
        Vector3 RayOrigin = transform.position + Vector3.up * LugarDoRaio; 
        Ray ray = new Ray(RayOrigin, transform.forward);
        RaycastHit hit; 
        Debug.DrawRay(RayOrigin, transform.forward * distanciaDoRaio, Color.red);

        if(Physics.Raycast(ray, out hit, distanciaDoRaio))
        {
            Debug.Log(hit.collider.name);
        }
    
    }



}
