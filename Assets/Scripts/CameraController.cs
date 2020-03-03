using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    

    public float mainSpeed = 30.0f; //Regular Speed
    public float scrollSpeed = 200.0f; //Zoom speed
    float shiftAdd = 75.0f; //Amount to accelerate when shift is pressed
    float maxShift = 300.0f; //Maximum speed when holding shift
    public float camSens = 0.25f; //Mouse sensitivity
    
    private Vector3 lastMouse = new Vector3(255,255,255); //Kinda in the middle of the screen
    private float totalRun = 1.0f;

    private bool topView = false;
    public GameObject text;

    // Update is called once per frame
    void Update()
    {
        if (!topView) {
            //Angle control with mouse 
            lastMouse = Input.mousePosition - lastMouse;
            if (Input.GetMouseButton(2)) {
                lastMouse = new Vector3(-lastMouse.y * camSens, lastMouse.x * camSens, 0);
                lastMouse = new Vector3(transform.eulerAngles.x + lastMouse.x, transform.eulerAngles.y + lastMouse.y, 0);
                transform.eulerAngles = lastMouse;
            }
            lastMouse = Input.mousePosition;

            //Keyboard commands
            Vector3 p = GetBaseInput(); //WASD Input
            Vector3 z = new Vector3(); //Scroll input

            //Get Scroll
            if (Input.GetAxis("Mouse ScrollWheel") < 0) {
                z = new Vector3(0, 0, -1);
            } else if (Input.GetAxis("Mouse ScrollWheel") > 0) {
                z = new Vector3(0, 0, 1);
            }

            //WASD Speed
            if (Input.GetKey(KeyCode.LeftShift)) {
                totalRun += Time.deltaTime;
                p = p * totalRun * shiftAdd;
                z = new Vector3(0, 0, 0);
                p.x = Mathf.Clamp(p.x, -maxShift, maxShift);
                p.y = Mathf.Clamp(p.y, -maxShift, maxShift);
                p.z = Mathf.Clamp(p.z, -maxShift, maxShift);
            } else {
                totalRun += Mathf.Clamp(totalRun * 0.5f, 1f, 1000f);
                p = p * mainSpeed;
                z = z * scrollSpeed;
            }

            p = p * Time.deltaTime;
            z = z * Time.deltaTime;

            Vector3 newPosition = this.transform.position;
            //Actual movement 
            this.transform.Translate(z);
            newPosition.y = transform.position.y;
            this.transform.Translate(p);
            newPosition.x = transform.position.x;
            newPosition.z = transform.position.z;
            transform.position = newPosition;

            if (Input.GetKeyDown(KeyCode.Space)) {
                topView = true;
                transform.GetComponent<Camera>().orthographic = true; 

                

                Vector3 size = TileSystem.size;
                int width = TileSystem.width;
                int length = TileSystem.length;

                float x = (size.x + length) / 2;
                float zed = (size.z + width) / 2;
                float y = (width * size.z * 1.1f);

                transform.position = new Vector3(x, y, zed);
                transform.eulerAngles = new Vector3(90, 0, 0);

                UpdateText();

            }

        } else {

            if (Input.GetKeyDown(KeyCode.Space)) {
                foreach (GameObject item in GameObject.FindGameObjectsWithTag("Text")) {
                    if(item.name != "TextBase") {
                        Destroy(item);
                    }
                    
                }
  
                topView = false;
                transform.GetComponent<Camera>().orthographic = false;
            }

        }
        
       
    }


    public void UpdateText() {

        if (topView) {
            foreach (GameObject item in GameObject.FindGameObjectsWithTag("Text")) {
                if (item.name != "TextBase") {
                    Destroy(item);
                }

            }

            for (int i = 0; i < TileSystem.length; i++) {
                for (int j = 0; j < TileSystem.width; j++) {
                    GameObject height = Instantiate(text, TileSystem.gridData.tileCentres[i, j], Quaternion.identity);
                    height.GetComponentInChildren<TextMesh>().text = TileSystem.gridData.height[i, j].ToString();

                }
            }
        }

    }

    private Vector3 GetBaseInput() {
        Vector3 p_Velocity = new Vector3();

        if (Input.GetKey(KeyCode.W)){
            p_Velocity += new Vector3(0, 0, 1);
        }

        if (Input.GetKey(KeyCode.S)){
            p_Velocity += new Vector3(0, 0, -1);
        }

        if (Input.GetKey(KeyCode.A)){
            p_Velocity += new Vector3(-1, 0, 0);
        }

        if (Input.GetKey(KeyCode.D)){
            p_Velocity += new Vector3(1, 0, 0);
        }

        return p_Velocity;
    }

}
