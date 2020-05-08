using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class InitSim : MonoBehaviour
{

    public GameObject termite;
    const string defaultSupervisor = "S1_3X3.xml";
    string info;


    // Start is called before the first frame update
    void Start()
    {
        //Try Getting SceneInfo GameObject 
        GameObject sceneInfo = GameObject.FindGameObjectWithTag("SceneInfo");

        if(sceneInfo != null) {
            info = sceneInfo.GetComponent<Text>().text;
            print("Loaded: " + info + "supervisor");
        } else {
            info = defaultSupervisor;
            print("Loaded: " + defaultSupervisor + "(Default) supervisor");
        }

        //Initialize TermiteFSM
        termite.GetComponent<TermiteFSMBrain>().Initialize(info);

        //Initialize TermiteTS
        Coord temp_size = termite.GetComponent<TermiteFSMBrain>().supervisorio.size;
        transform.GetComponent<TermiteTS>().Initialize(temp_size.x, temp_size.y, "TermiteTile");

        //Initialize InterfaceFSM
        transform.GetComponent<InterfaceFSM>().Initialize();

        Destroy(sceneInfo);
        Destroy(this);

        

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
