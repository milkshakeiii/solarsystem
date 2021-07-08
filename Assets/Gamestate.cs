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

    public List<Vessel> Vessels()
    {
        List<Vessel> vessels = new List<Vessel>();
        foreach (PlayerProgress playerProgress in PlayerProgresses)
        {
            vessels.AddRange(playerProgress.Vessels);
        }
        return vessels;
    }
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

    public Position Rotate(float radians)
    {
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);
        return new Position {
            x = cos * x - sin * y,
            y = sin * x + cos * y
        };
    }
}

[Serializable]
public struct Vessel
{
    public Position Position;
    public float facing;

    public LightHull LightHull;
    public DarkHull DarkHull;
    public PowerCore PowerCore;
    public List<Engine> Engines;
    public List<Laser> Lasers;
    public List<Collector> Collectors;
    public List<Shipyard> Shipyards;


    public static float MaxPortionMaxEnergySpentTurningPerSecond()
    {
        return 1f / 6f;
    }

    public float EnergyToRadiansTurningCoversion()
    {
        return Weight();
    }

    public List<FunctionalComponent> FunctionalComponents()
    {
        List<FunctionalComponent> components = new List<FunctionalComponent>();
        components.AddRange(Engines);
        components.AddRange(Lasers);
        components.AddRange(Collectors);
        components.AddRange(Shipyards);
        components.Add(PowerCore);
        return components;
    }

    public List<PixelComponent> PixelComponents()
    {
        List<PixelComponent> components = new List<PixelComponent>();
        components.AddRange(FunctionalComponents());
        components.Add(LightHull);
        components.Add(DarkHull);
        return components;
    }
    
    public float Weight()
    {
        float componentsWeight = 0;
        foreach (FunctionalComponent component in FunctionalComponents())
        {
            componentsWeight += component.Size * component.Size;
        }
        return LightHull.PixelPositions.Count + DarkHull.PixelPositions.Count * 2 + componentsWeight;
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

    public Position PixelPositionToWorldPosition(PixelPosition pixelPosition)
    {
        Position floatPosition = new Position { x = pixelPosition.x, y = pixelPosition.y };
        Position rotatedFloatPosition = floatPosition.Rotate(facing);
        return new Position { x = Position.x + rotatedFloatPosition.x, y = Position.y + rotatedFloatPosition.y };
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
    public static PixelPosition NotAPosition()
    {
        return new PixelPosition { x = float.NaN, y = float.NaN };
    }
}

[Serializable]
public struct Asteroid
{
    public Position Position;
    public float Size;
}

[Serializable]
public class PixelComponent
{
    public PixelPosition RootPixelPosition; //relative to parent vessel
    public List<PixelPosition> PixelPositions; //relative to root pixel position
    public List<float> SecondsOfDamage;

    public virtual float SecondsToDestroy()
    {
        return 1f;
    }
}

[Serializable]
public class LightHull : PixelComponent
{
    
}

[Serializable]
public class DarkHull : PixelComponent
{
    public override float SecondsToDestroy()
    {
        return 3f;
    }
}

[Serializable]
public class FunctionalComponent : PixelComponent
{
    public float facing; //up is 0 or 2pi
    public float Size;
    public float Quality;
}

public class PowerCore : FunctionalComponent
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

public class Engine : FunctionalComponent
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

public class Laser : FunctionalComponent
{
    public float EnergyCostPerSecond()
    {
        return Size;
    }

    public float Width()
    {
        return Size;
    }

    public float Length()
    {
        return Quality * 5;
    }
}

public class Collector : FunctionalComponent
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

public class Shipyard : FunctionalComponent
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

struct BeamHit
{
    public int playerIndex;
    public int vesselIndex;
    public int pixelComponentIndex;
    public int pixelIndex;
    public int beamIndex;
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
            if (game.Gamestates.Count % 2 == 1)
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

        //lasers
        for (int i = 0; i < command.ActivateLasers.Count; i++)
        {
            Laser laser = vessel.Lasers[i];
            float energyCost = laser.EnergyCostPerSecond() * game.SecondsPerTick();
            if (command.ActivateLasers[i] && vessel.PowerCore.StoredEnergy > energyCost)
            {
                //calculate beam boxes
                Position widthVector = new Position { x = 0, y = laser.Width() };
                Position laserWorldPosition = vessel.PixelPositionToWorldPosition(laser.RootPixelPosition);

                Position laserBeamCornersStraight = widthVector.Rotate(vessel.facing);
                laserBeamCornersStraight = laserBeamCornersStraight.Rotate(laser.facing);
                Position laserBeamCornersLeft = laserBeamCornersStraight.Rotate((1 / 2) * (float)Math.PI);
                Position laserBeamCornersRight = laserBeamCornersStraight.Rotate(-(1 / 2) * (float)Math.PI);
                Position leftCornerWorldPosition = new Position {
                    x = laserWorldPosition.x + laserBeamCornersLeft.x,
                    y = laserWorldPosition.y + laserBeamCornersLeft.y
                };
                Position rightCornerWorldPosition = new Position {
                    x = laserWorldPosition.x + laserBeamCornersRight.x,
                    y = laserWorldPosition.y + laserBeamCornersRight.y
                };

                int numberOfSteps = Mathf.RoundToInt(laser.Width());
                float stepSize = Vector2.Distance(laserBeamCornersLeft.ToVector2(), laserBeamCornersRight.ToVector2()) / numberOfSteps;
                Vector2 stepVector = (laserBeamCornersRight.ToVector2() - laserBeamCornersLeft.ToVector2()).normalized * stepSize;
                Vector2 orthoganalVector = new Position { x = stepVector.x, y = stepVector.y }.Rotate((float)Math.PI / 2).ToVector2();

                List<Vector2> beamCornerPoints = new List<Vector2> { leftCornerWorldPosition.ToVector2() };
                Vector2 lastBeamCornerPoint = beamCornerPoints[0];
                for (int j = 0; j < numberOfSteps; j++) {
                    lastBeamCornerPoint += stepVector;
                    beamCornerPoints.Add(lastBeamCornerPoint);
                }

                //boxcast into enemy pixels
                List<BeamHit> withinBeamBox = new List<BeamHit>();
                for (int j = 0; j < game.Players.Count; j++)
                {
                    Player otherPlayer = game.Players[j];
                    PlayerProgress otherPlayerProgress = gamestate.PlayerProgresses[j];
                    if (otherPlayer.TeamNumber != commandingPlayer.TeamNumber)
                    {
                        for (int k = 0; k < otherPlayerProgress.Vessels.Count; k++)
                        {
                            Vessel otherVessel = otherPlayerProgress.Vessels[k];
                            List<PixelComponent> otherVesselPixelComponents = otherVessel.PixelComponents();
                            for (int m = 0; m < otherVesselPixelComponents.Count; m++)
                            {
                                PixelComponent pixelComponent = otherVesselPixelComponents[m];
                                for (int n = 0; n < pixelComponent.PixelPositions.Count; n++)
                                {
                                    Position enemyPixelPosition = otherVessel.PixelPositionToWorldPosition(pixelComponent.PixelPositions[n]);
                                    Vector2 enemyPixelVector = enemyPixelPosition.ToVector2();
                                    for (int a = 0; a < beamCornerPoints.Count - 1; a++)
                                    {
                                        bool rightHemiplane = Vector2.Dot(stepVector, enemyPixelVector - beamCornerPoints[a]) > 0;
                                        bool nextRightHemiplane = Vector2.Dot(stepVector, enemyPixelVector - beamCornerPoints[a+1]) > 0;
                                        bool frontHemiplane = Vector2.Dot(orthoganalVector, enemyPixelVector - beamCornerPoints[a]) > 0;
                                        bool hit = rightHemiplane && (!nextRightHemiplane) && frontHemiplane;
                                        if (hit)
                                        {
                                            withinBeamBox.Add(new BeamHit
                                            {
                                                playerIndex = j,
                                                vesselIndex = k,
                                                pixelComponentIndex = m,
                                                pixelIndex = n,
                                                beamIndex = a
                                            });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                Dictionary<int, List<BeamHit>> beamIndexToBeamHit = new Dictionary<int, List<BeamHit>>();
                foreach (BeamHit beamHit in withinBeamBox)
                {
                    if (beamIndexToBeamHit.ContainsKey(beamHit.beamIndex))
                    {
                        beamIndexToBeamHit[beamHit.beamIndex].Add(beamHit);
                    }
                    else
                    {
                        beamIndexToBeamHit[beamHit.beamIndex] = new List<BeamHit>() { beamHit };
                    }
                }
                List<BeamHit> damagingHits = new List<BeamHit>();
                foreach (int key in beamIndexToBeamHit.Keys)
                {
                    List<BeamHit> beamHitsForThisBeam = beamIndexToBeamHit[key];
                    BeamHit nearestBeamHit = beamHitsForThisBeam[0];
                    float nearestSquareDistance = float.MaxValue;
                    for (int j = 0; j < beamHitsForThisBeam.Count; j++)
                    {
                        BeamHit thisHit = beamHitsForThisBeam[j];
                        Vessel enemyVessel = gamestate.PlayerProgresses[thisHit.playerIndex].Vessels[thisHit.vesselIndex];
                        PixelPosition hitPixelPosition = enemyVessel.PixelComponents()[thisHit.pixelComponentIndex].PixelPositions[thisHit.pixelIndex];
                        Vector2 hitWorldPosition = enemyVessel.PixelPositionToWorldPosition(hitPixelPosition).ToVector2();
                        float squareDistance = Vector2.SqrMagnitude(hitWorldPosition - laserWorldPosition.ToVector2());
                        if (squareDistance < nearestSquareDistance)
                        {
                            nearestBeamHit = thisHit;
                            nearestSquareDistance = squareDistance;
                        }
                    }
                    damagingHits.Add(nearestBeamHit);
                }

                foreach (BeamHit thisHit in damagingHits)
                {
                    Vessel enemyVessel = gamestate.PlayerProgresses[thisHit.playerIndex].Vessels[thisHit.vesselIndex];
                    enemyVessel.PixelComponents()[thisHit.pixelComponentIndex].SecondsOfDamage[thisHit.pixelIndex] += game.SecondsPerTick();
                }

                vessel.PowerCore.StoredEnergy -= energyCost; // !
            }
        }

        //delete destroyed pixels
        foreach (Vessel damagedVessel in gamestate.Vessels())
        {
            List<int> deletedIndices = new List<int>();
            foreach (PixelComponent pixelComponent in damagedVessel.PixelComponents())
            {
                for (int i = pixelComponent.PixelPositions.Count - 1; i >= 0 ; i--)
                {
                    if (pixelComponent.SecondsOfDamage[i] < pixelComponent.SecondsToDestroy())
                    {
                        pixelComponent.SecondsOfDamage.RemoveAt(i);
                        pixelComponent.PixelPositions.RemoveAt(i);
                    }
                }
            }
        }

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
                    Vector2 collectorSpacePosition = vessel.Position.ToVector2() + collector.RootPixelPosition.ToVector2();
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
    }
}