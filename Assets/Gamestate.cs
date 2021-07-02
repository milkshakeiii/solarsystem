using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Game
{
    public int StatesPerTurn;
    public int SecondsPerTurn;
    public List<Gamestate> Gamestates;
    public readonly float Height;
    public readonly float Width;

    public List<Player> Players;
    public float StartTime; //seconds since epoch
    public float TimeControl; //seconds
    public float TimeIncrement; //seconds
}

[Serializable]
public struct Player
{
    public string Name;
    public float ELO;
    public List<Vessel> Deck;
    public List<float> ResearchThresholds;
    public int TeamNumber;
}

[Serializable]
public struct Gamestate
{
    public List<PlayerProgress> PlayerProgresses;
    public List<Asteroid> Asteroids;
}

[Serializable]
public struct PlayerProgress
{
    public List<Vessel> Vessels;
    public float Research; //[0, 1]
}

[Serializable]
public struct Position
{
    public float x;
    public float y;

    public static Position Up()
    {
        return new Position
        {
            x = 0,
            y = 1
        };
    }

    public Vector2 ToVector2()
    {
        return new Vector2(x, y);
    }
}

[Serializable]
public struct Vessel
{
    public Position Position;
    public float facing;

    public List<Engine> Engines;
    public PowerCore PowerCore;
    public List<Laser> Lasers;
    public List<Collector> Collectors;
    public List<Shipyard> Shipyards;
    public List<PixelPosition> LightHullPositions;
    public List<float> LightHullSecondsOfDamage;
    public List<PixelPosition> DarkHullPositions;
    public List<float> DarkHullSecondsOfDamage;


    public static float MaxPortionMaxEnergySpentTurningPerSecond()
    {
        return 1f / 6f;
    }

    public static float EnergyToRadiansTurningCoversion()
    {
        return 1f;
    }

    public List<Component> Components()
    {
        List<Component> components = new List<Component>();
        components.AddRange(Engines);
        components.AddRange(Lasers);
        components.AddRange(Collectors);
        components.AddRange(Shipyards);
        components.Add(PowerCore);
        return components;
    }

    public float Weight()
    {
        float componentsWeight = 0;
        foreach (Component component in Components())
        {
            componentsWeight += component.Size * component.Size;
        }
        return LightHullPositions.Count + DarkHullPositions.Count * 2 + componentsWeight;
    }

    public float UnitsPerSecondInDirection(Position direction)
    {
        float directionFacing = Vector2.Angle(Vector2.up, direction.ToVector2()) * Mathf.Deg2Rad;
        float result = 0;
        foreach (Engine engine in Engines)
        {
            float facingDifference = directionFacing % (2 * (float)Math.PI) - facing % (2 * (float)Math.PI);
            float facingDifferenceClamped = Mathf.Clamp(facingDifference, -(float)Math.PI, (float)Math.PI);
            float facingDifferenceRatio = Math.Abs(facingDifferenceClamped / (float)Math.PI);
            result += (1 - facingDifferenceRatio) * engine.UnitsPerSecondStraight();
        }
        return result;
    }
}

[Serializable]
public struct PixelPosition
{
    public int x;
    public int y;
}

[Serializable]
public struct Asteroid
{
    public Position Position;
    public float Size;
}

[Serializable]
public class Component
{
    public PixelPosition RootPixelPosition; //relative to parent vessel
    public List<PixelPosition> PixelPositions; //relative to root pixel position
    public List<float> SecondsOfDamage;
    public float facing; //up is 0 or 2pi
    public float Size;
    public float Quality;
}

public class PowerCore : Component
{
    public float StoredEnergy;

    public float MaxEnergy()
    {
        return Size;
    }

    public float EnergyPerSecond()
    {
        return Quality;
    }
}

public class Engine : Component
{
    public float UnitsPerSecondStraight()
    {
        return Size * 5;
    }

    public float EnergyCostPerSecond()
    {
        return Size * (1 / (1 + Quality));
    }
}

public class Laser : Component
{

}

public class Collector : Component
{

}

public class Shipyard : Component
{

}

public struct GameTurn
{
    public List<PlayerAction> PlayerActions;
}

[Serializable]
public struct PlayerAction
{
    public List<Command> VesselCommands;
}

[Serializable]
public struct Command
{
    public float TargetRotation;
    public float TargetDisplacement;
    public List<bool> ActivateComponents;
}

static class GameplayFunctions
{
    public static Gamestate NextGamestate(Game game, Gamestate sourceGamestate, GameTurn doTurn)
    {
        string sourceJson = JsonUtility.ToJson(sourceGamestate); //for purpose of making a deep copy
        Gamestate nextGamestate = JsonUtility.FromJson<Gamestate>(sourceJson);

        if (doTurn.PlayerActions.Count != game.Players.Count)
            throw new UnityException("Players in gameturn did not match players in game");

        for (int i = 0; i < doTurn.PlayerActions.Count; i++)
        {
            int playerIndex = i;
            if (game.Gamestates.Count%2 == 1)
            {
                playerIndex = doTurn.PlayerActions.Count - i - 1; //invert player order every other turn
            }
            Player player = game.Players[playerIndex];
            PlayerAction playerAction = doTurn.PlayerActions[playerIndex];
            PlayerProgress playerProgress = nextGamestate.PlayerProgresses[playerIndex];

            if (!(playerProgress.Vessels.Count == playerAction.VesselCommands.Count))
                throw new UnityException("Player commands did not match player vessels");

            for (int j = 0; j < playerAction.VesselCommands.Count; j++)
            {
                Command command = playerAction.VesselCommands[j];
                Vessel vessel = playerProgress.Vessels[j];
                GameplayFunctions.DoCommand(command, ref vessel, game, ref nextGamestate, player, ref playerProgress);
            }
        }

        return nextGamestate;
    }

    public static void DoCommand(Command command,
                                 ref Vessel vessel,
                                 Game game,
                                 ref Gamestate gamestate,
                                 Player commandingPlayer,
                                 ref PlayerProgress playerProgress)
    {
        //rotation
        bool rightRotation = command.TargetRotation >= 0;
        float desiredRotationAmount = Math.Abs(command.TargetRotation);
        float energyUsed = Math.Min(vessel.PowerCore.StoredEnergy, Vessel.MaxPortionMaxEnergySpentTurningPerSecond());
        float possibleRotationAmount = energyUsed * Vessel.EnergyToRadiansTurningCoversion();
        float actualRotationAmount = Math.Min(desiredRotationAmount, possibleRotationAmount);
        vessel.PowerCore.StoredEnergy -= energyUsed;
        if (rightRotation)
            vessel.facing += actualRotationAmount;
        else
            vessel.facing -= actualRotationAmount;

        //movement

    }
}