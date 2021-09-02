using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamestateDisplayer : MonoBehaviour
{
    public GameObject VesselPrefab;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Display(Gamestate gamestate)
    {
        foreach (Vessel vessel in gamestate.Vessels())
        {
            GameObject newVessel = Instantiate(VesselPrefab);
            newVessel.transform.parent = transform;
            newVessel.GetComponent<DisplayVessel>().Initialize(vessel);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
