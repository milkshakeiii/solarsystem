using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateCommandLine : MonoBehaviour
{
    public GameObject TickButtonPrefab;

    private CommandPanel commandPanel;
    private int vesselNumber;

    private int clickedButton = -1;

    public void Initialize(CommandPanel newCommandPanel, int newVesselNumber)
    {
        commandPanel = newCommandPanel;
        vesselNumber = newVesselNumber;

        float y = GetComponent<RectTransform>().anchorMax.y;
        commandPanel.SpawnTurnButtons(TickButtonPrefab, y, y, SetClickedButton);
    }

    public void SetClickedButton(int number)
    {
        clickedButton = number;
        Debug.Log(clickedButton);
    }


}
