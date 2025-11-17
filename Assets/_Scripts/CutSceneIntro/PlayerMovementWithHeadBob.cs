using UnityEngine;

public class PlayerMovementWithHeadBob : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float mouseSensitivity = 120f;

    [Header("Head Bob")]
    public float bobFrequency = 8f;       // frequência
    public float bobAmplitude = 0.05f;    // força do sobe/desce
    public float bobSmooth = 10f;         // suavidade

    private Transform cam;
    private Vector3 camStartLocalPos;
    private float xRotation;

    void Start()
    {
        cam = Camera.main.transform;
        camStartLocalPos = cam.localPosition;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
        HandleHeadBob();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -85f, 85f);

        cam.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 dir = (transform.forward * v + transform.right * h).normalized;

        transform.Translate(dir * moveSpeed * Time.deltaTime, Space.World);
    }

    void HandleHeadBob()
    {
        float speed = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).magnitude;

        if (speed > 0.1f) // andando
        {
            float bob = Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
            Vector3 targetPos = camStartLocalPos + new Vector3(0, bob, 0);
            cam.localPosition = Vector3.Lerp(cam.localPosition, targetPos, Time.deltaTime * bobSmooth);
        }
        else // parado → volta suave
        {
            cam.localPosition = Vector3.Lerp(cam.localPosition, camStartLocalPos, Time.deltaTime * bobSmooth);
        }
    }
}
