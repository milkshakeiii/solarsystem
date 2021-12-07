using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateCommandLine : MonoBehaviour
{
    public GameObject TickButtonPrefab;

    private CommandPanel commandPanel;
    private int vesselNumber;

    private int clickedButton = -1;
    private float targetRotation = 0f;

    public void Initialize(CommandPanel newCommandPanel, int newVesselNumber)
    {
        commandPanel = newCommandPanel;
        vesselNumber = newVesselNumber;

        float y = GetComponent<RectTransform>().anchorMax.y;
        commandPanel.SpawnTurnButtons(TickButtonPrefab, y, y, ButtonClicked);
    }

    public void ButtonClicked(int number)
    {
        if (clickedButton != number)
        {
            clickedButton = number;
            targetRotation = 0f;
        }
        else
        {
            Debug.Log(targetRotation);
            commandPanel.WriteRotateCommand(clickedButton, targetRotation);
            clickedButton = -1;
        }
    }

    private void Update()
    {
        if (clickedButton != -1)
        {
            targetRotation += Input.GetAxis("Mouse ScrollWheel");
        }
    }


}
