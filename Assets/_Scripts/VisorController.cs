using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisorController : MonoBehaviour
{
    public GameObject panelVisor;

    // Update is called once per frame

    void Start()
    {
        panelVisor.SetActive(false);
    }

    void Update()
    {
        // Detecta quando a tecla V é pressionada (início)
        if (Input.GetKeyDown(KeyCode.V))
        {
            panelVisor.SetActive(true);
        }
        // Detecta quando a tecla V é solta (fim)
        else if (Input.GetKeyUp(KeyCode.V))
        {
            panelVisor.SetActive(false);
        }
    }
}
