using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoloDroneManager : MonoBehaviour
{

    public TermiteTS tileSystem;
    public GameObject drone2test;
    public TermiteDroneBrain droneBrain;


    // Start is called before the first frame update
    void Start()
    {

        tileSystem.Initialize(3, 3, "TermiteTile");
        droneBrain.Initialize(gameObject);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
