using FishNet.Object;
using UnityEngine;

//This is made by Bobsi Unity - Youtube
public class PlayerController : NetworkBehaviour
{
    [Header("Base setup")]

    public float _walkingSpeed = 7.5f;

    public float _runningSpeed = 11.5f;

    public float _jumpSpeed = 8.0f;

    public float _gravity = 20.0f;

    public float _lookSpeed = 2.0f;

    public float _lookXLimit = 45.0f;

    [HideInInspector]
    public bool _canMove = true;

    [SerializeField]
    private float _cameraYOffset = 0.4f;

    private CharacterController _characterController;
    private Vector3 _moveDirection = Vector3.zero;

    private Camera _playerCamera;
    private float _rotationX;

    private void Start()
    {
        _characterController = GetComponent<CharacterController>();

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        var isRunning = false;

        // Press Left Shift to run
        isRunning = Input.GetKey(KeyCode.LeftShift);

        // We are grounded, so recalculate move direction based on axis
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        float curSpeedX = _canMove ? (isRunning ? _runningSpeed : _walkingSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = _canMove ? (isRunning ? _runningSpeed : _walkingSpeed) * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = _moveDirection.y;
        _moveDirection = forward * curSpeedX + right * curSpeedY;

        if (Input.GetButton("Jump") && _canMove && _characterController.isGrounded)
        {
            _moveDirection.y = _jumpSpeed;
        }
        else
        {
            _moveDirection.y = movementDirectionY;
        }

        if (!_characterController.isGrounded)
        {
            _moveDirection.y -= _gravity * Time.deltaTime;
        }

        // Move the controller
        _characterController.Move(_moveDirection * Time.deltaTime);

        // Player and Camera rotation
        if (_canMove && _playerCamera != null)
        {
            _rotationX += -Input.GetAxis("Mouse Y") * _lookSpeed;
            _rotationX = Mathf.Clamp(_rotationX, -_lookXLimit, _lookXLimit);
            _playerCamera.transform.localRotation = Quaternion.Euler(_rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * _lookSpeed, 0);
        }
    }

    private void OnDestroy()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (IsOwner)
        {
            _playerCamera = Camera.main;
            _playerCamera.transform.position = new Vector3(transform.position.x, transform.position.y + _cameraYOffset,
                transform.position.z);
            _playerCamera.transform.SetParent(transform);
            gameObject.name = "Player Character (Local)";
        }
        else
        {
            gameObject.GetComponent<PlayerController>().enabled = false;
            gameObject.name = "Player Character (Remote)";
        }
    }
}