using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
//using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Audio;
using UnityEngine.PlayerLoop;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class TermiteFSMBrain : MonoBehaviour {

    // Termite Components
    public TermiteAnimationComponent animationComponent;
    public TermiteCommunicationComponent communicationComponent;
    public TermiteInterfaceComponent interfaceComponent;
    

    // External References
    public GameObject manager;
    public CentralController centralController;


    //Supervisor FSM
    public FSM supervisorio;


    // Variables
    public int id;
    public Coord position;
    public List<FSM.Event> myPlan = new List<FSM.Event>();


    //States
    public bool isAuto = false;
    public float wait;

    //Temp variable
    public bool isAlone { get { return communicationComponent.isAlone; } }

    

    public void Initialize(Supervisor sup, List<FSM.Event> previousEvents) {

        // Set central controller
        centralController = manager.GetComponent<CentralController>();

        // Instantiate supervisor
        //supervisorio = new FSM(automaton);
        supervisorio = new FSM(sup);

        // Get initial gridosition - normally (0,0)
        position = supervisorio.currentState.GetPosition();

        // Instantiate Handlers
        animationComponent.Initialize(manager);
        interfaceComponent.Initialize(manager);
        communicationComponent.Initialize(manager);

        // Set initial position
        animationComponent.FixPosition();

        supervisorio.RunEvents(previousEvents);

    }
    void Update() {


        // Error handling
        if (supervisorio == null) {
            print("Error: supervisor not loaded yet");
        }

        wait = Mathf.Max(wait - Time.deltaTime, 0);

        if (isAuto && wait<=0) {
            if (!supervisorio.currentState.marked) {
                UsePlanMode();
                //UseRandomMode();
            }
        }


    }

    public void ProcessIntent(FSM.Event _event) {
        //print(Time.time +"- ID:" + id +"- From: " + position + " Called Intent: (" + _event + "), alone?: " + isAlone);
        //FSM.Event _event = supervisorio.eventsConteiner[eventID];
        if(_event == null) {
            return;
        }

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

        communicationComponent.CallIntent(_event.id, dest);



    }

    //AI functions
    void UsePlanMode() {
        if (myPlan.Count == 0) {
            PlanAction(10, 5);
            
        }
        
        if (!communicationComponent.IsTransitioning && !animationComponent.IsAnimating && myPlan.Count> 0 ) {

            if (supervisorio.FeasibleEvents(supervisorio.currentState, true).Contains(myPlan[0]) ) {
                
                ProcessIntent(myPlan[0]);
                myPlan.RemoveAt(0);


            } else {

                
                //ProcessIntent(ChoseAtRandom());
                //myPlan.RemoveAt(0);
                PlanAction();
            }

        }
        
    }


    void UseRandomMode() {
        if (!communicationComponent.IsTransitioning && !animationComponent.IsAnimating) {

            
            ProcessIntent(ChoseAtRandom());
        }
    }

    FSM.Event ChoseAtRandom() {
        List<FSM.Event> feasible = supervisorio.FeasibleEvents(supervisorio.currentState, true);

        if (feasible.Count > 0) {
            int rand = UnityEngine.Random.Range(0, feasible.Count);

            return feasible[rand];
        } else {
            return null;
        }

        
    }
    
    private void PlanAction(int steps = 5, int tries = 5) {

        List<FSM.Event> eventPlan = new List<FSM.Event>();
        int maxScore = -1;

        for (int i = 0; i < tries; i++) {

            FSM.State imaginaryState = supervisorio.currentState;
            List<FSM.Event> tryPlan = new List<FSM.Event>();

            for (int j = 0; j < steps; j++) {

                //print(i + "" + j + " Started--- "+ imaginaryState);
                List<FSM.Event> feasible = supervisorio.FeasibleEvents(imaginaryState, true);

                if (feasible.Count > 0) {
                    FSM.Event tryEvent = feasible[UnityEngine.Random.Range(0, feasible.Count)];
                    //print(i + "" + j + " Did--- " + tryEvent);

                    tryPlan.Add(tryEvent);

                    imaginaryState = supervisorio.ImagineEvent(tryEvent, imaginaryState);
                    //print(i + "" + j + " Ended--- " + imaginaryState);
                }

            }

            if (EvaluatePlan(tryPlan, steps) > maxScore) {

                maxScore = EvaluatePlan(tryPlan, steps);
                eventPlan = tryPlan;
            }


        }
        //print("Score: " + maxScore);
        myPlan = eventPlan;


    }

    private int EvaluatePlan(List<FSM.Event> tryPlan, int posImportance) {

        int count = 0;

        for (int i = 0; i < tryPlan.Count; i++) {

            switch (tryPlan[i].type) {

                case "typeGet":
                    count += 100 / ((i + 1) * posImportance);
                    break;

                case "typePlace":
                    if (i == 0) {
                        count += 10000;
                    } else {
                        count += 100 / ((i + 1) * posImportance);
                    }

                    break;

                case "typeMovementIO":
                    if (tryPlan[i].label == "out") {
                        if (i == 0) {
                            count += 1000;
                        } else {
                            count += 100 / ((i + 1) * posImportance);
                        }

                    }
                    break;

                default:
                    break;
            }

        }

        return count;
    }


    //Update Bot coordinate position to match state
    public void UpdatePosition() {
        position = supervisorio.currentState.GetPosition();
    }

    public void ActionDenied() {
        wait =  manager.GetComponent<SimManager>().isFastAnim ? 0.5f : 0f ;
    }

}
