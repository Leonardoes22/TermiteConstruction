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

   


    //Termite Reference (Temp)
    public GameObject termite;
    TermiteFSMBrain termiteBrain;
    

    // Start is called before the first frame update
    void Start() {

        canvas = (GameObject)Instantiate(Resources.Load("Canvas"));

    }

    // Update is called once per frame
    void Update() {

    }


    public void Initialize() {
        
        termiteBrain = termite.GetComponent<TermiteFSMBrain>();

        CreateStateButtons();

        stateDisplay = (GameObject) Instantiate(Resources.Load("Text"), new Vector3(-860, 510, 0), Quaternion.identity);
        stateDisplay.transform.SetParent(canvas.transform, false);
        stateDisplay.GetComponent<Text>().text = termiteBrain.supervisorio.currentState.ToString();

        exitButton = (GameObject)Instantiate(Resources.Load("Button"), new Vector3(-860, 480, 0), Quaternion.identity);
        exitButton.transform.SetParent(canvas.transform, false);
        exitButton.GetComponentInChildren<Text>().text = "Sair";
        exitButton.GetComponentInChildren<Button>().onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));

    }


    public void CreateStateButtons() {


        int stateId = termiteBrain.supervisorio.currentState.id; 
        int countBtn = 0;
        
        foreach (var transition in termiteBrain.supervisorio.transitionList) {
            if(transition.source == stateId) {
                countBtn++;

                GameObject btn = (GameObject) Instantiate(Resources.Load("Button"), new Vector3(860, 540 - countBtn * 30, 0), Quaternion.identity);
                btn.transform.SetParent(canvas.transform, false);
                btn.tag = "StateButton";

                int eventId = transition.evento;
                string eventLabel = termiteBrain.supervisorio.eventsConteiner[eventId].label;

                btn.GetComponentInChildren<Text>().text = eventLabel;
                btn.GetComponent<Button>().onClick.AddListener(() => termiteBrain.hmiHandler.StateButtonListener(eventId));
            }
        }

    }

    public void DestroyStateButtons() {
        foreach (GameObject btn in GameObject.FindGameObjectsWithTag("StateButton")) {
            Destroy(btn);
        }
    }




    void ButtonListener(int eventId) {

    }

}
