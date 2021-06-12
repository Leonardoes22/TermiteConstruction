using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimEvents : MonoBehaviour
{

    public static SimEvents current;

    private void Awake() {

        current = this;

    }

    // Bot selection event
    public event Action<GameObject> onBotSelected;
    public void BotSelected(GameObject gameObject) {

        if(onBotSelected != null) {
            onBotSelected(gameObject);
        }

    }

    // Automatic mode toggle event
    public event Action<TermiteDroneBrain, bool> onAutoToggle;
    public void AutoToggle(TermiteDroneBrain selectedBrain, bool toggleValue) {
        if (onBotSelected != null) {
            onAutoToggle(selectedBrain, toggleValue);
        }
    }

}
