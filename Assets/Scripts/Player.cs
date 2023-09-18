using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float slowWalkMultiplier = 0.5f;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float gravity = 9.81f;

    private Camera playerCamera;
    private Vector3 moveDirection = Vector3.zero;
    private CharacterController characterController;
    private float verticalLookRotation = 0f;

    private NetworkVariable<Vector3> networkedPosition = new NetworkVariable<Vector3>();
    private NetworkVariable<Quaternion> networkedRotation = new NetworkVariable<Quaternion>();

    private void Start() 
    {
        characterController = GetComponent<CharacterController>();
        playerCamera = transform.Find("Camera").GetComponent<Camera>();
        
        if (!IsOwner)
        {
            playerCamera.enabled = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        networkedPosition.OnValueChanged += OnPositionChanged;
        networkedRotation.OnValueChanged += OnRotationChanged;
    }

    private void OnPositionChanged(Vector3 oldValue, Vector3 newValue)
    {
        transform.position = newValue;
    }

    private void OnRotationChanged(Quaternion oldValue, Quaternion newValue)
    {
        transform.rotation = newValue;
    }

    private void Update()
    {
        if (IsOwner) 
        {
            OwnerHandleInput();
            HandleMouseLook();
        }
    }

    private bool IsPlayerGrounded()
    {
        float extraHeightText = 0.1f;
        bool grounded = Physics.Raycast(characterController.bounds.center,
        Vector3.down, characterController.bounds.extents.y + extraHeightText);
        return grounded;
    }

    private void OwnerHandleInput()
    {
        float xMove = Input.GetAxis("Horizontal");
        float zMove = Input.GetAxis("Vertical");

        bool isShiftKeyDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        
        Vector3 move = transform.right * xMove + transform.forward * zMove;
        
        float currentSpeed = isShiftKeyDown ? movementSpeed * slowWalkMultiplier : movementSpeed;

        moveDirection.x = move.x * currentSpeed;
        moveDirection.z = move.z * currentSpeed;

        if (IsPlayerGrounded() && Input.GetButtonDown("Jump"))
        {
            moveDirection.y = jumpForce;
        }
        moveDirection.y -= gravity * Time.deltaTime;

        MoveServerRpc(moveDirection * Time.deltaTime, Vector3.zero); 
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        verticalLookRotation -= mouseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);

        playerCamera.transform.localRotation = Quaternion.Euler(verticalLookRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
        MoveServerRpc(Vector3.zero, Vector3.up * mouseX);
    }

    [ServerRpc]
    private void MoveServerRpc(Vector3 movement, Vector3 rotation)
    {
        if (movement == Vector3.zero && rotation == Vector3.zero)
        {
            return;
        }
        characterController.Move(movement);
        
        if(rotation != Vector3.zero)
        {
            transform.Rotate(rotation);
            networkedRotation.Value = transform.rotation;
        }
        networkedPosition.Value = transform.position;
    }
}
