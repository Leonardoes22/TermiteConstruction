using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    // Local variables for independence
    public Coord localPosition;
    public Coord destinyCoord;

    

    public void Initialize(TermiteTS ts) {
        tileSystem = ts;
        isPlacing = false; //fix starting with isPlacing == true;

        localPosition = new Coord(1, 1);
        FixPosition();

    }

    void FixPosition() {

        coreTransform.position = tileSystem.centreMap[localPosition];
        coreTransform.position += new Vector3(0,40f,0);

        print(localPosition + "-" + tileSystem.centreMap[localPosition]);
        print(coreTransform.position);

    }

    public void JumpToNeighbor(string neighbor) {

        localPosition += commandDict[neighbor];
        FixPosition();

    }

    public void CommandAnimation(string command) {

        switch (command) {

            case "get":
                myTileTransform.gameObject.GetComponent<MeshRenderer>().enabled = true;
                break;

            case "put":
                myTileTransform.gameObject.GetComponent<MeshRenderer>().enabled = false;
                break;

            default:
                JumpToNeighbor(command);
                break;
        }


    }


    // Start is called before the first frame update
    void Start()
    {

        foreach (var item in commandDict) {
            print(item);
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
