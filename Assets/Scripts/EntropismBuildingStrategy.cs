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

    /// <summary>
    /// Procedurally Generates a building with the "Entropism" architectural style.
    /// </summary>
    /// <param name="meshChoices"> An array of GameObjects that represent the set of meshes that will be procedurally placed to create the buildings</param>
    /// <param name="transform"> A Transform containing the position,rotation,and scale of the building</param>
    /// <param name="BuildArea"> A BuildArea2D which states the maximum space that can be used to build the building</param>
    /// <param name="InitShape"> </param>
    /// <param name="seed"> an integer representing the seed that will be used to procedurally generate the building</param>
    /// <param name="minLevel"> an integer representing the minimal number of floors that the building will have</param>
    /// <param name="maxLevel"> an integer representing the maximum number of floors that the building will have</param>
    /// <param name="CurrentBuildingLevel"></param>
    public static void generateEntropismBuilding(GameObject[] meshChoices, Transform transform, BuildArea2D BuildArea, 
        Shape2D InitShape, int seed, int minLevel, int maxLevel, ref int CurrentBuildingLevel)
    {

        Random.InitState(seed);
        int BuildingHeight = Random.Range(minLevel, maxLevel + 1);
        CurrentBuildingLevel = 0;

        BSPDivideShape2D(InitShape,  BuildArea,transform);

        

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
                        GenerateBaseLevel(shape, transform,BuildArea.perUnitMultiplier, meshChoices);
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

    /// <summary>
    /// Generates the base floor of the entropism styled buildings
    /// </summary>
    /// <param name="shape"> The initial Shape2D that indicates the initial size of the floor</param>
    /// <param name="transform"> The Transform that will be used to dictate the position,rotation,and scale of the buildings</param>
    /// <param name="perUnitMultiplier"> the length of the "blocks" used to create the mesh</param>
    /// <param name="MeshChoices"> An Array of GameObjects that will be procedurally placed to create the base floor</param>
    static void GenerateBaseLevel(Shape2D shape, Transform transform,  int perUnitMultiplier, GameObject[] MeshChoices)
    {
        if(shape.Identifier == (int)EntropismBuildingOpt.Roof)
        {
            shape.Identifier = (int)EntropismBuildingOpt.NoBuild;
            return;
        }


        GameObject Level = new GameObject("BaseFloor ");

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

    /// <summary>
    /// Given a "Building Block type" expressed as as an EntropismBuildingMesh, randomly select a prefab from the
    /// array of prefabs stored in MeshVersion
    /// </summary>
    /// <param name="meshversion"> an integer out parameter, stores the resulting index of from the meshVersion array</param>
    /// <param name="BuildingMesh"> an EntropismBuildingMesh that states the type of building block requested (Is it a wall,window,door,etc)</param>
    /// <param name="meshChoices"> an array of GameObjects that contain a MeshVersion component that stores the alternative prefabs that can be used</param>
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

    /// <summary>
    /// Generates the floors after the base of floor of an Entropism styled building
    /// </summary>
    /// <param name="shape"> The initial Shape2D that indicates the initial size of the floor </param>
    /// <param name="transform"> The Transform that will be used to dictate the position,rotation,and scale of the buildings </param>
    /// <param name="CurrentBuildingLevel"> an integer that reperesents which floor this floor is at (first floor,second floor,etc)</param>
    /// <param name="MaxBuildingHeight"> an integer that the maximum floors that is allowed for this building</param>
    /// <param name="perUnitMultiplier">the length of the "blocks" used to create the mesh</param>
    /// <param name="MeshChoices">An Array of GameObjects that will be procedurally placed to create the base floor</param>
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

    /// <summary>
    /// Divides the initShape using binary space partioning and parents it to thet transform
    /// </summary>
    /// <param name="initShape"> The initial Shape2D that will be divided </param>
    /// <param name="BuildArea"> A BuildArea2D that stores the inforamtion necessary to divide the Shape2D</param>
    /// <param name="transform"> A transform that will contain the divided Shape2Ds</param>
    static void BSPDivideShape2D(Shape2D initShape,BuildArea2D BuildArea,Transform transform)
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

    /// <summary>
    /// Slices a Shape2D into 2 based on the given parameters
    /// </summary>
    /// <param name="shape"> the Shape2D that will be sliced </param>
    /// <param name="isHorizontalSlice"> a boolean that indicates if the Shape2D will be sliced horizontally or vertically </param>
    /// <param name="perUnitMultiplier"> the length of the "blocks" used to create the mesh</param>
    /// <param name="newUnitLength"> the length/width(depending on if it was cut horizontally or vertically)of the new Shape2D</param>
    /// <param name="transform"> the Transform that is parented to the Shape2D</param>
    /// <param name="otherShape"> an out parameter shape2D that will store the Shape2D created after the cut</param>
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

    /// <summary>
    /// Attempt to resize the Shape2D. Resizing will only happen if it passes the random number generator's check and if 
    /// resizing will still upholding the size requirements
    /// set for the Shape2D in question
    /// </summary>
    /// <param name="transform"> The Shape2D's parent transform </param>
    /// <param name="shape"> The Shape2D that we are attempting to resize </param>
    /// <param name="ShapeToOriginalVertices"> A Dictionary containing a Shape2D and its original vertices before resizing</param>
    /// <param name="buildArea"> The BuildArea indicating the maximum space that this building can use</param>
    /// <param name="expanded"> A boolean out parameter indicating whether the Shape2D was shrunk or expanded </param>
    /// <returns></returns>
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

    /// <summary>
    /// Attempt to shrink the Shape2D. Resizing will only happen if it passes the random number generator's check and if shrinking will still upholding the size requirements
    /// set for the Shape2D in question
    /// </summary>
    /// <param name="transform"> The Shape2D's parent transform </param>
    /// <param name="shape">The Shape2D that we are attempting to resize</param>
    /// <param name="ShapeToOriginalVertices">  A Dictionary containing a Shape2D and its original vertices before resizing</param>
    /// <param name="buildArea">The BuildArea indicating the maximum space that this building can use</param>
    /// <returns> Returns true if the Shape2D was shrunk</returns>
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

    /// <summary>
    /// Shrink the Shape2D. The amount it is shrunk is based on random number generation but still considers the requirements it needs to uphold
    /// </summary>
    /// <param name="shape"> The shape that will be shrunk</param>
    /// <param name="buildArea"> The build area of the Building to be generated</param>
    /// <param name="transform"> The Shape2D's parent transform </param>
    /// <returns></returns>
    static bool AttemptShrink(Shape2D shape,BuildArea2D buildArea,Transform transform)
    {
        int maxShrinkWidth;
        int maxShrinkBreadth;

        bool isResized = false;

        GetShapeShrinkTolerance(shape, buildArea.perUnitMultiplier, out maxShrinkWidth, out maxShrinkBreadth);

        if (maxShrinkWidth > 0)
        {
            int resize = Random.Range(0, maxShrinkWidth);
            Utils.ResizeSquareShape2DOnRight(shape, resize, -transform.right, buildArea.perUnitMultiplier);
            if (resize > 0) { isResized = true; }
        }

        GetShapeShrinkTolerance(shape, buildArea.perUnitMultiplier, out maxShrinkWidth, out maxShrinkBreadth);

        if (maxShrinkWidth > 0)
        {
            int resize = Random.Range(0, maxShrinkWidth);
            Utils.ResizeSquareShape2DOnLeft(shape, resize, transform.right, buildArea.perUnitMultiplier);
            if (resize > 0) { isResized = true; }
        }

        GetShapeShrinkTolerance(shape, buildArea.perUnitMultiplier, out maxShrinkWidth, out maxShrinkBreadth);

        if (maxShrinkBreadth > 0)
        {
            int resize = Random.Range(0, maxShrinkBreadth);
            Utils.ResizeSquareShape2DOnFront(shape, resize, -transform.forward, buildArea.perUnitMultiplier);
            if (resize > 0) { isResized = true; }
        }

        GetShapeShrinkTolerance(shape, buildArea.perUnitMultiplier, out maxShrinkWidth, out maxShrinkBreadth);

        if (maxShrinkBreadth > 0)
        {
            int resize = Random.Range(0, maxShrinkBreadth);
            Utils.ResizeSquareShape2DOnBack(shape, resize, transform.forward, buildArea.perUnitMultiplier);
            if (resize > 0) { isResized = true; }
        }

        return isResized;
    }

    /// <summary>
    /// Expands the Shape2D. The amount it is expanded is based on random number generation but still considers the requirements it needs to uphold
    /// </summary>
    /// <param name="shape">The shape that will be expanded</param>
    /// <param name="buildArea">The build area of the Building to be generated </param>
    /// <param name="transform">The Shape2D's parent transform </param>
    /// <param name="ShapeToOriginalVertices">A Dictionary containing a Shape2D and its original vertices before resizing</param>
    /// <returns></returns>
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

    /// <summary>
    /// Find the maximum shrink size that a certain shape can have
    /// </summary>
    /// <param name="shape"> The Shape2D that we are examining </param>
    /// <param name="perUnitMultiplier"> </param>
    /// <param name="maxShrinkWidth"> an out parameter that will store the maximum width that the Shape2D can shrink to</param>
    /// <param name="maxShrinkBreadth">an out parameter that will store the maximum breadth that the Shape2D can shrink to</param>
    static void GetShapeShrinkTolerance(Shape2D shape, int perUnitMultiplier, out int maxShrinkWidth,out int maxShrinkBreadth)
    {
        int currentWidth = Utils.GetPerUnitLength(shape.vertices[0], shape.vertices[1], perUnitMultiplier);
        int currentBreadth = Utils.GetPerUnitLength(shape.vertices[0], shape.vertices[3], perUnitMultiplier);

        maxShrinkWidth = currentWidth - minWidth;
        maxShrinkBreadth = currentBreadth - minBreadth;
    }

    /// <summary>
    /// Find the maximum expand size that a certain shape can have
    /// </summary>
    /// <param name="shape">The Shape2D that we are examining </param>
    /// <param name="perUnitMultiplier"></param>
    /// <param name="ExpandWidth">an out parameter that will store the maximum width that the Shape2D can expand to</param>
    /// <param name="ExpandBreadth">an out parameter that will store the maximum breadth that the Shape2D can expand to</param>
    /// <param name="ShapeToOriginalVertices">A Dictionary containing a Shape2D and its original vertices before resizing</param>
    /// <param name="direction"> The direction we are expanding towards</param>
    /// <param name="transform"> The Shape2D's parent transform </param>
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


