using System;
using System.Collections.Generic;
using UnityEngine;

static class ShipReader
{
    private static PixelComponent BuildPixelComponent(List<Vector2Int> pixelPositions,
                                                      Texture2D image,
                                                      Color color)
    {
        // light hull, dark hull, power core, engine, laser, laser focus, collector, shipyard
        Color lightHullColor = image.GetPixel(0, 45);
        Color darkHullColor = image.GetPixel(1, 45);
        Color powerCoreColor = image.GetPixel(2, 45);
        Color engineColor = image.GetPixel(3, 45);
        Color laserColor = image.GetPixel(4, 45);
        Color collectorColor = image.GetPixel(5, 45);
        Color shipyardColor = image.GetPixel(6, 45);
        HashSet<Color> usedColors = new HashSet<Color>()
        {
            lightHullColor,
            darkHullColor,
            powerCoreColor,
            engineColor,
            laserColor,
            collectorColor,
            shipyardColor,
        };

        if (!usedColors.Contains(color))
        {
            return null;
        }

        int maxX = int.MinValue;
        int maxY = int.MinValue;
        int minX = int.MaxValue;
        int minY = int.MaxValue;
        foreach (Vector2Int foundPixel in pixelPositions)
        {
            maxX = Mathf.Max(foundPixel.x, maxX);
            maxY = Mathf.Max(foundPixel.y, maxY);
            minX = Mathf.Min(foundPixel.x, minX);
            minY = Mathf.Min(foundPixel.y, minY);
        }
        PixelPosition center = new PixelPosition((minX + maxX) / 2, (minY) + (maxY) / 2);
        List<PixelPosition> pixelPositionStructs = new List<PixelPosition>();
        List<float> secondsOfDamage = new List<float>();
        foreach (Vector2Int pixelPosition in pixelPositions)
        {
            pixelPositionStructs.Add(new PixelPosition(pixelPosition.x, pixelPosition.y));
            secondsOfDamage.Add(0f);
        }

        if (color == lightHullColor)
        {
            return new LightHull(center, pixelPositionStructs, secondsOfDamage, color);
        }
        else if (color == darkHullColor)
        {
            return new DarkHull(center, pixelPositionStructs, secondsOfDamage, color);
        }
        // else we need an infodot

        HashSet<Vector2Int> infoDotCandidates = new HashSet<Vector2Int>();
        foreach (Vector2Int pixelPosition in pixelPositions)
        {
            List<Color> adjacentColors = new List<Color>();
            List<Vector2Int> adjacentSquares = new List<Vector2Int>()
            {
                pixelPosition + new Vector2Int(0, 1),
                pixelPosition + new Vector2Int(1, 0),
                pixelPosition + new Vector2Int(0, -1),
                pixelPosition + new Vector2Int(-1, 0)
            };
            foreach (Vector2Int adjacentPosition in adjacentSquares)
            {
                if (!OutOfBoundsOrTopRow(adjacentPosition, image))
                {
                    Color adjacentColor = image.GetPixel(adjacentPosition.x, adjacentPosition.y);
                    if (adjacentColor.a < 1f)
                    {
                        infoDotCandidates.Add(adjacentPosition);
                    }
                }
            }
        }
        if (infoDotCandidates.Count == 0)
        {
            throw new UnityException("A non-hull pixel component had no infodot.");
        }
        if (infoDotCandidates.Count > 1)
        {
            throw new UnityException("A non-hull pixel component had more than one infodot.");
        }
        Vector2Int infoDot = (new List<Vector2Int>(infoDotCandidates))[0];
        Color infoColor = image.GetPixel(infoDot.x, infoDot.y);

        float facing = Mathf.Atan2(infoDot.y - center.Y, infoDot.x - center.X);
        float quality = infoColor.a;
        float size = Mathf.Sqrt(pixelPositionStructs.Count);

        if (color == powerCoreColor)
        {
            return new PowerCore(center, pixelPositionStructs, secondsOfDamage, color, facing, size, quality);
        }
        else if (color == engineColor)
        {
            return new Engine(center, pixelPositionStructs, secondsOfDamage, color, facing, size, quality);
        }
        else if (color == laserColor)
        {
            return new Laser(center, pixelPositionStructs, secondsOfDamage, color, facing, size, quality);
        }
        else if (color == collectorColor)
        {
            return new Collector(center, pixelPositionStructs, secondsOfDamage, color, facing, size, quality);
        }
        else // (color == shipyardColor)
        {
            return new Shipyard(center, pixelPositionStructs, secondsOfDamage, color, facing, size, quality);
        }
    }

    public static Vessel ReadShip(string filepath)
    {
        Vessel readShip = new Vessel(new Position(0, 0),
                                     0f,
                                     null,
                                     null,
                                     null,
                                     new List<Engine>(),
                                     new List<Laser>(),
                                     new List<Collector>(),
                                     new List<Shipyard>());

        byte[] imageData = System.IO.File.ReadAllBytes(filepath);
        Texture2D image = new Texture2D(2, 2);
        ImageConversion.LoadImage(image, imageData);
        int height = image.height;
        int width = image.width;

        HashSet<Vector2Int> pixelsScanned = new HashSet<Vector2Int>();

        for (int i = 0; i < height-1; i++)
        {
            for (int j = 0; j < width; j++)
            {
                Vector2Int position = new Vector2Int(j, i);
                if (!pixelsScanned.Contains(position))
                {
                    Color colorHere = image.GetPixel(j, i);
                    List<Vector2Int> pixelPositions = BucketFill(position, colorHere, pixelsScanned, image);
                    PixelComponent newComponent = BuildPixelComponent(pixelPositions,
                                                                      image,
                                                                      colorHere);
                    if (newComponent != null)
                    {
                        readShip.AddComponent(newComponent);
                    }
                }
            }
        }

        return readShip;
    }

    private static List<Vector2Int> BucketFill(Vector2Int start, Color color, HashSet<Vector2Int> pixelsScanned, Texture2D image)
    {
        Stack<Vector2Int> frontier = new Stack<Vector2Int>();
        List<Vector2Int> foundPixels = new List<Vector2Int>();
        frontier.Push(start);
        while (frontier.Count > 0)
        {
            Vector2Int currentPosition = frontier.Pop();
            if (OutOfBoundsOrTopRow(currentPosition, image))
            {
                continue;
            }
            if (pixelsScanned.Contains(currentPosition))
            {
                continue;
            }
            Color currentColor = image.GetPixel(currentPosition.x, currentPosition.y);
            if (!ColorMatchExceptAlpha(currentColor, color))
            {
                continue;
            }
            pixelsScanned.Add(currentPosition);
            foundPixels.Add(currentPosition);
            List<Vector2Int> adjacentPixels = new List<Vector2Int>() {new Vector2Int(currentPosition.x+1, currentPosition.y),
                                                                      new Vector2Int(currentPosition.x, currentPosition.y+1),
                                                                      new Vector2Int(currentPosition.x-1, currentPosition.y),
                                                                      new Vector2Int(currentPosition.x, currentPosition.y-1)};
            foreach (Vector2Int adjacentPixel in adjacentPixels)
            {
                frontier.Push(adjacentPixel);
            }
            
        }

        return foundPixels;
    }

    private static bool OutOfBoundsOrTopRow(Vector2 position, Texture2D image)
    {
        return position.x < 0 ||
               position.x >= image.width ||
               position.y < 0 ||
               position.y >= image.height-1;
    }

    private static bool ColorMatchExceptAlpha(Color a, Color b)
    {
        return a.r == b.r && a.g == b.g && a.b == b.b;
    }
}