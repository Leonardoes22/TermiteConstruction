using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoloDroneInterface : MonoBehaviour
{

    // Basic Interface
    public GameObject canvas;
    public GameObject stateDisplay;
    public GameObject endText;
    public GameObject extraEventsZone;
    public GameObject allowedEventsZone;


    // Solo Drone
    public TermiteDroneBrain soloDrone;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        UpdateStateDisplay();

    }

    //Update the state display to the selectedBot's current state
    public void UpdateStateDisplay() {
        stateDisplay.GetComponent<Text>().text = soloDrone.supervisor.currentState.ToString();
    }

    public void CreateStateButtons() {

        int stateId = soloDrone.supervisor.currentState.id;
        int countBtn = 0;
        List<FSM.Event> feasibleEvents = soloDrone.supervisor.FeasibleEvents(soloDrone.supervisor.currentState, true);

        foreach (var e in feasibleEvents) {

            countBtn++;

            GameObject btn = (GameObject)Instantiate(Resources.Load("Button"), new Vector3(0, 200 - countBtn * 30, 0), Quaternion.identity);
            btn.transform.SetParent(extraEventsZone.transform, false);
            btn.tag = "StateButton";

            int eventId = e.id;
            string eventLabel = e.label;

            btn.GetComponentInChildren<Text>().text = eventLabel;
            //btn.GetComponent<Button>().onClick.AddListener(() => soloDrone.interfaceComponent.StateButtonListener(eventId));

        }

        // Temp code for debug feasible external events
        countBtn=0;
        foreach (var e in soloDrone.supervisor.FeasibleEvents(soloDrone.supervisor.currentState)) {



            if (!feasibleEvents.Contains(e)) {
                countBtn++;
                GameObject externalEventLabel = (GameObject)Instantiate(Resources.Load("Text"), new Vector3(0, 200 - countBtn * 30, 0), Quaternion.identity);
                externalEventLabel.transform.SetParent(allowedEventsZone.transform, false);

                externalEventLabel.GetComponent<Text>().text = e.label;
                externalEventLabel.GetComponent<Text>().fontSize = 20;
                externalEventLabel.GetComponent<Text>().color = Color.blue;
                externalEventLabel.GetComponent<Text>().alignment = TextAnchor.UpperRight;
                externalEventLabel.tag = "StateButton";
            }


        }


    }

    public void DestroyStateButtons() {

        foreach (GameObject btn in GameObject.FindGameObjectsWithTag("StateButton")) {
            Destroy(btn);
        }

    }



}
