using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TermiteInterfaceComponent : MonoBehaviour
{
    // External References
    public TermiteFSMBrain brain;
    public InterfaceFSM hmi;


    public bool hovering = false;
    public bool selected = false;

    bool outlined {
        get { return (hovering || selected); }
    }

    public void Initialize(GameObject manager) {
        hmi = manager.GetComponent<InterfaceFSM>();
    }

    private void Update() {
        
        CheckSelection(); //Handle bot selection mechanic

        //While transitioning keep buttons hidden
        if(selected && brain.transitionHandler.IsTransitioning) {
            hmi.DestroyStateButtons();
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

                    if (hit.collider.gameObject == brain.gameObject) {

                        selected = true;

                        //Tell interface to select self
                        hmi.SelectBot(brain);

                    } else {
                        selected = false;
                    }

                    //UpdateStateButtons();
                }

            }

            UpdateOutline();

        } else if(!selected) { //If alone always selected

            selected = true;
            hmi.SelectBot(brain);

        }



    }
    public void UpdateOutline() {

        if (outlined && !brain.isAlone) {
            brain.gameObject.GetComponent<MeshRenderer>().material.SetFloat("_FirstOutlineWidth", 0.03f);
        } else {
            brain.gameObject.GetComponent<MeshRenderer>().material.SetFloat("_FirstOutlineWidth", 0f);
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

        brain.CallIntent(brain.supervisorio.eventsConteiner[id]);

    }

    public void AutoToggleListener(bool isAutoState) {

        brain.isAuto = isAutoState;
        brain.decisionHandler.myPlan = new List<FSM.Event>();

    }
}