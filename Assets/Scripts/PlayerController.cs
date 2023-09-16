using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : NetworkBehaviour
{
    public float movementSpeed = 50f;
    public float rotationSpeed = 130f;
    public NetworkVariable<Color> playerColor = new NetworkVariable<Color>();

    void Update()
    {
        if (IsLocalPlayer)
        {
            transform.Translate(CalcMovement());
            transform.Rotate(CalcRotation());

            if (!IsHost)
            {
                Vector3 pos = transform.position;
                if (Mathf.Abs(pos.x) > 2.5f || Mathf.Abs(pos.z) > 2.5f)
                {
                    transform.position = new Vector3(Mathf.Clamp(pos.x, -2.5f, 2.5f), pos.y, Mathf.Clamp(pos.z, -2.5f, 2.5f));
                }
            }
        }
    }

    private Vector3 CalcRotation()
    {
        bool isShiftKeyDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        Vector3 rotVect = Vector3.zero;
        if (!isShiftKeyDown)
        {
            rotVect = new Vector3(0, Input.GetAxis("Horizontal"), 0);
            rotVect *= rotationSpeed * Time.deltaTime;
        }
        return rotVect;
    }

    private Vector3 CalcMovement()
    {
        bool isShiftKeyDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float x_move = 0.0f;
        float z_move = Input.GetAxis("Vertical");

        if (isShiftKeyDown)
        {
            x_move = Input.GetAxis("Horizontal");
        }

        Vector3 moveVect = new Vector3(x_move, 0, z_move);
        moveVect *= movementSpeed * Time.deltaTime;

        return moveVect;
    }

    [ServerRpc]
    public void UpdatePositionServerRpc(Vector3 newPosition)
    {
        transform.position = newPosition;
    }

    public void OnStartButtonClicked()
    {
        SceneManager.LoadScene("NewSceneName");
    }
}
