using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;
using Animancer.FSM;
using System;

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

    [Header("Combat Settings")]
    public bool _EnableCombat = true;
    private bool _isArmed = false;

    [Header("Jump Settings")]
    public bool _EnableJump = true;
    public float _JumpForce = 5f;
    public int _MaxJumps = 1;
    private int _jumpsLeft = 0;
    private bool _IsGrounded = false;
    private CharacterController _ChController;

    private float _groundedTimer = 0f;
    private float _groundedGraceTime = 0.2f;

    [Header("Weapon/Hands Settings")]
    public GameObject hand;         
    public GameObject handwithItem1; 



    [Header("Animation Movement Settings disarmed ")]
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
        _MyCamera = Camera.main.transform;
        hand.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        
    }

    void Update()
    {
        if (_EnableMovement) Movement();
        if (_EnableGravity) NormalGravity();
        if (_EnableJump) Jump();

        HandleGrounding();
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
        float HorizontalInput = Input.GetAxisRaw("Horizontal");
        float VerticalInput = Input.GetAxisRaw("Vertical");

        if(HorizontalInput != 0 || VerticalInput != 0)
        {
            animancer.Play(walkClip, walkTransitionDuration);
        }
        else
        {
            animancer.Play(idleClip, idleTransitionDuration);
        }
    }


    #endregion


}
