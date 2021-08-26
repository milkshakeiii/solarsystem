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
        Color laserFocusColor = image.GetPixel(5, 0);
        Color collectorColor = image.GetPixel(6, 0);
        Color shipyard = image.GetPixel(7, 0);

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
            result = new PowerCore(center, pixelPositionStructs, secondsOfDamage);
        }
        else if (color == engineColor)
        {
            result = new Engine(center, pixelPositionStructs, secondsOfDamage);
        }
        else if (color == laserColor)
        {
            result = new Laser(center, pixelPositionStructs, secondsOfDamage, image, laserFocusColor);
        }
        else if (color == laserFocusColor)
        {
            return null;
        }
        else if (color == collectorColor)
        {
            result = new Collector(center, pixelPositionStructs, secondsOfDamage);
        }
        else
        {
            throw new UnityException("Unrecognized color in ship image.");
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