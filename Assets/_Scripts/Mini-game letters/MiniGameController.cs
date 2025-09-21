using UnityEngine;

public class MiniGameController : MonoBehaviour
{
    [Header("UI / Gameplay")]
    public GameObject miniGameUI;
    public PlayerController playerController;
    public Camera overridePlayerCamera;
    public bool unlockCursorOnOpen = true;

    // player states
    private bool prevEnableMovement;
    private bool prevEnableGravity;
    private bool prevEnableJump;
    private bool isOpen = false;

    // camera / audiolistener
    private Camera playerCamera;
    private AudioListener playerAudio;
    private bool prevAudioEnabled;
    private bool cameraFound = false;

    // Cinemachine brain (handled via reflection-safe Component)
    private Behaviour cinemachineBrainBehaviour;
    private bool prevCinemachineBrainEnabled;

    void Start()
    {
        if (miniGameUI != null) miniGameUI.SetActive(false);

        if (playerController == null)
        {
            var go = GameObject.FindWithTag("Player");
            if (go != null) playerController = go.GetComponent<PlayerController>();
        }

        ResolvePlayerCamera();
    }

    void ResolvePlayerCamera()
    {
        if (overridePlayerCamera != null)
            playerCamera = overridePlayerCamera;
        else if (playerController != null && playerController._MyCamera != null)
            playerCamera = playerController._MyCamera.GetComponent<Camera>();

        if (playerCamera == null && playerController != null)
            playerCamera = playerController.GetComponentInChildren<Camera>();

        if (playerCamera != null)
        {
            playerAudio = playerCamera.GetComponent<AudioListener>();
            cameraFound = true;

            // tenta pegar CinemachineBrain sem depender do namespace
            var comp = playerCamera.GetComponent("CinemachineBrain");
            if (comp != null && comp is Behaviour b)
            {
                cinemachineBrainBehaviour = b;
                prevCinemachineBrainEnabled = b.enabled;
            }
            else
            {
                cinemachineBrainBehaviour = null;
            }
        }
        else
        {
            cameraFound = false;
            cinemachineBrainBehaviour = null;
        }
    }

    public void Open()
    {
        if (isOpen) return;
        isOpen = true;

        if (playerController != null)
        {
            prevEnableMovement = playerController._EnableMovement;
            prevEnableGravity = playerController._EnableGravity;
            prevEnableJump = playerController._EnableJump;

            playerController._EnableMovement = false;
            playerController._EnableGravity = false;
            playerController._EnableJump = false;
        }

        if (!cameraFound) ResolvePlayerCamera();

        // apenas desativa o movimento da Cinemachine, não a câmera
        if (cinemachineBrainBehaviour != null)
        {
            prevCinemachineBrainEnabled = cinemachineBrainBehaviour.enabled;
            cinemachineBrainBehaviour.enabled = false;
        }

        if (playerAudio != null)
        {
            prevAudioEnabled = playerAudio.enabled;
            playerAudio.enabled = false;
        }

        if (miniGameUI != null) miniGameUI.SetActive(true);

        if (unlockCursorOnOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void Close()
    {
        if (!isOpen) return;
        isOpen = false;

        if (playerController != null)
        {
            playerController._EnableMovement = prevEnableMovement;
            playerController._EnableGravity = prevEnableGravity;
            playerController._EnableJump = prevEnableJump;
        }

        // restaurar CinemachineBrain
        if (cinemachineBrainBehaviour != null)
            cinemachineBrainBehaviour.enabled = prevCinemachineBrainEnabled;

        if (playerAudio != null)
            playerAudio.enabled = prevAudioEnabled;

        if (miniGameUI != null) miniGameUI.SetActive(false);

        if (unlockCursorOnOpen)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void ForceClose() => Close();
}
