using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;

public class Player : NetworkBehaviour
{
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float slowWalkMultiplier = 0.5f;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float gravity = 9.81f;
    [SerializeField] private Vector2 spawnAreaSize = new Vector2(5, 5);

    private Camera playerCamera;
    private Vector3 moveDirection = Vector3.zero;
    private CharacterController characterController;
    private float verticalLookRotation = 0f;

    private NetworkVariable<Color> networkedColor = new NetworkVariable<Color>(default, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server);

    public NetworkVariable<Color> NetworkedColor
    {
        get { return networkedColor; }
        set { networkedColor = value; }
    }

    private NetworkVariable<Vector3> networkedPosition = new NetworkVariable<Vector3>();
    private NetworkVariable<Quaternion> networkedRotation = new NetworkVariable<Quaternion>();

    private Renderer headRenderer;
    private Renderer legsRenderer;
    private Renderer torsoRenderer;
    private NetworkObject networkObject;

    private static List<Color> availableColors = new List<Color>
    {
        Color.black,
        Color.blue,
        Color.green,
        Color.yellow,
        Color.magenta
    };

    private Vector3 boundaryCenter = new Vector3(0, 2.5f, 0);

    private static NetworkVariable<int> positionIndex = new NetworkVariable<int>(0);
    private Vector3[] startPositions = new Vector3[]
    {
        new Vector3(4, 2, 0),
        new Vector3(-4, 2, 0),
        new Vector3(0, 2, 4),
        new Vector3(0, 2, -4)
    };

    private Vector3 spawnPosition;
    public ParticleSystem hostParticleEffectPrefab;

    private void Awake()
    {
        networkObject = GetComponent<NetworkObject>();
    }

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        if (!characterController)
        {
            Debug.LogError("CharacterController not found on player.");
            return;
        }

        playerCamera = transform.Find("Camera").GetComponent<Camera>();
        if (!playerCamera)
        {
            Debug.LogError("Player camera not found.");
            return;
        }

        playerCamera.enabled = networkObject.IsOwner;

        headRenderer = transform.Find("SK_Soldier_Head").GetComponent<Renderer>();
        legsRenderer = transform.Find("SK_Soldier_Legs").GetComponent<Renderer>();
        torsoRenderer = transform.Find("SK_Soldier_Torso").GetComponent<Renderer>();

        if (networkObject.IsOwner)
        {
            RequestColorServerRpc();
            if (NetworkManager.Singleton && hostParticleEffectPrefab) 
            {
                var effectInstance = NetworkObject.Instantiate(hostParticleEffectPrefab);
                effectInstance.transform.SetParent(transform);
                effectInstance.transform.position = transform.position;
                var particleSystem = effectInstance.GetComponent<ParticleSystem>();
                if (particleSystem)
                {
                    particleSystem.Play();
                }
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        networkedColor.OnValueChanged += OnColorChanged;
        networkedPosition.OnValueChanged += OnPositionChanged;
        networkedRotation.OnValueChanged += OnRotationChanged;

        AssignSpawnPoint();
    }

    private void AssignSpawnPoint()
    {
        if (IsServer)
        {
            spawnPosition = GenerateSpawnPosition();
            transform.position = spawnPosition;
        }
    }

    private Vector3 GenerateSpawnPosition()
    {
        if (positionIndex.Value >= startPositions.Length)
        {
            positionIndex.Value = 0;
        }
        
        return startPositions[positionIndex.Value++];
    }

    private void OnPositionChanged(Vector3 oldValue, Vector3 newValue)
    {
        transform.position = newValue;
    }

    private void OnRotationChanged(Quaternion oldValue, Quaternion newValue)
    {
        transform.rotation = newValue;
    }

    private void SetPlayerColor(Color color)
    {
        if (headRenderer != null) headRenderer.material.color = color;
        if (legsRenderer != null) legsRenderer.material.color = color;
        if (torsoRenderer != null) torsoRenderer.material.color = color;
    }

    private void OnColorChanged(Color oldValue, Color newValue)
    {
        Debug.Log($"Color changed from {oldValue} to {newValue}");
        SetPlayerColor(newValue);
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
        bool grounded = Physics.Raycast(characterController.bounds.center, Vector3.down, characterController.bounds.extents.y + extraHeightText);
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

        MoveServerRpc(moveDirection * Time.deltaTime);
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        verticalLookRotation -= mouseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);

        playerCamera.transform.localRotation = Quaternion.Euler(verticalLookRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
        RotateServerRpc(Vector3.up * mouseX);
    }

    [ServerRpc]
    private void MoveServerRpc(Vector3 movement)
    {
    
        characterController.Move(movement);

        if (!networkObject.IsOwner) 
        {
            float halfPlaneSize = 2.5f; 
            Vector3 newPosition = transform.position;

            newPosition.x = Mathf.Clamp(newPosition.x, -halfPlaneSize, halfPlaneSize);
            newPosition.z = Mathf.Clamp(newPosition.z, -halfPlaneSize, halfPlaneSize);

            transform.position = newPosition;
        }

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

    [ServerRpc]
    private void RequestColorServerRpc()
    {
        if (availableColors.Count > 0)
        {
            Color assignedColor = availableColors[0];
            availableColors.RemoveAt(0);
            NetworkedColor.Value = assignedColor;
        }
        else
        {
            NetworkedColor.Value = Color.gray;
        }
    }

    public override void OnNetworkDespawn()
    {
        try 
        {
            if (IsServer)
            {
                if (networkObject == null)
                {
                    Debug.LogError("OnNetworkDespawn: networkObject is null");
                    return;
                }

                if (!networkObject.IsSpawned)
                {
                    Debug.LogError("OnNetworkDespawn: networkObject is not spawned");
                    return;
                }

                if (NetworkedColor == null)
                {
                    Debug.LogError("OnNetworkDespawn: NetworkedColor is null");
                    return;
                }

                if (availableColors == null)
                {
                    Debug.LogError("OnNetworkDespawn: availableColors is null");
                    return;
                }

                if (availableColors.Count <= 5)  
                {
                    availableColors.Add(NetworkedColor.Value);
                }

                NetworkedColor.Value = Color.gray; 
            }
        } 
        catch (System.Exception e) 
        {
            Debug.LogError("Error in OnNetworkDespawn: " + e.Message);
        }
    }
}
