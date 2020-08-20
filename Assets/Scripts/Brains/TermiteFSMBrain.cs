using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Audio;
using UnityEngine.PlayerLoop;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class TermiteFSMBrain : MonoBehaviour {

    // External gameobjects
    public GameObject manager;
    public CentralController centralController;

    // Handlers
    public TermiteCommunicationComponent transitionHandler;
    public TermiteInterfaceComponent hmiHandler;
    public TermiteAnimationComponent animationHandler;
    public TermiteAIComponent decisionHandler;

    //Supervisor FSM
    public FSM supervisorio;


    // Variables
    public int id;
    public Coord position;
    public List<FSM.Event> unknowEventsBuffer = new List<FSM.Event>();


    //States
    public bool isAuto = false; 
    public bool isAlone;

    void Update() {

        isAlone = centralController.botList.Count == 1;

        AcknowledgeExternalEvents();
        
        // Error handling
        if (supervisorio == null) {
            print("Error: supervisor not loaded yet");
        }

    }

    public void Initialize(string automaton, List<FSM.Event> previousEvents) {

        // Set central controller
        centralController = manager.GetComponent<CentralController>();

        // Instantiate supervisor
        supervisorio = new FSM(automaton);

        // Get initial gridosition - normally (0,0)
        position = supervisorio.currentState.GetPosition();

        // Instantiate Handlers
        animationHandler.Initialize(manager);
        hmiHandler.Initialize(manager);

        // Set initial position
        animationHandler.FixPosition();

        supervisorio.RunEvents(previousEvents);

    }


    // State Logic Functions
    public void CallIntent(FSM.Event _event) {
        //print(Time.time +"- ID:" + id +"- From: " + position + " Called Intent: (" + _event + "), alone?: " + isAlone);
        //FSM.Event _event = supervisorio.eventsConteiner[eventID];
        Coord dest = position;
        
        switch (_event.type) {
            
            case "typeMovement":
                switch (_event.label) {
                    case "u":
                        dest = position + Coord.left; break;
                    case "d":
                        dest = position + Coord.right; break;
                    case "l":
                        dest = position + Coord.down; break;
                    case "r":
                        dest = position + Coord.up; break;
                }
                break;

            case "typeMovementIO":

                if (_event.label == "out") {
                    dest = Coord.origin;
                } else if (_event.label == "in") {
                    dest = new Coord(1, 1);
                } else {
                    dest = new Coord((int)Char.GetNumericValue(_event.label[2]), (int)Char.GetNumericValue(_event.label[3]));
                }

                break;

            case "typePlace":
                dest = new Coord((int)Char.GetNumericValue(_event.label[1]), (int)Char.GetNumericValue(_event.label[2]));
                break;

        }


        if (isAlone || (!isAlone && centralController.RequestIntent(gameObject, dest))) {

            transitionHandler.StartTransition(_event.id, dest);
            animationHandler.StartAnimation(_event.id);

        }

    }
    public void UpdateState() {

        transitionHandler.EndTransition();
        hmiHandler.UpdateStateButtons();

        animationHandler.FixPosition();

        //manager.GetComponent<TermiteTS>().UpdateMap(supervisorio.currentState.heightMap);
        centralController.HeightMapUp(supervisorio.currentState.heightMap);

        position = supervisorio.currentState.GetPosition();
        
    }

    public void AcknowledgeExternalEvents() {

        if(unknowEventsBuffer.Count > 0) { 

            while(unknowEventsBuffer.Count > 0) {
                supervisorio.TriggerEvent(unknowEventsBuffer[0], true);
                unknowEventsBuffer.RemoveAt(0);
            }
            hmiHandler.UpdateStateButtons();
        }

    }
    
}
