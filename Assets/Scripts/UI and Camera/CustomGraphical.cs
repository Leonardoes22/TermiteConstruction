using UnityEngine;

public static class CustomGraphical {


    public static GameObject DrawQuad(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Color col) {

        GameObject gameObject = new GameObject("Quad");
        gameObject.transform.position = p0;

        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        Mesh mesh = new Mesh();

        meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));
        meshRenderer.sharedMaterial.color = col;
        meshRenderer.sharedMaterial.SetFloat("_Glossiness", 0f);

        gameObject.tag = "CustomMesh";

        Vector3[] vertices = new Vector3[4] {
            p0,
            p1,
            p2,
            p3,
        };

        for (int i = 0; i < vertices.Length; i++) {
            vertices[i] = vertices[i] - p0;
        }

        mesh.vertices = vertices;

        int[] tris = new int[6] {
            //Upper Left
            0,1,2,
            //Lower Right
            2,3,0
        };
        mesh.triangles = tris;

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
        return gameObject;

    }
    public static GameObject DrawQuad(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3) {

        return DrawQuad(p0, p1, p2, p3, Color.gray);

    }

    public static GameObject DrawFrame(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float margin, Color col) {

        GameObject frame = new GameObject("Frame");
        frame.transform.position = p0;

        Vector3[] marginBorder = new Vector3[4] {
            new Vector3(-margin, 0, -margin),
            p1-p0 + new Vector3(-margin,0, margin),
            p2-p0 + new Vector3(margin, 0, margin),
            p3-p0 + new Vector3(margin, 0, -margin)
        };
    

        CustomGraphical.DrawQuad(p1, marginBorder[1], marginBorder[2], p2, col).transform.SetParent(frame.transform);
        CustomGraphical.DrawQuad(p2, marginBorder[2], marginBorder[3], p3, col).transform.SetParent(frame.transform);
        CustomGraphical.DrawQuad(p3, marginBorder[3], marginBorder[0], p0, col).transform.SetParent(frame.transform);
        CustomGraphical.DrawQuad(p0, marginBorder[0], marginBorder[1], p1, col).transform.SetParent(frame.transform);

        return frame;

    }
    public static GameObject DrawFrame(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float margin) {

        return DrawFrame(p0, p1, p2, p3, margin, Color.gray);

    }

    public static GameObject DrawStripeFrame(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float[] margin, Color[] col) {

        GameObject stripeFrame = new GameObject("StripeFrame");
        stripeFrame.transform.position = p0;

        p0 = p0 - p0;
        p1 = p1 - p0;
        p2 = p2 - p0;
        p3 = p3 - p0;


        for (int i = 0; i < margin.Length; i++) {
            DrawFrame(p0, p1, p2, p3, margin[i], col[i]).transform.SetParent(stripeFrame.transform);

            p0 = p0 + new Vector3(-margin[i], 0, -margin[i]);
            p1 = p1 + new Vector3(-margin[i], 0, margin[i]);
            p2 = p2 + new Vector3(margin[i], 0, margin[i]);
            p3 = p3 + new Vector3(margin[i], 0, -margin[i]);

        }

        return stripeFrame;

    }
 

    public static GameObject DrawChessQuad(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Coord size, Color[] col) {

        GameObject chessQuad = new GameObject("ChessQuad");
        chessQuad.transform.position = p0;

        p0 = p0 - p0;
        p1 = p1 - p0;
        p2 = p2 - p0;
        p3 = p3 - p0;


        Vector3 right = (p3 - p0) / size.x;
        Vector3 up = (p1 - p0) / size.y;


        for (int i = 0; i < size.x; i++) {
            for (int j = 0; j < size.y; j++) {

                Vector3 itP0 = p0 + i * right + j * up;
                Vector3 itP1 = p0 + i * right + (j + 1) * up;
                Vector3 itP2 = p0 + (i + 1) * right + (j + 1) * up;
                Vector3 itP3 = p0 + (i + 1) * right + j * up;

                Color itCol = (i + j) % 2 == 0 ? col[0] : col[1];

                DrawQuad(itP0, itP1, itP2, itP3, itCol).transform.SetParent(chessQuad.transform);

            }
        }

        return chessQuad;

    }
    public static GameObject DrawChessQuad(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Coord size) {

        Color[] defaultColor = { Color.black, Color.white };

        return DrawChessQuad(p0, p1, p2, p3, size, defaultColor);      

    }

}