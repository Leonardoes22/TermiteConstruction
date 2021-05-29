using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TermiteDroneBrain : MonoBehaviour
{

    // Termite Components
    public AnimationComponent animationComponent;
    public DroneInterfaceComponent interfaceComponent;

    //Supervisor FSM
    public FSM supervisor;

    // Variables
    public Coord position;


    public void Initialize(GameObject manager) {

        // Temporary: Instantiate supervisor
        var structurePlant = new StructurePlant("Drone2Animate.xml");
        var sup = structurePlant.supList[0];
        supervisor = new FSM(sup);


        // Get initial gridposition - normally (0,0)
        position = supervisor.currentState.GetPosition();


        // Initialize Components
        animationComponent.Initialize(manager);
        interfaceComponent.Initialize(manager);


    }

    public void ProcessIntent(int eventId) {


        FSM.Event evento = supervisor.eventsConteiner[eventId];
        supervisor.TriggerEvent(evento);

        StartCoroutine(animationComponent.CallAnimation(evento.label, FinishedAnimation));

    }

    private void FinishedAnimation() {
        // Activated when an animation finishes.
        // I used to update states

        interfaceComponent.UpdateStateButtons();
        UpdatePosition();
    }

    //Update Bot coordinate position to match state
    public void UpdatePosition() {
        position = supervisor.currentState.GetPosition();
    }
}
