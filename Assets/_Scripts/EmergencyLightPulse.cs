using UnityEngine;

public class EmergencyLightPulse : MonoBehaviour
{
    public Light emergencyLight;
    public float minIntensity = 0f;
    public float maxIntensity = 8f;
    public float speed = 5f;

    void Update()
    {
        emergencyLight.intensity = Mathf.Lerp(minIntensity, maxIntensity,
            Mathf.Abs(Mathf.Sin(Time.time * speed)));
    }
}
