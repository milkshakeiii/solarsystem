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

    public float SecondsPerTick()
    {
        return SecondsPerTurn / StatesPerTurn;
    }
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
    public float Research;
    public float StoredResources;
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

    public float EnergyToRadiansTurningCoversion()
    {
        return Weight();
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
            result += (1 - facingDifferenceRatio) * engine.ThrustPerSecond() * (1 / Weight());
        }
        return result;
    }

    public float EnergyCostPerSecondInDirection(Position direction)
    {
        float directionFacing = Vector2.Angle(Vector2.up, direction.ToVector2()) * Mathf.Deg2Rad;
        float result = 0;
        foreach (Engine engine in Engines)
        {
            float facingDifference = directionFacing % (2 * (float)Math.PI) - facing % (2 * (float)Math.PI);
            if (facingDifference < (float)Math.PI / 2)
                result += engine.EnergyCostPerSecond();
        }
        return result;
    }

    public float BuildTime()
    {
        return Weight();
    }

    public float BuildCost()
    {
        return Weight();
    }
}

[Serializable]
public struct PixelPosition
{
    public int x;
    public int y;

    public Vector2 ToVector2()
    {
        return new Vector2(x, y);
    }
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
    public float ThrustPerSecond()
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
    public float CollectionRadius()
    {
        return Size/2;
    }

    public float CollectionEfficiency()
    {
        return Quality;
    }

    public float MaximumAsteroidSize()
    {
        return Size;
    }

    public float ResourcesPerSecondForAsteroid(Asteroid asteroid)
    {
        return asteroid.Size * Quality;
    }

    public float EnergyCostPerSecond()
    {
        return Size;
    }
}

public class Shipyard : Component
{
    public float SecondOfVesselBuilt;
    public bool BuildInProgress;

    public float EnergyCostPerSecond()
    {
        return Size;
    }

    public float BuildSpeed()
    {
        return Quality;
    }
}

public struct GameTick
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
    public Position TargetDisplacement;

    public List<bool> RunShipyards;
    public List<bool> BeginShipyardRuns;
    public List<bool> CancelShipyardRuns;
    public List<Vessel> VesselsToBuild;

    public List<bool> ActivateCollectors;
    public List<bool> ActivateLasers;
}

static class GameplayFunctions
{
    public static Gamestate NextGamestate(Game game, Gamestate sourceGamestate, GameTick doTurn)
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
        float possibleRotationAmount = energyUsed * vessel.EnergyToRadiansTurningCoversion();
        float actualRotationAmount = Math.Min(desiredRotationAmount, possibleRotationAmount);
        vessel.PowerCore.StoredEnergy -= energyUsed; // !
        if (rightRotation)
            vessel.facing += actualRotationAmount; // !
        else
            vessel.facing -= actualRotationAmount; // !

        //movement
        float unitsPerSecondInDesiredDirection = vessel.UnitsPerSecondInDirection(command.TargetDisplacement);
        float desiredMoveAmount = command.TargetDisplacement.ToVector2().magnitude;
        float possibleMoveAmount = game.SecondsPerTick() * unitsPerSecondInDesiredDirection;
        float energyCostPerSecondInDirection = vessel.EnergyCostPerSecondInDirection(command.TargetDisplacement);
        float secondsOfEnergyAvailable = vessel.PowerCore.StoredEnergy / energyCostPerSecondInDirection;
        float secondsOfEnergyAvailableThisTick = Math.Min(game.SecondsPerTick(), secondsOfEnergyAvailable);
        float moveAmountIfSufficientEnergy = Math.Min(desiredMoveAmount, possibleMoveAmount);
        float actualMoveAmount = moveAmountIfSufficientEnergy * (secondsOfEnergyAvailableThisTick / game.SecondsPerTick());
        Vector2 actualDisplacement = command.TargetDisplacement.ToVector2().normalized * actualMoveAmount;
        vessel.PowerCore.StoredEnergy -= (actualMoveAmount / desiredMoveAmount) * game.SecondsPerTick() * energyCostPerSecondInDirection; // !
        vessel.Position.x += actualDisplacement.x;
        vessel.Position.y += actualDisplacement.y;

        //shipyards
        for (int i = 0; i < command.RunShipyards.Count; i++)
        {
            Shipyard shipyard = vessel.Shipyards[i];
            if (command.RunShipyards[i])
            {
                Vessel vesselToProduce = command.VesselsToBuild[i];
                if (command.BeginShipyardRuns[i])
                {
                    bool resourcesAvailable = playerProgress.StoredResources >= vesselToProduce.BuildCost();
                    if (resourcesAvailable)
                    {
                        playerProgress.StoredResources -= vesselToProduce.BuildCost();
                        shipyard.BuildInProgress = true;
                    }
                }
                if (command.RunShipyards[i] && shipyard.BuildInProgress)
                {
                    float secondsOfEnergyAvailableForShipyard = vessel.PowerCore.StoredEnergy / shipyard.EnergyCostPerSecond();
                    float secondsOfEnergyAvailableForShipyardThisTick = Math.Min(game.SecondsPerTick(), secondsOfEnergyAvailableForShipyard);
                    float buildSecondsRemaining = (vessel.BuildTime() - shipyard.SecondOfVesselBuilt) / shipyard.BuildSpeed();
                    float secondsOfEnergySpent = Math.Min(secondsOfEnergyAvailableForShipyardThisTick, buildSecondsRemaining);
                    float energySpent = secondsOfEnergySpent * shipyard.EnergyCostPerSecond();
                    vessel.PowerCore.StoredEnergy -= energySpent; // !
                    shipyard.SecondOfVesselBuilt += secondsOfEnergySpent * shipyard.BuildSpeed(); // !
                    if (shipyard.SecondOfVesselBuilt >= vessel.BuildTime())
                    {
                        shipyard.SecondOfVesselBuilt = 0; // !
                        shipyard.BuildInProgress = false; // !
                        playerProgress.Vessels.Add(vesselToProduce); // !
                        vesselToProduce.Position = new Position
                        {
                            x = vessel.Position.x + shipyard.RootPixelPosition.x,
                            y = vessel.Position.y + shipyard.RootPixelPosition.y
                        }; // !
                    }
                }
                if (command.CancelShipyardRuns[i])
                {
                    shipyard.SecondOfVesselBuilt = 0; // !
                    shipyard.BuildInProgress = false; // !
                }
            }
        }

        //collectors
        for (int i = 0; i < command.ActivateCollectors.Count; i++)
        {
            Collector collector = vessel.Collectors[i];
            if (command.ActivateCollectors[i])
            {
                List<Asteroid> asteroidsInRange = new List<Asteroid>();
                Asteroid biggestAsteroid = gamestate.Asteroids[0];
                bool asteroidExistsInRange = false;
                foreach (Asteroid asteroid in gamestate.Asteroids)
                {
                    Vector2 collectorSpacePosition = vessel.Position.ToVector2() + collector.RootPixelPositions.ToVector2();
                    float squareDistanceToAsteroid = Vector2.SqrMagnitude(collectorSpacePosition - asteroid.Position.ToVector2());
                    bool inRange = squareDistanceToAsteroid < collector.CollectionRadius() * collector.CollectionRadius();
                    asteroidExistsInRange = asteroidExistsInRange || inRange;
                    if (inRange && asteroid.Size > biggestAsteroid.Size)
                    {
                        biggestAsteroid = asteroid;
                    }
                }
                if (asteroidExistsInRange)
                {
                    float resourcesGained = collector.ResourcesPerSecondForAsteroid(biggestAsteroid) * game.SecondsPerTick();
                    float energyCost = collector.EnergyCostPerSecond() * game.SecondsPerTick();
                    if (vessel.PowerCore.StoredEnergy > energyCost)
                    {
                        vessel.PowerCore.StoredEnergy -= energyCost; // !
                        playerProgress.StoredResources += resourcesGained; // !
                    }
                }
            }
        }

        //lasers
        for (int i = 0; i < command.ActivateLasers.Count; i++)
        {
            Laser laser = vessel.Lasers[i];
            if (command.ActivateLasers[i])
            {

            }
        }
    }
}