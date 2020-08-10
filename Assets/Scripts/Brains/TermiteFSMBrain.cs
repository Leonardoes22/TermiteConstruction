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

    // Bot Children (Must be loaded)
    public GameObject myTile;
    public GameObject grabber;
    public Transform[] whegsTransform;

    // Handlers
    public TransitionHandler transitionHandler;
    AnimationHandler animationHandler;
    public HMIHandler hmiHandler;
    DecisionHandler decisionHandler;

    //Supervisor FSM
    public FSM supervisorio;

    // Working On TODO
    public int id;
    bool hasTile { get { return supervisorio != null ? supervisorio.currentState.hasTile : false; } }
    Coord position;

    public bool isAuto = false; 
    bool isAlone;
    //bool isOn = false;


    List<FSM.Event> teste;

    void Start() {

        

    }

    void Update() {

        isAlone = centralController.botList.Count == 1;
        
        // Error handling
        if (supervisorio == null) {
            print("Error: supervisor not loaded yet");
        }
        if (myTile == null) {
            print("Error: missing internal tile reference");
        }
        if (myTile == null) {
            print("Error: missing grabber reference");
        }


        // Animation handling
        animationHandler.Animate();

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

    void InstantiateBotChildren() {
        grabber = transform.Find("Grabber").gameObject;
        myTile = grabber.transform.GetChild(0).GetChild(0).gameObject;
        whegsTransform = new Transform[4];
        for (int i = 0; i < 4; i++) {
            whegsTransform[i] = transform.Find("Wheg" + i);
        }
    }

    void InstantiateHandlers() {
        transitionHandler = new TransitionHandler(this);
        animationHandler = new AnimationHandler(this);
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

        // Instantiate bot children
        InstantiateBotChildren();

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
    void UpdateState() {

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
    
    class AnimationHandler {

        // External References
        TermiteFSMBrain brain;
        TermiteTS tileSystem;

        //Animation Variables
        Vector3 initialPos;
        int nextDirection;
        public int currentDirection = 3;

        // Animation queue
        public List<Action> animationBuffer = new List<Action>();

        // Animation states
        public bool isGrabbing = false;
        public bool isPlacing = false;
        bool isMovingFoward = false;
        int isTurning = 0;
        bool fastMode {

            get {
                return brain.centralController.gameObject.GetComponent<SimManager>().isFastAnim;
            }
        }

        // Animation parameters
        const float fowardSpeed = 22f * 2;
        const float grabSpeed = 100f;
        const float whegSpeed = 300f;
        const float turnSpeed = 120f * 2;
        Vector3[] compass = { new Vector3(0, 270, 0), new Vector3(0, 180, 0), new Vector3(0, 90, 0), new Vector3(0, 0, 0) };

        const float baseHeight = 5f;
        const float tileHeight = 2.25f;
        Vector3 outsidePos = new Vector3(-27.5f * 1.5f, baseHeight, -27.5f * 1.5f);

        // Animation transforms
        Transform coreTransform;
        Transform myTileTransform;
        Transform grabberTransform;
        Transform[] whegsTransform;

        // Properties
        public bool IsAnimating { get; private set; } = false;

        bool IsAnimatingStep {
            get {
                if(isGrabbing || isPlacing || isMovingFoward || isTurning != 0) {
                    return true;
                } else {
                    return false;
                }
            }
        }

        // Constructor
        public AnimationHandler(TermiteFSMBrain brain) {

            this.brain = brain;
            this.coreTransform = brain.transform;
            this.myTileTransform = brain.myTile.transform;
            this.grabberTransform = brain.grabber.transform;
            this.whegsTransform = brain.whegsTransform;

            tileSystem = brain.manager.GetComponent<TermiteTS>();
 
        }


        // Methods


        public void FixPosition() {

            if (brain.supervisorio.currentState.InGrid){

                Coord gridPos = brain.supervisorio.currentState.GetPosition();

                float heightModifier = tileSystem.heightMap[gridPos] * tileHeight;

                coreTransform.position = tileSystem.centreMap[gridPos] + new Vector3(0,baseHeight+heightModifier,0); // Temp Correction
            } else {
                coreTransform.position = outsidePos + new Vector3(0, 0, brain.id * 27.5f);
                currentDirection = 3;
            }

        }
        
        public void StartAnimation(int eventID) {

            //print(Time.time + "- Started Animation");

            IsAnimating = true;

            FSM.Event _event = brain.supervisorio.eventsConteiner[eventID];
            string eventType = _event.type;

            switch (eventType) {

                case "typeGet":
                    animationBuffer.Add(GrabTile);
                    break;

                case "typePlace":

                    int x = int.Parse(_event.label[1].ToString());
                    int y = int.Parse(_event.label[2].ToString());
                    /*
                    print(_event.label);
                    print("My: " + brain.position);
                    print("Target: " + x + "." + y);
                    */
                    if(brain.position.y < y) {
                        animationBuffer.Add(() => TurnTo(0));
                    } else if (brain.position.y > y) {
                        animationBuffer.Add(() => TurnTo(2));
                    } else if(brain.position.x < x) {
                        animationBuffer.Add(() => TurnTo(3));
                    } else {
                        animationBuffer.Add(() => TurnTo(1));
                    }
                    animationBuffer.Add(PlaceTile);

                    break;

                case "typeMovement":

                    switch (_event.label) {

                        case "u":
                            animationBuffer.Add(() => TurnTo(1));
                            break;
                        
                        case "d":
                            animationBuffer.Add(() => TurnTo(3));
                            break;

                        case "l":
                            animationBuffer.Add(() => TurnTo(2));
                            break;

                        case "r":
                            animationBuffer.Add(() => TurnTo(0));
                            break;

                    }
                    animationBuffer.Add(GoFoward);
                    break;

                case "typeMovementIO":

                    if(_event.label == "out") {
                        coreTransform.position = outsidePos;
                        coreTransform.eulerAngles = compass[3];
                    } else if(_event.label == "in")  {
                        coreTransform.position = tileSystem.centreMap[new Coord(1,1)];
                        coreTransform.eulerAngles = compass[3];
                    }
                    break;

              
            }

        }
        
        public void Animate() {

            if(IsAnimating) {
                
                //Animation Logic
                if (!IsAnimatingStep) {
                   
                    //Animate next step in queue
                    if (animationBuffer.Count > 0) {

                        animationBuffer[0].Invoke(); //Call animation command
                        animationBuffer.RemoveAt(0); // Remove animation command from queue

                    } else {
                        IsAnimating = false;
                        brain.UpdateState();
                        
                    }

                }

                // Perform Animations
                if (isMovingFoward) {
                    AnimateFoward();
                }
                if (isGrabbing) {
                    AnimateGrab();
                }
                if (isPlacing) {
                    AnimatePlacing();
                }
                if (Math.Abs(isTurning) == 1) {
                    AnimateTurning();
                }


            }

        }

        // Animation commands
        public void GrabTile() {
            
            myTileTransform.gameObject.GetComponent<MeshRenderer>().enabled = true;
            isGrabbing = true; 
        }
        public void PlaceTile() {
            
            isPlacing = true;

            //print("Placing");
        }
        public void TurnRight() {
            
            nextDirection = currentDirection - 1 >= 0 ? (currentDirection - 1) % 4 : 3;
            isTurning = 1;

            //print("Turning right");

        }
        public void TurnLeft() {
            
            nextDirection = (currentDirection + 1) % 4;
            isTurning = -1;
            //print("Turning left");
        }
        public void TurnTo(int destiny) {

            if (destiny == currentDirection) {
                //print("Already there");
            } else {
                for (int i = 0; i < 2; i++) {
                    nextDirection = currentDirection - 1 >= 0 ? currentDirection - 1 : 3;
                    if (nextDirection == destiny) {
                        isTurning = 1;
                        break;
                    }
                    nextDirection = destiny;
                    isTurning = -1;
                }
            }

            //brain.manager.GetComponent<InterfaceFSM>().debug2.GetComponent<Text>().text = Time.time + "-- turning to " + destiny ;
            
        }

        public void GoFoward() {

            Coord tempCoord = brain.supervisorio.currentState.GetPosition();
            if(tempCoord != Coord.origin) {
                initialPos = tileSystem.centreMap[brain.supervisorio.currentState.GetPosition()];

                isMovingFoward = true;
            }
            
        }

        // Animate grabbing tile
        void AnimateGrab() {

            if (!fastMode) {
                grabberTransform.Rotate(new Vector3(0, 0, grabSpeed * Time.deltaTime));
            } 

            if (grabberTransform.eulerAngles.z > 140
                || fastMode) {
                grabberTransform.localEulerAngles = new Vector3(0,0,140);
                isGrabbing = false;
            }

        }
        void AnimatePlacing() {
            if (!fastMode) {
                grabberTransform.Rotate(new Vector3(0, 0, -grabSpeed * Time.deltaTime));
            } 

            if (grabberTransform.eulerAngles.z < 2
                || fastMode) {
                grabberTransform.localEulerAngles = new Vector3(0, 0, 0); 
                isPlacing = false;
                myTileTransform.gameObject.GetComponent<MeshRenderer>().enabled = false; // TODO

            }
        }
        // Animate turning bot
        void AnimateTurning() {

            //brain.manager.GetComponent<InterfaceFSM>().debug.GetComponent<Text>().text = Time.time + "-- gira gira ";

            if (!fastMode) {
                
                for (int i = 0; i < 4; i++) {
                    whegsTransform[i].Rotate(new Vector3(0, 0, (i < 2 ? -1 : 1) * isTurning * whegSpeed * Time.deltaTime));
                }

                coreTransform.Rotate(new Vector3(0, isTurning * turnSpeed * Time.deltaTime, 0));

            }
            


            if (Mathf.Abs(Mathf.Abs(coreTransform.eulerAngles.y) - compass[nextDirection].y) < 8
                || fastMode) {

                currentDirection = nextDirection;
                coreTransform.eulerAngles = compass[currentDirection];
                isTurning = 0;

            }


        }
        // Animate foward walk
        void AnimateFoward() {

            if (!fastMode) {
                for (int i = 0; i < 4; i++) {
                    whegsTransform[i].Rotate(new Vector3(0, 0, -whegSpeed * Time.deltaTime));
                }

                coreTransform.Translate(new Vector3(fowardSpeed * Time.deltaTime, 0, 0));
            } else {
                coreTransform.Translate(new Vector3(27.5f, 0, 0));
            }

            

            if(Vector3.Distance(coreTransform.position,initialPos) > 27.5f) {
                
                isMovingFoward = false;

            }

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
                hmi.stateDisplay.GetComponent<Text>().text = brain.supervisorio.currentState.ToString();
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
