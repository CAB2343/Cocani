using UnityEngine;
using System.Collections;
using Cinemachine;

public class CameraShake : MonoBehaviour
{
    public float magnitude = 0.1f;

    private Vector3 originalPos;
    public bool isShaking = false;
    public CinemachineVirtualCamera virtualCamera;
    public float amplitude = 0.5f;
    public float frequency = 1f;

    private CinemachineBasicMultiChannelPerlin noise;

    void Start()
    {
        originalPos = transform.localPosition;

        if (virtualCamera != null)
        {
            noise = virtualCamera.GetCinemachineComponent<Cinemachine.CinemachineBasicMultiChannelPerlin>();
            if (noise == null)
            {
                Debug.LogError("CinemachineBasicMultiChannelPerlin not found on the virtual camera.");
            }
        }
        else
        {
            Debug.LogError("VirtualCamera reference is missing.");
        }
    }

    void Update()
    {
        if (noise == null) return;

        if (isShaking)
        {
            noise.m_AmplitudeGain = amplitude;
            noise.m_FrequencyGain = frequency;
            Debug.Log("Camera is shaking");
        }
        else
        {
            noise.m_AmplitudeGain = 0f;
            noise.m_FrequencyGain = 0f;
        }
    }
    
}