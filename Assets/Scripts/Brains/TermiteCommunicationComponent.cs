using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TermiteCommunicationComponent : MonoBehaviour
{
    // Termite Components
    public TermiteFSMBrain brain;
    public TermiteAnimationComponent animationComponent;
    public TermiteInterfaceComponent interfaceComponent;
    

    //External References
    public CentralController centralController;


    //States
    public bool isAlone { get { return centralController.botList.Count == 1; } }
    public bool IsTransitioning { get; set; } = false;


    //Variables
    public List<FSM.Event> unknownEventsBuffer = new List<FSM.Event>();

    private FSM.Event _event;
    public Coord reservedDest;

    public void Initialize(GameObject manager) {
        centralController = manager.GetComponent<CentralController>();
    }

    private void Update() {

        if (!IsTransitioning && unknownEventsBuffer.Count > 0) {

            AcknowledgeExternalEvents();
            interfaceComponent.UpdateStateButtons();
        }
        
    }

    public void StartTransition(int eventID, Coord dest) {

        //print(Time.time + "- Started Transition: " + dest + " e " + brain.position);

        _event = brain.supervisorio.eventsConteiner[eventID];

        // START TRANSITION
        reservedDest = dest;
        IsTransitioning = true;

        

        // START ANIMATION
        animationComponent.StartAnimation(eventID);
    }

    public void EndTransition() {

        AcknowledgeExternalEvents();

        // UPDATE STATE
        // Moved update state to transition start to solve crashes
        // but it generated anticipated tile bug with the HeightMapUp function
        brain.supervisorio.TriggerEvent(_event);
        centralController.NotifyTransistionEnd(gameObject, _event);
        IsTransitioning = false;

        interfaceComponent.UpdateStateButtons();
        animationComponent.FixPosition();

        centralController.HeightMapUp(brain.supervisorio.currentState.heightMap);
        brain.UpdatePosition();
        
        

        //print(Time.time + "- Ended Transition");

    }

    // Allows other bots intention to occur
    public bool Allow(Coord dest) {

        if(brain.position == dest && dest != Coord.origin) {
            return false;
        } else if (IsTransitioning && reservedDest == dest) {
            return false;
        } else {
            return true;
        }

    }

    // Start a transition if all other bots allow and the intended event is still possible
    public void CallIntent(int eventID, Coord destination) {

        if(centralController.RequestIntent(gameObject, destination)) {

            if (brain.supervisorio.FeasibleEvents().Contains(brain.supervisorio.eventsConteiner[eventID])) {
                StartTransition(eventID, destination);
            }

        } else {
            brain.ActionDenied();
        }

    }

    //  
    /// <summary>
    /// Triggers all external events that happened
    /// </summary>
    public void AcknowledgeExternalEvents() {

        while (unknownEventsBuffer.Count > 0) {
            brain.supervisorio.TriggerEvent(unknownEventsBuffer[0], true);
            unknownEventsBuffer.RemoveAt(0);
        }

    }
}


