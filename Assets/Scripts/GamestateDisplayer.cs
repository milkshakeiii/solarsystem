using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamestateDisplayer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Gamestate testGamestate = new Gamestate();
        Display(testGamestate);
    }

    private void Display(Gamestate gamestate)
    {
        foreach (Vessel vessel in gamestate.Vessels())
        {

        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
