using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Events;

public class StressManager : MonoBehaviour
{
    public Slider stressSlider;
    public float sliderSpeed = 0.1f;
    public CameraShake cameraShake;

    void Start()
    {
        stressSlider.value = 100f;
    }

    void Update()
    {
        stressSlider.value = stressSlider.value - sliderSpeed;
        Debug.Log(stressSlider.value);
        if(cameraShake.isShaking == false && stressSlider.value <= 50f)
        {
            cameraShake.isShaking = true;
        }
    }
}


