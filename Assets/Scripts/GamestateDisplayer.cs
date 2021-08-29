using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamestateDisplayer : MonoBehaviour
{
    public GameObject VesselPrefab;

    // Start is called before the first frame update
    void Start()
    {
        Vessel testMothership = ShipReader.ReadShip("C:\\Users\\milks\\Documents\\testship.png");
        List<Vessel> testDeck = new List<Vessel>() { testMothership };
        Player testPlayer = new Player("test", 1000f, testDeck, new List<float>() { 0 }, 0);
        Game testGame = new Game(20, 10, new List<Gamestate>(), 100f, 100f, new List<Player>() { testPlayer }, 1000000, 15, 10);
        Gamestate testGamestate = testGame.BuildFirstGamestate();
        testGame.Gamestates.Add(testGamestate);
        Display(testGamestate);
        
        Command testCommand = new Command(2f,
                                          new Position(1, 1),
                                          new List<bool>(),
                                          new List<bool>(),
                                          new List<bool>(),
                                          new List<Vessel>(),
                                          new List<bool>(),                                          
                                          new List<bool>());
        PlayerAction testAction = new PlayerAction(new List<Command>() { testCommand });
        GameTick testTurn = new GameTick(new List<PlayerAction>() { testAction });
        Gamestate secondGamestate = GameplayFunctions.NextGamestate(testGame, testGamestate, testTurn);
        testGame.Gamestates.Add(secondGamestate);
        Display(secondGamestate);
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
