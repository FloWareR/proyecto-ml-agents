using System;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private PlayerLocomotion _playerLocomotion;
    private InputManager _inputManager;
    private CameraManager _cameraManager;
    private Animator _animator;

    public bool isInteracting;
    public bool isHeavyAttacking;
    
    private static readonly int IsInteracting = Animator.StringToHash("isInteracting");
    private static readonly int IsJumping = Animator.StringToHash("isJumping");
    private static readonly int IsGrounded = Animator.StringToHash("isGrounded");
    private static readonly int IsHeavyAttacking = Animator.StringToHash("isHeavyAttacking");
    private static readonly int IsIdle = Animator.StringToHash("isIdle");
    private static readonly int CameraDelta = Animator.StringToHash("CameraDelta");

    private void Awake()
    {
        _inputManager = GetComponent<InputManager>();
        _playerLocomotion = GetComponent<PlayerLocomotion>();
        _cameraManager = FindObjectOfType<CameraManager>();
        _animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        _inputManager.HandleAllInput();
    }

    private void FixedUpdate()
    {
        _playerLocomotion.HandleAllMovement();
    }

    private void LateUpdate()
    {
        _cameraManager.HandleAllCameraMovement();
        isInteracting = _animator.GetBool(IsInteracting);
        _playerLocomotion.isJumping = _animator.GetBool(IsJumping);
        isHeavyAttacking = _animator.GetBool(IsHeavyAttacking);

        _animator.SetBool(IsGrounded, _playerLocomotion.isGrounded);
    }
}
