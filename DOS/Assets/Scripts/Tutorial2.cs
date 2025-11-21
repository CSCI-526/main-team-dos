using UnityEngine;
using TMPro;
using System.Diagnostics;
using System;

public class Tutorial2 : MonoBehaviour
{
    private string[] instructions =
    {
        "Move the Obstacle on to the Gate Trigger to Open the Gate\nYou Can Reset The Obstacle By Pressing 'O'",
        "You can Jump Over / Teleport the Enemy away to get to the Exit.",
    };

    private string[] instructionsColor =
    {
        "FF0000",
        "009D2B",
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
        instructionText.text = $"<color=#{instructionsColor[instructionIndex]}>{instructions[instructionIndex]}</color>";
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
            instructionText.text = $"<color=#{instructionsColor[instructionIndex]}>{instructions[instructionIndex]}</color>";
        }
        else
        {
            instructionText.text = $"<color=#009D2B>Now Reach the Exit Door</color>";
            exitDoor.SetActive(true);
        }
    }
}
