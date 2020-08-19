using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TermiteInterfaceComponent : MonoBehaviour
{
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
        CheckSelection();
    }
    //Mouso Collision Functions
    private void OnMouseEnter() {

        hovering = true;

    }

    private void OnMouseExit() {

        hovering = false;

    }

    public void End() {
        hmi.endText.SetActive(true);
    }

    public void UpdateOutline() {

        if (outlined && !brain.isAlone) {
            brain.gameObject.GetComponent<MeshRenderer>().material.SetFloat("_FirstOutlineWidth", 0.03f);
        } else {
            brain.gameObject.GetComponent<MeshRenderer>().material.SetFloat("_FirstOutlineWidth", 0f);
        }

    }

    public void UpdateStateDisplay() {

        if (selected || brain.isAlone) {
            hmi.autoToggle.GetComponent<UnityEngine.UI.Toggle>().isOn = brain.isAuto;

        }



    }

    public void UpdateStateButtons() {

        if (selected) {
            if (!brain.isAuto) {

                hmi.DestroyStateButtons();

                hmi.CreateStateButtons();

            } else {
                HideStateButtons();
            }
        }



    }

    public void HideStateButtons() {
        hmi.DestroyStateButtons();
    }

    public void UpdateDisplay() {

        UpdateStateDisplay();

        UpdateStateButtons();


    }


    public void CheckSelection() {


        if (!brain.isAlone) { //If multibot check selection

            if (Input.GetMouseButtonDown(0)) {
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit)) {

                    if (hit.collider.gameObject == brain.gameObject) {

                        selected = true;

                        //Tell interface to select self
                        hmi.selectedBotBrain = brain;

                    } else {
                        selected = false;
                    }

                    UpdateDisplay();
                }

            }

            UpdateOutline();

        } else { //If alone always selected

            selected = true;
            hmi.selectedBotBrain = brain;

        }



    }


    //Communication Methods
    public void StateButtonListener(int id) {

        brain.CallIntent(brain.supervisorio.eventsConteiner[id]);

    }

    public void AutoToggleListener(bool isAutoState) {

        brain.isAuto = isAutoState;
        brain.decisionHandler.myPlan = new List<FSM.Event>();
        UpdateStateButtons();

    }
}