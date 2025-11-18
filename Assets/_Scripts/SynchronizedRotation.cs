using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SynchronizedRotation : MonoBehaviour
{
    void Update()
    {
        Vector3 cameraRotation = Camera.main.transform.rotation.eulerAngles;

        transform.rotation = Quaternion.Euler(0, cameraRotation.y, 0);
    }
}
