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
    public TransitionHandler transitionHandler;
    public TermiteAnimationComponent animationHandler;
    public HMIHandler hmiHandler;
    DecisionHandler decisionHandler;

    //Supervisor FSM
    public FSM supervisorio;

    // Working On TODO
    public int id;
    bool hasTile { get { return supervisorio != null ? supervisorio.currentState.hasTile : false; } }
    public Coord position;

    public bool isAuto = false; 
    bool isAlone;
    //bool isOn = false;




    void Update() {

        isAlone = centralController.botList.Count == 1;
        
        // Error handling
        if (supervisorio == null) {
            print("Error: supervisor not loaded yet");
        }


        // Multibot handling
        hmiHandler.CheckSelection();
        
        if (supervisorio.currentState.marked) {
            hmiHandler.End();
        }
        
        if (isAuto) {
            
            if(decisionHandler.myPlan.Count == 0) {
                decisionHandler.PlanAction(10, 5);
                foreach (var item in decisionHandler.myPlan) {
                    //print(item);
                }
            } 
            
            if (!transitionHandler.IsTransitioning && !animationHandler.IsAnimating) {

                if (supervisorio.FeasibleEvents(supervisorio.currentState, true).Contains(decisionHandler.myPlan[0])) {
                    if (!supervisorio.currentState.marked) {
                        CallIntent(decisionHandler.myPlan[0]);
                        decisionHandler.myPlan.RemoveAt(0);
                    }


                    
                } else {
                    decisionHandler.PlanAction(10, 5);
                }

            }
            
        }

        

    }

    //Mouso Collision Functions

    private void OnMouseEnter() {

        hmiHandler.hovering = true;

    }

    private void OnMouseExit() {

        hmiHandler.hovering = false;

    }

    // Initialization Routine Functions

    void InstantiateHandlers() {
        transitionHandler = new TransitionHandler(this);
        
        animationHandler.Initialize(manager);


        hmiHandler = new HMIHandler(this, manager.GetComponent<InterfaceFSM>());
        decisionHandler = new DecisionHandler(this);
    }

    public void Initialize(string automaton, List<FSM.Event> previousEvents) {

        // Set central controller
        centralController = manager.GetComponent<CentralController>();

        // Instantiate supervisor
        supervisorio = new FSM(automaton);

        // Get initial gridosition - normally (0,0)
        position = supervisorio.currentState.GetPosition();

        // Instantiate Handlers
        InstantiateHandlers();

        // Set initial position
        animationHandler.FixPosition();

        supervisorio.RunEvents(previousEvents);

    }

    // State Logic Functions

    void CallIntent(FSM.Event _event) {
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
            hmiHandler.HideStateButtons();

        }

    }
    public void UpdateState() {

        transitionHandler.EndTransition();
        hmiHandler.UpdateDisplay();

        animationHandler.FixPosition();

        //manager.GetComponent<TermiteTS>().UpdateMap(supervisorio.currentState.heightMap);
        centralController.HeightMapUp(supervisorio.currentState.heightMap);

        position = supervisorio.currentState.GetPosition();
        

    }

    // Robot Handlers

    class DecisionHandler {

        TermiteFSMBrain brain;

        public List<FSM.Event> myPlan = new List<FSM.Event>();

        public DecisionHandler(TermiteFSMBrain brain) {

            this.brain = brain;

        }


        public void PlanAction(int steps=5, int tries=5) {

            List<FSM.Event> eventPlan = new List<FSM.Event>();
            int maxScore = -1;

            for (int i = 0; i < tries; i++) {

                FSM.State imaginaryState = brain.supervisorio.currentState;
                List<FSM.Event> tryPlan = new List<FSM.Event>();

                for (int j = 0; j < steps; j++) {

                    //print(i + "" + j + " Started--- "+ imaginaryState);
                    List<FSM.Event> feasible = brain.supervisorio.FeasibleEvents(imaginaryState, true);

                    if(feasible.Count > 0) {
                        FSM.Event tryEvent = feasible[UnityEngine.Random.Range(0, feasible.Count)];
                        //print(i + "" + j + " Did--- " + tryEvent);

                        tryPlan.Add(tryEvent);

                        imaginaryState = brain.supervisorio.ImagineEvent(tryEvent, imaginaryState);
                        //print(i + "" + j + " Ended--- " + imaginaryState);
                    }






                }

                if(EvaluatePlan(tryPlan, steps) > maxScore) {
                    
                    maxScore = EvaluatePlan(tryPlan, steps);
                    eventPlan = tryPlan;
                }
                

            }
            //print("Score: " + maxScore);
            myPlan = eventPlan;
            

        }

        int EvaluatePlan(List<FSM.Event> tryPlan, int posImportance) {

            int count = 0;

            for (int i = 0; i < tryPlan.Count; i++) {

                switch (tryPlan[i].type) {

                    case "typeGet":
                        count += 100 / ((i + 1) * posImportance);
                        break;

                    case "typePlace":
                        if(i == 0) {
                            count += 10000;
                        } else {
                            count += 100 / ((i + 1) * posImportance);
                        }
                        
                        break;

                    case "typeMovementIO":
                        if(tryPlan[i].label == "out" ) {
                            if(i == 0) {
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
    }


    public class TransitionHandler{

        TermiteFSMBrain brain;
        bool _isTransitioning;

        // Transition
        private FSM.Event _event;
        //private Coord reservedPos;
        private Coord reservedDest;

        public bool IsTransitioning { get { return _isTransitioning; } }

        public TransitionHandler(TermiteFSMBrain brain) {
            this.brain = brain;
            _isTransitioning = false;
        }


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

            if((dest != brain.position && (dest != reservedDest || !IsTransitioning)) ||  dest == Coord.origin) {
                //print("Allowed: " + dest + "mine" + brain.position);
                return true;
                
            }
            //print("Blocked: " + dest);
            return false;
        }


    }
    
    
    
    public class HMIHandler {
        
        TermiteFSMBrain brain;
        InterfaceFSM hmi;

        public bool hovering = false;
        public bool selected = false;

        bool outlined {
            get { return (hovering || selected);  }
        }

        public HMIHandler(TermiteFSMBrain brain, InterfaceFSM hmi) {

            this.brain = brain;
            this.hmi = hmi;

        }


        public void End() {
            hmi.endText.SetActive(true);
        }

        public void UpdateOutline() {

            if (outlined && !brain.isAlone) {
                brain.gameObject.GetComponent<MeshRenderer>().material.SetFloat("_FirstOutlineWidth", 0.03f);
            } else {
                brain.gameObject.GetComponent<MeshRenderer>().material.SetFloat("_FirstOutlineWidth", 0f);
            }

        }

        public void UpdateStateDisplay() {

            if(selected || brain.isAlone) {
                hmi.autoToggle.GetComponent<UnityEngine.UI.Toggle>().isOn = brain.isAuto;

            }



        }

        public void UpdateStateButtons() {

            if (selected) {
                if (!brain.isAuto) {

                    hmi.DestroyStateButtons();

                    hmi.CreateStateButtons();

                } else {
                    HideStateButtons();
                }
            }
            

            
        }

        public void HideStateButtons() {
            hmi.DestroyStateButtons();
        }

        public void UpdateDisplay() {

            UpdateStateDisplay();

            UpdateStateButtons();


        }


        public void CheckSelection() {


            if (!brain.isAlone) { //If multibot check selection

                if (Input.GetMouseButtonDown(0)) {
                    if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit)) {

                        if(hit.collider.gameObject == brain.gameObject) {

                            selected = true;

                            //Tell interface to select self
                            hmi.selectedBotBrain = brain;

                        } else {
                            selected = false;
                        }
                  
                        UpdateDisplay();
                    }

                }

                UpdateOutline();

            } else { //If alone always selected

                selected = true;
                hmi.selectedBotBrain = brain;

            }



        }


        //Communication Methods
        public void StateButtonListener(int id) {

            brain.CallIntent(brain.supervisorio.eventsConteiner[id]);
            
        }

        public void AutoToggleListener( bool isAutoState) {

            brain.isAuto = isAutoState;
            brain.decisionHandler.myPlan = new List<FSM.Event>();
            UpdateStateButtons();

        }
        
    }


}
