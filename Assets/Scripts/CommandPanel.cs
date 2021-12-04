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
    private int currentSelectedOffset = -1;
    private List<PlayerAction> workingActions;
    private Dictionary<int, List<PlayerAction>> preparedCommands = new Dictionary<int, List<PlayerAction>>();
    private Game game;
    private int selectedVessel;

    public GameObject TickButtonPrefab;
    public GameObject Timeline;
    public GameObject RotateCommandPrefab;

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

    public void SelectWorkingState(int ticksPastStart)
    {
        currentSelectedOffset = ticksPastStart;
        Gamestate simulatedGamestate = game.MostAdvancedGamestate();
        for (int i = 0; i < currentSelectedOffset; i++)
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
            simulatedGamestate = GameplayFunctions.NextGamestate(game, simulatedGamestate, tick);
        }

        GamestateDisplayer.GetInstance().Display(simulatedGamestate);
    }

    public void LockInCommands()
    {
        preparedCommands[currentCommander] = workingActions;
    }

    private void UISetup()
    {
        Debug.Log(game);
        preparedCommands = new Dictionary<int, List<PlayerAction>>();
        SpawnTurnButtons(TickButtonPrefab,
                         Timeline.GetComponent<RectTransform>().anchorMax.y,
                         0.95f,
                         SelectWorkingState);
    }

    public int ButtonCount()
    {
        return game.StatesPerTurn;
    }

    public void WriteRotateCommand(int startTurn, int endTurn, float targetRotation)
    {
        Vessel selectedShip = game.MostAdvancedGamestate().PlayerProgresses[currentCommander].Vessels[selectedVessel];
        for (int i = startTurn; i <= endTurn; i++)
        {
            Command modifyMe;
            if (workingActions[i].VesselCommands.ContainsKey(selectedShip.UUID))
            {
                modifyMe = workingActions[i].VesselCommands[selectedShip.UUID];
            }
            else
            {
                modifyMe = new Command();
            }    
            modifyMe.TargetRotation = targetRotation;
            workingActions[i].VesselCommands[selectedShip.UUID] = modifyMe;
        }

    }

    public void SpawnTurnButtons(GameObject buttonPrefab, float minY, float maxY, Action<int> callback)
    {
        for (int i = 0; i < ButtonCount(); i++)
        {
            GameObject button = Instantiate(buttonPrefab, Timeline.transform.parent);
            button.GetComponent<RectTransform>().anchorMin = new Vector2(ButtonXForTurn(i), minY);
            button.GetComponent<RectTransform>().anchorMax = new Vector2(ButtonXForTurn(i), maxY);
            int selectNumber = i;
            button.GetComponentInChildren<UnityEngine.UI.Button>().onClick.AddListener(() => callback(selectNumber));
        }
    }

    private float ButtonXForTurn(int turnNumber)
    {
        float minX = Timeline.GetComponent<RectTransform>().anchorMin.x;
        float maxX = Timeline.GetComponent<RectTransform>().anchorMax.x;
        int buttonCount = game.StatesPerTurn;
        return minX + turnNumber * ((maxX - minX) / buttonCount);
    }

    private void BeginMakingCommands()
    {
        currentCommander = waitingCommandRequests.Dequeue();

        startingGamestateIndex = game.Gamestates.Count - 1;

        workingActions = new List<PlayerAction>();
        for (int i = 0; i < game.StatesPerTurn; i++)
        {
            Dictionary<Guid, Command> emptyCommands = new Dictionary<Guid, Command>();
            foreach (Vessel vessel in game.MostAdvancedGamestate().PlayerProgresses[currentCommander].Vessels)
            {
                Command emptyCommand = Command.EmptyCommandForVessel(vessel);
                emptyCommands[vessel.UUID] = emptyCommand;
            }
            PlayerAction emptyAction = new PlayerAction(emptyCommands);
            workingActions.Add(emptyAction);
        }

        UISetup();

        SelectWorkingState(0);
        SelectShip(0);
    }

    public void SelectShip(int shipIndex)
    {
        selectedVessel = shipIndex;
        Vessel selectedShip = game.MostAdvancedGamestate().PlayerProgresses[currentCommander].Vessels[shipIndex];
        if (selectedShip.PowerCore != null)
        {
            GameObject rotateCommand = Instantiate(RotateCommandPrefab, this.transform);
            rotateCommand.GetComponent<RotateCommandLine>().Initialize(this, shipIndex);
        }
    }

    private void SetUpForGame(Game forGame)
    {
        game = forGame;
    }

    public void EnqueueCommandRequest(Game forGame, int playerIndex)
    {
        if (game.Uninitialized())
        {
            SetUpForGame(forGame);
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
