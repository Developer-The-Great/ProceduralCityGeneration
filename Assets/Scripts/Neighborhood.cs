using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(Shape2D))]
public class Neighborhood : MonoBehaviour
{
    Shape2D NeighborhoodShape2D;
    public District district;

    public float GizmoCircleRadius = 5.2f;
    // Start is called before the first frame update
    void Start()
    {
        NeighborhoodShape2D = GetComponent<Shape2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Vector3 GetPosition()
    {

        if(NeighborhoodShape2D == null) { NeighborhoodShape2D = GetComponent<Shape2D>(); }
        //NeighborhoodShape2D.vertices
        Vector3 sumVector3 = Vector3.zero;

        foreach(Vector3 vertex in NeighborhoodShape2D.vertices)
        {
            sumVector3 += vertex;
        }

        sumVector3 /= NeighborhoodShape2D.vertices.Length;

        return sumVector3;
    }

    private void OnDrawGizmos()
    {
        switch (district)
        {
            case District.Entropism:
                Gizmos.color = Color.gray;
                break;
            case District.Kitsch:
                Gizmos.color = Color.yellow;
                break;
            case District.NeoKitsch:
                Gizmos.color = Color.green;
                break;
            case District.NeoMilitarism:
                Gizmos.color = Color.black;
                break;
        }

        Vector3[] vertices = NeighborhoodShape2D.vertices;
        Gizmos.DrawSphere(vertices[0], GizmoCircleRadius);
        Gizmos.DrawSphere(vertices[1], GizmoCircleRadius);
        Gizmos.DrawSphere(vertices[2], GizmoCircleRadius);
        Gizmos.DrawSphere(vertices[3], GizmoCircleRadius);
        Gizmos.DrawLine(vertices[0], vertices[1]);
        Gizmos.DrawLine(vertices[1], vertices[2]);
        Gizmos.DrawLine(vertices[2], vertices[3]);
        Gizmos.DrawLine(vertices[3], vertices[0]);

    }



}

public enum District
{
    NeoMilitarism,
    NeoKitsch,
    Kitsch,
    Entropism
}



