using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InterfaceFSM : MonoBehaviour {

    // Interface
    public GameObject canvas;
    public GameObject stateDisplay;
    public GameObject autoToggle;
    public GameObject endText;
    public GameObject exitButton;
    public GameObject fastAnimToggle;

    public GameObject disbandButton;
    public GameObject addBotButton;
    
    

    //public GameObject debug;


    public TermiteFSMBrain selectedBotBrain;


    // Update is called once per frame
    void Update() {

        disbandButton.SetActive(gameObject.GetComponent<CentralController>().botList.Count > 1);
        UpdateStateDisplay();
    }

    
    public void Initialize() {

        //Set first selected bot
        selectedBotBrain = gameObject.GetComponent<CentralController>().botList[0].GetComponent<TermiteFSMBrain>();

        SetupListeners();

        CreateStateButtons();

        /*visual debug for build version
        debug = (GameObject)Instantiate(Resources.Load("Text"), new Vector3(0, 0, 0), Quaternion.identity);
        debug.transform.SetParent(canvas.transform, false);
        debug.GetComponent<Text>().horizontalOverflow = UnityEngine.HorizontalWrapMode.Overflow;
        debug.GetComponent<Text>().verticalOverflow = UnityEngine.VerticalWrapMode.Overflow;
        */
    }

    public void CreateStateButtons() {

        int stateId = selectedBotBrain.supervisorio.currentState.id; 
        int countBtn = 0;

        foreach (var e in selectedBotBrain.supervisorio.FeasibleEvents(selectedBotBrain.supervisorio.currentState, true)) {

            countBtn++;

            GameObject btn = (GameObject)Instantiate(Resources.Load("Button"), new Vector3(860, 540 - countBtn * 30, 0), Quaternion.identity);
            btn.transform.SetParent(canvas.transform, false);
            btn.tag = "StateButton";

            int eventId = e.id;
            string eventLabel = e.label;

            btn.GetComponentInChildren<Text>().text = eventLabel;
            btn.GetComponent<Button>().onClick.AddListener(() => selectedBotBrain.hmiHandler.StateButtonListener(eventId));

        }

    }
    
    public void DestroyStateButtons() {
        
        foreach (GameObject btn in GameObject.FindGameObjectsWithTag("StateButton")) {
            Destroy(btn);
        }

    }


    //Setup all the UI elements listeners
    public void SetupListeners() {

        //Auto Toggle
        autoToggle.GetComponent<Toggle>().onValueChanged.AddListener(AutoToggleListener);
        autoToggle.GetComponent<UnityEngine.UI.Toggle>().isOn = selectedBotBrain.isAuto;

        //Auto Toggle
        fastAnimToggle.GetComponent<Toggle>().onValueChanged.AddListener(FastAnimToggleListener);
        fastAnimToggle.GetComponent<UnityEngine.UI.Toggle>().isOn = selectedBotBrain.isAuto;

        //Exit Button
        exitButton.GetComponentInChildren<Button>().onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));

        if (gameObject.GetComponent<SimManager>().isMultibot) {

            //Disband Button
            disbandButton.SetActive(true);
            disbandButton.GetComponentInChildren<Button>().onClick.AddListener(() => gameObject.GetComponent<CentralController>().DisbandBot(selectedBotBrain.gameObject));

            //Add Bot Button
            addBotButton.SetActive(true);
            addBotButton.GetComponentInChildren<Button>().onClick.AddListener(() => gameObject.GetComponent<CentralController>().SpawnBot());
        }

    }

    
    //Update the state display to the selectedBot's current state
    public void UpdateStateDisplay() {
        stateDisplay.GetComponent<Text>().text = selectedBotBrain.supervisorio.currentState.ToString();
    }

    
    // Button Listeners ----
    void AutoToggleListener(bool autoToggleState) {

        selectedBotBrain.hmiHandler.AutoToggleListener(autoToggleState);

    }

    void FastAnimToggleListener(bool fastAnimToggleState) {
        gameObject.GetComponent<SimManager>().FastAnimToggleListener(fastAnimToggleState);
    }



}
