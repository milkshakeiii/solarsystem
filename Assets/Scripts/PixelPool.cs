using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PixelPool : MonoBehaviour
{
    private static PixelPool instance;

    public static PixelPool GetInstance()
    {
        return instance;
    }

    public GameObject PixelPrefab;

    private Queue<GameObject> freePixels = new Queue<GameObject>();

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public GameObject GetPixel(GameObject forVessel)
    {
        GameObject gottenPixel;
        if (freePixels.Count != 0)
        {
            gottenPixel = freePixels.Dequeue();
            gottenPixel.SetActive(true);
            gottenPixel.transform.parent = forVessel.transform;
        }
        else
        {
            gottenPixel = Instantiate(PixelPrefab, forVessel.transform);
        }
        return gottenPixel;
    }

    public void GivePixel(GameObject pixel)
    {
        pixel.SetActive(false);
        pixel.transform.parent = transform;
        freePixels.Enqueue(pixel);
    }
}
