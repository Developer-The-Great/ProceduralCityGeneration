using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshVertexDisplayer : MonoBehaviour
{

    Mesh mesh;
    private static float GizmoCircleRadius = 0.2f;
    // Start is called before the first frame update
    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        foreach(Vector3 vertex in mesh.vertices)
        {
            Gizmos.DrawSphere(vertex, GizmoCircleRadius);
        }
        
    }
}
