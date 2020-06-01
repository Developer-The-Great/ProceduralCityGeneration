using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils : MonoBehaviour
{

    public static Shape2D GetTopShape2DInTransform(Transform transform)
    {
        Shape2D shape = null;
        float highestHeight = float.MinValue;
        foreach(Transform child in transform)
        {
            Shape2D currentShape = child.GetComponent<Shape2D>();

            if(currentShape)
            {
                if (currentShape.vertices[0].y > highestHeight)
                {
                    shape = currentShape;
                    highestHeight = currentShape.vertices[0].y;
                }
            }

        }

        return shape;
    }

    public static void PlaceRoofObjectOnShape(Transform transform, GameObject[] meshChoice,int roofMeshIndex, Shape2D shape,int perUnitMultiplier,float RoofAddChance,int normalMult, int versionIndex =-1)
    {
        for (int i = 0; i < shape.vertices.Length; i++)
        {
            Vector3 Direction = Utils.GetVertexDirectionInPolygon(i, shape.vertices);

            //find how many units are needed to cover face

            float distance = Vector3.Distance(Vector3.zero, Direction);
            int UnitBuildingCount = (Mathf.RoundToInt(distance) / perUnitMultiplier);

            Direction.Normalize();
            Vector3 normal = Utils.Get2DNormal(Direction);
            Quaternion LookRot = Quaternion.LookRotation(normal, Vector3.up);

            float roofAddDecision = Random.Range(0, 1.0f);

            if (roofAddDecision < RoofAddChance)
            {
                int generateInt = Random.Range(0, UnitBuildingCount + 1);
                Debug.Log("generateInt" + generateInt);
                Vector3 position = shape.vertices[i] + generateInt * Direction + (normal * normalMult);

                Utils.SpawnMesh(transform, meshChoice, roofMeshIndex, position, LookRot, versionIndex);

            }

        }
    }

    public static void PlaceRoofObjects(Transform transform,int perUnitMultiplier,float RoofAddChance,GameObject[] meshChoice,int roofMeshIndex, int normalMult,int versionIndex = -1)
    {
        List<Shape2D> leftoverShapes = new List<Shape2D>();

        Utils.FindShape2DInTransform(transform, leftoverShapes, int.MinValue);

        foreach (Shape2D shape in leftoverShapes)
        {
            for (int i = 0; i < shape.vertices.Length; i++)
            {
                PlaceRoofObjectOnShape(transform, meshChoice, roofMeshIndex, shape, perUnitMultiplier, RoofAddChance, normalMult,versionIndex);


            }

        }
    }


    public static void SliceSquareShape2D(Shape2D shape, bool isHorizontalSlice, int perUnitMultiplier, int newUnitLength, Transform transform, out Shape2D otherShape)
    {
        Vector3[] vertices = new Vector3[shape.vertices.Length];
        System.Array.Copy(shape.vertices, vertices, vertices.Length);

        Shape2D otherShape2D = null;


        if (isHorizontalSlice)
        {

            int currentUnitBreadth = Utils.GetPerUnitLength(shape.vertices[0], shape.vertices[3], perUnitMultiplier);

            if (newUnitLength > currentUnitBreadth)
            {
                otherShape = null;
                return;
            }
            Utils.SpawnShape(transform, vertices, transform.position, shape.Identifier, out otherShape2D);

            int otherInitShapeLength = currentUnitBreadth - newUnitLength;

            Utils.ResizeSquareShape2DOnBack(shape, otherInitShapeLength, transform.forward, perUnitMultiplier);

            Utils.ResizeSquareShape2DOnFront(otherShape2D, newUnitLength, -transform.forward, perUnitMultiplier);

        }
        else
        {

            int currentUnitWidth = Utils.GetPerUnitLength(shape.vertices[0], shape.vertices[1], perUnitMultiplier);
            if (newUnitLength > currentUnitWidth)
            {
                otherShape = null;
                return;
            }
            Utils.SpawnShape(transform, vertices, transform.position, shape.Identifier, out otherShape2D);

            int otherInitShapeLength = currentUnitWidth - newUnitLength;

            Utils.ResizeSquareShape2DOnRight(shape, otherInitShapeLength, -transform.right, perUnitMultiplier);

            Utils.ResizeSquareShape2DOnLeft(otherShape2D, newUnitLength, transform.right, perUnitMultiplier);
        }

        otherShape = otherShape2D;

    }



    //gets the [UN-NORMALIZED]direction  of a Vector3 vertices[startIndex] of to the next vertex in the array
    public static Vector3 GetVertexDirectionInPolygon(int startIndex, Vector3[] vertices)
    {
        Vector3 start = vertices[startIndex];
        int endIndex = startIndex + 1 <= vertices.Length - 1 ? startIndex + 1 : 0;
        Vector3 end = vertices[endIndex];

        Vector3 Direction = end - start;

        return Direction;
    }

    public static Vector3 GetNextVertex(int startIndex,Vector3[] vertices)
    {
        int endIndex = startIndex + 1 <= vertices.Length - 1 ? startIndex + 1 : 0;
        Vector3 end = vertices[endIndex];

        return end;
    }

    public static Vector3 Get2DNormal(Vector3 vector)
    {
        return new Vector3(vector.z, 0, -vector.x);
    }

    public static GameObject SpawnMesh(Transform transform,GameObject[]MeshChoices, int meshIndex, Vector3 position, Quaternion orientation,int meshVersionIndex = -1)
    {
        GameObject obj = MeshChoices[meshIndex];

        if (meshVersionIndex != -1)
        {
            MeshVersions MeshVersions = MeshChoices[meshIndex].GetComponent<MeshVersions>();

            if(MeshVersions &&  meshVersionIndex < MeshVersions.versions.Length)
            {
                obj = MeshVersions.versions[meshVersionIndex];
            }

        }

        GameObject newMesh = Instantiate(obj, position, orientation);
        newMesh.transform.parent = transform;
        return newMesh;
    }

    public static void SpawnShape(Transform transform, Vector3[] vertices, Vector3 position, int identifier)
    {
        GameObject newShape = new GameObject("Shape2D");
        newShape.transform.position = position;
        newShape.transform.parent = transform;

        Shape2D Shape = newShape.AddComponent<Shape2D>();
        Shape.Init(vertices, identifier);
    }

    public static void SpawnShape(Transform transform, Vector3[] vertices, Vector3 position, int identifier, out Shape2D newSpawnedShape)
    {
        GameObject newShape = new GameObject("Shape2D");
        newShape.transform.position = position;
        newShape.transform.parent = transform;

        Shape2D Shape = newShape.AddComponent<Shape2D>();
        Shape.Init(vertices, identifier);

        newSpawnedShape = Shape;
    }

    public static Vector3[] RaiseVerticesInY(Vector3[] vertices, float raiseAmount)
    {
        Vector3[] result = new Vector3[vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
        {
            result[i] = vertices[i] + new Vector3(0, raiseAmount, 0);

        }

        return result;

    }

    public static Vector3[] ShiftVertices(Vector3[] vertices, Vector3 shiftAmount)
    {
        Vector3[] result = new Vector3[vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
        {
            result[i] = vertices[i] + shiftAmount;

        }

        return result;

    }

    public static Vector3[] ShrinkVerticesXZ(Vector3[] vertices,float shrinkMultiplier,Vector3 position)
    {
        Vector3[] result = new Vector3[vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
        {
            

            Vector3 direction = vertices[i] - new Vector3(position.x,vertices[i].y,position.z);
            direction *= shrinkMultiplier;

            result[i] = new Vector3(position.x, vertices[i].y, position.z) + direction;


            
        }

        return result;
    }

    public static Vector3 ResizeVector(Vector3 Start,Vector3 End, float resizeCoefficient)
    {
        Vector3 direction = End - Start;

        direction *= resizeCoefficient;

        return Start + direction;
    }

    public static int ChooseFromArray(int[] choices)
    {
        int choiceIndex = Random.Range(0, choices.Length);

        return choices[choiceIndex];

    }

    public static void FindShape2DInTransform(Transform transform,List<Shape2D> listToFill,int NoBuildIdentifier)
    {
        foreach (Transform child in transform)
        {
            Shape2D Shape = child.gameObject.GetComponent<Shape2D>();
            if (Shape && Shape.Identifier != NoBuildIdentifier)
            {
                listToFill.Add(Shape);
            }

        }
    }

    public static void FindShape2DInTransformOfType( int Identifier,Transform transform, List<Shape2D> listToFill)
    {
        foreach (Transform child in transform)
        {
            Shape2D Shape = child.gameObject.GetComponent<Shape2D>();
            if (Shape && Shape.Identifier == Identifier)
            {
                listToFill.Add(Shape);
            }

        }
    }


    public static void ResizeSquareShape2DOnLeft(Shape2D shape,int unit,Vector3 left,int perUnitMultiplier)
    {
        shape.vertices[1] += left * unit * perUnitMultiplier;
        shape.vertices[2] += left * unit * perUnitMultiplier;

    }

    public static void ResizeSquareShape2DOnRight(Shape2D shape, int unit, Vector3 right, int perUnitMultiplier)
    {
        shape.vertices[0] += right * unit * perUnitMultiplier;
        shape.vertices[3] += right * unit * perUnitMultiplier;
    }

    public static void ResizeSquareShape2DOnFront(Shape2D shape, int unit, Vector3 forward, int perUnitMultiplier)
    {
        shape.vertices[0] += forward * unit * perUnitMultiplier;
        shape.vertices[1] += forward * unit * perUnitMultiplier;
    }

    public static void ResizeSquareShape2DOnBack(Shape2D shape, int unit, Vector3 back, int perUnitMultiplier)
    {
        shape.vertices[2] += back * unit * perUnitMultiplier;
        shape.vertices[3] += back * unit * perUnitMultiplier;
    }

    public static int GetPerUnitLength(Vector3 start,Vector3 end, int perUnitMultiplier)
    {
        float Distance = Vector3.Distance(start, end);

        int unitDistance = (int)Distance / perUnitMultiplier;

        return unitDistance;
    }
}
