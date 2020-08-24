public class HeightMap {

    private int[,] _heights;
    private Coord _size;
    public Coord Shape {
        get => _size;
    }

    //Construtor com inteiros
    public HeightMap(int x, int y) {
        _size = new Coord(x, y);
        _heights = new int[x, y];
    }
    //Construtor com Coords
    public HeightMap(Coord size) {
        _size = size;
        _heights = new int[size.x, size.y];
    }

    public int this[Coord index] {

        get => _heights[index.x - 1, index.y - 1];
        set => _heights[index.x - 1, index.y - 1] = value;
    }
    public int this[int x, int y] {

        get => _heights[x - 1, y - 1];
        set => _heights[x - 1, y - 1] = value;
    }

    


    // Sobrescreve ToString() para vizualizar o mapa de alturas
    public override string ToString() {

        string strOut = "";
        for (int i = 0; i < _size.x; i++) {
            for (int j = 0; j < _size.y; j++) {

                strOut += _heights[i, j] + ", ";

            }
            strOut += "\n";
        }
        return strOut;
    }
}