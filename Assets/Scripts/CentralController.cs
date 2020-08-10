using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CentralController : MonoBehaviour
{

    public List<GameObject> botList = new List<GameObject>();
    public List<FSM.Event> externalEvents = new List<FSM.Event>();
    SimManager simManager;
    TermiteTS tileSystem;

    public HeightMap heightMap;
    int uidCount = 0;
    

    // Update is called once per frame
    void Update()
    {
        


    }

    public void Initialize() {

        simManager = gameObject.GetComponent<SimManager>();
        tileSystem = gameObject.GetComponent<TermiteTS>();
        heightMap = tileSystem.heightMap;
        SpawnBot();
    }

    

    public TermiteFSMBrain SpawnBot() {

        //Instantiate the bot
        GameObject newBot = (GameObject) Instantiate(Resources.Load("TermiteBot"));

        //Instantiate tile 
        GameObject newBotTile = (GameObject)Instantiate(Resources.Load("TermiteTile"));
        newBotTile.transform.SetParent(newBot.transform.GetChild(1).GetChild(0)); //Set bot as parent
        newBotTile.transform.localPosition = new Vector3(0.1633892f, -0.0009015298f, -0.01356555f); // Fix localposition
        newBotTile.GetComponent<MeshRenderer>().enabled = false;

        //Add MeshCollider
        newBot.AddComponent<MeshCollider>();

        //Add correct core material
        newBot.GetComponent<MeshRenderer>().material = (Material) Resources.Load("CoreInteractive");

        //Instantiate brain
        newBot.AddComponent<TermiteFSMBrain>();

        //Set bot id
        newBot.GetComponent<TermiteFSMBrain>().id = uidCount++;

        newBot.GetComponent<TermiteFSMBrain>().manager = this.gameObject;
        newBot.GetComponent<TermiteFSMBrain>().Initialize(simManager.info);

        

        //Add to botList
        botList.Add(newBot);

        //Update already made actions
        for (int i = 0; i < externalEvents.Count; i++) {
            newBot.GetComponent<TermiteFSMBrain>().supervisorio.TriggerEvent(externalEvents[i], true);
        }

        
        
        return newBot.GetComponent<TermiteFSMBrain>();
        

    }

    public void DisbandBot(GameObject selBot) {

        for (int i = 0; i < botList.Count; i++) {

            if(botList[i] == selBot) {
                botList.RemoveAt(i);
                Destroy(selBot);
            }

        }

    }


    public void HeightMapUp(HeightMap hm) {

        heightMap = HeightMapSynth(heightMap, hm);

        tileSystem.UpdateMap(heightMap);
    }

    public void NotifyTransistionEnd(GameObject source, FSM.Event _event) {

        externalEvents.Add(_event);

        foreach (var bot in botList) {
            if(bot == source) {
                
            } else{
                bot.GetComponent<TermiteFSMBrain>().supervisorio.TriggerEvent(_event, true);
            }
        }

    }

    public bool RequestIntent(GameObject source, Coord dest) {

        bool permission = true;

        foreach (var bot in botList) {

            if (bot != source) {
                if (!bot.GetComponent<TermiteFSMBrain>().transitionHandler.Allow(dest)) {
                    permission = false;
                }
            } 
        }

        return permission;
    }

    HeightMap HeightMapSynth(HeightMap hm1, HeightMap hm2) {


        HeightMap newHeightMap = new HeightMap(hm1.Shape);

        for (int i = 1; i < hm1.Shape.x+1; i++) {
            for (int j = 1; j < hm1.Shape.y+1; j++) {
                newHeightMap[i, j] = Mathf.Max(hm1[i, j], hm2[i, j]);
            }
        }

        return newHeightMap;

    }
    
}
