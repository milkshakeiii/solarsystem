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
}

[Serializable]
public struct Vessel
{
    public Position Position;

    public List<Component> Components;
    public List<PixelPosition> LightHullPositions;
    public List<PixelPosition> DarkHullPositions;
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

public enum ComponentType
{
    shipyard = 0,
    collector = 1,
    laser = 3,
    engine = 4,
    powerCore = 2
}

[Serializable]
public struct Component
{
    public PixelPosition RootPixelPosition; //relative to parent vessel
    public List<PixelPosition> PixelPositions; //relative to root pixel position
    public ComponentType ComponentType;
    public float Size;
    public float Quality;
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
    public float TargetMoveAmount;
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

        return sourceGamestate;
    }
}