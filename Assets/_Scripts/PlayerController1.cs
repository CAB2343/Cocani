using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;
using Animancer.FSM;
using System;

public class PlayerController : MonoBehaviour
{
    #region Variables
    [Header("Player Settings")]

    [Header("Camera Settings")]
    public bool _FPSCamera = true;
    [NonSerialized] public Transform _MyCamera;

    [Header("Gravity Settings")]
    public bool _EnableGravity = true;
    public float _gravity = -10f;
    private float _verticalVelocity = 0f;

    [Header("Movement Settings")]
    public bool _EnableMovement = true;
    public float _MoveSpeed = 5f;          // Velocidade base configurável
    public float _RotationSpeed = 200f;
    private CharacterController _ChController;

    [Header("Jump Settings")]
    public bool _EnableJump = true;
    public float _JumpForce = 5f;
    public int _MaxJumps = 1;
    private int _jumpsLeft = 0;
    private bool _IsGrounded = false;

    private float _groundedTimer = 0f;
    private float _groundedGraceTime = 0.2f;

    [Header("Weapon/Hands Settings")]
    public GameObject hand;
    public GameObject handwithItem1;

    [Header("Animation Movement Settings")]
    public AnimancerComponent animancer;
    public AnimationClip idleClip;
    public float idleTransitionDuration = 0.1f;
    public AnimationClip walkClip;
    public float walkTransitionDuration = 0.1f;

    // ------------------ Crouch Settings (Agachar) ------------------
    [Header("Crouch (Agachar) Settings")]
    public bool _EnableCrouch = true;
    public KeyCode _CrouchKey = KeyCode.LeftControl;

    [Tooltip("Altura em pé (se deixar 0, será a altura inicial do CharacterController).")]
    public float _StandingHeight = 0f;

    [Tooltip("Altura ao agachar.")]
    public float _CrouchHeight = 1.2f;

    [Tooltip("Velocidade da transição de altura/câmera.")]
    public float _CrouchTransitionSpeed = 10f;

    [Tooltip("Multiplicador da velocidade de movimento enquanto agachado.")]
    public float _CrouchSpeedMultiplier = 0.5f;

    [Tooltip("Offset vertical local negativo para a câmera quando agachado.")]
    public float _CrouchCameraYOffset = -0.6f;

    [Tooltip("Camadas que bloqueiam levantar.")]
    public LayerMask _CrouchObstructionMask = ~0;

    // Estados internos
    private bool _isCrouching = false;
    private float _currentTargetHeight;
    private float _originalControllerHeight;
    private Vector3 _originalControllerCenter;
    private Vector3 _originalCameraLocalPos;

    #endregion

    #region Unity Methods
    void Start()
    {
        _ChController = GetComponent<CharacterController>();
        _jumpsLeft = _MaxJumps;

        _MyCamera = Camera.main != null ? Camera.main.transform : null;

        hand.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Inicializa alturas
        _originalControllerHeight = _ChController.height;
        _originalControllerCenter = _ChController.center;

        if (_StandingHeight <= 0f)
            _StandingHeight = _originalControllerHeight; // Usa a altura atual como de pé

        _currentTargetHeight = _StandingHeight;

        if (_MyCamera != null)
            _originalCameraLocalPos = _MyCamera.localPosition;
    }

    void Update()
    {
        if (_EnableCrouch) Agachar();           // 1º: atualiza estado de agachar
        if (_EnableMovement) Movement();        // 2º: movimento usa estado atual
        if (_EnableGravity) NormalGravity();
        if (_EnableJump) Jump();

        HandleGrounding();
        Animate();
    }
    #endregion

    #region Movement
    void Movement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 inputDir = new Vector3(horizontal, 0f, vertical).normalized;

        if (inputDir.sqrMagnitude > 1f)
            inputDir.Normalize();

        // Converte para direção relativa à câmera
        if (_MyCamera != null)
        {
            inputDir = _MyCamera.TransformDirection(inputDir);
            inputDir.y = 0f;
        }

        // Velocidade efetiva (não alteramos a variável base _MoveSpeed)
        float effectiveSpeed = _MoveSpeed * (_isCrouching ? _CrouchSpeedMultiplier : 1f);

        _ChController.Move(inputDir * effectiveSpeed * Time.deltaTime);

        // Rotação somente se não for FPS
        if (inputDir != Vector3.zero && _FPSCamera == false)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(inputDir),
                _RotationSpeed * Time.deltaTime
            );
        }
    }
    #endregion

    #region Crouch
    void Agachar()
    {
        // Entrada (hold). Para toggle basta usar GetKeyDown e inverter _isCrouching se puder levantar.
        bool wantsCrouch = Input.GetKey(_CrouchKey);

        if (wantsCrouch)
        {
            _isCrouching = true;
        }
        else
        {
            // Só tenta levantar se há espaço
            if (CanStandUp())
                _isCrouching = false;
        }

        // Define altura alvo
        float targetHeight = _isCrouching ? _CrouchHeight : _StandingHeight;
        _currentTargetHeight = Mathf.Lerp(_ChController.height, targetHeight, Time.deltaTime * _CrouchTransitionSpeed);

        // Mantém pés no chão: center.y = halfHeight
        float previousBottom = GetControllerBottomY();
        _ChController.height = _currentTargetHeight;
        _ChController.center = new Vector3(_originalControllerCenter.x, _currentTargetHeight / 2f, _originalControllerCenter.z);
        AdjustTransformToKeepFeet(previousBottom);

        // Ajuste suave da câmera
        if (_MyCamera != null)
        {
            Vector3 targetCamPos = _originalCameraLocalPos;
            if (_isCrouching)
                targetCamPos += Vector3.up * _CrouchCameraYOffset;

            _MyCamera.localPosition = Vector3.Lerp(
                _MyCamera.localPosition,
                targetCamPos,
                Time.deltaTime * _CrouchTransitionSpeed
            );
        }
    }

    bool CanStandUp()
    {
        if (_ChController == null) return true;
        // Se já perto da altura em pé, ok
        if (Mathf.Abs(_ChController.height - _StandingHeight) < 0.05f)
            return true;

        float radius = _ChController.radius;
        float castDistance = _StandingHeight - _ChController.height;

        // Posição do topo atual
        Vector3 start = transform.position + Vector3.up * (_ChController.height - radius);
        // SphereCast para cima
        if (Physics.SphereCast(start, radius * 0.95f, Vector3.up, out RaycastHit hit, castDistance, _CrouchObstructionMask, QueryTriggerInteraction.Ignore))
        {
            // Bloqueado
            return false;
        }
        return true;
    }

    float GetControllerBottomY()
    {
        // Y do pé = transform.position.y + (center.y - height/2)
        return transform.position.y + (_ChController.center.y - _ChController.height / 2f);
    }

    void AdjustTransformToKeepFeet(float previousBottomY)
    {
        float newBottom = GetControllerBottomY();
        float delta = previousBottomY - newBottom;
        if (Mathf.Abs(delta) > 0.0001f)
        {
            // Move o transform para manter os pés estáveis
            transform.position += new Vector3(0f, delta, 0f);
        }
    }
    #endregion

    #region Gravity
    void NormalGravity()
    {
        _verticalVelocity += _gravity * Time.deltaTime;
        _ChController.Move(Vector3.up * _verticalVelocity * Time.deltaTime);
    }
    #endregion

    #region Jump
    void Jump()
    {
        if (Input.GetButtonDown("Jump") && _jumpsLeft > 0)
        {
            _verticalVelocity = _JumpForce;
            _jumpsLeft--;
        }
    }
    #endregion

    #region Ground Check
    void HandleGrounding()
    {
        if (_ChController.isGrounded && _verticalVelocity <= 0f)
        {
            _groundedTimer = _groundedGraceTime;
            _jumpsLeft = _MaxJumps;
            _verticalVelocity = -1f;
        }
        else
        {
            _groundedTimer -= Time.deltaTime;
        }

        _IsGrounded = _groundedTimer > 0f;
    }
    #endregion

    #region Animation
    void Animate()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        if (horizontal != 0 || vertical != 0)
        {
            if (walkClip != null)
                animancer.Play(walkClip, walkTransitionDuration);
        }
        else
        {
            if (idleClip != null)
                animancer.Play(idleClip, idleTransitionDuration);
        }
    }
    #endregion
}