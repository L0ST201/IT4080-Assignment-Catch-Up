using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    void Start()
    {
        if(Application.isEditor) 
        {
            // Placeholder for furue logic
        }
        else
        {
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
        }
    }
}