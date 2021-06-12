using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotSelector : MonoBehaviour
{

    void Update()
    {
        // Checks for mouse click on gameobjects
        if (Input.GetMouseButtonDown(0)) {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit)) {

                // Checks if hit current gameobject
                if (hit.collider.gameObject == gameObject) {

                    // Broadcast bot selected event
                    SimEvents.current.BotSelected(this.gameObject);

                }

            }

        }

    }
}
