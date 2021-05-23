using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneInterfaceComponent : MonoBehaviour
{

    // Termite Components
    public TermiteDroneBrain brain;

    // External References
    public SoloDroneInterface hmi;


    public void Initialize(GameObject manager) {
        hmi = manager.GetComponent<SoloDroneInterface>();
        hmi.CreateStateButtons();
    }

    private void Update() {
        if (brain.supervisor.currentState.marked) {
            End();
        }
    }

    public void StateButtonListener(int id) {

        brain.ProcessIntent(id);

    }

    public void End() {
        hmi.endText.SetActive(true);
    }

    public void UpdateStateButtons() {

        hmi.DestroyStateButtons();
        hmi.CreateStateButtons();

    }

}
