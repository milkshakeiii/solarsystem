using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamestateDisplayer : MonoBehaviour
{
    private static GamestateDisplayer instance;

    public static GamestateDisplayer GetInstance()
    {
        return instance;
    }

    public GameObject VesselPrefab;

    private List<GameObject> displayVessels = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    public void Display(Gamestate gamestate)
    {
        foreach (GameObject displayVessel in displayVessels)
        {
            Destroy(displayVessel);
        }
        foreach (Vessel vessel in gamestate.Vessels())
        {
            GameObject newVessel = Instantiate(VesselPrefab);
            newVessel.transform.parent = transform;
            newVessel.GetComponent<DisplayVessel>().Initialize(vessel);
            displayVessels.Add(newVessel);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
