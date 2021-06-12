using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using Button = UnityEngine.UI.Button;

public class ManualUIController : MonoBehaviour
{

    // Selected Bot
    TermiteDroneBrain selectedBotBrain;

    // UI Elements
    public GameObject background;
    public GameObject movementButtons;
    public GameObject extraEventsView;
    public GameObject allowedEventsView;

    // 
    private string[] movementEvents = { "u", "d", "l", "r", "ne", "nw", "se", "sw", "getBrick" };

    void Start()
    {
        SimEvents.current.onBotSelected += UpdateManualMode;
        SimEvents.current.onAutoToggle += UpdateManualMode;

        MovementButtonAddListener();
        
    }

    private void OnDisable() {
        SimEvents.current.onBotSelected -= UpdateManualMode;
        SimEvents.current.onAutoToggle -= UpdateManualMode;
    }

    private void SetVisibility(bool value) {

        background.SetActive(value);
        movementButtons.SetActive(value);
        extraEventsView.SetActive(value);
        allowedEventsView.SetActive(value);


        if (value) {
            UpdateEventButtons();
        }

    }
    private void UpdateManualMode(GameObject selected) {

        var newBrain = selected.GetComponent<TermiteDroneBrain>();

        if (selectedBotBrain != newBrain) {

            selectedBotBrain = newBrain;

            SetVisibility(!selectedBotBrain.isAuto);

        }

    }
    private void UpdateManualMode(TermiteDroneBrain selected, bool toggleValue) {

         SetVisibility(!toggleValue);

    }

    // Update is called once per frame
    void Update() {

    }

    private void MovementButtonAddListener() {

        for (int i = 0; i < movementButtons.transform.childCount; i++) {


            GameObject btnObj = movementButtons.transform.GetChild(i).gameObject;
            string eventLabel = btnObj.GetComponentInChildren<TextMeshProUGUI>().text;
            
            //int eventId = selectedBotBrain.supervisor.eventsConteiner.FirstOrDefault(x => x.Value.label == eventLabel).Key;
            var btn = btnObj.GetComponent<UnityEngine.UI.Button>();
            btn.onClick.AddListener(() => {
                EventButtonListener(selectedBotBrain.supervisor.eventsConteiner.FirstOrDefault(x => x.Value.label == eventLabel).Key); });

        }

    }

    private void CreateEventButtons() {

        FSM selectedBotFSM = selectedBotBrain.supervisor;

        int stateId = selectedBotFSM.currentState.id;
        List<FSM.Event> feasibleEvents = selectedBotFSM.FeasibleEvents(selectedBotFSM.currentState, true);
        int countBtn = 0;

        foreach (var e in feasibleEvents) {

            if (!movementEvents.Contains(e.label)) {
                countBtn++;

                GameObject btn = (GameObject)Instantiate(Resources.Load("Button"), new Vector3(0, 200 - countBtn * 30, 0), Quaternion.identity);
                btn.transform.SetParent(extraEventsView.transform.GetChild(0).GetChild(0), false);
                btn.tag = "StateButton";

                int eventId = e.id;
                string eventLabel = e.label;

                btn.GetComponentInChildren<Text>().text = eventLabel;
                btn.GetComponent<Button>().onClick.AddListener(() => EventButtonListener(eventId));

            }



        }

        countBtn = 0;
        foreach (var e in selectedBotFSM.FeasibleEvents()) {

            if (!feasibleEvents.Contains(e)) {
                countBtn++;
                GameObject externalEventLabel = (GameObject)Instantiate(Resources.Load("Text"), new Vector3(0, 200 - countBtn * 30, 0), Quaternion.identity);
                externalEventLabel.transform.SetParent(allowedEventsView.transform.GetChild(0).GetChild(0), false);

                externalEventLabel.GetComponent<Text>().text = e.label;
                externalEventLabel.GetComponent<Text>().fontSize = 20;
                externalEventLabel.GetComponent<Text>().color = Color.blue;
                externalEventLabel.GetComponent<Text>().alignment = TextAnchor.UpperRight;
                externalEventLabel.tag = "StateButton";
            }


        }

    }
    private void DestroyEventButtons() {

        foreach (GameObject btn in GameObject.FindGameObjectsWithTag("StateButton")) {
            Destroy(btn);
        }

    }

    private void UpdateEventButtons() {
        DestroyEventButtons();
        CreateEventButtons();
    }

    public void EventButtonListener(int eventId) {

        selectedBotBrain.ProcessIntent(eventId);

    }

}
