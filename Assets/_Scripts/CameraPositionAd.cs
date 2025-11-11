using UnityEngine;

public class CameraPositionAd : MonoBehaviour
{
    public Transform cameraPosition;

    void LateUpdate()
    {
        // Atualiza apenas a posição da câmera virtual
        transform.position = cameraPosition.position;
    }
}
