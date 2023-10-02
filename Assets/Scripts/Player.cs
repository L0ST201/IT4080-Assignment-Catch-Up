using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float slowWalkMultiplier = 0.5f;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float gravity = 9.81f;
    [SerializeField] private Vector3 minBoundary = new Vector3(-2, 0, -2);
    [SerializeField] private Vector3 maxBoundary = new Vector3(2, 0, 2);

    private Camera playerCamera;
    private Vector3 moveDirection = Vector3.zero;
    private CharacterController characterController;
    private Animator animator; 
    private float verticalLookRotation = 0f;

    private NetworkVariable<Vector3> networkedPosition = new NetworkVariable<Vector3>();
    private NetworkVariable<Quaternion> networkedRotation = new NetworkVariable<Quaternion>();

    private bool isReloading = false;

    private void Awake()
    {
        playerCamera = transform.Find("Camera").GetComponent<Camera>();
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>(); 

        if (!playerCamera) Debug.LogError("Player camera not found.");
        if (!characterController) Debug.LogError("CharacterController not found on player.");
        if (!animator) Debug.LogError("Animator not found on player."); 
    }

    private void Start()
    {
        playerCamera.enabled = IsOwner;
        if (playerCamera.TryGetComponent(out AudioListener audioListener))
        {
            audioListener.enabled = IsOwner;
        }

        networkedPosition.OnValueChanged += OnPositionChanged;
        networkedRotation.OnValueChanged += OnRotationChanged;
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
        float extraHeightTest = 0.1f;
        return Physics.SphereCast(characterController.bounds.center, characterController.radius, Vector3.down, out RaycastHit hit, characterController.bounds.extents.y + extraHeightTest);
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
        moveDirection.y -= gravity * Time.deltaTime;

    // Animation logic
        animator.SetFloat("X", xMove);
        animator.SetFloat("Y", zMove);
        animator.SetFloat("Speed", Mathf.Sqrt(xMove * xMove + zMove * zMove));

        if (Input.GetMouseButton(1))
        {
            animator.SetBool("Aiming", true);
        }
        else
        {
            animator.SetBool("Aiming", false);
        }

        if (Input.GetMouseButtonDown(0) && !isReloading)
        {
            animator.SetTrigger("Shoot");
        }

        if (Input.GetKeyDown(KeyCode.R) && !isReloading)
        {
            isReloading = true;
            animator.SetTrigger("Reloading");
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            animator.SetTrigger("Roll");
        }

        if (IsPlayerGrounded() && Input.GetButtonDown("Jump"))
        {
            moveDirection.y = jumpForce;
            animator.SetFloat("Jump", 1.0f);
        }
        else
        {
            animator.SetFloat("Jump", 0.0f);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            animator.SetTrigger("Pickup");
        }

        MoveServerRpc(moveDirection * Time.deltaTime);
    }

    public void RollSound()
    {
        // Roll sound logic goes here
    }

    public void CantRotate()
    {
        // logic for cantRotate on roll
    }

    public void EndRoll()
    {
        // logic for end roll.
    }

    public void FootStep()
    {
        // Footstep sound logic goes here
    }

      public void EndPickup()
    {
        // Handle the end of the pickup animation. 
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        verticalLookRotation -= mouseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);

        playerCamera.transform.localRotation = Quaternion.Euler(verticalLookRotation, 0f, 0f);

        Vector3 rotation = Vector3.up * mouseX;
        transform.Rotate(rotation);
        RotateServerRpc(rotation);
    }

    private void OnPositionChanged(Vector3 oldValue, Vector3 newValue)
    {
        transform.position = newValue;
    }

    private void OnRotationChanged(Quaternion oldValue, Quaternion newValue)
    {
        transform.rotation = newValue;
    }

    [ServerRpc]
    private void MoveServerRpc(Vector3 movement)
    {
        if (!IsHostPlayer())
        {
            Vector3 intendedPosition = transform.position + movement;
            intendedPosition = Vector3.Max(minBoundary, intendedPosition);
            intendedPosition = Vector3.Min(maxBoundary, intendedPosition);
            movement = intendedPosition - transform.position;
        }
        
        characterController.Move(movement);

        if (transform.position != networkedPosition.Value)
        {
            networkedPosition.Value = transform.position;
        }
    }

    [ServerRpc]
    private void RotateServerRpc(Vector3 rotation)
    {
        if (rotation != Vector3.zero && transform.rotation != Quaternion.Euler(rotation))
        {
            transform.Rotate(rotation);
            networkedRotation.Value = transform.rotation;
        }
    }

    private bool IsHostPlayer()
    {
        return NetworkManager.Singleton != null && NetworkManager.Singleton.LocalClientId == NetworkManager.ServerClientId;
    }
}
