using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Animancer;
//using Animancer.FSM;
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
    public GameObject handWithGun; 
    private bool _isShooting = false;
    private float _shootTimer = 0f;


    [Header("Animation Movement Settings disarmed ")]
    // public AnimancerComponent animancer;
    public AnimationClip idleClip;
    public float idleTransitionDuration = 0.1f;
    public AnimationClip walkClip;
    public float walkTransitionDuration = 0.1f;
    public AnimationClip punchClip;
    public float punchTransitionDuration = 0.25f;

    [Header("Animation Movement Settings handgun")]
    // public AnimancerComponent animancerGun;
    public AnimationClip idleGunClip;
    public float idleGunTransitionDuration = 0.1f;
    public AnimationClip walkGunClip;
    public float walkGunTransitionDuration = 0.1f;
    public AnimationClip GunShootClip;
    public float shootTransitionDuration = 0.25f;
    public AnimationClip GunAimingClip;
    public float GunAimingTransitionDuration = 0.1f;
    public AnimationClip GunAimingShootClip;
    public float GunAimingShootTransitionDuration = 0.1f;

    [Header("Check is")]
    [NonSerialized] public bool _CheckAiming = false;
    [NonSerialized] public bool _CheckShooting = false;
    [NonSerialized] public bool _CheckPunching = false;




    #endregion

    #region Unity Methods
    void Start()
    {
        
        _ChController = GetComponent<CharacterController>();
        _jumpsLeft = _MaxJumps;
        _MyCamera = Camera.main.transform;
        hand.SetActive(true);
        handWithGun.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        
    }

    void Update()
    {
        if (_EnableMovement) Movement();
        if (_EnableGravity) Gravity();
        if (_EnableJump) Jump();
        if (_EnableCombat) Punch();
        if (_EnableCombat) weaponSwitch();
        HandleShootingTimer();


        HandleGrounding();
        Animate();


        // if(_CheckShooting == true) Debug.Log("Shooting is " + _CheckShooting);
        // if(_CheckPunching == true) Debug.Log("Punch is " + _CheckPunching);
        // if(_CheckAiming == true) Debug.Log("Aiming is " + _CheckAiming);
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

    #region Combat

    void weaponSwitch()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            hand.SetActive(true);
            handWithGun.SetActive(false);
            _isArmed = false;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            hand.SetActive(false);
            handWithGun.SetActive(true);
            _isArmed = true;
        }
    }

    void HandleShootingTimer()
    {
        if (_isShooting)
        {
            _shootTimer -= Time.deltaTime;
            if (_shootTimer <= 0f)
            {
                _isShooting = false;
            }
        }
    }



    void Punch()
    {

    }
    void Shoot()
    {
        
    }

    #endregion

    #region Gravity
    void Gravity()
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

    }


    #endregion


}
