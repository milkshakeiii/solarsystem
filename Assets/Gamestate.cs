using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Game
{
    int StatesPerTurn;
    int SecondsPerTurn;
    List<Gamestate> gamestates;
}

[Serializable]
public struct Gamestate
{
    readonly float Height;
    readonly float Width;

    List<Vessel> Vessels;
    List<Asteroid> Asteroids;
}

[Serializable]
public struct Position
{
    float x;
    float y;
}

[Serializable]
public struct Vessel
{
    Position Position;
}

[Serializable]
public struct Asteroid
{
    Position Position;
}

[Serializable]
public struct Component
{
    enum ComponentType
    {
        shipyard = 0,
        collector = 1,
        powerCore = 2,
        laser = 3,
        engine = 4
    }
}