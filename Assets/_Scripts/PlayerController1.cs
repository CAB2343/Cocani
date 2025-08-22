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
    public float standHeight = 2f; // Altura do jogador em pé
    Vector3 standCenter = new Vector3(0f, 0.5f, 0f); // Centro do jogador em pé
    public float crouchHeight = 1f; // Altura do jogador agachado
    Vector3 crouchCenter = new Vector3(0f, 0.25f, 0f); // Centro do jogador agachado


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
        _MyCamera = Camera.main.transform;
        hand.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;


        
    }

    void Update()
    {
        if (_EnableMovement){
            Movement();
            Agachar();
        } 
        if (_EnableGravity) NormalGravity();
        if (_EnableJump) Jump();

        HandleGrounding();
        Animate();

    }
    #endregion

    #region Movement

    void Agachar()
    {
        if(Input.GetKeyDown(KeyCode.LeftControl))
        {
            _ChController.height = crouchHeight;
            _ChController.center = crouchCenter;
            _MoveSpeed = 2.5f;
        }
        if(Input.GetKeyUp(KeyCode.LeftControl))
        {
            _ChController.height = standHeight;
            _ChController.center = standCenter;
            _MoveSpeed = 5f;

        }
    }

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
            if(walkClip != null)
            {
                animancer.Play(walkClip, walkTransitionDuration);
            }
        }
        else
        {
            if(idleClip != null)
            {
                animancer.Play(idleClip, idleTransitionDuration);
            }
        }
    }


    #endregion


}
