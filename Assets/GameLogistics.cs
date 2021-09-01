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
        
    }

    private bool AllTurnsReady()
    {
        foreach (TurnSource commandSource in turnSources)
        {
            if (!commandSource.TurnReady())
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
            List<GameTick> gameTicks = new List<GameTick>();
            for (int i = 0; i < game.StatesPerTurn; i++)
            {
                List<PlayerAction> actionsThisTick = new List<PlayerAction>();
                foreach (TurnSource turnSource in turnSources)
                {
                    actionsThisTick.Add(turnSource.GetTurn()[i]);
                }
                GameTick nextTick = new GameTick(actionsThisTick);
                Gamestate nextGamestate = GameplayFunctions.NextGamestate(game, game.MostAdvancedGamestate(), nextTick);
                game.Gamestates.Add(nextGamestate);
            }
            
        }
    }
}

public abstract class TurnSource
{
    public abstract bool TurnReady();

    public abstract List<PlayerAction> GetTurn();
}