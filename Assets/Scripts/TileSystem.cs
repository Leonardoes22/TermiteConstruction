using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class TileSystem 
{

    public static Map gridData;
    public static GameObject floor;

    public static int length, width;
    public static Vector3 size;


    public static void GenerateWorld(int l, int w) {

        gridData = new Map(l, w);

        size = floor.GetComponent<Renderer>().bounds.size;

        length = l;
        width = w;


        for (int i = 0; i < length; i++) {
            for (int j = 0; j < width; j++) {

                GameObject tile = MonoBehaviour.Instantiate(floor, new Vector3((i+0.5f)*size.x,0,(j+0.5f)*size.z), Quaternion.identity);
                gridData.tileCentres[i, j] = GetTileBase(tile.transform.position);
                
            }
        }
        //MonoBehaviour.Destroy(floor);

    }

    //Instantiate a tile prefab and add it to the Info Map
    public static void PlaceTile(int l, int w) {

        gridData.AddTile(l, w);
        MonoBehaviour.Instantiate(floor, gridData.tileCentres[l, w] + new Vector3(0,size.y, 0) * gridData.height[l,w] , Quaternion.identity);

    }


    //Returns true or false weather Vector3(x,z) position is inside Grid
    public static bool InWorld(Vector3 entityPos) {

        bool inside = false;
        float x = entityPos.x;
        float z = entityPos.z;

        if ( x < length * size.x && z < width * size.z) {
            if (x >= 0 && z >= 0) {
                inside = true;
            }
        }
        return inside;
    }

    //Returns Vector3(x,0,z) containing the closest tile centre
    public static Vector3 GetTileBase(Vector3 entityPos) {

        float x = Mathf.Floor(entityPos.x/ size.x) * size.x;
        float z = Mathf.Floor(entityPos.z/ size.z) * size.z;

        x += size.x / 2;
        z += size.z / 2;

        return new Vector3(x, 0, z);
    }

    public static Vector3 GetTileCentre(Vector3 entityPos) {

        Vector3 tileCentre = GetTileBase(entityPos);
        int[] tileCoord = gridData.CentreToIndex(tileCentre);
        float height = size.y * gridData.height[tileCoord[0], tileCoord[1]];
        tileCentre.y = height;

        return tileCentre;
    }

    //Returns the selected neighbour Vector3 (x,z) centre with the same height as the caller. Return caller's transform if neighbour not in grid.
    public static Vector3 GetNeighbourBase(Vector3 entityPos, Vector3 direction) {

        direction.y = 0;//Safety

        float theta = Vector3.Angle(direction, Vector3.right);//Get angle
        bool ax = Mathf.Abs(theta % 90 - 45) > 22.5f;// Check if in axis or cross

        //Get absolute values
        float absx = Mathf.Abs(direction.x);
        float absz = Mathf.Abs(direction.z);

        if (ax) {
            direction.x = absx > absz ? direction.x : 0f;
            direction.z = absz <= absx ? 0f : direction.z;
        } 

        direction.x /= absx != 0 ? absx : 1;
        direction.z /= absz != 0 ? absz : 1;

        direction.x *= size.x;
        direction.z *= size.z;

        Vector3 neighbourCentre = GetTileBase(entityPos) + direction;
        neighbourCentre = InWorld(neighbourCentre) ? neighbourCentre : entityPos;
        neighbourCentre.y = 0;

        return neighbourCentre;
    }

    public static Vector3 GetNeighbourCentre(Vector3 entityPos, Vector3 direction) {

        Vector3 tileCentre = GetNeighbourBase(entityPos, direction);
        float height = size.y * gridData.GetHeightInt(tileCentre);
        tileCentre.y = height;

        return tileCentre;
    }

    //Set the tile GameObject type
    public static void SetTileType(GameObject type) {
        floor = type;
    }

    /* Class containing the grid information
     * 
     * - width(z) and length(x) are the grid sizes integer values
     * - height[x,z] is the number of tiles above first layer
     * - tileCentres[x,z] contains the Vector3(x,0,z) with the tiles centres
     * 

    */
   
    public class Map {

        public int width, length;
        public int[,] height;
        public Vector3[,] tileCentres;

        public bool[,] occupied;

        //public GameObject occupant;


        public Map(int l, int w) {
            width = w;
            length = l;
            height = new int[l, w];

            tileCentres = new Vector3[l, w];
            occupied = new bool[l, w];
            
        }

        //Returns the Info Map Coordinate related to a Vector3 tileCentre (-1,-1) if not in grid;
        public int[] CentreToIndex(Vector3 tileCentre) {

            int[] index = { -1, -1 };

            for (int i = 0; i < length; i++) {
                for (int j = 0; j < width; j++) {
                    if (tileCentres[i, j] == tileCentre) {
                        index[0] = i;
                        index[1]  = j;
                    } 
                }
            }

            return index;

        }

        //Adding a tile increase that tile height
        public void AddTile(int l, int w) {
            height[l, w]++;
        }

        public int GetHeightInt(Vector3 tileCentre) {

            int[] tileCoord = gridData.CentreToIndex(tileCentre);

            return gridData.height[tileCoord[0], tileCoord[1]];
        }

        public bool Walkable(Vector3 entityPos, Vector3 destiny) {

            bool walkable = false;

            int entityHeight = GetHeightInt(GetTileBase(entityPos));
            int destinyHeight = GetHeightInt(GetTileBase(destiny));

            if(Mathf.Abs(destinyHeight-entityHeight) <= 1) {
                walkable = true;
            }

            bool free = false;

            int[] coord = CentreToIndex(GetTileBase(destiny));
            free = !occupied[coord[0], coord[1]];


            return walkable && free;
        }

        public void Occupy(Vector3 pos) {

            int[] coord = CentreToIndex(GetTileBase(pos));
            occupied[coord[0], coord[1]] = true;
        }

        public void Leave(Vector3 pos) {
            int[] coord = CentreToIndex(GetTileBase(pos));
            occupied[coord[0], coord[1]] = false;
        }

    }

   
}