using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class CutSceneController : MonoBehaviour
{

    public VideoPlayer cutSceneVideoPlayer;
    public GameObject CutSceneCanvas;
    public GameObject barraBussula;

    private bool hasEnded = false;

    void Start()
    {
        if(cutSceneVideoPlayer != null)
        {
            cutSceneVideoPlayer.Play();
            CutSceneCanvas.SetActive(true);
            barraBussula.SetActive(false);

        }

    }

    void Update()
    {
        if (cutSceneVideoPlayer.isPlaying)
        {
            if(cutSceneVideoPlayer.time >= cutSceneVideoPlayer.length - 0.1f && !hasEnded)
            {
                hasEnded = true;
                CutSceneCanvas.SetActive(false);
                barraBussula.SetActive(true);
            }
        }

    }   

}
