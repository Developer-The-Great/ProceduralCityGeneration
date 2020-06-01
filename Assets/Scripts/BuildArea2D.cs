using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum ArchitecturalStyle
{
    NeoMilitarism,
    NeoKitschMansion,
    Kitsch,
    Entropism,
    NeoKitschSkyscraper
    
}
public class BuildArea2D : MonoBehaviour
{
    public const int minimumWidth = 4;
    public const int minimumBreadth = 4;

    [Range(4, 20)]public int Width;
    [Range(4, 20)] public int Breadth;

    
   

    private static float GizmoCircleRadius = 0.1f;

    public int perUnitMultiplier = 3;
    [Range(0, 100)] public int seed;
    

    public ArchitecturalStyle BuildingStyle;

    private BuildingGenerator buildGenerator;

    // Start is called before the first frame update
    void Start()
    {
        buildGenerator = gameObject.AddComponent<BuildingGenerator>();
        
    }

    public BuildingGenerator StartGenerate()
    {
        buildGenerator = gameObject.AddComponent<BuildingGenerator>();
        return buildGenerator;
    }

    public void ResetBuild()
    {
        //Destroy(buildGenerator);
        DestroyImmediate(buildGenerator);
        while (transform.childCount > 0)
        {
            foreach (Transform child in transform)
            {
                DestroyImmediate(child.gameObject);
            }
        }
        
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Vector3[] GetSquareVertices()
    {
        Vector3[] vertices = new Vector3[4];
        float SizedWidth = Width * perUnitMultiplier;
        float SizedBreadth = Breadth * perUnitMultiplier;

        
        Vector3 AddRight = SizedWidth/2 * Vector3.Normalize(transform.right);
        Vector3 AddForward = SizedBreadth/2 * Vector3.Normalize(transform.forward);


        vertices[0] = transform.position + AddRight + AddForward;
        vertices[1] = transform.position - AddRight + AddForward;
        vertices[2] = transform.position - AddRight - AddForward;
        vertices[3] = transform.position + AddRight - AddForward;




        return vertices;
    }



    public Vector3[] GetRelativeSquareVertices()
    {
        Vector3[] vertices = new Vector3[4];
        float SizedWidth = Width * perUnitMultiplier;
        float SizedBreadth = Breadth * perUnitMultiplier;


        Vector3 AddRight = SizedWidth / 2f * Vector3.Normalize(transform.right);
        Vector3 AddForward = SizedBreadth / 2f * Vector3.Normalize(transform.forward);


        vertices[0] =  AddRight + AddForward;
        vertices[1] =  - AddRight + AddForward;
        vertices[2] =  - AddRight - AddForward;
        vertices[3] =  AddRight - AddForward;




        return vertices;
    }

    private void OnDrawGizmos()
    {


        Vector3[] vertices = GetSquareVertices();

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(vertices[0], GizmoCircleRadius);
        Gizmos.DrawSphere(vertices[1], GizmoCircleRadius);
        Gizmos.DrawSphere(vertices[2], GizmoCircleRadius);
        Gizmos.DrawSphere(vertices[3], GizmoCircleRadius);

        Gizmos.color = Color.black;
        Gizmos.DrawLine(vertices[0], vertices[1]);
        Gizmos.DrawLine(vertices[1], vertices[2]);
        Gizmos.DrawLine(vertices[2], vertices[3]);
        Gizmos.DrawLine(vertices[3], vertices[0]);

    }


    
}
