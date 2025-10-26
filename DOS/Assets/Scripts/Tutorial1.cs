using UnityEngine;
using TMPro;
using System.Diagnostics;
using System;

public class Tutorial1 : MonoBehaviour
{
    private string[] instructions =
    {
        "Press 'D/→' To Go Right",
        "Press 'A/←' To Go Left",
        "Press 'W/↑' To Jump",
        "Shoot Blue Portal Using Left Click",
        "Shoot Orange Portal Using Right Click",
        "Jump Into The Portals",
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
        // Done Going Right
        if (instructionIndex == 0 && (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)))
        {
            NextInstruction();
        }
        // Done Going Left
        else if (instructionIndex == 1 && (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)))
        {
            NextInstruction();
        }
        // Done Jumping
        else if (instructionIndex == 2 && (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)))
        {
            NextInstruction();
        }
    }

    void OnEnable()
    {
        PortalGun.OnBluePortalCreated += HandleBluePortalCreated;
        PortalGun.OnOrangePortalCreated += HandleOrangePortalCreated;
        Portal.OnPlayerTeleport += HandlePlayerTeleport;
    }

    void OnDisable()
    {
        PortalGun.OnBluePortalCreated -= HandleBluePortalCreated;
        PortalGun.OnOrangePortalCreated -= HandleOrangePortalCreated;
        Portal.OnPlayerTeleport -= HandlePlayerTeleport;
    }

    private void HandleBluePortalCreated()
    {
        if (instructionIndex == 3)
        {
            NextInstruction();
        }
    }

    private void HandleOrangePortalCreated()
    {
        if (instructionIndex == 4)
        {
            NextInstruction();
        }
    }

    private void HandlePlayerTeleport()
    {
        if (instructionIndex == 5)
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
