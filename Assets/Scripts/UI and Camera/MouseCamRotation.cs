using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UIElements;

public class MouseCamRotation : MonoBehaviour
{
    public GameObject centerObject;
    public bool active = false;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (active) { 
            if (Input.GetMouseButton(0)) {

                //Turn Sideways
                Quaternion camTurnAngle = Quaternion.AngleAxis(Input.GetAxis("Mouse X") * 5.0f, Vector3.up);
                transform.position = centerObject.transform.position + camTurnAngle * (transform.position - centerObject.transform.position);

                // Calculate Up-Down Axis
                Vector3 yAxe = Vector3.ProjectOnPlane(transform.position, Vector3.up);
                yAxe = Quaternion.AngleAxis(90, Vector3.up) * yAxe;

                //Rotate Up-Down if in limit
                camTurnAngle = Quaternion.AngleAxis(Input.GetAxis("Mouse Y") * 5.0f, yAxe);
                Vector3 newPos = centerObject.transform.position + camTurnAngle * (transform.position - centerObject.transform.position);

                if (Vector3.Project(newPos, Vector3.up).magnitude < transform.position.magnitude * 0.9) {
                   
                }
                transform.position = newPos;


            }

            if (Input.mouseScrollDelta.magnitude > 0 ) {

                transform.position = (1 - Input.GetAxis("Mouse ScrollWheel")) * transform.position;
            
            }

            //Face center 
            transform.LookAt(centerObject.transform);

        }
    }

}
