using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TermiteTS : MonoBehaviour {

    //Grid Properties
    Coord gridSize; // Tamanho do Grid
    HeightMap heightMap; // Mapa de alturas
    public CentreMap centreMap; // Mapa de centros: y=0
    public Vector3[] border = new Vector3[4]; // Borda do grid: pontos
    public Vector3 center;

    public GameObject floor;
    TileObj tile; // Contém Objeto Tile e tamanho


    float margin = 100; // Margem do cenário

    // Start is called before the first frame update
    void Start() {


    }


    // Update is called once per frame
    void Update() {
    }

    //Configuração inicial
    public void Initialize(int x, int y, string name) {

        //Configura tamanho do grid
        gridSize = new Coord(x, y);

        //Cria Tile
        tile = new TileObj(name);

        //Configura Alturas
        heightMap = new HeightMap(gridSize);


        // Configura Centros
        centreMap = new CentreMap(gridSize, tile.Size);



        LimitGrid();

    }

    // Acessório Gráfico de Iniciaização 
    void LimitGrid() {

        Vector3 edge = new Vector3(gridSize.x * tile.Size.x, 0 , gridSize.y * tile.Size.z);

        border[0] = Vector3.zero;
        border[1] = new Vector3(0, 0, edge.z);
        border[2] = edge;
        border[3] = new Vector3(edge.x, 0, 0);

        center = (border[2] / 2);

        floor = new GameObject("Floor");
        floor.transform.position = center;


        CustomGraphical.DrawStripeFrame(border[0], border[1], border[2], border[3], new float[2] { 5, margin}, new Color[2] { Color.yellow, Color.grey })
            .transform.SetParent(floor.transform);

        CustomGraphical.DrawChessQuad(border[0], border[1], border[2], border[3], gridSize, new Color[2] { new Color(0.1f, 0.2f, 0.1f) , new Color(0.6f, 0.6f, 0.7f) })
            .transform.SetParent(floor.transform);

        


    }

    // Devolve Vector3 do próximo tile
    public Vector3 NextTilePosition(Coord index) {

        float height = tile.Size.y * (heightMap[index]+0.5f);
        Vector3 pos = centreMap[index];
        pos.y = height;

        return pos;
    }

    // Coloca um Tile
    void PlaceTile(Coord index) {

        GameObject g = (GameObject) Instantiate(Resources.Load("TermiteTile")); // Instancia um tile
        g.transform.position = NextTilePosition(index); // Posiciona o tile no lugar certo
        heightMap[index] += 1; // Atualiza a altura da célula

        g.name = "Tile:" + index + ", Height:" + heightMap[index]; // Nomeia o tile
        g.tag = "Tile"; // Configura a Tag do tile

    }
    // Sobrecarga para loops for
    void PlaceTile(int x, int y) {
        PlaceTile(new Coord(x, y));
    }

    //Remove um tile da célula
    void RemoveTile(Coord index) {

        Destroy(GameObject.Find("Tile:" + index + ", Height:" + heightMap[index])); // Destroi o tile mais acima
        
        heightMap[index] -= 1; // Atualiza a altura da célula

    }
    // Sobrecarga para loops for
    void RemoveTile(int x, int y) {
        RemoveTile(new Coord(x, y));
    }

    // Atualiza a estrutura
    public void UpdateMap(HeightMap newHeightMap) {


        for (int i = 1; i < gridSize.x+1; i++) {
            for (int j = 1; j < gridSize.y+1; j++) {

                int diff = newHeightMap[i,j] - heightMap[i,j];

                if (diff > 0) {
                    for (int t = 0; t < diff; t++) {
                        PlaceTile(i,j);
                    } 

                    } else if (diff < 0) {
                    for (int s = 0; s < Mathf.Abs(diff); s++) {
                        RemoveTile(i,j);
                    }
                }
            }
        }



    }


    //Classes auxiliar
    class TileObj {

        public UnityEngine.Object TileObject { get; }
        public Vector3 Size { get; }

        public TileObj(string path) {

            TileObject = Resources.Load(path);

            GameObject t = (GameObject)Instantiate(TileObject);
            Size = t.GetComponent<Renderer>().bounds.size;
            Destroy(t);

        }

    }
    

}