using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamestateDisplayer : MonoBehaviour
{
    public GameObject VesselPrefab;

    // Start is called before the first frame update
    void Start()
    {
        Game testGame = new Game();
        Player testPlayer = new Player();
        testGame.Players = new List<Player>() { testPlayer };
        Gamestate testGamestate = testGame.BuildFirstGamestate();
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
