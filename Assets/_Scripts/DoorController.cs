using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;
using UnityEngine.InputSystem.XInput;


public class DoorController : MonoBehaviour
{
    public AnimancerComponent animancer;
    public AnimationClip openDoor;
    public AnimationClip closeDoor;
    public bool stayClosed = false;
    private bool isOpen = false;

    void Start()
    {
        animancer.Play(closeDoor);
        isOpen = false;
    }
    void OnTriggerEnter(Collider other)
    {
        if (isOpen == false && other.CompareTag("Player") && stayClosed == false)
        {
            animancer.Play(openDoor);
            isOpen = true;
        }
                
    }
    void OnTriggerExit(Collider other)
    {
        if(isOpen == true && other.CompareTag("Player") && stayClosed == false)
        {
            Debug.Log("Player saiu do trigger, fechando porta.");
            animancer.Play(closeDoor);
            isOpen = false;
        }

    }
}
