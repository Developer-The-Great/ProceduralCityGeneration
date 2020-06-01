using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class NeoKitschMansionBuildingStrategy : MonoBehaviour
{
    static float shrinkMin = 0.7f;
    static float shrinkMax = 0.9f;
    static float UVConst = 5.0f;
    static float SideUVConst = 0.75f;
    static float normalBuildingChance = 0.65f;
    static float forceCornerChance = 0.4f;
    static float shrinkerBuildingLeanCoefficient = 0.2f;
    public static float NeoMilitCopyChance = 0.72f;
    public static Vector2 PreferedNeoMilitCopySize = new Vector2(8, 8); 

    public static void generateNeoKitschMansionBuilding(GameObject[] meshChoices, Transform transform, BuildArea2D BuildArea, 
        Shape2D InitShape, int seed, int minLevel, int maxLevel, ref int CurrentBuildingLevel, GameObject[] neoKitschSkyscrapers)
    {

        CurrentBuildingLevel = 0;
        Random.InitState(seed);

        int BuildingHeight = Random.Range(minLevel, (int)((minLevel + maxLevel) * 0.4f));

        bool isNeoMilitCopy = BuildArea.Width <= PreferedNeoMilitCopySize.x && BuildArea.Breadth <= PreferedNeoMilitCopySize.y;
        //BuildingHeight = isNeoMilitCopy ? Random.Range((minLevel + maxLevel) / 2, maxLevel + 1) : BuildingHeight;

        if (isNeoMilitCopy)
        {
            Debug.Log("isNeoMilitCopy " + isNeoMilitCopy);
            Debug.Log("BuildArea.Width" + BuildArea.Width);
            Debug.Log("BuildArea.Breadth " + BuildArea.Breadth);

            InitShape.Init(BuildArea.GetSquareVertices(), 1);
            NeoMilitaristBuildingStrategy.generateNeoMilitaristBuilding(neoKitschSkyscrapers, transform, BuildArea, InitShape, seed, (minLevel + maxLevel) / 2, maxLevel+1, ref CurrentBuildingLevel,true,false);

            return;
        }


        bool isShrinker = BuildingHeight > (minLevel+maxLevel)/2;



        GenerateInitialShape2D(transform, InitShape, BuildArea, isShrinker);


        
        
        

        Vector3 shiftDirection = new Vector3(0, BuildArea.perUnitMultiplier, 0);

        if(isShrinker)
        {
            

            float LeanCoefficient = Random.Range(0, shrinkerBuildingLeanCoefficient);

            Vector3 forward = transform.forward * LeanCoefficient;

            Quaternion rot = Quaternion.AngleAxis(Random.Range(0,360), Vector3.up);

            forward = rot * forward;

            shiftDirection += forward;
        }




        Vector3[] UVVertices = new Vector3[InitShape.vertices.Length];

        UVVertices = InitShape.vertices;
        UVVertices = Utils.ShiftVertices(UVVertices, transform.position);



        List<Shape2D> shapesFound = new List<Shape2D>();

        //while building is not in desired height
        while (CurrentBuildingLevel <= BuildingHeight)
        {
   
            foreach (Transform child in transform)
            {
                Shape2D Shape = child.gameObject.GetComponent<Shape2D>();
                if (Shape && Shape.Identifier != -1)
                {
                    shapesFound.Add(Shape);
                }

            }


            foreach (Shape2D shape in shapesFound)
            {
                
                switch((NeoKitschMansionBuildingOpt)shape.Identifier)
                {
                    case NeoKitschMansionBuildingOpt.NgonPrism:
                        GenerateLevel(transform, shape, BuildArea, meshChoices, shiftDirection, CurrentBuildingLevel,BuildingHeight, UVVertices);
                        shape.Identifier = -1;
                        break;
                }



            }

            shapesFound.Clear();

            CurrentBuildingLevel++;//for each shape2D
            //generate building 
        }
    }

    static void GenerateLevel(Transform transform, Shape2D shape, BuildArea2D BuildArea, GameObject[] meshChoices, Vector3 shiftDirection, int CurrentBuildingLevel, int BuildHeight,Vector3[] UVVertices)
    {
        GameObject LevelPart = new GameObject("Floor " + CurrentBuildingLevel);
        LevelPart.transform.position = new Vector3(transform.position.x, 0, transform.position.z);
        LevelPart.transform.parent = transform;

        Vector3 individualShift = shiftDirection / 3.0f;

        //generate 
        if (CurrentBuildingLevel == BuildHeight)
        {
            GameObject Roof = GeneratePolygonPrism(LevelPart.transform, shape.vertices, BuildArea, meshChoices, individualShift, NeoKitschMansionMesh.Conrete, "Concrete",UVVertices);
            Roof.transform.parent = LevelPart.transform;

            return;
        }


        if(CurrentBuildingLevel % 2 != 0)
        {
            shape.vertices = Utils.ShrinkVerticesXZ(shape.vertices, 9f / 10f, Vector3.zero);
            GameObject window2 = GeneratePolygonPrism(LevelPart.transform, shape.vertices, BuildArea, meshChoices, individualShift, NeoKitschMansionMesh.Window, "Window", UVVertices);
            shape.vertices = Utils.ShiftVertices(shape.vertices, individualShift);

            shape.vertices = Utils.ShrinkVerticesXZ(shape.vertices, 10f / 9f, Vector3.zero);
            GameObject conrete1 = GeneratePolygonPrism(LevelPart.transform, shape.vertices, BuildArea, meshChoices, individualShift, NeoKitschMansionMesh.Conrete, "Concrete", UVVertices);
            shape.vertices = Utils.ShiftVertices(shape.vertices, individualShift);

            shape.vertices = Utils.ShrinkVerticesXZ(shape.vertices, 9f / 10f, Vector3.zero);
            GameObject window1 = GeneratePolygonPrism(LevelPart.transform, shape.vertices, BuildArea, meshChoices, individualShift, NeoKitschMansionMesh.Window, "Window", UVVertices);
            shape.vertices = Utils.ShiftVertices(shape.vertices, individualShift);

            shape.vertices = Utils.ShrinkVerticesXZ(shape.vertices, 10f / 9f, Vector3.zero);

        }
        else
        {
            GameObject conrete1 = GeneratePolygonPrism(LevelPart.transform, shape.vertices, BuildArea, meshChoices, individualShift, NeoKitschMansionMesh.Conrete, "Concrete", UVVertices);
            shape.vertices = Utils.ShiftVertices(shape.vertices, individualShift);

            shape.vertices = Utils.ShrinkVerticesXZ(shape.vertices, 9f / 10f, Vector3.zero);
            GameObject window2 = GeneratePolygonPrism(LevelPart.transform, shape.vertices, BuildArea, meshChoices, individualShift, NeoKitschMansionMesh.Window, "Window", UVVertices);
            shape.vertices = Utils.ShiftVertices(shape.vertices, individualShift);

            shape.vertices = Utils.ShrinkVerticesXZ(shape.vertices, 10f / 9f, Vector3.zero);
            GameObject conrete2 = GeneratePolygonPrism(LevelPart.transform, shape.vertices, BuildArea, meshChoices, individualShift, NeoKitschMansionMesh.Conrete, "Concrete", UVVertices);
            shape.vertices = Utils.ShiftVertices(shape.vertices, individualShift);

        }

        Utils.SpawnShape(transform, shape.vertices, transform.position, (int)NeoKitschMansionBuildingOpt.NgonPrism);

    }

    static void GenerateInitialShape2D(Transform transform, Shape2D InitShape,BuildArea2D BuildArea,  bool isShrink)
    {


        const int VertexInPolygon = 16;
        const int PrismCapVertices = 1;

        Vector3[] vertices = new Vector3[VertexInPolygon + PrismCapVertices];

        Vector3[] extremePoints = new Vector3[4];
        Vector3[] cornerPoints = BuildArea.GetRelativeSquareVertices();

        extremePoints[0] =  transform.forward * BuildArea.Breadth / 2.0f * BuildArea.perUnitMultiplier;
        extremePoints[1] =  - transform.right * BuildArea.Width/ 2.0f * BuildArea.perUnitMultiplier;
        extremePoints[2] =  - transform.forward * BuildArea.Breadth / 2.0f * BuildArea.perUnitMultiplier;
        extremePoints[3] =  transform.right * BuildArea.Width / 2.0f * BuildArea.perUnitMultiplier;





        if(isShrink)
        {
            for (int i = 0; i < 4; i++)
            {
                float shrink = Random.Range(shrinkMin, shrinkMax);

                extremePoints[i] = Utils.ResizeVector(Vector3.zero, extremePoints[i], shrink);

                shrink = Random.Range(shrinkMin, shrinkMax);

                cornerPoints[i] = Utils.ResizeVector(Vector3.zero, cornerPoints[i], shrink);

            }
        }

        


        vertices[vertices.Length - 1] = Vector3.zero;

        //initialize four extreme points
        int vertexIndex = 0;

        //for each extreme point
        for(int i =0; i< extremePoints.Length;i++)
        {
            Vector3 corner = Utils.GetNextVertex(i, cornerPoints);
            Vector3 nextExtremePoint = Utils.GetNextVertex(i, extremePoints);

            Vector3 barycentricPoint = FindPointInTriangle(extremePoints[i], corner, nextExtremePoint);

            vertices[vertexIndex] = extremePoints[i];
            vertices[vertexIndex+1] = barycentricPoint;
            vertices[vertexIndex + 2] = barycentricPoint;
            vertices[vertexIndex + 3] = nextExtremePoint;





            vertexIndex += 4;
        }


        InitShape.Init(vertices, 1);
        //initialize shape2D with generated vertices






    }

    static GameObject GeneratePolygonPrism(Transform transform, Vector3[] shapeVertices, BuildArea2D BuildArea, GameObject[] meshChoices,Vector3 shiftDirection, NeoKitschMansionMesh MeshChoiceIndex,string name,Vector3[] UVVertices)
    {
        //Instantiate(meshChoices[0], Vector3.zero, Quaternion.identity);


        Material mat = meshChoices[(int)MeshChoiceIndex].GetComponent<MeshRenderer>().sharedMaterial;
        Material newMat = new Material(mat);


        GameObject LevelPart = new GameObject(name);
        LevelPart.transform.position = new Vector3(transform.position.x, 0, transform.position.z);
        LevelPart.transform.parent = transform;

        Mesh mesh = LevelPart.AddComponent<MeshFilter>().mesh;
        LevelPart.AddComponent<MeshRenderer>();
        
        LevelPart.GetComponent<Renderer>().material = newMat;

        //get shape2D vertices
        const int VertexInPolygon = 16;
        Vector3[] vertices = new Vector3[shapeVertices.Length * 2 + VertexInPolygon*2];

        int sideTriangles = (shapeVertices.Length * 2) * 6;
        int capTriangles = (VertexInPolygon * 3);
        int[] triangles = new int[sideTriangles + capTriangles];

        //copy vertices from shape2D to a new array
        Vector3[] raisedVertices = new Vector3[shapeVertices.Length];
        Vector3[] bottomCapVertices = new Vector3[VertexInPolygon];
        Vector3[] topCapVertices = new Vector3[VertexInPolygon];

        //shift them to a certain direction
        raisedVertices = Utils.ShiftVertices(shapeVertices, shiftDirection);

        for(int i= 0; i < bottomCapVertices.Length;i++)
        {
            bottomCapVertices[i] = shapeVertices[i];
        }
        for (int i = 0; i < bottomCapVertices.Length; i++)
        {
            topCapVertices[i] = raisedVertices[i];
        }

        //concatenate them both
        shapeVertices.CopyTo(vertices, 0);
        raisedVertices.CopyTo(vertices, shapeVertices.Length);
        bottomCapVertices.CopyTo(vertices, shapeVertices.Length + raisedVertices.Length);
        topCapVertices.CopyTo(vertices, shapeVertices.Length + raisedVertices.Length + bottomCapVertices.Length);

        //set UVs
        Vector2[] UV = new Vector2[vertices.Length];
        float totalDistance = 0;
        float[] distance = GetDistanceToEachVertices(shapeVertices,out totalDistance);

        int bottomPolyVerts = shapeVertices.Length;

        //-------------- SET UVs for bottomPolyVerts------------//
        Vector3[] corners = BuildArea.GetRelativeSquareVertices();
        float currentDistance = 0;
        for (int i =0,j=0; i < bottomPolyVerts-1 ; i+=2,j++)
        {
            UV[i] = new Vector2(currentDistance, 0);

            currentDistance += distance[j];
            UV[i+1] = new Vector2(currentDistance , 0);
        }
        UV[shapeVertices.Length-1] = GetUVBasedOnPosition(corners, transform.position, transform.forward, transform.right) * UVConst;
        //new Vector2(UVConst/2.0f, UVConst / 2.0f)

        currentDistance = 0;
        int UpperPolyVerts = shapeVertices.Length*2;
        for (int i = bottomPolyVerts, j = 0; i < UpperPolyVerts-1; i += 2,j++)
        {
            UV[i] = new Vector2(currentDistance, shiftDirection.magnitude);
            currentDistance += distance[j];
            UV[i + 1] = new Vector2(currentDistance, shiftDirection.magnitude);
        }
        UV[shapeVertices.Length*2 -1] = GetUVBasedOnPosition(corners, transform.position, transform.forward, transform.right) * UVConst;



        //--------SET CAP UVs---------//
        
        //Debug.Log("Vert Check corner[o]" + corners[0]);
        int BottomCapPolyVerts = UpperPolyVerts + VertexInPolygon;

        for (int i = UpperPolyVerts , j =0 ; i < BottomCapPolyVerts; i += 2,j+=2)
        {
            UV[i] = GetUVBasedOnPosition(corners, UVVertices[j], transform.forward, transform.right) * UVConst;
            UV[i + 1] = GetUVBasedOnPosition(corners, UVVertices[j+1], transform.forward, transform.right) * UVConst;
        }

        int UpperCapPolyVerts = BottomCapPolyVerts + VertexInPolygon;

        for (int i = BottomCapPolyVerts, j = 0; i < UpperCapPolyVerts; i += 2, j+=2)
        {
            UV[i] = GetUVBasedOnPosition(corners, UVVertices[j],transform.forward,transform.right) * UVConst;
            UV[i + 1] = GetUVBasedOnPosition(corners, UVVertices[j+1], transform.forward, transform.right) * UVConst;
        }


        
        ///////////////////

        int triIndex = 0;
        //for each vertex in shape2D
        for(int i = 0; i < shapeVertices.Length-2;i++)
        {
            int leftBottom = i;
            int leftUpper = i + shapeVertices.Length;
            int rightUpper = i + shapeVertices.Length + 1;
            int rightBottom = i + 1;

            triIndex = SetQuad(triangles, triIndex, leftBottom, leftUpper, rightUpper, rightBottom);
            
        }


        int CapFillingIndex = triIndex;

        for (int i = 0; i < VertexInPolygon - 1; i++)
        {
            int leftUpper = i + shapeVertices.Length * 2;
            int rightUpper = leftUpper + 1;
            int rightBottom = shapeVertices.Length - 1;

            CapFillingIndex = SetTriangleRight(triangles, CapFillingIndex, leftUpper, rightUpper, rightBottom);

        }

        for (int i = 0; i < VertexInPolygon - 1; i++)
        {
            int leftBottom = i + shapeVertices.Length * 2 + VertexInPolygon;
            int leftUpper = leftBottom + 1;
            int rightBottom = (shapeVertices.Length*2) - 1;

            CapFillingIndex = SetTriangleLeft(triangles, CapFillingIndex, leftUpper,leftBottom,  rightBottom);

        }




        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = UV;
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        

        shapeVertices = raisedVertices;

        return LevelPart;
    }

    static int SetTriangleLeft(int[] triangles, int i, int leftBottom, int leftUpper, int rightBottom )
    {
        triangles[i] = leftBottom;
        triangles[i + 1] = leftUpper;
        triangles[i + 2] = rightBottom;
        
        return i + 3;
    }

    static int SetTriangleRight(int[] triangles, int i, int leftUpper, int rightUpper,int rightBottom )
    {
        triangles[i] = leftUpper;
        triangles[i + 1] = rightUpper;
        triangles[i + 2] = rightBottom;

        return i + 3;
    }

    static int SetQuad(int[] triangles, int i, int leftBottom, int leftUpper,int rightUpper, int rightBottom)
    {
        int resultInt = i;
        resultInt = SetTriangleLeft(triangles, resultInt, leftBottom, leftUpper, rightBottom);
        resultInt = SetTriangleRight(triangles, resultInt, leftUpper, rightUpper, rightBottom);

        return resultInt;

    }


    static Vector3 FindPointInTriangle(Vector3 a, Vector3 b,Vector3 c)
    {
        float BarycentricCoefficient = Random.Range(0.5f, 1.0f);
        float v = Random.Range(0, BarycentricCoefficient);
        float u = Random.Range(0, BarycentricCoefficient - v);

        Vector3 Result = a + (b - a) * v + (c - a) * u;

        return Result;
    }

    static float[] GetDistanceToEachVertices(Vector3[] Shape2DVertex,out float total)
    {
        float[] distances = new float[(Shape2DVertex.Length ) / 2];
        //Debug.Log("Shape2DVertex.Length " + (Shape2DVertex.Length));

        total = 0;
        for (int i = 0,j =0 ; i < (Shape2DVertex.Length)-1; i += 2,j++)
        {
            
            //Debug.Log("i:" + i);
            float vertexDistance = Vector3.Distance(Shape2DVertex[i], Shape2DVertex[i + 1]);
            //Debug.Log("GetDistanceToEachVertices:" + vertexDistance);

            if (j <= distances.Length)
            {
                distances[j] = vertexDistance;
            }
           
            total += vertexDistance;
        }

        for (int i = 0; i < distances.Length ; i ++)
        {
            distances[i] = distances[i] * SideUVConst;
        }

        return distances;

    }

    static Vector2 GetUVBasedOnPosition(Vector3[] squareVerts,Vector3 position,Vector3 forward,Vector3 right)
    {
        Vector3 direction = position - squareVerts[2];
        direction.y = 0;

        float maxU = Vector3.Distance(squareVerts[2], squareVerts[3]);
        float maxV = Vector3.Distance(squareVerts[2], squareVerts[1]);

        //Debug.Log("maxV:" + maxV);
        //Debug.Log("maxU:" + maxU);

        float u = Vector3.Dot(direction, right)/maxU;
        float v = Vector3.Dot(direction, forward) / maxV;

        //Debug.Log("u " + u);
        //Debug.Log("v " + v);

        return new Vector2(u, v);
;
    }

}

public enum NeoKitschMansionBuildingOpt
{
    NgonPrism =1
   
}
public enum NeoKitschMansionMesh
{
    Conrete = 0,
    Window =1
}
