using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BotUIController : MonoBehaviour
{
    // Selected Bot
    TermiteDroneBrain selectedBotBrain;

    // UI Elements
    public Button removeBotBtn;
    public Toggle autoToggle;
    public Text stateDisplay;


    private void Start() {

        SimEvents.current.onBotSelected += SetSelectedBot;

    }

    private void OnDisable() {

        SimEvents.current.onBotSelected -= SetSelectedBot;

    }

    private void Update() {

         UpdateStateDisplay();
        
    }

    private void UpdateStateDisplay() {
        // Set the State Display to the current state of
        // the selected bot


        string state;

        if (selectedBotBrain != null) {
            state = selectedBotBrain.State;
        } else {
            state = "---";
        }

        stateDisplay.text = state;

    }


    private void SetSelectedBot(GameObject selected) {
        // When a bot is selected, check if it wasn't already selected
        // and update UI accordingly


        var newBrain = selected.GetComponent<TermiteDroneBrain>();

        if (selectedBotBrain != newBrain) {

            selectedBotBrain = newBrain;

            removeBotBtn.interactable = !selectedBotBrain.isSolo;
            autoToggle.isOn = selectedBotBrain.isAuto;

        }
    }

    public void RemoveBotListener() {

        if(selectedBotBrain != null) {
            selectedBotBrain.RemoveSelf();
        }

    }

    public void ToggleAutoListener(bool value) {

        if (selectedBotBrain != null) {
            SimEvents.current.AutoToggle(selectedBotBrain,value);
        }
        
    }

}
