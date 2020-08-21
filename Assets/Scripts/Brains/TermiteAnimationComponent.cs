using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TermiteAnimationComponent : MonoBehaviour
{
    // Termite Components
    public TermiteFSMBrain brain;
    public TermiteCommunicationComponent communicationComponent;
    public TermiteInterfaceComponent interfaceComponent;
    

    // External References
    public TermiteTS tileSystem;

    // Animation transforms
    public Transform coreTransform;
    public Transform myTileTransform;
    public Transform grabberTransform;
    public Transform[] whegsTransform;


    // Animation parameters
    const float fowardSpeed = 22f * 2;
    const float grabSpeed = 100f;
    const float whegSpeed = 300f;
    const float turnSpeed = 120f * 2;
    Vector3[] compass = { new Vector3(0, 270, 0), new Vector3(0, 180, 0), new Vector3(0, 90, 0), new Vector3(0, 0, 0) };

    // Modifiers
    bool fastMode {

        get {
            return brain.centralController.gameObject.GetComponent<SimManager>().isFastAnim;
        }
    }
    public bool debugMode;

    // Animation queue
    public List<Action> animationBuffer = new List<Action>();

    // Animation states
    public bool isGrabbing = false;
    public bool isPlacing = false;
    bool isMovingFoward = false;
    int isTurning = 0;
    public bool IsAnimating { get; private set; } = false;
    bool IsAnimatingStep {
        get {
            if (isGrabbing || isPlacing || isMovingFoward || isTurning != 0) {
                return true;
            } else {
                return false;
            }
        }
    }

    // Animation Variables
    float forwardTimeElapsed;
    Vector3 initialPos;
    int nextDirection;
    public int currentDirection = 3;


    /* TermiteBot Info
     * 
     * max height of mass: 7.75
     * min height of mass: 4
     * 
     */


    public void Initialize(GameObject manager) {
        tileSystem = manager.GetComponent<TermiteTS>();
        isPlacing = false; //fix starting with isPlacing == true;
    }

    void Update() {

        if (IsAnimating) {
            Animate();
        }
    }

    

    // Fix the bot position to prevent animation errors spreading
    public void FixPosition() {

        if (brain.supervisorio.currentState.InGrid) {

            // If robot in grid place in correct grid position and height

            Coord gridPos = brain.supervisorio.currentState.GetPosition();
            int pileHeight = tileSystem.heightMap[gridPos];
            float height = 4f;
            if (pileHeight > 0) {
                height += pileHeight * tileSystem.tile.Size.y - 1.75f; // Get tile pile height, reduce to walkable height, add bot center of mass height
            } 


            coreTransform.position = tileSystem.centreMap[gridPos] + new Vector3(0, height, 0); 

        } else {
            // If robot not in grid place in default outside position facing to default orientation
            coreTransform.position = new Vector3(-27.5f * 1.5f, 5f, -27.5f * 1.5f) + new Vector3(0, 0, brain.id * 27.5f);
            currentDirection = 3;
        }

        if (debugMode) { print("FixPosition: " + transform.position); }

    }

    public void StartAnimation(int eventID) {
        //OBS: Still needs refactoring

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
                
                if (brain.position.y < y) {
                    animationBuffer.Add(() => TurnTo(0));
                } else if (brain.position.y > y) {
                    animationBuffer.Add(() => TurnTo(2));
                } else if (brain.position.x < x) {
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

                if (_event.label == "out") {
                    coreTransform.position = new Vector3(-27.5f * 1.5f, 5f, -27.5f * 1.5f);
                    coreTransform.eulerAngles = compass[3];
                } else if (_event.label == "in") {
                    coreTransform.position = tileSystem.centreMap[new Coord(1, 1)];
                    coreTransform.eulerAngles = compass[3];
                }
                break;


        }

    }


    //Internal Animation Commands

    // Live Animation function
    private void Animate() {

        //Animation Logic
        if (!IsAnimatingStep) {

            //Animate next step in queue
            if (animationBuffer.Count > 0) {

                animationBuffer[0].Invoke(); // Call animation command
                animationBuffer.RemoveAt(0); // Remove animation command from queue
                

            } else {
                //OBS: Maybe the UpdateState function could be in the TermiteBotBrain
                // If no steps left: end animation and update state

                IsAnimating = false;
                communicationComponent.EndTransition();

                if (debugMode) { print("Animation Ended - Updating State"); }

            }

        }

        // Perform Animations
        if (isMovingFoward) {
            AnimateFoward();
        } else if (isGrabbing) {
            AnimateGrab();
        }else if (isPlacing) {
            AnimatePlacing();
        }else if (Math.Abs(isTurning) == 1) {
            AnimateTurning();
        }

    }


    // Animation commands
    private void GrabTile() {

        myTileTransform.gameObject.GetComponent<MeshRenderer>().enabled = true; //Show tile
        isGrabbing = true; //Start grabbing

        if (debugMode) { print("Command: GrabTile"); }
    }
    private void PlaceTile() {

        isPlacing = true; //Start placing

        if (debugMode) { print("Command: PlaceTile"); }
    }
    private void TurnRight() {

        nextDirection = currentDirection - 1 >= 0 ? (currentDirection - 1) % 4 : 3; // Calculate to the right compass direction
        isTurning = 1;

        if (debugMode) { print("Command: TurnRight"); }

    }
    private void TurnLeft() {

        nextDirection = (currentDirection + 1) % 4; // Calculate to the left compass direction
        isTurning = -1;
        
        if (debugMode) { print("Command: TurnLeft"); }
    }
    private void TurnTo(int destiny) {

        if (destiny != currentDirection) {

            //Check quickest side to turn to
            for (int i = 0; i < 2; i++) {
                nextDirection = currentDirection - 1 >= 0 ? currentDirection - 1 : 3;
                if (nextDirection == destiny) {
                    isTurning = 1;
                    break;
                }
                nextDirection = destiny;
                isTurning = -1;
            }

            if (debugMode) { print("Command: TurnTo(" + destiny + ")"); }

        } 

        

    }
    private void GoFoward() {

       
        initialPos = tileSystem.centreMap[brain.position]; //Get initial walking position to calculate walking distance
        forwardTimeElapsed = 0f;

        isMovingFoward = true;

        if (debugMode) { print("Command: GoFoward"); }
    }



    // Animate grabbing tile
    private void AnimateGrab() {

        if (!fastMode) {
            grabberTransform.Rotate(new Vector3(0, 0, grabSpeed * Time.deltaTime)); // If not in fast mode rotate grabber untill limit
        }

        if (grabberTransform.eulerAngles.z > 140
            || fastMode) {
            
            // If limit is reached or in fast mode: fix and end animation

            grabberTransform.localEulerAngles = new Vector3(0, 0, 140);
            isGrabbing = false;

            if (debugMode) { print("Animate: Finished Grabing"); }

        }

    }
    private void AnimatePlacing() {

        if (!fastMode) {
            grabberTransform.Rotate(new Vector3(0, 0, -grabSpeed * Time.deltaTime)); // If not in fast mode rotate grabber untill limit
        }

        if (grabberTransform.eulerAngles.z < 2
            || fastMode) {

            // If limit is reached or in fast mode: fix, end animation and hide tile

            grabberTransform.localEulerAngles = new Vector3(0, 0, 0);
            isPlacing = false;
            myTileTransform.gameObject.GetComponent<MeshRenderer>().enabled = false;

            if (debugMode) { print("Animate: Finished Placing"); }

        }
    }
    private void AnimateTurning() {

        //brain.manager.GetComponent<InterfaceFSM>().debug.GetComponent<Text>().text = Time.time + "-- gira gira ";

        if (!fastMode) {

            for (int i = 0; i < 4; i++) {
                whegsTransform[i].Rotate(new Vector3(0, 0, (i < 2 ? -1 : 1) * isTurning * whegSpeed * Time.deltaTime)); //Calculate which side each wheg is turning to and rotate them
            }

            coreTransform.Rotate(new Vector3(0, isTurning * turnSpeed * Time.deltaTime, 0)); // Rotate robot unitil facing correct direction

        }



        if (Mathf.Abs(Mathf.Abs(coreTransform.eulerAngles.y) - compass[nextDirection].y) < 8
            || fastMode) {

            //If rotatio is finished or in fast mode: update facing direction, fix animation, end animation

            currentDirection = nextDirection;
            coreTransform.eulerAngles = compass[currentDirection];
            isTurning = 0;

            if (debugMode) { print("Animate: Finished Turning"); }
        }


    }
    private void AnimateFoward() {
        // OBS. Not currently fixing animation, may bug 


        forwardTimeElapsed += Time.deltaTime;
        float initialHeight = tileSystem.heightMap[brain.position];
        float finalHeight = tileSystem.heightMap[communicationComponent.reservedDest];
        float heightDiff = finalHeight - initialHeight;




        if (!fastMode) {

            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, Mathf.Lerp(heightDiff * 15, 0, forwardTimeElapsed/0.6f));

            for (int i = 0; i < 4; i++) {
                whegsTransform[i].Rotate(new Vector3(0, 0, -whegSpeed * Time.deltaTime)); //Turn whegs
            }

            coreTransform.Translate(new Vector3(fowardSpeed * Time.deltaTime, 0, 0)); // Move body

        } else {

            coreTransform.Translate(new Vector3(27.5f, 0, 0)); // Teleport body
        }



        if (Vector3.Distance(coreTransform.position, initialPos) > 27.5f + (Mathf.Abs(heightDiff) * 2)) {


            //If far enought from initial position: end animation

            transform.eulerAngles = new Vector3(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);

            isMovingFoward = false;

            if (debugMode) { print("Animate: Finished Forward"); }
        }

    }
}


