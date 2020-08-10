using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class SimManager : MonoBehaviour {

    //Gameobject References
    public CentralController centralController;
    public TermiteTS tileSystem;
    public InterfaceFSM interfaceFSM;

    //Constants
    const string defaultSupervisor = "S2_3X3.xml";

    //Simulation Parameters
    public bool isFastAnim;

    //Scenario Parameters
    public bool isMultibot;
    Coord size;


    void Start() {

        LoadScenario();

        //Initialize TermiteTS
        tileSystem.Initialize(size.x, size.y, "TermiteTile");

        //Reference CentralController
        centralController.Initialize();

        //Initialize InterfaceFSM
        interfaceFSM.Initialize();
    }

    void LoadScenario() {

        //Load Selected Scenario or the Default one 
        GameObject sceneInfo = GameObject.FindGameObjectWithTag("SceneInfo");

        string info;
        if (sceneInfo != null) {
            info = sceneInfo.GetComponent<Text>().text;
            print("Loaded: " + info + "supervisor");
        } else {
            info = defaultSupervisor;
            print("Loaded: " + defaultSupervisor + "(Default) supervisor");
        }


        // Setup Scenario Parameters
        FSM fsm = new FSM(info);
        isMultibot = fsm.isMultiBot;
        size = fsm.size;


        Destroy(sceneInfo);

    }

    public void FastAnimToggleListener(bool isFastAnimState) {

        isFastAnim = isFastAnimState;

    }

}