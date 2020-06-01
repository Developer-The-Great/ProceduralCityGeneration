using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shape2D : MonoBehaviour
{
    public Vector3[] vertices;
    public int Identifier;
    // Start is called before the first frame update


    public float GizmoCircleRadius = 0.2f;

    public bool TurnOffAllGizmos = false;
    public bool VertexGizmozDisplay;
    public bool VertexCrossDisplay;

    private Color CrossColor = Color.magenta;

    public void Init(Vector3[] vertices,int Identifier)
    {
        this.vertices = vertices;
        this.Identifier = Identifier;
        BuildingGenerator generator;
        generator = transform.root.GetComponent<BuildingGenerator>();

    }

    void SetColor(Color color)
    {
        CrossColor = color;
    }

    private void OnDrawGizmos()
    {
        if(vertices == null || TurnOffAllGizmos) { return; }

        if (VertexGizmozDisplay)
        {
            return;
        }



        Gizmos.color = Color.green;
        Gizmos.DrawSphere(vertices[0], GizmoCircleRadius);
        Gizmos.DrawSphere(vertices[1], GizmoCircleRadius);
        Gizmos.DrawSphere(vertices[2], GizmoCircleRadius);
        Gizmos.DrawSphere(vertices[3], GizmoCircleRadius);

        Gizmos.color = Color.black;
        Gizmos.DrawLine(vertices[0], vertices[1]);
        Gizmos.DrawLine(vertices[1], vertices[2]);
        Gizmos.DrawLine(vertices[2], vertices[3]);
        Gizmos.DrawLine(vertices[3], vertices[0]);

        if(VertexCrossDisplay)
        {
            Gizmos.color = CrossColor;
            Gizmos.DrawLine(vertices[0], vertices[2]);
            Gizmos.DrawLine(vertices[1], vertices[3]);
        }
       

    }

}
