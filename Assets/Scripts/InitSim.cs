using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class InitSim : MonoBehaviour
{

    public GameObject termite;
    

    // Start is called before the first frame update
    void Start()
    {
        //Get SceneInfo GameObject and its Info
        GameObject sceneInfo = GameObject.FindGameObjectWithTag("SceneInfo");
        string info = sceneInfo.GetComponent<Text>().text;

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
