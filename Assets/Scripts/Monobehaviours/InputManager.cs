using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;

public class InputManager : MonoBehaviour
{
    [NonSerialized] public float VerticalInput;
    [NonSerialized] public float HorizontalInput;
    [NonSerialized] public float CameraInputX;
    [NonSerialized] public float CameraInputY;
    [NonSerialized] public float MoveAmount;

    public Vector2 movementInput;
    public Vector2 cameraInput;

    private bool _isJoystickInput;
    
    private IMC_Default _playerControls;
    private AnimatorManager _animatorManager;
    private PlayerLocomotion _playerLocomotion;
    private CameraManager _cameraManager;

    // Toggle for agent control
    public bool isAgentControlled;

    private void Awake()
    {
        _animatorManager = GetComponent<AnimatorManager>();
        _playerLocomotion = GetComponent<PlayerLocomotion>();
        _cameraManager = FindObjectOfType<CameraManager>();
    }

    private void OnEnable()
    {
        if (!isAgentControlled)
        {
            if (_playerControls == null)
            {
                _playerControls = new IMC_Default();
                _playerControls.PlayerMovement.Movement.performed += i => movementInput = i.ReadValue<Vector2>();
                _playerControls.PlayerMovement.Camera.performed += InputOriginChecker;
            }
            _playerControls.Enable();
        }
    }

    private void OnDisable()
    {
        if (!isAgentControlled)
        {
            _playerControls.Disable();
        }
    }

    public void HandleAllInput()
    {
        HandleMovementInput();
    }
    
    private void InputOriginChecker(InputAction.CallbackContext context)
    {
        if (!isAgentControlled)
        {
            cameraInput = context.ReadValue<Vector2>();
            _isJoystickInput = context.control.device is UnityEngine.InputSystem.Gamepad;
        }
    }
    
    private void HandleMovementInput()
    {
        if (isAgentControlled)
        {
            // Use agent-controlled inputs, set by the ML-Agent
            VerticalInput = movementInput.y;
            HorizontalInput = movementInput.x;
            CameraInputY = cameraInput.y * 15f;
            CameraInputX = cameraInput.x * 15f;
        }
        else
        {
            // Player-driven input for manual control
            VerticalInput = movementInput.y;
            HorizontalInput = movementInput.x;
            CameraInputY = cameraInput.y;
            CameraInputX = cameraInput.x;
        }

        // Update animator and movement
        _cameraManager.isJoystickInput = _isJoystickInput;
        MoveAmount = Mathf.Clamp01(Mathf.Abs(HorizontalInput) + Mathf.Abs(VerticalInput));
        _animatorManager.UpdateAnimatorValues(HorizontalInput, VerticalInput, _playerLocomotion.isSprinting);
    }

    // Agent control method for movement and camera
    public void SetAgentInputs(float horizontal, float vertical, float cameraX, float cameraY)
    {
        movementInput = new Vector2(horizontal, vertical);
        cameraInput = new Vector2(cameraX, cameraY);
    }
}
