using UnityEngine;
using UnityEngine.Serialization;

public class CameraManager : MonoBehaviour
{
    [Header("References")] 
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Transform lookAt; 
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform camera;
    [SerializeField] private LayerMask collisionLayers;

    [Header("Camera adjustments")] 
    [SerializeField] private float cameraFollowSpeed;
    [SerializeField] private float mouseSensitivityX;
    [SerializeField] private float mouseSensitivityY;
    [SerializeField] private float joystickSensitivityX;
    [SerializeField] private float joystickSensitivityY;
    [SerializeField] private float minimumPivotAngle;
    [SerializeField] private float maximumPivotAngle;
    
    [Header("Zoom Settings")]
    [SerializeField] private float zoomedInPosition; 
    [SerializeField] private float zoomSpeed;
    [SerializeField] private float zoomedInFOV;
    [SerializeField] private float defaultFOV;
    
    [Header("Camera collision parameters")] 
    [SerializeField] private float cameraCollisionRadius;
    [SerializeField] private float cameraCollisionOffset;
    [SerializeField] private float minimumCollisionOffset;

    public bool isJoystickInput;
    public bool isZoomedIn = false; 

    
    private float _defaultPosition;
    private float _lookAngle;
    private float _pivotAngle;
    private Transform _targetTransform;
    private Transform _cameraTransform;
    private Vector3 _cameraFollowVelocity = Vector3.zero;
    private Vector3 _cameraVectorPosition;

    private InputManager _inputManager;
    private PlayerManager _playerManager;
   // private Camera _cameraComponent;
    

    private void Awake()
    {
        if (camera != null) _cameraTransform = camera.transform;
        _defaultPosition = _cameraTransform.localPosition.z;
        _playerManager = FindObjectOfType<PlayerManager>();
        _targetTransform = _playerManager.transform;
        _inputManager = FindObjectOfType<InputManager>();
        //_cameraComponent = Camera.main; 
        //_cameraComponent.fieldOfView = defaultFOV;
        _defaultPosition = _cameraTransform.localPosition.z;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        HandleAllCameraMovement();
        HandleZoomFOV();
    }

    public void HandleAllCameraMovement()
    {
        FollowTarget();
        RotateCamera();
        HandleCameraCollision();
    }
    
    private void FollowTarget()
    {
        var targetPosition = Vector3.SmoothDamp(transform.position, _targetTransform.position, ref _cameraFollowVelocity, cameraFollowSpeed);
        transform.position = targetPosition;
    }

    private void RotateCamera()
    {
        Debug.Log(_inputManager.CameraInputX);
        
        var sensitivityX =  mouseSensitivityX;
        var sensitivityY =  mouseSensitivityY;

        if (_playerManager.isHeavyAttacking)
        {
            sensitivityX = sensitivityX * .1f;
            sensitivityY = sensitivityY * .1f;
        }
        
        _lookAngle += _inputManager.CameraInputX * sensitivityX;
        _pivotAngle -= _inputManager.CameraInputY * sensitivityY;
        _pivotAngle = Mathf.Clamp(_pivotAngle, minimumPivotAngle, maximumPivotAngle);

        Vector3 rotation = Vector3.zero;
        rotation.y = _lookAngle;
        var targetRotation = Quaternion.Euler(rotation);
        transform.rotation = targetRotation;

        rotation = Vector3.zero;
        rotation.x = _pivotAngle;
        targetRotation = Quaternion.Euler(rotation);
        cameraPivot.localRotation = targetRotation;
    }

    private void HandleCameraCollision()
    {
        float targetPosition = isZoomedIn ? zoomedInPosition : _defaultPosition;

        RaycastHit hit;
        Vector3 direction = _cameraTransform.position - cameraPivot.position;
        direction.Normalize();

        if (Physics.SphereCast(cameraPivot.position, cameraCollisionRadius, direction, out hit, Mathf.Abs(targetPosition), collisionLayers))
        {
            float distance = Vector3.Distance(cameraPivot.position, hit.point);
            targetPosition = -(distance - cameraCollisionOffset);
        }

        if (Mathf.Abs(targetPosition) < minimumCollisionOffset)
        {
            targetPosition = targetPosition - minimumCollisionOffset;
        }

        _cameraVectorPosition.z = Mathf.Lerp(_cameraTransform.localPosition.z, targetPosition, zoomSpeed * Time.deltaTime);
        _cameraTransform.localPosition = _cameraVectorPosition;
    }

    
    public void HandleZoomIn()
    {
        isZoomedIn = !isZoomedIn;
    
        float targetPosition = isZoomedIn ? zoomedInPosition : _defaultPosition;
    
        _cameraVectorPosition.z = Mathf.Lerp(_cameraTransform.localPosition.z, targetPosition, zoomSpeed * Time.deltaTime);
        _cameraTransform.localPosition = _cameraVectorPosition;
    }
    
    private void HandleZoomFOV()
    {
       // float targetFOV = isZoomedIn ? zoomedInFOV : defaultFOV;
       // _cameraComponent.fieldOfView = Mathf.Lerp(_cameraComponent.fieldOfView, targetFOV, zoomSpeed * Time.deltaTime);
    }

}
