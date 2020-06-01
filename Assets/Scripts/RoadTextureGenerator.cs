using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadTextureGenerator : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateMesh(Vector3[] vertices,GameObject materialObj,float UVConst,bool isGround,Transform transform)
    {
        Material mat = materialObj.GetComponent<MeshRenderer>().sharedMaterial;
        Material newMat = new Material(mat);

        Mesh mesh = new Mesh();

        GameObject RoadObj = new GameObject("Road");
        RoadObj.transform.parent = transform;

        RoadObj.AddComponent<MeshFilter>();
        RoadObj.AddComponent<MeshRenderer>();




        RoadObj.GetComponent<MeshFilter>().mesh = mesh;

        

        

        RoadObj.GetComponent<Renderer>().material = newMat;

        mesh.vertices = vertices;
        Vector2[] UV = new Vector2[vertices.Length];
        int[] triangles = new int[6];

        triangles[0] = 2;
        triangles[1] = 1;
        triangles[2] = 3;
        triangles[3] = 3;
        triangles[4] = 1;
        triangles[5] = 0;

        mesh.triangles = triangles;

        float distancev2v3 = Vector3.Distance(vertices[2], vertices[3]) * UVConst;
        float distancev2v1 = Vector3.Distance(vertices[2], vertices[1]) * UVConst;



        UV[0] = new Vector2(2.0f, distancev2v1);
        UV[1] = new Vector2(0.0f, distancev2v1);
        UV[2] = new Vector2(0.0f, 0);
        UV[3] = new Vector2(2, 0);

        if (isGround)
        {
            UV[0] = new Vector2(distancev2v3, distancev2v1);
            UV[3] = new Vector2(distancev2v3, 0);
        }


        mesh.uv = UV;

        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

    }
}
