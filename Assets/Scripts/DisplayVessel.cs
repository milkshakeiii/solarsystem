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


        transform.position = gameVessel.Position.ToVector2();
        transform.eulerAngles = new Vector3(0, gameVessel.Facing * Mathf.Rad2Deg, 0);
    }
}
