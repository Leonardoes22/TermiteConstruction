using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class TermiteInterfaceComponent : MonoBehaviour
{
    // Termite Components
    public TermiteFSMBrain brain;
    public TermiteAnimationComponent animationComponent;
    public TermiteCommunicationComponent communicationComponent;
    

    // External References
    public InterfaceFSM hmi;


    // Interface and Selection States
    public bool hovering = false;
    public bool selected { get{ return hmi.selectedBotBrain == brain; } }
    bool outlined {
        get { return (hovering || selected); }
    }

    public void Initialize(GameObject manager) {
        hmi = manager.GetComponent<InterfaceFSM>();
    }

    // Passive Behaviour
    private void Update() {
        
        CheckSelection(); //Handle bot selection mechanic

        //While transitioning keep buttons hidden
        if(selected && communicationComponent.IsTransitioning) {
            hmi.DestroyStateButtons();
        }

        if (brain.supervisorio.currentState.marked) {
            End();
        }

    }


    //Mouse hovering handlers
    private void OnMouseEnter() {
        hovering = true;
    }
    private void OnMouseExit() {
        hovering = false;
    }


    //Check which bot is selected; activate or hide outline; update state buttons on selection change
    public void CheckSelection() {


        if (!brain.isAlone) { //If multibot check selection

            if (Input.GetMouseButtonDown(0)) {
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit)) {

                    if (hit.collider.gameObject == gameObject) {

                        //Tell interface to select self
                        hmi.SelectBot(brain);

                    } 

                }

            }

            UpdateOutline();

        } else if(!selected) { //If alone always selected

            hmi.SelectBot(brain);

        }



    }
    public void UpdateOutline() {

        if (outlined && !brain.isAlone) {
            gameObject.GetComponent<MeshRenderer>().material.SetFloat("_FirstOutlineWidth", 0.03f);
        } else {
            gameObject.GetComponent<MeshRenderer>().material.SetFloat("_FirstOutlineWidth", 0f);
        }

    }

    // InterfaceFSM communication functions
    public void End() {
        hmi.endText.SetActive(true);
    }
    public void UpdateStateButtons() {

        if (selected) {

            hmi.DestroyStateButtons();

            if (!brain.isAuto) {

                hmi.CreateStateButtons();

            } 
        }

    }


    //Communication Methods
    public void StateButtonListener(int id) {

        brain.ProcessIntent(brain.supervisorio.eventsConteiner[id]);

    }

    public void AutoToggleListener(bool isAutoState) {

        brain.isAuto = isAutoState;
        brain.myPlan = new List<FSM.Event>();

    }
}