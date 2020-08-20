using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TermiteCommunicationComponent : MonoBehaviour
{
    public TermiteFSMBrain brain;
    bool _isTransitioning = false;

    // Transition
    private FSM.Event _event;
    //private Coord reservedPos;
    private Coord reservedDest;

    public bool IsTransitioning { get { return _isTransitioning; } }


    public void StartTransition(int eventID, Coord dest) {

        //print(Time.time + "- Started Transition: " + dest + " e " + brain.position);

        _event = brain.supervisorio.eventsConteiner[eventID];

        reservedDest = dest;

        _isTransitioning = true;



    }

    public void EndTransition() {

        brain.supervisorio.TriggerEvent(_event); //TODO bugging
        brain.centralController.NotifyTransistionEnd(brain.gameObject, _event);
        _isTransitioning = false;
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
}


