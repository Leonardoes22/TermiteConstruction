using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotAnimationComponent : MonoBehaviour
{


    /* 
     * Animation Essential Components
     */

    // Robot Reference World Grid
    public TermiteTS tileSystem;

    // Robot Parts - Animation Transforms
    public Transform coreTransform;
    public Transform myTileTransform;
    public Transform grabberTransform;
    public Transform[] whegsTransform;


    /*
     * Animation Parameters and Constants
     */
    const float ForwardSpeed = 22f * 2;
    const float grabSpeed = 100f;
    const float whegSpeed = 300f;
    const float turnSpeed = 120f * 2;
    Vector3[] compass = { new Vector3(0, 270, 0), new Vector3(0, 180, 0), new Vector3(0, 90, 0), new Vector3(0, 0, 0) };

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
    float forwardTimeElapsed;
    Vector3 initialPos;
    int nextDirection;
    public int currentDirection = 3;

    // Local variables for independence
    public Coord localPosition = new Coord(1,1);
    public Coord destinyCoord;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (IsAnimating) {
            Animate();
        }
    }


    public void TestAnimation() {
        
        animationBuffer.Add(() => TurnTo(1));


        animationBuffer.Add(GoForward);
        IsAnimating = true;
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
                //communicationComponent.EndTransition(); non&

                if (debugMode) { print("Animation Ended - Updating State"); }

            }

        }

        // Perform Animations
        if (isMovingForward) {
            AnimateForward();
        } else if (isGrabbing) {
            AnimateGrab();
        } else if (isPlacing) {
            AnimatePlacing();
        } else if (Math.Abs(isTurning) == 1) {
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
    private void GoForward() {


        //initialPos = tileSystem.centreMap[brain.position]; //Get initial walking position to calculate walking distance
        initialPos = tileSystem.centreMap[localPosition]; //Get initial walking position to calculate walking distance

        switch (currentDirection) {

            case 0:
                destinyCoord = localPosition;
                break;

            default:
                break;
        }

        destinyCoord = localPosition + new Coord(1, 0);

        forwardTimeElapsed = 0f;

        isMovingForward = true;

        if (debugMode) { print("Command: GoForward"); }
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
    private void AnimateForward() {
        // OBS. Not currently fixing animation, may bug 


        forwardTimeElapsed += Time.deltaTime;
        //float initialHeight = tileSystem.heightMap[brain.position];
        float initialHeight = tileSystem.heightMap[localPosition];
        //float finalHeight = tileSystem.heightMap[communicationComponent.reservedDest];
        float finalHeight = tileSystem.heightMap[destinyCoord];
        float heightDiff = finalHeight - initialHeight;




        if (!fastMode) {

            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, Mathf.Lerp(heightDiff * 15, 0, forwardTimeElapsed / 0.6f));

            for (int i = 0; i < 4; i++) {
                whegsTransform[i].Rotate(new Vector3(0, 0, -whegSpeed * Time.deltaTime)); //Turn whegs
            }

            coreTransform.Translate(new Vector3(ForwardSpeed * Time.deltaTime, 0, 0)); // Move body

        } else {

            coreTransform.Translate(new Vector3(27.5f, 0, 0)); // Teleport body
        }



        if (Vector3.Distance(coreTransform.position, initialPos) > 27.5f + (Mathf.Abs(heightDiff) * 2)) {


            //If far enought from initial position: end animation

            transform.eulerAngles = new Vector3(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);

            isMovingForward = false;

            if (debugMode) { print("Animate: Finished Forward"); }
        }

    }

}
