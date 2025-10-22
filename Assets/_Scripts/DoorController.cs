using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;


public class DoorController : MonoBehaviour
{
    public AnimancerComponent animancer;
    public AnimationClip openDoor;
    public AnimationClip closeDoor;
    public bool stayClosed = false;
    private bool isOpen = false;

    void Start()
    {
        if (animancer != null && closeDoor != null)
            animancer.Play(closeDoor);
        isOpen = false;
    }
    void OnTriggerEnter(Collider other)
    {
        if (isOpen == false && other.CompareTag("Player") && stayClosed == false)
        {
            if (animancer != null && openDoor != null)
                animancer.Play(openDoor);
            isOpen = true;
        }
                
    }
    void OnTriggerExit(Collider other)
    {
        if(isOpen == true && other.CompareTag("Player") && stayClosed == false)
        {
            Debug.Log("Player saiu do trigger, fechando porta.");
            if (animancer != null && closeDoor != null)
                animancer.Play(closeDoor);
            isOpen = false;
        }

    }
}
