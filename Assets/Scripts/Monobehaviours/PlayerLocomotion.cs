using UnityEngine;

public class PlayerLocomotion : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraObject;
    [SerializeField] private Transform lookAtTarget;  // New LookAt target reference

    [Header("Movement Speed")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float airControlMultiplier;
    [SerializeField] private float groundDrag;

    [Header("Movement Flags")]
    public bool isSprinting;
    public bool isGrounded;
    public bool isJumping;
    public bool _isRotatingToLookAt; 

    [SerializeField] private LayerMask groundLayer;

    [Header("Falling")]
    [SerializeField] private float rayCastHeightOffset;
    [SerializeField] private float customGravityMultiplier;

    [Header("Jumping")]
    [SerializeField] private float jumpForce;

    private Vector3 _moveDirection;
    private InputManager _inputManager;
    private Rigidbody _rigidbody;
    private PlayerManager _playerManager;
    private AnimatorManager _animatorManager;
    private ParticleManager _particleManager;
    private static readonly int IsJumping = Animator.StringToHash("isJumping");


    private void Awake()
    {
        _inputManager = GetComponent<InputManager>();
        _rigidbody = GetComponent<Rigidbody>();
        _playerManager = GetComponent<PlayerManager>();
        _animatorManager = GetComponent<AnimatorManager>();
        _particleManager = GetComponent<ParticleManager>();

        if (Camera.main != null) cameraObject = Camera.main.transform;

        _rigidbody.drag = groundDrag; 
    }

    public void HandleAllMovement()
    {
        HandleFallingAndLanding();
        HandleTrail();
        
        if (_playerManager.isInteracting || _isRotatingToLookAt) return;
        HandleMovement();
        HandleRotation();

    }

    private void HandleMovement()
    {
        _moveDirection = (cameraObject.forward * _inputManager.VerticalInput) + (cameraObject.right * _inputManager.HorizontalInput);
        _moveDirection.Normalize();
        _moveDirection.y = 0f;

        var targetSpeed = isSprinting && isGrounded ? sprintSpeed :
                          _inputManager.MoveAmount >= 0.5f ? runSpeed : walkSpeed;

        var moveForce = _moveDirection * (isGrounded ? targetSpeed : targetSpeed * airControlMultiplier);
        _rigidbody.AddForce(new Vector3(moveForce.x, 0, moveForce.z), ForceMode.Acceleration);

        CapSpeed(targetSpeed);
    }

    private void CapSpeed(float maxSpeed)
    {
        var horizontalVelocity = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z);
        if (horizontalVelocity.magnitude > maxSpeed)
        {
            var clampedVelocity = horizontalVelocity.normalized * maxSpeed;
            _rigidbody.velocity = new Vector3(clampedVelocity.x, _rigidbody.velocity.y, clampedVelocity.z);
        }
    }

    private void HandleRotation()
    {
        if (_isRotatingToLookAt) return;

        Vector3 targetDirection = cameraObject.forward;
        targetDirection.y = 0f;
        targetDirection.Normalize();

        if (targetDirection == Vector3.zero) 
            targetDirection = transform.forward;

        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

        float trueRotation = isGrounded ? rotationSpeed : rotationSpeed * airControlMultiplier;

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, trueRotation * Time.deltaTime);
    }

    private void HandleTrail()
    {
        if (isSprinting && isGrounded)
        {
            _particleManager.ToggleParticleSystem("runTrail", true);  
        }
        else
        {
            _particleManager.ToggleParticleSystem("runTrail", false);  
        }    
    }
    
    private void HandleFallingAndLanding()
    {
        RaycastHit hit;
        var rayCastOrigin = transform.position + Vector3.up * rayCastHeightOffset;

        if (Physics.SphereCast(rayCastOrigin, 0.2f, -Vector3.up, out hit, 0.5f, groundLayer))
        {
            if (!isGrounded && !_playerManager.isInteracting)
            {
                _animatorManager.PlayTargetAnimation("Landing", true);
                var particlePosition = transform.position;
                _particleManager.SpawnTemporaryParticle("landTrail", transform.position, Quaternion.identity);
            }

            
            isGrounded = true;
            isJumping = false;
            _rigidbody.drag = groundDrag; 
        }
        else if (isGrounded)
        {
            isGrounded = false;
            _animatorManager.PlayTargetAnimation("Falling", false);
            _rigidbody.drag = 0f; 
            
        } 
        if (!isGrounded)
        {
            _rigidbody.AddForce(Physics.gravity * (customGravityMultiplier - 1), ForceMode.Acceleration);
        }
    }

    public void HandleJumping()
    {
        if (isGrounded)
        {
            _animatorManager.AnimatorComponent.SetBool(IsJumping, true);
            _animatorManager.PlayTargetAnimation("Jumping", false);

            _rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            var particlePosition = transform.position;
            _particleManager.SpawnTemporaryParticle("jumpTrail", transform.position, Quaternion.identity);

            isGrounded = false;
            isJumping = true;
        }
    }

    public void RotateAtLookAt()
    {
        if (lookAtTarget == null) return;
        _isRotatingToLookAt = true;
        StartCoroutine(SmoothLookAt());
    }

    private System.Collections.IEnumerator SmoothLookAt()
    {
        do
        {
            var directionToTarget = (lookAtTarget.position - transform.position).normalized;
            directionToTarget.y = 0f; // Keep only horizontal rotation

            var targetRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            yield return null;
            
        } while (_playerManager.isInteracting);

        _isRotatingToLookAt = false;
    }

    public void StopAllMovement()
    {
        _rigidbody.useGravity = false;
        _rigidbody.velocity = Vector3.zero;
    
        _rigidbody.constraints = RigidbodyConstraints.FreezePosition;
    }
    
    public void ReinstateMovement()
    {
        _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        _rigidbody.useGravity = true;

        if (!isGrounded)
        {
            _animatorManager.PlayTargetAnimation("Falling", false);
        }
    }
}
