using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DroneAnimationComponent : MonoBehaviour
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

    // Animation queue
    public List<Action> animationBuffer = new List<Action>();

    // Animation states
    public bool isGrabbing = false;
    public bool isPlacing = false;
    bool isMovingForward = false;
    int isTurning = 0;
    public bool IsAnimating { get; private set; } = false;
    bool IsAnimatingStep {
        get {
            if (isGrabbing || isPlacing || isMovingForward || isTurning != 0) {
                return true;
            } else {
                return false;
            }
        }
    }

    // Animation Temp Variables
    Vector3 initialPos;
    int nextDirection;

    // Local variables: added for independence of the component
    public Coord localPosition;
    public Coord destinyCoord;

    
    // External Commands
    //------------------

    public void Initialize(GameObject manager) {
        // Initializes the robot's animationComponent by getting the tileSystem reference.
        // Used instead of start because it must be called after first frame.


        tileSystem = manager.GetComponent<TermiteTS>();
        isPlacing = false; //fix starting with isPlacing == true;

        localPosition = new Coord(1, 1); // Starting position
        FixPosition();

    }

    public void CommandAnimation(string command) {
        // Temporary function used by external agent to call an animation

        string movementType = GetMovementType(command);

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

        localPosition += commandDict[neighbor];
        FixPosition();
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
}
