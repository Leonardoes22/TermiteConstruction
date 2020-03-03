using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Master : MonoBehaviour
{

    public GameObject floor, termiteBot;

    // Start is called before the first frame update
    void Start()
    {
        TileSystem.SetTileType(floor);
        TileSystem.GenerateWorld(5, 5);

        TileSystem.PlaceTile(3, 3);
        TileSystem.PlaceTile(2, 2);
        TileSystem.PlaceTile(2, 2);
        TileSystem.PlaceTile(2, 2);
        TileSystem.PlaceTile(2, 1);
        TileSystem.PlaceTile(2, 1);
        TileSystem.PlaceTile(1, 1);
        termiteBot.GetComponent<TermiteBotBrain>().InitPosition();

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
