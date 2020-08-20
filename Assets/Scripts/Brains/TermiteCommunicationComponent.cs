using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TermiteCommunicationComponent : MonoBehaviour
{
    // Termite Components
    public TermiteFSMBrain brain;
    public TermiteInterfaceComponent interfaceComponent;
    public TermiteAnimationComponent animationComponent;
    public TermiteAIComponent AIComponent;

    //External References
    public CentralController centralController;
    
    public bool isAlone { get { return centralController.botList.Count == 1; } }

    // Transition
    private FSM.Event _event;
    private Coord reservedDest;
    public List<FSM.Event> unknownEventsBuffer = new List<FSM.Event>();

    public bool IsTransitioning { get; set; } = false;

    private void Update() {
        AcknowledgeExternalEvents();

    }

    public void Initialize(GameObject manager) {
        centralController = manager.GetComponent<CentralController>();
    }

    public void StartTransition(int eventID, Coord dest) {

        //print(Time.time + "- Started Transition: " + dest + " e " + brain.position);

        _event = brain.supervisorio.eventsConteiner[eventID];

        reservedDest = dest;

        IsTransitioning = true;



    }

    public void EndTransition() {

        brain.supervisorio.TriggerEvent(_event); //TODO bugging
        centralController.NotifyTransistionEnd(gameObject, _event);
        IsTransitioning = false;

        interfaceComponent.UpdateStateButtons();
        animationComponent.FixPosition();

        centralController.HeightMapUp(brain.supervisorio.currentState.heightMap);
        brain.position = brain.supervisorio.currentState.GetPosition();
        

        //print(Time.time + "- Ended Transition");

    }

    public bool Allow(Coord dest) {

        if ((dest != brain.position && (dest != reservedDest || !IsTransitioning)) || dest == Coord.origin) {
            //print("Allowed: " + dest + "mine" + brain.position);
            return true;

        }
        //print("Blocked: " + dest);
        return false;
    }


    public void CallIntent(int eventID, Coord destination) {

        if(centralController.RequestIntent(gameObject, destination)) {

            StartTransition(eventID, destination);
            animationComponent.StartAnimation(eventID);

        }

    }

    public void AcknowledgeExternalEvents() {

        if (unknownEventsBuffer.Count > 0) {

            while (unknownEventsBuffer.Count > 0) {
                brain.supervisorio.TriggerEvent(unknownEventsBuffer[0], true);
                unknownEventsBuffer.RemoveAt(0);
            }
            interfaceComponent.UpdateStateButtons();
        }

    }
}


