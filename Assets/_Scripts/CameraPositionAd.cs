using UnityEngine;

public class CameraPositionAd : MonoBehaviour
{
    public Transform cameraPosition;

    void LateUpdate()
    {
        transform.position = cameraPosition.position;
    }
}
