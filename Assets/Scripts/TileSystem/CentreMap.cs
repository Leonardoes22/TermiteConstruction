using UnityEngine;
public class CentreMap {

    private Vector3[,] _centres;

    public CentreMap(Coord size, Vector3 tileSize) {

        _centres = new Vector3[size.x, size.y];
        for (int i = 0; i < size.x; i++) {
            for (int j = 0; j < size.y; j++) {
                _centres[i, j] = new Vector3(tileSize.x * (i + 0.5f), 0, tileSize.z * (j + 0.5f));
            }
        }
    }
    public CentreMap(int x, int y, Vector3 tileSize) {

        _centres = new Vector3[x, y];
        for (int i = 0; i < x; i++) {
            for (int j = 0; j < y; j++) {
                _centres[i, j] = new Vector3(tileSize.x * (i + 0.5f), 0, tileSize.z * (j + 0.5f));
            }
        }
    }

    public Vector3 this[Coord index] {

        get => _centres[index.x - 1, index.y - 1];

    }


}