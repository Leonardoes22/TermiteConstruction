using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TreeEditor;
using UnityEngine;
using UnityEngine.UI;

public class TermiteFSMBrain : MonoBehaviour {

    Coord position;
    public FSM supervisorio;
    public GameObject master;


    bool hasTile { get { return supervisorio != null ? supervisorio.currentState.hasTile : false; } }


    // Animation states and variables
    public bool isGrabbing = false;
    public bool isPlacing = false;
    
    float grabSpeed = 100f;
    bool isWaiting = true;

    float whegSpeed = 300f;
    int isTurning = 0;
    float turnSpeed = 150f;
    int nextDirection;
    int currentDirection = 3 ;
    Vector3[] compass = {new Vector3(0, 270, 0), new Vector3(0, 180, 0), new Vector3(0, 90, 0), new Vector3(0, 0, 0) };


    
    // Bot Children (Must be loaded)
    GameObject myTile; 
    GameObject grabber;
    GameObject[] whegs;

    // Start is called before the first frame update
    void Start()
    {
        grabber = transform.Find("Grabber").gameObject;
        myTile = grabber.transform.Find("GrabberArm/TermiteTile").gameObject;

        whegs = new GameObject[4];
        for( int i = 0; i < 4; i++) {
            whegs[i] = transform.Find("Wheg" + i).gameObject;
        }

        myTile.transform.localPosition = new Vector3(0.1633892f, -0.0009015298f,- 0.01356555f);
    }

    // Update is called once per frame
    void Update()
    {
        print(currentDirection);


        // Error handling
        if(supervisorio == null) {
            print("Error: supervisor not loaded yet");
        }
        if (myTile == null) {
            print("Error: missing internal tile reference");
        }
        if (myTile == null) {
            print("Error: missing grabber reference");
        }


        // Animation handling
        if (isGrabbing ) {
            AnimateGrab();
        }
        if (isPlacing) {
            AnimatePlacing();
        }
        if (Math.Abs(isTurning) == 1) {
            AnimateTurning();
        }

    }

    public void Initialize(string automaton) {

        supervisorio = new FSM(automaton);
        FastPositionUpdate();

    }
   
    public void StateButtonListener(int id) {
        supervisorio.CallEvent(id);
        StateUpdate();
    }
   
   void StateUpdate() {

        //Update State Display
        master.GetComponent<InterfaceFSM>().stateDisplay.GetComponent<Text>().text = supervisorio.currentState.ToString();

        //Update State Buttons
        master.GetComponent<InterfaceFSM>().DestroyStateButtons();
        master.GetComponent<InterfaceFSM>().CreateStateButtons();

        //Update Tile Config
        master.GetComponent<TermiteTS>().UpdateMap(supervisorio.currentState.heightMap);

        //Update Termite Position
        FastPositionUpdate();

        //Update Tile Carrying
        myTile.GetComponent<MeshRenderer>().enabled = hasTile;
        
    }

    void FastPositionUpdate() {

        position = supervisorio.currentState.GetPosition();

        if ((position.x + position.x) == 0) {

            transform.position = new Vector3(-10, 0,-10);

        } else {

            transform.position = master.GetComponent<TermiteTS>().NextTilePosition(position);
        }
    }











    // Animation commands
    public void GrabTile() {
        if (!hasTile && isWaiting) {
            myTile.GetComponent<MeshRenderer>().enabled = true;
            isGrabbing = true;
            isWaiting = false;
        } else if(isWaiting) {
            print("Error: tried to grab excessive tiles");
        } else {
            print("Already doing something else");
        }
    }

    public void PlaceTile() {
        if (!hasTile && isWaiting) {
            isPlacing = true;
            isWaiting = false;
        } else if (isWaiting) {
            print("Error: there is no tiles to place");
        } else {
            print("Already doing something else");
        }
    }

    public void TurnRight() {
        if(isTurning == 0 && isWaiting) {
            nextDirection = currentDirection - 1 >= 0 ? (currentDirection - 1) % 4 : 3;
            isTurning = 1;
            isWaiting = false;
        } else if (isTurning != 0) {
            print("Already turning to the " + (isTurning == 1 ? "right" : "left"));
        } else {
            print("Already doing something else");
        }

    }
    public void TurnLeft() {
        if (isTurning == 0 && isWaiting) {
            nextDirection = (currentDirection + 1) % 4;
            isTurning = -1;
            isWaiting = false;
        } else if (isTurning != 0) {
            print("Already turning to the " + (isTurning == 1 ? "right" : "left"));
        } else {
            print("Already doing something else");
        }
    }

    public void TurnTo(int destiny) {
        if(destiny == currentDirection) {
            print("Already there");
        } else if (isTurning == 0 && isWaiting) {
            for (int i = 0; i < 2; i++) {
                nextDirection = currentDirection - 1 >= 0 ? currentDirection - 1 : 3;
                if (nextDirection == destiny) {
                    isTurning = 1;
                    break;
                }
                nextDirection = destiny;
                isTurning = -1;
            }
            isWaiting = false;
        } else if (isTurning != 0) {
            print("Already turning to the " + (isTurning == 1 ? "right" : "left"));
        } else {
            print("Already doing something else");
        }
    }

    // Animations

    // Animate grabbing tile
    void AnimateGrab() {

        grabber.transform.Rotate(new Vector3(0, 0, grabSpeed * Time.deltaTime));

        if(grabber.transform.eulerAngles.z > 140) {
            grabber.transform.eulerAngles = new Vector3(0,0,140);
            isGrabbing = false;
            isWaiting = true;
        }

    }

    void AnimatePlacing() {
        grabber.transform.Rotate(new Vector3(0, 0, -grabSpeed * Time.deltaTime));

        if (grabber.transform.eulerAngles.z < 2) {
            grabber.transform.eulerAngles = new Vector3(0, 0, 0);
            isPlacing = false;
            isWaiting = true;
            myTile.GetComponent<MeshRenderer>().enabled = false;

        }
    }

    // Animate turning bot
    void AnimateTurning() {

        for (int i = 0; i < 4; i++) {
            whegs[i].transform.Rotate(new Vector3(0, 0, (i < 2 ? -1 : 1) * isTurning * whegSpeed * Time.deltaTime));
        }


        transform.Rotate(new Vector3(0, isTurning * turnSpeed * Time.deltaTime, 0));


        if (Mathf.Abs(Mathf.Abs(transform.eulerAngles.y) - compass[nextDirection].y) < 2) {

            currentDirection = nextDirection;
            transform.eulerAngles = compass[currentDirection];
            isTurning = 0;
            isWaiting = true;

        }


    }
    
}
