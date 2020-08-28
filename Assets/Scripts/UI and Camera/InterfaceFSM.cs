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

    // Supervisor and State Selector
    public GameObject spawnOptionsScreen;
    public GameObject supervisorDropdown;
    public GameObject stateInputField;
    public GameObject spawnBotButton;

    

    //public GameObject debug;


    public TermiteFSMBrain selectedBotBrain;


    // Update is called once per frame
    void Update() {

        disbandButton.SetActive(gameObject.GetComponent<CentralController>().botList.Count > 1);
        UpdateStateDisplay();
        UpdateAutoToggle();
    }

    
    public void Initialize() {

        //Set first selected bot
        SelectBot(gameObject.GetComponent<CentralController>().botList[0].GetComponent<TermiteFSMBrain>());

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
        List<FSM.Event> feasibleEvents = selectedBotBrain.supervisorio.FeasibleEvents(selectedBotBrain.supervisorio.currentState, true);

        foreach (var e in feasibleEvents) {

            countBtn++;

            GameObject btn = (GameObject)Instantiate(Resources.Load("Button"), new Vector3(860, 540 - countBtn * 30, 0), Quaternion.identity);
            btn.transform.SetParent(canvas.transform, false);
            btn.tag = "StateButton";

            int eventId = e.id;
            string eventLabel = e.label;

            btn.GetComponentInChildren<Text>().text = eventLabel;
            btn.GetComponent<Button>().onClick.AddListener(() => selectedBotBrain.interfaceComponent.StateButtonListener(eventId));

        }

        // Temp code for debug feasible external events
        countBtn++;
        countBtn++;
        foreach (var e in selectedBotBrain.supervisorio.FeasibleEvents(selectedBotBrain.supervisorio.currentState)) {

            

            if (!feasibleEvents.Contains(e)) {
                countBtn++;
                GameObject externalEventLabel = (GameObject)Instantiate(Resources.Load("Text"), new Vector3(860, 540 - countBtn * 30, 0), Quaternion.identity);
                externalEventLabel.transform.SetParent(canvas.transform, false);

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
            disbandButton.GetComponentInChildren<Button>().onClick.AddListener(DisBandBotListener);

            //Add Bot Button
            addBotButton.SetActive(true);
            //addBotButton.GetComponentInChildren<Button>().onClick.AddListener(() => gameObject.GetComponent<CentralController>().SpawnBot());
            addBotButton.GetComponentInChildren<Button>().onClick.AddListener(() => AddBotListener());

            //Spawn Bot Button
            spawnBotButton.GetComponentInChildren<Button>().onClick.AddListener(() => SpawnBotListener());

            //Supervisor Selector Dropdown
            supervisorDropdown.GetComponent<Dropdown>().onValueChanged.AddListener( delegate { SupervisorDropdownChanged(); });
        }

    }

    
    public void SelectBot(TermiteFSMBrain botBrain) {
        selectedBotBrain = botBrain;

        DestroyStateButtons();
        CreateStateButtons();
    }

    //Update the state display to the selectedBot's current state
    public void UpdateStateDisplay() {
        stateDisplay.GetComponent<Text>().text = selectedBotBrain.supervisorio.currentState.ToString();
    }

    public void UpdateAutoToggle() {
        autoToggle.GetComponent<UnityEngine.UI.Toggle>().isOn = selectedBotBrain.isAuto;
    }
    
    // Button Listeners ----
    void AutoToggleListener(bool autoToggleState) {

        selectedBotBrain.interfaceComponent.AutoToggleListener(autoToggleState);

        DestroyStateButtons();
        if (!autoToggleState) {
            CreateStateButtons();
        }

    }

    void FastAnimToggleListener(bool fastAnimToggleState) {
        gameObject.GetComponent<SimManager>().FastAnimToggleListener(fastAnimToggleState);
    }

    void DisBandBotListener() {

        gameObject.GetComponent<CentralController>().DisbandBot(selectedBotBrain.gameObject);
        SelectBot(gameObject.GetComponent<CentralController>().botList[0].GetComponent<TermiteFSMBrain>());
    }

    void AddBotListener() {


        spawnOptionsScreen.SetActive(!spawnOptionsScreen.activeInHierarchy);
        
        


        List<string> ops = new List<string>();
        var supList = gameObject.GetComponent<SimManager>().structurePlant.supList;

        for (int i = 0; i < supList.Count; i++) {
            ops.Add("Supervisorio: " + (i+1));
        }

        spawnOptionsScreen.GetComponentInChildren<Dropdown>().ClearOptions();
        spawnOptionsScreen.GetComponentInChildren<Dropdown>().AddOptions(ops);

        if (spawnOptionsScreen.activeInHierarchy) {
            string name = spawnOptionsScreen.GetComponentInChildren<Dropdown>().gameObject.GetComponentInChildren<Text>().text;

            int selected = int.Parse(name.Split(':')[1]) - 1;
            
            stateInputField.GetComponent<InputField>().text = gameObject.GetComponent<SimManager>().structurePlant.supList[selected].initialState.ToString();
             
        }

    }

    void SupervisorDropdownChanged() {

        string name = spawnOptionsScreen.GetComponentInChildren<Dropdown>().gameObject.GetComponentInChildren<Text>().text;
        int selected = int.Parse(name.Split(':')[1]) - 1;

        stateInputField.GetComponent<InputField>().text = gameObject.GetComponent<SimManager>().structurePlant.supList[selected].initialState.ToString();

    }

    void SpawnBotListener() {

        string name = spawnOptionsScreen.GetComponentInChildren<Dropdown>().gameObject.GetComponentInChildren<Text>().text;
        int selected = int.Parse(name.Split(':')[1]) - 1;

        gameObject.GetComponent<CentralController>().SpawnBot(selected);

        spawnOptionsScreen.SetActive(false);

    }

    

}
