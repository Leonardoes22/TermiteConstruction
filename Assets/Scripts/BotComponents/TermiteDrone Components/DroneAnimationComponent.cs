using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DroneAnimationComponent : AnimationComponent
{

    /* 
     * Animation Essential Components
     */

    // Robot Reference World Grid
    public TermiteTS tileSystem;

    // Robot Parts - Animation Transforms
    public Transform coreTransform;
    public Transform myTileTransform;


    /*
     * Animation Parameters and Constants
     */
    Vector3[] compass = { new Vector3(0, 270, 0), new Vector3(0, 180, 0), new Vector3(0, 90, 0), new Vector3(0, 0, 0) };
    // Command Dictionnary
    Dictionary<string, Coord> commandDict = new Dictionary<string, Coord>() {
        {"d", new Coord(1,0) },
        {"u", new Coord(-1,0) },
        {"l", new Coord(0,-1) },
        {"r", new Coord(0,1) },
        {"se", new Coord(1,1) },
        {"ne", new Coord(-1,1) },
        {"nw", new Coord(-1,-1) },
        {"sw", new Coord(1,-1) }

    };

    /*
     * Animation Variables
     */

    // Modifiers
    public bool fastMode;
    public bool debugMode;
    public float movingSpeed = 60f;

    // Animation queue
    public List<Action> animationBuffer = new List<Action>();

    // Animation states
    public bool isGrabbing = false;
    public bool isPlacing = false;
    bool isMovingInGrid = false;
    int isTurning = 0;
    public bool IsAnimating { get; private set; } = false;
    bool IsAnimatingStep {
        get {
            if (isGrabbing || isPlacing || isMovingInGrid || isTurning != 0) {
                return true;
            } else {
                return false;
            }
        }
    }

    // Animation Temp Variables
    Vector3 initialPos;
    Vector3 nextPos;

    // Local variables: added for independence of the component
    public Coord localPosition;
    public Coord destinyCoord;

    // Update Routine

    void Update() {

        if (isMovingInGrid) {
            AnimateMoveInGrid();
        }
    }


    // External Commands
    //------------------

    override
    public void Initialize(GameObject manager) {
        // Initializes the robot's animationComponent by getting the tileSystem reference.
        // Used instead of start because it must be called after first frame.


        tileSystem = manager.GetComponent<TermiteTS>();
        isPlacing = false; //fix starting with isPlacing == true;

        localPosition = new Coord(0, 0); // Starting position
        FixPosition();

    }

    override
    public IEnumerator CallAnimation(string command, Action onComplete) {
        // Temporary function used by external agent to call an animation

        string movementType = GetMovementType(command);

        // Setup animation based on animation type
        switch (movementType) {

            case "typeGet":
                GrabTile();
                break;

            case "typePlace":
                PlaceTile();
                break;

            case "typeMovement":
                MoveInGrid(command);
                break;

            case "typeMovementIn":
                EnterGrid(command);
                break;

            case "typeMovementOut":
                ExitGrid();
                break;


            default:
                if (debugMode) { print($"Movement {command} of type {movementType} not implemented"); }
                break;
        }

        // Wait for animation to finish to warn who called it
        while (IsAnimatingStep) {
            yield return null;
        }


        onComplete?.Invoke();

    }


    // Internal Animation Commands
    //--------------------------

    private string GetMovementType(string eventLabel) {

        string[] movementEvents = { "u", "d", "l", "r", "ne", "nw", "se", "sw" };

        if (movementEvents.Contains(eventLabel)) {
            return "typeMovement";
        } else if (eventLabel.StartsWith("in")) {
            return "typeMovementIn";
        } else if (eventLabel.StartsWith("out")) {
            return "typeMovementOut";
        } else if (eventLabel == "getBrick") {
            return "typeGet";
        } else if (eventLabel[0] == 'a') {
            if (eventLabel.EndsWith("_r2")) {      
                return "typePlaceR2";
            } else {
                return "typePlace";
            }
        } else {
            return "typeOther";
        }
    }

    private void FixPosition() {
        // Fix the robots spacial position based on localPosition
        // in case that any animation is imprecise

        // Checks if in grid or outside
        Vector3 correctPosition = tileSystem.InGrid(localPosition) ? tileSystem.centreMap[localPosition] : new Vector3(-27.5f * 1.5f, 5f, -27.5f * 1.5f);

        // Corrects robot position
        coreTransform.position = correctPosition; // In 2D
        coreTransform.position += new Vector3(0,40f,0); // Fix Height

        if (debugMode) {
            print(localPosition + "-" + correctPosition);
            print(coreTransform.position);
        }
        

    }



    // Animation Commands

    private void MoveInGrid(string neighbor) {

        destinyCoord = localPosition + commandDict[neighbor];

        initialPos = tileSystem.centreMap[localPosition];
        nextPos = tileSystem.centreMap[destinyCoord];

        isMovingInGrid = true;

        if (debugMode) { print("Command: MoveInGrid to "+ neighbor); }

    }

    private void EnterGrid(string inCommand) {

        string inStr = inCommand.Substring(2);
        Coord entryPosition = new Coord(int.Parse(inStr[0].ToString()), int.Parse(inStr[1].ToString()));

        localPosition = entryPosition;
        FixPosition();

    }

    private void ExitGrid() {

        localPosition = Coord.origin;
        print(Coord.origin);
        FixPosition();

    }

    private void PlaceTile() {
        myTileTransform.gameObject.GetComponent<MeshRenderer>().enabled = false;
        FixPosition();
    }

    private void GrabTile() {
        myTileTransform.gameObject.GetComponent<MeshRenderer>().enabled = true;
        FixPosition();
    }


    // Animation Routines

    private void AnimateMoveInGrid() {

        // Step Init
        Vector3 currentPos = coreTransform.position;
        currentPos = Vector3.ProjectOnPlane(currentPos, Vector3.up); // Get 2D position
        Vector3 movingDirection = (nextPos - currentPos).normalized;

        // Animation 
        coreTransform.Translate(movingDirection * Time.deltaTime*movingSpeed);

        // End Animation Condition
        bool endCondition = Vector3.Distance(currentPos, nextPos) <= 1f;
        if (endCondition) {

            localPosition = destinyCoord;
            isMovingInGrid = false;
            FixPosition();
        }

    }



}
