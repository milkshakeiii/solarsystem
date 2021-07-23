using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamestateDisplayer : MonoBehaviour
{
    public GameObject VesselPrefab;

    // Start is called before the first frame update
    void Start()
    {
        Gamestate testGamestate = new Gamestate();
        testGamestate.PlayerProgresses = new List<PlayerProgress>();
        PlayerProgress testPlayerProgress = new PlayerProgress();
        testPlayerProgress.Vessels = new List<Vessel>();
        Vessel newVessel = new Vessel();
        testPlayerProgress.Vessels.Add(newVessel);
        testGamestate.PlayerProgresses.Add(testPlayerProgress);
        Display(testGamestate);
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
