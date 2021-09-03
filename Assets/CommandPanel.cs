using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandPanel : MonoBehaviour
{
    private static CommandPanel instance;

    public static CommandPanel GetInstance()
    {
        return instance;
    }

    private Queue<int> waitingCommandRequests = new Queue<int>();
    private int currentCommander = -1;
    private int startingGamestateIndex;
    private List<PlayerAction> workingActions;
    private Dictionary<int, List<PlayerAction>> preparedCommands = new Dictionary<int, List<PlayerAction>>();
    private Game game;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (waitingCommandRequests.Count > 0 && (currentCommander == -1 || preparedCommands.ContainsKey(currentCommander)))
        {
            BeginMakingCommands();
        }
    }

    private void SelectWorkingState(int ticksPastStart)
    {
        for (int i = 0; i < ticksPastStart; i++)
        {
            List<PlayerAction> playerActions = new List<PlayerAction>();
            for (int j = 0; j < game.Players.Count; j++)
            {
                if (j == currentCommander)
                {
                    playerActions.Add(workingActions[i]);
                }
                else
                {
                    playerActions.Add(new PlayerAction(new Dictionary<Guid, Command>()));
                }
            }
            GameTick tick = new GameTick(playerActions);
            game.Gamestates.Add(GameplayFunctions.NextGamestate(game, game.MostAdvancedGamestate(), tick));
        }

        GamestateDisplayer.GetInstance().Display(game.Gamestates[startingGamestateIndex+ticksPastStart]);
    }

    private void BeginMakingCommands()
    {
        currentCommander = waitingCommandRequests.Dequeue();

        startingGamestateIndex = game.Gamestates.Count - 1;
        workingActions = new List<PlayerAction>();
        for (int i = 0; i < game.StatesPerTurn; i++)
        {
            workingActions.Add(new PlayerAction(new Dictionary<Guid, Command>()));
        }

        SelectWorkingState(0);
    }

    public void EnqueueCommandRequest(Game forGame, int playerIndex)
    {
        if (game.Uninitialized())
        {
            game = forGame;
        }
        else if (forGame.UUID != game.UUID)
        {
            throw new UnityException("I'm already doing commands for a different game");
        }
        waitingCommandRequests.Enqueue(playerIndex);
    }

    public bool CommandsReady(Game forGame, int playerIndex)
    {
        if (forGame.UUID != game.UUID)
        {
            throw new UnityException("I'm doing commands for a different game");
        }
        return preparedCommands.ContainsKey(playerIndex);
    }

    public List<PlayerAction> GetCommands(Game forGame, int playerIndex)
    {
        if (forGame.UUID != game.UUID)
        {
            throw new UnityException("I'm doing commands for a different game");
        }
        return preparedCommands[playerIndex];
    }
}
