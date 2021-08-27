using System;
using System.Collections.Generic;
using UnityEngine;

static class ShipReader
{
    private static PixelComponent BuildPixelComponent(List<Vector2Int> pixelPositions,
                                               Texture2D image,
                                               Color color)
    {
        PixelComponent result;

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

        // light hull, dark hull, power core, engine, laser, laser focus, collector, shipyard
        Color lightHullColor = image.GetPixel(0, 0);
        Color darkHullColor = image.GetPixel(1, 0);
        Color powerCoreColor = image.GetPixel(2, 0);
        Color engineColor = image.GetPixel(3, 0);
        Color laserColor = image.GetPixel(4, 0);
        Color collectorColor = image.GetPixel(5, 0);
        Color shipyardColor = image.GetPixel(6, 0);
        HashSet<Color> usedColors = new HashSet<Color>()
        {
            lightHullColor,
            darkHullColor,
            powerCoreColor,
            engineColor,
            laserColor,
            collectorColor,
            shipyardColor,
            Color.white,
        };

        List<Vector2Int> infoDotCandidates = new List<Vector2Int>();
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
                    if (!usedColors.Contains(adjacentColor))
                    {
                        infoDotCandidates.Add(adjacentPosition);
                    }
                }
            }
        }
        if (infoDotCandidates.Count == 0)
        {
            throw new UnityException("A pixel component had no infodot.");
        }
        Vector2Int infoDot = infoDotCandidates[0];
        int bestDistance = int.MaxValue;
        foreach (Vector2Int candidate in infoDotCandidates)
        {
            int distance = Mathf.Abs(candidate.x - center.X) + Mathf.Abs(candidate.y - center.Y);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                infoDot = candidate;
            }
        }
        Color infoColor = image.GetPixel(infoDot.x, infoDot.y);

        float facing = Mathf.Atan2(infoDot.y - center.Y, infoDot.x - center.X);
        float quality = (float)(infoColor.r + infoColor.g + infoColor.b) / 3f;
        float size = Mathf.Sqrt(pixelPositionStructs.Count);

        if (color == lightHullColor)
        {
            result = new LightHull(center, pixelPositionStructs, secondsOfDamage);
        }
        else if (color == darkHullColor)
        {
            result = new DarkHull(center, pixelPositionStructs, secondsOfDamage);
        }
        else if (color == powerCoreColor)
        {
            result = new PowerCore(center, pixelPositionStructs, secondsOfDamage, facing, size, quality);
        }
        else if (color == engineColor)
        {
            result = new Engine(center, pixelPositionStructs, secondsOfDamage, facing, size, quality);
        }
        else if (color == laserColor)
        {
            result = new Laser(center, pixelPositionStructs, secondsOfDamage, facing, size, quality);
        }
        else if (color == collectorColor)
        {
            result = new Collector(center, pixelPositionStructs, secondsOfDamage, facing, size, quality);
        }
        else if (color == shipyardColor)
        {
            result = new Shipyard(center, pixelPositionStructs, secondsOfDamage, facing, size, quality);
        }
        else
        {
            return null;
        }

        return result;
    }

    public static Vessel ReadShip(string filepath)
    {
        Vessel readShip = new Vessel();

        byte[] imageData = System.IO.File.ReadAllBytes(filepath);
        int height = 46;
        int width = 45;
        Texture2D image = new Texture2D(width, height);       

        HashSet<Vector2Int> pixelsScanned = new HashSet<Vector2Int>();

        for (int i = 1; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                Vector2Int position = new Vector2Int(j, i);
                if (!pixelsScanned.Contains(position))
                {
                    Color colorHere = image.GetPixel(j, i);
                    List<Vector2Int> pixelPositions = BucketFillExtremes(position, colorHere, pixelsScanned, image);
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

    private static List<Vector2Int> BucketFillExtremes(Vector2Int start, Color color, HashSet<Vector2Int> pixelsScanned, Texture2D image)
    {
        Stack<Vector2Int> frontier = new Stack<Vector2Int>;
        List<Vector2Int> foundPixels = new List<Vector2Int>();
        frontier.Push(start);
        while (frontier.Count > 0)
        {
            Vector2Int currentPosition = frontier.Pop();
            if (OutOfBoundsOrTopRow(currentPosition, image))
                continue;
            if (pixelsScanned.Contains(currentPosition))
                continue;
            Color currentColor = image.GetPixel(currentPosition.x, currentPosition.y);
            if (currentColor != color)
                continue;
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
        return position.x >= 0 &&
               position.x < image.width &&
               position.y >= 1 &&
               position.y < image.height;
    }
}