using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using TreeEditor;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class TermiteFSMBrain : MonoBehaviour {

    // External gameobjects
    public GameObject master;
    public GameObject controllerInterface;

    // Bot Children (Must be loaded)
    GameObject myTile;
    GameObject grabber;
    Transform[] whegsTransform;

    // Handlers
    TransitionHandler transitionHandler;
    AnimationHandler animationHandler;
    public HMIHandler hmiHandler;
    DecisionHandler decisionHandler;

    //Supervisor FSM
    public FSM supervisorio;

    // Working On TODO
    bool hasTile { get { return supervisorio != null ? supervisorio.currentState.hasTile : false; } }
    Coord position;

    bool isAuto = true;
    bool selected = false;

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

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

        
        
    }

    private void OnMouseDown() {

        selected = true;

    }

    private void OnMouseEnter() {

        gameObject.GetComponent<MeshRenderer>().material.SetFloat("_FirstOutlineWidth", 0.03f);

    }

    private void OnMouseExit() {

        if (!selected) {
            gameObject.GetComponent<MeshRenderer>().material.SetFloat("_FirstOutlineWidth", 0f);
        }

    }
    void InstantiateBotChildren() {
        grabber = transform.Find("Grabber").gameObject;
        myTile = grabber.transform.Find("GrabberArm/TermiteTile").gameObject;
        whegsTransform = new Transform[4];
        for (int i = 0; i < 4; i++) {
            whegsTransform[i] = transform.Find("Wheg" + i);
        }
    }

    void InstantiateHandlers() {
        transitionHandler = new TransitionHandler(this);
        animationHandler = new AnimationHandler(this, transform, myTile.transform, grabber.transform, whegsTransform);
        hmiHandler = new HMIHandler(this, master.GetComponent<InterfaceFSM>());
        decisionHandler = new DecisionHandler();
    }

    public void Initialize(string automaton) {

        // Instantiate supervisor
        supervisorio = new FSM(automaton);

        // Get initial griposition - normally (0,0)
        position = supervisorio.currentState.GetPosition();

        // Instantiate bot children
        InstantiateBotChildren();

        // Instantiate Handlers
        InstantiateHandlers();

        // Set myTile local position
        myTile.transform.localPosition = new Vector3(0.1633892f, -0.0009015298f, -0.01356555f); // TODO

        // Set initial position
        animationHandler.FixPosition();
    }

    void CallIntent(int eventID) {

        if (controllerInterface != null) {
            //TODO: Interface between bots
        } else {
            transitionHandler.StartTransition(eventID);
            animationHandler.StartAnimation(eventID);
            hmiHandler.HideStateButtons();
        }

    }
    void UpdateState() {

        transitionHandler.EndTransition();
        hmiHandler.UpdateDisplay();
        animationHandler.FixPosition();
        master.GetComponent<TermiteTS>().UpdateMap(supervisorio.currentState.heightMap);
        position = supervisorio.currentState.GetPosition();
        

    }

    class DecisionHandler {

        public DecisionHandler() {

        }

    }

    class TransitionHandler{

        TermiteFSMBrain brain;
        bool _isTransitioning;

        // Transition
        private FSM.Event _event;
        private FSM.State _sourceState;
        private FSM.State _destinyState;

        bool IsTransitioning { get { return _isTransitioning; } }

        public TransitionHandler(TermiteFSMBrain brain) {
            this.brain = brain;
            _isTransitioning = false;
        }

        public void StartTransition(int eventID) {

            _sourceState = brain.supervisorio.currentState;
            _event = brain.supervisorio.eventsConteiner[eventID];

            _isTransitioning = true;

        }

        public void EndTransition() {

            brain.supervisorio.TriggerEvent(_event); //TODO bugging
            _isTransitioning = false;

        }


    }
    
    class AnimationHandler {

        // External References
        TermiteFSMBrain brain;
        TermiteTS tileSystem;

        //Animation Variables
        Vector3 initialPos;
        int nextDirection;
        int currentDirection = 3;

        // Animation queue
        public List<Action> animationBuffer = new List<Action>();

        // Animation states
        public bool isGrabbing = false;
        public bool isPlacing = false;
        bool isMovingFoward = false;
        int isTurning = 0;

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
        public AnimationHandler(TermiteFSMBrain brain, Transform core, Transform myTile, Transform grabber, Transform[] whegs) {

            this.brain = brain;
            this.coreTransform = core;
            this.myTileTransform = myTile;
            this.grabberTransform = grabber;
            this.whegsTransform = whegs;

            tileSystem = brain.master.GetComponent<TermiteTS>();
 
        }


        // Methods


        public void FixPosition() {

            if (brain.supervisorio.currentState.InGrid){

                Coord gridPos = brain.supervisorio.currentState.GetPosition();

                float heightModifier = tileSystem.heightMap[gridPos] * tileHeight;

                coreTransform.position = tileSystem.centreMap[gridPos] + new Vector3(0,baseHeight+heightModifier,0); // Temp Correction
            } else {
                coreTransform.position = outsidePos;
            }

        }
        
        public void StartAnimation(int eventID) {

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
                    print(_event.label);
                    print("My: " + brain.position);
                    print("Target: " + x + "." + y);

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
                        coreTransform.eulerAngles = compass[0];
                    } else if(_event.label == "in")  {
                        coreTransform.position = tileSystem.centreMap[new Coord(1,1)];
                        coreTransform.eulerAngles = compass[3];
                    }

                    
                    break;

                case "other":
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
        }
        public void TurnRight() {
            
            nextDirection = currentDirection - 1 >= 0 ? (currentDirection - 1) % 4 : 3;
            isTurning = 1;

        }
        public void TurnLeft() {
            
            nextDirection = (currentDirection + 1) % 4;
            isTurning = -1;
            
        }
        public void TurnTo(int destiny) {

            if (destiny == currentDirection) {
                print("Already there");
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
        }

        public void GoFoward() {

            initialPos = tileSystem.centreMap[brain.supervisorio.currentState.GetPosition()];

            isMovingFoward = true;
        }

        // Animate grabbing tile
        void AnimateGrab() {

            grabberTransform.Rotate(new Vector3(0, 0, grabSpeed * Time.deltaTime));

            if (grabberTransform.eulerAngles.z > 140) {
                //TODO angle bug
                //grabber.transform.eulerAngles = new Vector3(0,0,140);
                isGrabbing = false;
            }

        }
        void AnimatePlacing() {
            
            grabberTransform.Rotate(new Vector3(0, 0, -grabSpeed * Time.deltaTime));

            if (grabberTransform.eulerAngles.z < 2) {
                //grabberTransform.eulerAngles = new Vector3(0, 0, 0); //TODO angle bug
                isPlacing = false;
                myTileTransform.gameObject.GetComponent<MeshRenderer>().enabled = false; // TODO

            }
        }
        // Animate turning bot
        void AnimateTurning() {

            for (int i = 0; i < 4; i++) {
                whegsTransform[i].Rotate(new Vector3(0, 0, (i < 2 ? -1 : 1) * isTurning * whegSpeed * Time.deltaTime));
            }


            coreTransform.Rotate(new Vector3(0, isTurning * turnSpeed * Time.deltaTime, 0));


            if (Mathf.Abs(Mathf.Abs(coreTransform.eulerAngles.y) - compass[nextDirection].y) < 2) {

                currentDirection = nextDirection;
                coreTransform.eulerAngles = compass[currentDirection];
                isTurning = 0;

            }


        }
        // Animate foward walk
        void AnimateFoward() {

            for (int i = 0; i < 4; i++) {
                whegsTransform[i].Rotate(new Vector3(0, 0, -whegSpeed * Time.deltaTime));
            }

            coreTransform.Translate(new Vector3(fowardSpeed * Time.deltaTime, 0, 0));

            if(Vector3.Distance(coreTransform.position,initialPos) > 27.5f) {
                
                isMovingFoward = false;

            }

        }

    }

    public class HMIHandler {

        TermiteFSMBrain brain;
        InterfaceFSM hmi;

        public HMIHandler(TermiteFSMBrain brain, InterfaceFSM hmi) {

            this.brain = brain;
            this.hmi = hmi;

        }

        public void UpdateDisplay() {

            //Update State Display
            hmi.stateDisplay.GetComponent<Text>().text = brain.supervisorio.currentState.ToString();

            hmi.CreateStateButtons();

        }

        public void HideStateButtons() {
            hmi.DestroyStateButtons();
        }

        public void StateButtonListener(int id) {

            brain.CallIntent(id);

        }
    }
    

}
