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
        if(Input.GetKeyDown(KeyCode.V) && panelVisor.activeSelf == false)
        {
            panelVisor.SetActive(true);
        }
        else if(Input.GetKeyDown(KeyCode.V) && panelVisor.activeSelf == true)
        {
            panelVisor.SetActive(false);
        }


    }
}
