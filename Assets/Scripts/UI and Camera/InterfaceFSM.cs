using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InterfaceFSM : MonoBehaviour {

    // Interface
    GameObject canvas;
    public GameObject stateDisplay;
    public GameObject exitButton;
    public GameObject autoToggle;
    public GameObject disbandButton;
    public GameObject addBotButton;
    public GameObject fastAnimToggle;
    public GameObject endText;

    //public GameObject debug;
    //public GameObject debug2;

    public TermiteFSMBrain selectedBotBrain;


    // Update is called once per frame
    void Update() {

        disbandButton.SetActive(gameObject.GetComponent<CentralController>().botList.Count > 1);

    }

    
    public void Initialize() {

       


        //Instantiate canvas
        canvas = (GameObject)Instantiate(Resources.Load("Canvas"));

        /*debug
        debug = (GameObject)Instantiate(Resources.Load("Text"), new Vector3(0, 0, 0), Quaternion.identity);
        debug.transform.SetParent(canvas.transform, false);
        debug.GetComponent<Text>().horizontalOverflow = UnityEngine.HorizontalWrapMode.Overflow;
        debug.GetComponent<Text>().verticalOverflow = UnityEngine.VerticalWrapMode.Overflow;
        debug2 = (GameObject)Instantiate(Resources.Load("Text"), new Vector3(0, 30, 0), Quaternion.identity);
        debug2.transform.SetParent(canvas.transform, false);
        debug2.GetComponent<Text>().horizontalOverflow = UnityEngine.HorizontalWrapMode.Overflow;
        debug2.GetComponent<Text>().verticalOverflow = UnityEngine.VerticalWrapMode.Overflow; */


        //State display
        stateDisplay = (GameObject) Instantiate(Resources.Load("Text"), new Vector3(-20, 490, 0), Quaternion.identity);
        stateDisplay.transform.SetParent(canvas.transform, false);
        stateDisplay.GetComponent<Text>().fontSize = 30;
        stateDisplay.GetComponent<Text>().horizontalOverflow = UnityEngine.HorizontalWrapMode.Overflow;
        stateDisplay.GetComponent<Text>().verticalOverflow = UnityEngine.VerticalWrapMode.Overflow;
        stateDisplay.GetComponent<Text>().text = selectedBotBrain.supervisorio.currentState.ToString();

        //End Text
        endText = (GameObject)Instantiate(Resources.Load("Text"), new Vector3(-200, 0, 0), Quaternion.identity);
        endText.transform.SetParent(canvas.transform, false);
        endText.GetComponent<Text>().text = "The Structure is Complete";
        endText.GetComponent<Text>().fontSize = 60;
        endText.GetComponent<Text>().horizontalOverflow = UnityEngine.HorizontalWrapMode.Overflow;
        endText.GetComponent<Text>().verticalOverflow = UnityEngine.VerticalWrapMode.Overflow;
        endText.GetComponent<Text>().color = Color.red;
        endText.SetActive(false);

        //Auto Toggle
        autoToggle = (GameObject)Instantiate(Resources.Load("Toggle"), new Vector3(60, 440, 0), Quaternion.identity);
        autoToggle.transform.SetParent(canvas.transform, false);
        autoToggle.GetComponentInChildren<Text>().text = "Auto";
        autoToggle.transform.localScale = new Vector3(2, 2, 0);
        autoToggle.GetComponent<Toggle>().onValueChanged.AddListener(AutoToggleListener);
        autoToggle.GetComponent<UnityEngine.UI.Toggle>().isOn = selectedBotBrain.isAuto;

        if (gameObject.GetComponent<SimManager>().isMultibot) {

            //Disband Button
            disbandButton = (GameObject)Instantiate(Resources.Load("Button"), new Vector3(120, 440, 0), Quaternion.identity);
            disbandButton.transform.SetParent(canvas.transform, false);
            disbandButton.GetComponentInChildren<Text>().text = "Disband";
            disbandButton.GetComponentInChildren<Button>().onClick.AddListener(() => gameObject.GetComponent<CentralController>().DisbandBot(selectedBotBrain.gameObject));
            

            //Add Bot Button
            addBotButton = (GameObject)Instantiate(Resources.Load("Button"), new Vector3(-860, 400, 0), Quaternion.identity);
            addBotButton.transform.SetParent(canvas.transform, false);
            addBotButton.GetComponentInChildren<Text>().text = "Add Bot";
            addBotButton.GetComponentInChildren<Button>().onClick.AddListener(() => gameObject.GetComponent<CentralController>().SpawnBot());
        }


        //Exit Button
        exitButton = (GameObject)Instantiate(Resources.Load("Button"), new Vector3(-860, 480, 0), Quaternion.identity);
        exitButton.transform.SetParent(canvas.transform, false);
        exitButton.GetComponentInChildren<Text>().text = "Sair";
        exitButton.GetComponentInChildren<Button>().onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));

        //Auto Toggle
        fastAnimToggle = (GameObject)Instantiate(Resources.Load("Toggle"), new Vector3(-860, 360, 0), Quaternion.identity);
        fastAnimToggle.transform.SetParent(canvas.transform, false);
        fastAnimToggle.GetComponentInChildren<Text>().text = "Fast Animation";
        fastAnimToggle.GetComponent<Toggle>().onValueChanged.AddListener(FastAnimToggleListener);
        fastAnimToggle.GetComponent<UnityEngine.UI.Toggle>().isOn = selectedBotBrain.isAuto;

        CreateStateButtons();

        
    }


    

    
    public void CreateStateButtons() {

        int stateId = selectedBotBrain.supervisorio.currentState.id; 
        int countBtn = 0;
        /*
        foreach (var transition in selectedBotBrain.supervisorio.transitionList) {
            if(transition.source == stateId) {
                countBtn++;

                GameObject btn = (GameObject) Instantiate(Resources.Load("Button"), new Vector3(860, 540 - countBtn * 30, 0), Quaternion.identity);
                btn.transform.SetParent(canvas.transform, false);
                btn.tag = "StateButton";

                int eventId = transition.evento;
                string eventLabel = selectedBotBrain.supervisorio.eventsConteiner[eventId].label;

                btn.GetComponentInChildren<Text>().text = eventLabel;
                btn.GetComponent<Button>().onClick.AddListener(() => selectedBotBrain.hmiHandler.StateButtonListener(eventId));
            }
        }
        */
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

    void AutoToggleListener(bool autoToggleState) {

        selectedBotBrain.hmiHandler.AutoToggleListener(autoToggleState);

    }

    

    void FastAnimToggleListener(bool fastAnimToggleState) {
        gameObject.GetComponent<SimManager>().FastAnimToggleListener(fastAnimToggleState);
    }



}
