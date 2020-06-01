using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum SliceDecision
{
    Horizontal,
    Vertical
}

public class EntropismBuildingStrategy : MonoBehaviour
{
    const int minWidth = 2;
    const int minBreadth = 2;
    const int maxDivisions = 3;
    const float BuildChance = 0.7f;
    const float ResizeChance = 0.9f;
    const float ExpandChance = 0.5f;
    const float RoofAdChance = 0.05f;

    public static void generateEntropismBuilding(GameObject[] meshChoices, Transform transform, BuildArea2D BuildArea, 
        Shape2D InitShape, int seed, int minLevel, int maxLevel, ref int CurrentBuildingLevel)
    {

        Random.InitState(seed);
        int BuildingHeight = Random.Range(minLevel, maxLevel + 1);
        CurrentBuildingLevel = 0;

        InitializeShape2D(InitShape,  BuildArea,transform);

        

        int[] BaseBuildOpt = { (int)EntropismBuildingOpt.Base, (int)EntropismBuildingOpt.NoBuild };
        int[] LevelBuildOpt = { (int)EntropismBuildingOpt.Level, (int)EntropismBuildingOpt.NoBuild };


        Dictionary<Shape2D, Vector3[]> ShapeToOriginalVertices = new Dictionary<Shape2D, Vector3[]>();
        

        foreach (Transform child in transform)
        {
            Shape2D Shape = child.gameObject.GetComponent<Shape2D>();
            if (Shape)
            {
                Vector3[] copiedVertices = new Vector3[Shape.vertices.Length];
                System.Array.Copy(Shape.vertices, copiedVertices, copiedVertices.Length);

                ShapeToOriginalVertices.Add(Shape, copiedVertices);
            }

        }

        List<Shape2D> shapesFound = new List<Shape2D>();

        //find all shapes with build instructions
        foreach (Transform child in transform)
        {
            Shape2D Shape = child.gameObject.GetComponent<Shape2D>();
            if (Shape && Shape.Identifier != (int)EntropismBuildingOpt.NoBuild)
            {
                shapesFound.Add(Shape);
            }

        }

        while (CurrentBuildingLevel <= BuildingHeight)
        {
            

            ////----------- Select which shapes will grow------------///// 
            //TODO: POTENTIAL BUG, ForceGrowIndex could be set to a Shape2D that is already set to NoBuild
            int ForceGrowIndex = Random.Range(0, shapesFound.Count);


            for(int i =0; i < shapesFound.Count; i++)
            {
                float decision = Random.Range(0, 1.0f);

                if(CurrentBuildingLevel == 0)
                {
                    shapesFound[i].Identifier = decision < BuildChance ? BaseBuildOpt[0] : BaseBuildOpt[1];
                }
                else
                {
                    shapesFound[i].Identifier = decision < BuildChance ? LevelBuildOpt[0] : LevelBuildOpt[1];
                }
                
            }

            shapesFound[ForceGrowIndex].Identifier = CurrentBuildingLevel == 0 ? 
                (int)EntropismBuildingOpt.Base : 
                (int)EntropismBuildingOpt.Level;

            


            foreach (Shape2D shape in shapesFound)
            {
                switch ((EntropismBuildingOpt)shape.Identifier)
                {
                    case EntropismBuildingOpt.Base:
                        AttemptShrinkSquareShape2D(transform, shape, ShapeToOriginalVertices, BuildArea);
                        GenerateBaseLevel(shape, transform, CurrentBuildingLevel, BuildArea.perUnitMultiplier, meshChoices);
                        break;

                    case EntropismBuildingOpt.Level:

                        Vector3[] verticeBeforeResize = new Vector3[shape.vertices.Length];
                        System.Array.Copy(shape.vertices, verticeBeforeResize, verticeBeforeResize.Length);
                        bool expanded = false;

                        if(AttemptResizeSquareShape2D(transform, shape, ShapeToOriginalVertices, BuildArea,out expanded))
                        {
                            if(expanded)
                            {
                                verticeBeforeResize = shape.vertices;
                            }
                            GenerateRoof(verticeBeforeResize, transform, CurrentBuildingLevel, BuildArea.perUnitMultiplier, meshChoices, BuildArea, expanded);
                        }

                        GenerateLevel(shape, transform, CurrentBuildingLevel,BuildingHeight, BuildArea.perUnitMultiplier, meshChoices);
                        break;

                    case EntropismBuildingOpt.Roof:
                        //GenerateRoof(shape.vertices, transform, CurrentBuildingLevel, BuildArea.perUnitMultiplier, meshChoices, BuildArea);
                        break;
                    

                }

            }
            CurrentBuildingLevel++;

             //shapesFound.Clear(); 
            
        }

        foreach (Transform child in transform)
        {
            Shape2D Shape = child.gameObject.GetComponent<Shape2D>();
            if (Shape && Shape.Identifier != (int)EntropismBuildingOpt.NoBuild)
            {
                shapesFound.Add(Shape);
            }

        }

        foreach (var shape in shapesFound)
        {
            GenerateRoof(shape.vertices, transform, CurrentBuildingLevel, BuildArea.perUnitMultiplier, meshChoices, BuildArea,false);
        }

        Utils.PlaceRoofObjects(transform, BuildArea.perUnitMultiplier, RoofAdChance, meshChoices, (int)EntropismBuildingMesh.RoofAd, 0, -1);

    }


    static void GenerateBaseLevel(Shape2D shape, Transform transform, int CurrentBuildingLevel, int perUnitMultiplier, GameObject[] MeshChoices)
    {
        if(shape.Identifier == (int)EntropismBuildingOpt.Roof)
        {
            shape.Identifier = (int)EntropismBuildingOpt.NoBuild;
            return;
        }


        GameObject Level = new GameObject("BaseFloor " + CurrentBuildingLevel);

        Level.transform.position = new Vector3(transform.position.x, shape.vertices[0].y, transform.position.z);
        Level.transform.parent = transform;

        int[] choice = { (int)EntropismBuildingMesh.Wall, (int)EntropismBuildingMesh.Window };

        for (int i = 0; i < shape.vertices.Length; i++)
        {
            

            Vector3 Direction = Utils.GetVertexDirectionInPolygon(i, shape.vertices);

            //find how many units are needed to cover face

            float distance = Vector3.Distance(Vector3.zero, Direction);
            int UnitBuildingCount = (Mathf.RoundToInt(distance) / perUnitMultiplier);

            Direction.Normalize();
            Vector3 normal = Utils.Get2DNormal(Direction);
            Quaternion LookRot = Quaternion.LookRotation(normal, Vector3.up);

            for (int j = 0; j < UnitBuildingCount; j++)
            {
                EntropismBuildingMesh MeshChoice = (EntropismBuildingMesh)Utils.ChooseFromArray(choice);

                EntropismBuildingMesh rawChoice = MeshChoice;

                if (i == 0 && j == UnitBuildingCount / 2)
                {

                    rawChoice = (int)EntropismBuildingMesh.Door;
                }
                int meshVersion;
                EntropismMeshVersionEdit(out meshVersion, rawChoice, MeshChoices);

                GameObject newMesh = Utils.SpawnMesh(transform, MeshChoices, (int)rawChoice, shape.vertices[i] + Direction * (j + 1) * perUnitMultiplier, LookRot, meshVersion);
                newMesh.transform.parent = Level.transform;
            }

        }

        shape.Identifier = (int)EntropismBuildingOpt.Level;
        shape.vertices = Utils.RaiseVerticesInY(shape.vertices, perUnitMultiplier);
    }

    static void EntropismMeshVersionEdit(out int meshversion,EntropismBuildingMesh BuildingMesh,GameObject[] meshChoices)
    {
        if(BuildingMesh == EntropismBuildingMesh.Window)
        {
            meshversion = Random.Range(0, meshChoices[(int)EntropismBuildingMesh.Window].GetComponent<MeshVersions>().GetCount());
        }
        else if(BuildingMesh == EntropismBuildingMesh.Roof)
        {
            if(Random.Range(0,1.0f) < 0.1f)
            {
                meshversion = Random.Range(0, meshChoices[(int)EntropismBuildingMesh.Roof].GetComponent<MeshVersions>().GetCount());
            }
            else
            {
                meshversion = -1;
            }
        }
        else if(BuildingMesh == EntropismBuildingMesh.Door)
        {
            meshversion = Random.Range(0, meshChoices[(int)EntropismBuildingMesh.Door].GetComponent<MeshVersions>().GetCount());
        }

        else
        {
            meshversion = -1;
        }
    }

    static void GenerateLevel(Shape2D shape, Transform transform, int CurrentBuildingLevel, int MaxBuildingHeight, int perUnitMultiplier, GameObject[] MeshChoices)
    {

        GameObject Level = new GameObject("Floor " + CurrentBuildingLevel);

        Level.transform.position = new Vector3(transform.position.x, shape.vertices[0].y, transform.position.z);
        Level.transform.parent = transform;

        int[] choice = { (int)EntropismBuildingMesh.Wall, (int)EntropismBuildingMesh.Balcony,(int)EntropismBuildingMesh.Window };



        for (int i = 0; i < shape.vertices.Length; i++)
        {
            

            Vector3 Direction = Utils.GetVertexDirectionInPolygon(i, shape.vertices);

            //find how many units are needed to cover face

            float distance = Vector3.Distance(Vector3.zero, Direction);
            int UnitBuildingCount = (Mathf.RoundToInt(distance) / perUnitMultiplier);

            Direction.Normalize();
            Vector3 normal = Utils.Get2DNormal(Direction);
            Quaternion LookRot = Quaternion.LookRotation(normal, Vector3.up);

            for (int j = 0; j < UnitBuildingCount; j++)
            {
                EntropismBuildingMesh MeshChoice = (EntropismBuildingMesh)Utils.ChooseFromArray(choice);

                int meshVersion;
                EntropismMeshVersionEdit(out meshVersion, MeshChoice, MeshChoices);

                GameObject newMesh = Utils.SpawnMesh(transform, MeshChoices, (int)MeshChoice, shape.vertices[i] + Direction * (j + 1) * perUnitMultiplier, LookRot,meshVersion);
                newMesh.transform.parent = Level.transform;
            }

        }

        if(CurrentBuildingLevel == MaxBuildingHeight) { shape.Identifier = (int)EntropismBuildingOpt.Roof; }

        shape.vertices = Utils.RaiseVerticesInY(shape.vertices, perUnitMultiplier);
    }

    static void GenerateRoof(Vector3[] vertices, Transform transform, int CurrentBuildingLevel, int perUnitMultiplier, 
        GameObject[] MeshChoices, BuildArea2D BuildArea,bool isUsingUnderRoof = true)
    {
        if(CurrentBuildingLevel == 0) { return; }

        EntropismBuildingMesh choice = isUsingUnderRoof ? EntropismBuildingMesh.UnderRoof : EntropismBuildingMesh.Roof;
        GameObject Level = new GameObject("Roof " + CurrentBuildingLevel);

        Level.transform.position = new Vector3(transform.position.x, vertices[0].y, transform.position.z);
        Level.transform.parent = transform;

        Vector3 startPosition = vertices[0] - transform.forward * perUnitMultiplier - transform.right * perUnitMultiplier;

        int width = (int)Vector3.Distance(vertices[0], vertices[1]) / perUnitMultiplier;
        int height = (int)Vector3.Distance(vertices[0],vertices[3]) / perUnitMultiplier;


        for (int j = 0; j < height; j++)
        {
  
            Vector3 forwardShift = transform.forward * j * BuildArea.perUnitMultiplier;


            for (int i = 0; i < width; i++)
            {
                Vector3 rightShift = transform.right * i * BuildArea.perUnitMultiplier;
                Vector3 position = startPosition - rightShift - forwardShift;
                Vector3 offset = transform.forward * BuildArea.perUnitMultiplier + transform.right * BuildArea.perUnitMultiplier;

                int meshVersion;
                EntropismMeshVersionEdit(out meshVersion, choice, MeshChoices);

                Utils.SpawnMesh(Level.transform, MeshChoices, (int)choice, position + offset, transform.rotation,meshVersion);


            }

        }


        //shape.vertices = Utils.RaiseVerticesInY(shape.vertices, 1);



    }

    static void InitializeShape2D(Shape2D initShape,BuildArea2D BuildArea,Transform transform)
    {

        initShape.Init(BuildArea.GetSquareVertices(), 0);

        int DivisionCount = Random.Range(0, maxDivisions+1);
        int index = 0;

        Queue<Shape2D> shapes = new Queue<Shape2D>();

        shapes.Enqueue(initShape);

        while(index < DivisionCount)
        {
            Shape2D shape = shapes.Dequeue();
            Shape2D otherShape = null;

            SliceDecision decision = (SliceDecision)Random.Range(0, 2);

            int currentUnitBreadth = Utils.GetPerUnitLength(shape.vertices[0], shape.vertices[3],BuildArea.perUnitMultiplier);
            int currentUnitWidth = Utils.GetPerUnitLength(shape.vertices[0], shape.vertices[1], BuildArea.perUnitMultiplier);

            //check how the following shape can be cut
            bool CanCutInHorizontal = currentUnitBreadth > minBreadth;
            bool CanCutVertical = currentUnitWidth > minWidth;

            int newSizeHorizontal = Random.Range(minBreadth, currentUnitBreadth+1 - minBreadth);
            int newSizeVertical = Random.Range(minWidth, currentUnitWidth + 1 - minWidth); 
            

            ///slice shape based on how it can be cut
            switch (decision)
            {
                case SliceDecision.Horizontal:
                    
                    if(CanCutInHorizontal) { SliceSquareShape2D(shape, true, BuildArea.perUnitMultiplier, newSizeHorizontal, transform,out  otherShape); }
                    else if(CanCutVertical) { SliceSquareShape2D(shape, false, BuildArea.perUnitMultiplier, newSizeVertical, transform, out otherShape); }

                    break;

                case SliceDecision.Vertical:

                    if (CanCutVertical) { SliceSquareShape2D(shape, false, BuildArea.perUnitMultiplier, newSizeVertical, transform, out otherShape); }
                    else if (CanCutInHorizontal) { SliceSquareShape2D(shape, true, BuildArea.perUnitMultiplier, newSizeHorizontal, transform, out otherShape); }

                    break;

            }

            if(otherShape != null)
            {
                shapes.Enqueue(otherShape);
            }

            shapes.Enqueue(shape);

            index++;
        }


    }




    static void SliceSquareShape2D(Shape2D shape, bool isHorizontalSlice, int perUnitMultiplier, int newUnitLength,Transform transform, out Shape2D otherShape)
    {
        Vector3[] vertices = new Vector3[shape.vertices.Length];
        System.Array.Copy(shape.vertices, vertices, vertices.Length);

        Shape2D otherShape2D = null;

        if (isHorizontalSlice)
        {
            
            int currentUnitBreadth = Utils.GetPerUnitLength(shape.vertices[0], shape.vertices[3], perUnitMultiplier);

            if(newUnitLength > currentUnitBreadth)
            {
                otherShape = null;
                return;
            }

            Utils.SpawnShape(transform, vertices, transform.position, shape.Identifier, out otherShape2D);

            int otherInitShapeLength = currentUnitBreadth - newUnitLength;

            Utils.ResizeSquareShape2DOnFront(otherShape2D, newUnitLength, -transform.forward, perUnitMultiplier);

            Utils.ResizeSquareShape2DOnBack(shape, otherInitShapeLength, transform.forward, perUnitMultiplier);

           

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

    static bool AttemptResizeSquareShape2D(Transform transform, Shape2D shape, Dictionary<Shape2D, Vector3[]> ShapeToOriginalVertices,BuildArea2D buildArea,out bool expanded)
    {
        //decide if we actually want to resize
        float decision = Random.Range(0, 1.0f);

        if(decision > ResizeChance)
        {
            expanded = false;
            return false;
        }

        bool isResized = false;

        float resizeDecision = Random.Range(0, 1.0f);

        if (resizeDecision > ExpandChance)
        {
            expanded = false;
            isResized = AttemptShrink(shape, buildArea, transform);
        }
        else
        {
            expanded = true;
            isResized = AttemptExpand(shape, buildArea, transform, ShapeToOriginalVertices);
        }

        return isResized;
    }

    static bool AttemptShrinkSquareShape2D(Transform transform, Shape2D shape, Dictionary<Shape2D, Vector3[]> ShapeToOriginalVertices, BuildArea2D buildArea)
    {
        //decide if we actually want to resize
        float decision = Random.Range(0, 1.0f);

        if (decision > ResizeChance)
        {
            return false;
        }

        bool isResized = false;

        
        isResized = AttemptShrink(shape, buildArea, transform);
        

        return isResized;
    }


    static bool AttemptShrink(Shape2D shape,BuildArea2D buildArea,Transform transform)
    {
        int maxShrinkWidth;
        int maxShrinkBreadth;

        bool isResized = false;

        GetShapeResizeTolerance(shape, buildArea.perUnitMultiplier, out maxShrinkWidth, out maxShrinkBreadth);

        if (maxShrinkWidth > 0)
        {
            int resize = Random.Range(0, maxShrinkWidth);
            Utils.ResizeSquareShape2DOnRight(shape, resize, -transform.right, buildArea.perUnitMultiplier);
            if (resize > 0) { isResized = true; }
        }

        GetShapeResizeTolerance(shape, buildArea.perUnitMultiplier, out maxShrinkWidth, out maxShrinkBreadth);

        if (maxShrinkWidth > 0)
        {
            int resize = Random.Range(0, maxShrinkWidth);
            Utils.ResizeSquareShape2DOnLeft(shape, resize, transform.right, buildArea.perUnitMultiplier);
            if (resize > 0) { isResized = true; }
        }

        GetShapeResizeTolerance(shape, buildArea.perUnitMultiplier, out maxShrinkWidth, out maxShrinkBreadth);

        if (maxShrinkBreadth > 0)
        {
            int resize = Random.Range(0, maxShrinkBreadth);
            Utils.ResizeSquareShape2DOnFront(shape, resize, -transform.forward, buildArea.perUnitMultiplier);
            if (resize > 0) { isResized = true; }
        }

        GetShapeResizeTolerance(shape, buildArea.perUnitMultiplier, out maxShrinkWidth, out maxShrinkBreadth);

        if (maxShrinkBreadth > 0)
        {
            int resize = Random.Range(0, maxShrinkBreadth);
            Utils.ResizeSquareShape2DOnBack(shape, resize, transform.forward, buildArea.perUnitMultiplier);
            if (resize > 0) { isResized = true; }
        }

        return isResized;
    }

    static bool AttemptExpand(Shape2D shape, BuildArea2D buildArea,Transform transform, Dictionary<Shape2D, Vector3[]> ShapeToOriginalVertices)
    {
        int maxExpandWidth;
        int maxExpandBreadth;

        bool isResized = false;


        GetShapeExpandTolerance(shape, buildArea.perUnitMultiplier, out maxExpandWidth, out maxExpandBreadth, ShapeToOriginalVertices,Directions.Right,transform);

        if (maxExpandWidth > 0)
        {
            int resize = Random.Range(1, maxExpandWidth);
            Utils.ResizeSquareShape2DOnRight(shape, resize, transform.right, buildArea.perUnitMultiplier);
            if (resize > 0) { isResized = true; }
        }

        GetShapeExpandTolerance(shape, buildArea.perUnitMultiplier, out maxExpandWidth, out maxExpandBreadth, ShapeToOriginalVertices, Directions.Left, transform);

        if (maxExpandWidth > 0)
        {
            int resize = Random.Range(1, maxExpandWidth);
            Utils.ResizeSquareShape2DOnLeft(shape, resize, -transform.right, buildArea.perUnitMultiplier);
            if (resize > 0) { isResized = true; }
        }

        GetShapeExpandTolerance(shape, buildArea.perUnitMultiplier, out maxExpandWidth, out maxExpandBreadth, ShapeToOriginalVertices,Directions.Front, transform);

        if (maxExpandBreadth > 0)
        {
            int resize = Random.Range(1, maxExpandBreadth);
            Utils.ResizeSquareShape2DOnFront(shape, resize, transform.forward, buildArea.perUnitMultiplier);
            if (resize > 0) { isResized = true; }
        }

        GetShapeExpandTolerance(shape, buildArea.perUnitMultiplier, out maxExpandWidth, out maxExpandBreadth, ShapeToOriginalVertices,Directions.Backward, transform);

        if (maxExpandBreadth > 0)
        {
            int resize = Random.Range(1, maxExpandBreadth);
            Utils.ResizeSquareShape2DOnBack(shape, resize, -transform.forward, buildArea.perUnitMultiplier);
            if (resize > 0) { isResized = true; }
        }

        return isResized;
    }

    static void GetShapeResizeTolerance(Shape2D shape, int perUnitMultiplier, out int maxShrinkWidth,out int maxShrinkBreadth)
    {
        int currentWidth = Utils.GetPerUnitLength(shape.vertices[0], shape.vertices[1], perUnitMultiplier);
        int currentBreadth = Utils.GetPerUnitLength(shape.vertices[0], shape.vertices[3], perUnitMultiplier);

        maxShrinkWidth = currentWidth - minWidth;
        maxShrinkBreadth = currentBreadth - minBreadth;
    }

    static void GetShapeExpandTolerance(Shape2D shape, int perUnitMultiplier, out int ExpandWidth, out int ExpandBreadth, Dictionary<Shape2D, Vector3[]> ShapeToOriginalVertices,Directions direction,Transform transform)
    {
        int currentWidth = Utils.GetPerUnitLength(shape.vertices[0], shape.vertices[1], perUnitMultiplier);
        int currentBreadth = Utils.GetPerUnitLength(shape.vertices[0], shape.vertices[3], perUnitMultiplier);

        Vector3[] originalVertices = ShapeToOriginalVertices[shape];

        
        int maxExpandWidth = 0;


        int maxExpandBreadth =0;

        switch(direction)
        {
            case Directions.Front:
                maxExpandBreadth = (int)Vector3.Dot(originalVertices[0]- shape.vertices[0],transform.forward)/perUnitMultiplier;
                break;
            case Directions.Backward:
                maxExpandBreadth = (int)Vector3.Dot(originalVertices[3] - shape.vertices[3], -transform.forward) / perUnitMultiplier;
                break;
            case Directions.Left:
                maxExpandWidth = (int)Vector3.Dot(originalVertices[1] - shape.vertices[1], -transform.right) / perUnitMultiplier;
                break;
            case Directions.Right:
                maxExpandWidth = (int)Vector3.Dot(originalVertices[0] - shape.vertices[0], transform.right) / perUnitMultiplier;
                break;
        }


        ExpandWidth = maxExpandWidth ;
        ExpandBreadth = maxExpandBreadth ;
    }
}



public enum Directions
{
    Front,
    Left,
    Backward,
    Right

}


public enum EntropismBuildingMesh
{
    Door,
    Window,
    Wall,
    Balcony,
    Roof,
    UnderRoof,
    RoofAd

}

public enum EntropismBuildingOpt
{
    NoBuild = -1,
    Base,
    Level,
    Roof,
}

public struct PrimitiveShape2D
{
    Vector3 Position;

    int width;
    int breadth;

}


