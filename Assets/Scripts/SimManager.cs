using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class SimManager : MonoBehaviour
{

    CentralController centralController;
    const string defaultSupervisor = "S2_3X3.xml";
    public string info;
    public bool isMultibot;
    public bool isFastAnim;


    // Start is called before the first frame update
    void Start()
    {
        //Reference CentralController
        centralController = gameObject.GetComponent<CentralController>();
        centralController.Initialize();

        //Try Getting SceneInfo GameObject 
        GameObject sceneInfo = GameObject.FindGameObjectWithTag("SceneInfo");

        if(sceneInfo != null) {
            info = sceneInfo.GetComponent<Text>().text;
            print("Loaded: " + info + "supervisor");
        } else {
            info = defaultSupervisor;
            print("Loaded: " + defaultSupervisor + "(Default) supervisor");
        }



        //Initialize bot
        TermiteFSMBrain firstBotBrain = centralController.SpawnBot();
        centralController.heightMap = firstBotBrain.supervisorio.currentState.heightMap;

        isMultibot = firstBotBrain.supervisorio.isMultiBot;


        //Initialize TermiteTS
        Coord temp_size = firstBotBrain.supervisorio.size;
        transform.GetComponent<TermiteTS>().Initialize(temp_size.x, temp_size.y, "TermiteTile");

        

        //Initialize InterfaceFSM
        transform.GetComponent<InterfaceFSM>().selectedBotBrain = firstBotBrain;
        firstBotBrain.hmiHandler.selected = true;
        transform.GetComponent<InterfaceFSM>().Initialize();

        Destroy(sceneInfo);
        


    }


    public void FastAnimToggleListener(bool isFastAnimState) {

        isFastAnim = isFastAnimState;

    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
