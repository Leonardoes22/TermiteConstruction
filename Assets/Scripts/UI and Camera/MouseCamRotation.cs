using UnityEngine;


public class MouseCamRotation : MonoBehaviour
{
    //Vars
    private Vector3 rotationCenter;
    private bool active = false;

    //Parameters
    private float minDistance = 50f;
    private float scrollSpeed = 250f;
    private float rotationSpeed = 5f;
    private float knockbackSpeed = 0.5f;
    private float angleTopLimit = 10f;
    private float angleBottomLimit = 90f;

    // Update is called once per frame
    void Update()
    {
        if (active) { 

            //Handle Rotation
            if (Input.GetMouseButton(0)) {
                
                //Turn Sideways
                Quaternion camTurnAngle = Quaternion.AngleAxis(Input.GetAxis("Mouse X") * rotationSpeed, Vector3.up);
                transform.position = rotationCenter + camTurnAngle * (transform.position - rotationCenter);
                
                // Calculate Up-Down Axis
                Vector3 yAxe = Vector3.ProjectOnPlane(transform.position - rotationCenter, Vector3.up);
                yAxe = Quaternion.AngleAxis(90, Vector3.up) * yAxe;
                yAxe = yAxe == Vector3.zero ? Vector3.left : yAxe; //Make sure yAxe is always non zero to avoid camera locking

                //Rotate Up-Down 
                camTurnAngle = Quaternion.AngleAxis(Input.GetAxis("Mouse Y") * rotationSpeed, yAxe);
                Vector3 newPos = rotationCenter + camTurnAngle * (transform.position - rotationCenter);

                //Limit Up-Down movement to avoid graphical glitch
                float angleY = Vector3.Angle(Vector3.up, transform.position - rotationCenter);
                if ((angleY < angleTopLimit || angleY > angleBottomLimit) && Mathf.Abs(newPos.y) > Mathf.Abs(transform.position.y) ) {
                   
                   newPos = transform.position;
                    
                }
                
                //Update position
                transform.position = newPos;
                
            }


            // Scroll in and out and define minimum distance
            if (Vector3.Distance(rotationCenter, transform.position) > minDistance || Input.GetAxis("Mouse ScrollWheel") < 0) {
                transform.Translate(new Vector3(0, 0, Input.GetAxis("Mouse ScrollWheel") * scrollSpeed));
            } else {
                transform.Translate(new Vector3(0, 0, -knockbackSpeed));
            }

            //Always face center 
            transform.LookAt(rotationCenter);

        }
    }

    public void SetRotationCenter(Vector3 rotCenter) {
        rotationCenter = rotCenter;
    }


    public void SetActive(bool value) {
        active = value;
    }

}
