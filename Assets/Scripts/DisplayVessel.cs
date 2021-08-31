using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayVessel : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Initialize(Vessel gameVessel)
    {
        foreach (PixelComponent pixelComponent in gameVessel.PixelComponents())
        {
            foreach (PixelPosition position in pixelComponent.PixelPositions)
            {
                GameObject pixel = PixelPool.GetInstance().GetPixel(gameObject);
                pixel.GetComponent<SpriteRenderer>().color = pixelComponent.Color;
                pixel.transform.localPosition = new Vector3(position.X, position.Y, 0);
            }
        }

        transform.position = gameVessel.WorldPosition.ToVector2();
        transform.eulerAngles = new Vector3(0, 0, gameVessel.Facing * Mathf.Rad2Deg);
    }
}
