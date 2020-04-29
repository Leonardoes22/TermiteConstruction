using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TermiteBrain : MonoBehaviour
{

    bool turning, walking;
    float dAngle, currentAngle;
    float turningTime = .2f;
    float walkTime = .5f;
    float totalTime;
    Vector3 destiny, movementOrigin;

    bool hasTile = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        if (turning) {

            TurnAnimation();


        } else if (walking) {

            WalkAnimation();

        } else {

            transform.position = TileSystem.GetTileCentre(transform.position);


            Vector3 input = GetBaseInput();
            if (input == Vector3.left || input == Vector3.right) {

                dAngle = 90 * input.x;

                currentAngle = transform.eulerAngles.y;
                totalTime = 0;
                turning = true;
            } else if (input == Vector3.forward ) {

                destiny = TileSystem.GetNeighbourCentre(transform.position, transform.rotation * Vector3.forward);

                if (TileSystem.gridData.Walkable(transform.position, destiny)) {
                    totalTime = 0;
                    movementOrigin = transform.position;
                    TileSystem.gridData.Occupy(destiny);
                    walking = true;
                }

                
            } else if(input == Vector3.back) {
                
                if (hasTile) {

                    PlaceTile();
                    
                } else {
                    hasTile = true;
                    transform.Find("TermiteTile").GetComponent<MeshRenderer>().enabled = true;
                }
               
            }

        }

    }


    private void PlaceTile() {

        Vector3 target = TileSystem.GetNeighbourBase(transform.position, transform.rotation * Vector3.forward);
        Vector3 position = TileSystem.GetTileBase(transform.position);

        int[] targetCoord = TileSystem.gridData.CentreToIndex(target);
        int[] coord = TileSystem.gridData.CentreToIndex(position);

        int targetHeight = TileSystem.gridData.height[targetCoord[0], targetCoord[1]];
        int height = TileSystem.gridData.height[coord[0], coord[1]];

        if(targetHeight == height && position != target && !TileSystem.gridData.occupied[targetCoord[0], targetCoord[1]]) {
            TileSystem.PlaceTile(targetCoord[0], targetCoord[1]);

            hasTile = false;
            transform.Find("TermiteTile").GetComponent<MeshRenderer>().enabled = false;

            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>().UpdateText();
        }


        

    }

    private void WalkAnimation() {

        if(totalTime < 1) {
            totalTime += Time.deltaTime/walkTime;
            transform.LookAt(destiny);
            transform.position = Vector3.Lerp(movementOrigin, destiny, totalTime);
        } else {
            transform.position = destiny;
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
            TileSystem.gridData.Leave(movementOrigin);
            walking = false;
        }
        


    }

    private void TurnAnimation() {
        
        if(totalTime < turningTime) {
            totalTime += Time.deltaTime;
            transform.Rotate(Vector3.up, Time.deltaTime * dAngle/turningTime);
        } else {

            transform.eulerAngles = Vector3.up * (currentAngle + dAngle);
            turning = false;
        }


    }


    private Vector3 GetBaseInput() {
        Vector3 p_Velocity = new Vector3();

        if (Input.GetKeyDown(KeyCode.UpArrow)) {
            p_Velocity += new Vector3(0, 0, 1);
        }

        if (Input.GetKeyDown(KeyCode.DownArrow)) {
            p_Velocity += new Vector3(0, 0, -1);
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            p_Velocity += new Vector3(-1, 0, 0);
        }

        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            p_Velocity += new Vector3(1, 0, 0);
        }

        return p_Velocity;
    }
}
