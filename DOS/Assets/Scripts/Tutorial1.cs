using UnityEngine;
using TMPro;
using System.Diagnostics;
using System;

public class Tutorial1 : MonoBehaviour
{
    private string[] instructions =
    {
        "Shoot Blue Portal Using Left Click / Press 'C'\nYou can only Shoot Portals on the Brown Surfaces",
        "Shoot Orange Portal Using Right Click /Press 'V'\nYou can only Shoot Portals on the Brown Surfaces",
        "Jump Into The Portals\nYou can use both portals to enter/exit\nYou can CLEAR the portals by pressing R",
    };

    private string[] instructionsColor =
    {
        "0F108C",
        "FF6E00",
        "000000",
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
        if (instructionIndex == 0)
        {
            NextInstruction();
        }
    }

    private void HandleOrangePortalCreated()
    {
        if (instructionIndex == 1)
        {
            NextInstruction();
        }
    }

    private void HandlePlayerTeleport()
    {
        if (instructionIndex == 2)
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
