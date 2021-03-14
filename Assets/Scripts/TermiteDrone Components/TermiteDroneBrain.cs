using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TermiteDroneBrain : MonoBehaviour
{

    // Termite Components
    public TermiteAnimationComponent animationComponent;

    //Supervisor FSM
    public FSM supervisorio;

    // Variables
    public Coord position;


    public void Initialize() {

        // Instantiate supervisor
        //supervisorio = new FSM(sup, customInit);

        // Get initial gridosition - normally (0,0)
        position = supervisorio.currentState.GetPosition();

        // Instantiate Handlers
        //animationComponent.Initialize();

        // Set initial position
        animationComponent.FixPosition();

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
