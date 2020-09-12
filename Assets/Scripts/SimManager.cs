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
    public string supervisorName;
    public int firstSup;
    public string firstState;

    public bool isMultibot { get { return structurePlant.isMultiBot; } }
    Coord size { get { return structurePlant.shape; } }

    public StructurePlant structurePlant; 

    void Start() {

        LoadScenario();

        //Initialize TermiteTS
        tileSystem.Initialize(size.x, size.y, "TermiteTile");

        //Reference CentralController
        centralController.Initialize(firstSup, firstState);

        //Initialize InterfaceFSM
        interfaceFSM.Initialize();

    }

    //Load the Simulation for one supervisor
    void LoadScenario() {

        //Load Selected Scenario or the Default one 
        GameObject sceneInfo = GameObject.FindGameObjectWithTag("SceneInfo");

        if (sceneInfo != null) {
            supervisorName = sceneInfo.GetComponent<Text>().text.Split('-')[0];
            firstSup = int.Parse(sceneInfo.GetComponent<Text>().text.Split('-')[1]);
            firstState = sceneInfo.GetComponent<Text>().text.Split('-')[2];
            print("Loaded: " + supervisorName + "supervisor");
        } else {
            supervisorName = defaultSupervisor;
            print("Loaded: " + defaultSupervisor + "(Default) supervisor");
        }


        // Setup Scenario StructurePlant
        structurePlant = new StructurePlant(supervisorName);

        Destroy(sceneInfo);

    }

    public void FastAnimToggleListener(bool isFastAnimState) {

        isFastAnim = isFastAnimState;

    }

}