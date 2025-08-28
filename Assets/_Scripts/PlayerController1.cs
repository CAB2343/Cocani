using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;
using Animancer.FSM;
using System;
using TMPro;

public class PlayerController : MonoBehaviour
{
    #region Variables

    [Header("Camera Settings")]
    public bool _FPSCamera = true;
    [NonSerialized] public Transform _MyCamera;

    [Header("Gravity Settings")]
    public bool _EnableGravity = true;
    public float _gravity = -10f;
    private float _verticalVelocity = 0f;

    [Header("Movement Settings")]
    public bool _EnableMovement = true;
    public float _MoveSpeed = 5f;          
    public float _RotationSpeed = 200f;
    private CharacterController _ChController;

    [Header("Crouch Settings")]
    public float crouchHeight = 1f;
    public float crouchCameraOffset = -0.5f;
    public float crouchTransitionSpeed = 8f;

    private float originalHeight;
    private Vector3 originalCenter;
    private Vector3 originalCameraLocalPos;
    private bool isCrouching = false;

    private float targetHeight;
    private Vector3 targetCenter;
    private Vector3 targetCameraPos;

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

    #endregion

    #region Unity Methods
    void Start()
    {
        _ChController = GetComponent<CharacterController>();
        _jumpsLeft = _MaxJumps;
        _MyCamera = Camera.main != null ? Camera.main.transform : null;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        originalHeight = _ChController.height;
        originalCenter = _ChController.center;
        if (_MyCamera != null)  originalCameraLocalPos = _MyCamera.localPosition;

        targetHeight = originalHeight;
        targetCenter = originalCenter;
        targetCameraPos = originalCameraLocalPos;
    }

    void Update()
    {
        if (_EnableMovement) Movement();        
        if (_EnableGravity) NormalGravity();
        if (_EnableJump) Jump();

        HandleGrounding();
        HandleCrouch();
        Animate();
    }
    #endregion

    #region Movement
    void Movement()
    {
        float HorizontalInput = Input.GetAxisRaw("Horizontal");
        float VerticalInput = Input.GetAxisRaw("Vertical");
        Vector3 moveDirection = new Vector3(HorizontalInput, 0, VerticalInput).normalized;
        moveDirection = _MyCamera.TransformDirection(moveDirection);
        moveDirection.y = 0;
        _ChController.Move(moveDirection * _MoveSpeed * Time.deltaTime);

        if (moveDirection != Vector3.zero && _FPSCamera == false)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(moveDirection),
                _RotationSpeed * Time.deltaTime
            );
        }
    }

    void HandleCrouch()
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (!isCrouching)
            {
                targetHeight = crouchHeight;
                targetCenter = new Vector3(originalCenter.x, crouchHeight / 2f, originalCenter.z);
                targetCameraPos = originalCameraLocalPos + new Vector3(0, crouchCameraOffset, 0);
                isCrouching = true;
                _MoveSpeed /= 2f; 
            }
        }
        else
        {
            if (isCrouching)
            {
                targetHeight = originalHeight;
                targetCenter = originalCenter;
                targetCameraPos = originalCameraLocalPos;
                isCrouching = false;
                _MoveSpeed *= 2f; // Restore original speed when standing up
            }
        }

        _ChController.height = Mathf.Lerp(_ChController.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);
        _ChController.center = Vector3.Lerp(_ChController.center, targetCenter, Time.deltaTime * crouchTransitionSpeed);

        if (_MyCamera != null)
            _MyCamera.localPosition = Vector3.Lerp(_MyCamera.localPosition, targetCameraPos, Time.deltaTime * crouchTransitionSpeed);
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