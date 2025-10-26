using UnityEngine;
using TMPro;
using System.Diagnostics;
using System;

public class Tutorial2 : MonoBehaviour
{
    private string[] instructions =
    {
        "Move the Obstacle on to the Red Gate Trigger to Open the Red Gate Using Portals\nYou Can Reset The Obstacle By Pressing 'O'",
        "Teleport the Enemy away to get to the Exit.\nRestart the Level by Pressing 'L'",
    };

    private int instructionIndex = 0;
    public GameObject exitDoor;
    GameObject instructionObject;
    TextMeshProUGUI instructionText;

    void Start()
    {
        instructionObject = GameObject.FindGameObjectWithTag("Instruction");
        instructionText = instructionObject.GetComponent<TextMeshProUGUI>();
        UnityEngine.Debug.Log($"Instruction: {instructions[instructionIndex]}");
        instructionText.text = instructions[instructionIndex];
    }

    void Update()
    {
        
    }

    void OnEnable()
    {
        GateTrigger.GateTriggered += HandleGateTriggered;
        Portal.OnEnemyTeleport += HandleEnemyTeleported;
    }

    void OnDisable()
    {
        GateTrigger.GateTriggered -= HandleGateTriggered;
        Portal.OnEnemyTeleport -= HandleEnemyTeleported;
    }

    private void HandleGateTriggered()
    {
        if (instructionIndex == 0)
        {
            NextInstruction();
        }
    }

    private void HandleEnemyTeleported()
    {
        if (instructionIndex == 1)
        {
            NextInstruction();
        }
    }
    
    void NextInstruction()
    {
        instructionIndex++;

        if (instructionIndex < instructions.Length)
        {
            instructionText.text = instructions[instructionIndex];
        }
        else
        {
            instructionText.text = "Now Reach the Exit Door";
            exitDoor.SetActive(true);
        }
    }
}
