using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogistics : MonoBehaviour
{
    private List<TurnSource> turnSources;
    private Game game;

    // Start is called before the first frame update
    void Start()
    {
        Vessel testMothership = ShipReader.ReadShip("C:\\Users\\milks\\Documents\\testship.png");
        List<Vessel> testDeck = new List<Vessel>() { testMothership };
        Player testPlayer = new Player("test", 1000f, testDeck, new List<float>() { 0 }, 0);
        Game testGame = new Game(20, 10, new List<Gamestate>(), 100f, 100f, new List<Player>() { testPlayer }, 1000000, 15, 10);
        Gamestate testGamestate = testGame.BuildFirstGamestate();
        testGame.Gamestates.Add(testGamestate);
        game = testGame;
        turnSources = new List<TurnSource>() { new LocalTurnSource() };
        turnSources[0].StartTurnPlanning(game, 0);
    }

    private bool AllTurnsReady()
    {
        for (int i = 0; i < turnSources.Count; i++)
        {
            if (!turnSources[i].TurnReady(game, i))
            {
                return false;
            }
        }
        return true;
    }

    // Update is called once per frame
    void Update()
    {
        if (AllTurnsReady())
        {
            Debug.Log("All turns ready");
            for (int i = 0; i < game.StatesPerTurn; i++)
            {
                List<PlayerAction> actionsThisTick = new List<PlayerAction>();
                for (int j = 0; j < turnSources.Count; j++)
                {
                    actionsThisTick.Add(turnSources[j].GetTurn(game, j)[i]);
                }
                GameTick nextTick = new GameTick(actionsThisTick);
                Gamestate nextGamestate = GameplayFunctions.NextGamestate(game, game.MostAdvancedGamestate(), nextTick);
                game.Gamestates.Add(nextGamestate);
            }
            for (int i = 0; i < turnSources.Count; i++)
            {
                turnSources[i].StartTurnPlanning(game, i);
            }
        }
    }
}

public abstract class TurnSource
{
    public abstract void StartTurnPlanning(Game game, int playerIndex);

    public abstract bool TurnReady(Game game, int playerIndex);

    public abstract List<PlayerAction> GetTurn(Game game, int playerIndex);
}

public class LocalTurnSource : TurnSource
{
    public override void StartTurnPlanning(Game game, int playerIndex)
    {
        CommandPanel.GetInstance().EnqueueCommandRequest(game, playerIndex);
    }

    public override bool TurnReady(Game game, int playerIndex)
    {
        return CommandPanel.GetInstance().CommandsReady(game, playerIndex);
    }

    public override List<PlayerAction> GetTurn(Game game, int playerIndex)
    {
        return CommandPanel.GetInstance().GetCommands(game, playerIndex);
    }
}