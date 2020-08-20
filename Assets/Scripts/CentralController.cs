using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CentralController : MonoBehaviour
{

    //Gameobject References
    public SimManager simManager;
    public TermiteTS tileSystem;

    //CentralController Vars
    public List<GameObject> botList = new List<GameObject>();
    public List<FSM.Event> externalEvents = new List<FSM.Event>();
    public HeightMap heightMap;
    int uidCount = 0;

    public void Initialize() {

        heightMap = tileSystem.heightMap;
        SpawnBot();
    }

    //Merge 2 heightmaps
    HeightMap HeightMapSynth(HeightMap hm1, HeightMap hm2) {


        HeightMap newHeightMap = new HeightMap(hm1.Shape);

        for (int i = 1; i < hm1.Shape.x + 1; i++) {
            for (int j = 1; j < hm1.Shape.y + 1; j++) {
                newHeightMap[i, j] = Mathf.Max(hm1[i, j], hm2[i, j]);
            }
        }

        return newHeightMap;

    }


   
    //Add bot to simulation
    public TermiteFSMBrain SpawnBot() {

        
        //Instantiate the bot
        GameObject newBot = (GameObject)Instantiate(Resources.Load("TermiteBotWithTile"));
        TermiteFSMBrain newBotBrain = newBot.GetComponent<TermiteFSMBrain>();

        
        //Set bot id
        newBotBrain.id = uidCount++;

        //Initialize Termite Brain
        newBotBrain.manager = this.gameObject;
        newBotBrain.Initialize(simManager.supervisorName, externalEvents);


        //Add to botList
        botList.Add(newBot);

        return newBotBrain;
    }

    //Remove bot from simulation
    public void DisbandBot(GameObject selBot) {

        for (int i = 0; i < botList.Count; i++) {

            if(botList[i] == selBot) {
                botList.RemoveAt(i);
                Destroy(selBot);
            }

        }

    }




    // MultiBot (TermiteFSM) Functions
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

    public void NotifyTransistionEnd(GameObject source, FSM.Event _event) {

        externalEvents.Add(_event);

        foreach (var bot in botList) {
            if (bot == source) {
                
            } else {
                bot.GetComponent<TermiteCommunicationComponent>().unknownEventsBuffer.Add(_event);
            }
        }

    }

    public void HeightMapUp(HeightMap hm) {

        heightMap = HeightMapSynth(heightMap, hm);

        tileSystem.UpdateMap(heightMap);
    }

}
