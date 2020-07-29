using UnityEngine;

public class CameraController : MonoBehaviour
{
    //Parameters
    public float mainSpeed = 300.0f; //Regular Speed
    public float scrollSpeed = 500.0f; //Zoom speed
    public float camSens = 0.25f; //Mouse sensitivity
    
    //Vars
    private Vector3 lastMouse = new Vector3(255,255,255); //Kinda in the middle of the screen

    // Update is called once per frame
    void Update() {
        
        //Angle control with mouse 
        lastMouse = Input.mousePosition - lastMouse;
        if (Input.GetMouseButton(0)) {
            print(lastMouse);
            lastMouse = new Vector3(-lastMouse.y * camSens, lastMouse.x * camSens, 0);
            lastMouse = new Vector3(transform.eulerAngles.x + lastMouse.x, transform.eulerAngles.y + lastMouse.y, 0);
            transform.eulerAngles = lastMouse;
        }
        lastMouse = Input.mousePosition;

        //Movement Control
        //Get WASD and Scroll Input
        Vector3 p = GetBaseInput(); 
        Vector3 z = GetScrollInput(); 

        //Apply Speed
        p = p * mainSpeed * Time.deltaTime; //WASD Speed
        z = z * scrollSpeed * Time.deltaTime; //ScrollSpeed

        //Move camera locking WASD Movement to the xz plane
        this.transform.Translate(z); //Scroll Movement
        float lockedY = transform.position.y; //Lock y position
        this.transform.Translate(p); // WASD Movement
        transform.position = new Vector3(transform.position.x, lockedY, transform.position.z); //Fix y position
        
    }


    private Vector3 GetBaseInput() {
        Vector3 baseInput = new Vector3();

        if (Input.GetKey(KeyCode.W)){
            baseInput += new Vector3(0, 0, 1);
        }

        if (Input.GetKey(KeyCode.S)){
            baseInput += new Vector3(0, 0, -1);
        }

        if (Input.GetKey(KeyCode.A)){
            baseInput += new Vector3(-1, 0, 0);
        }

        if (Input.GetKey(KeyCode.D)){
            baseInput += new Vector3(1, 0, 0);
        }

        return baseInput;
    }

    private Vector3 GetScrollInput() {

        Vector3 scrollInput = new Vector3();

        if (Input.GetAxis("Mouse ScrollWheel") < 0) {
            scrollInput = new Vector3(0, 0, -1);
        } else if (Input.GetAxis("Mouse ScrollWheel") > 0) {
            scrollInput = new Vector3(0, 0, 1);
        }

        return scrollInput;

    }

}
