using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KitschBuildingStrategy : MonoBehaviour
{
    const float enlargerChance = 0.5f;
    const float repeatCycleChance = 0.5f;
    const float AddChance = 0.1f;
    const float RoofAddChance = 0.02f;


    public static void generateKitschBuilding(GameObject[] meshChoices, Transform transform, BuildArea2D BuildArea, Shape2D InitShape, int seed, int minLevel, int maxLevel, ref int CurrentBuildingLevel)
    {

        Random.InitState(seed);

        Vector2 MinSize = new Vector2(0, 0);

        bool WidthIsEven = BuildArea.Width % 2 == 0;
        bool BreadthIsEven = BuildArea.Breadth % 2 == 0;
        MinSize.x = WidthIsEven ? 3 : 2;
        MinSize.y = BreadthIsEven ? 3 : 2;

        CurrentBuildingLevel = 0;
        //get height of building
        int BuildingHeight = Random.Range(minLevel, maxLevel + 1);

        //is the building a shrinker or an enlarger?
        bool isEnlarger = BuildingHeight > (minLevel+maxLevel)/2;


        //if its an enlarger
            //generate enlarger shape2D
        if(isEnlarger)
        {

            GenerateShrinkerShape2D(InitShape, BuildArea, transform);
            
        }
        else
        {

            GenerateEnlargerShape2D(InitShape, BuildArea);


        }

        

      



        Queue<KitschBuildingOpt> ResultingInstructions = GenerateBuildingInstructions(isEnlarger, BuildingHeight);





        List<Shape2D> shapesFound = new List<Shape2D>();

        int meshVersionIndex = Random.Range(0, 1);

        while (CurrentBuildingLevel <= BuildingHeight)
        {
            KitschBuildingOpt Instruction = ResultingInstructions.Dequeue();

            switch(Instruction)
            {
                case KitschBuildingOpt.Base:
                    GenerateBaseLevel(InitShape,transform,CurrentBuildingLevel,BuildArea.perUnitMultiplier,meshChoices,meshVersionIndex);
                    break;

                case KitschBuildingOpt.Level:
                    GenerateLevel(InitShape,transform,CurrentBuildingLevel,BuildArea.perUnitMultiplier,meshChoices, meshVersionIndex);
                    break;

                case KitschBuildingOpt.Roof:
                    //AttemptVerticesExpand(InitShape, BuildArea, transform);
                    GenerateTopRoof(InitShape, transform, CurrentBuildingLevel, BuildArea.perUnitMultiplier, meshChoices, BuildArea, meshVersionIndex);
                    break;

                case KitschBuildingOpt.Enlarger:
                    AttemptVerticesExpand(InitShape, BuildArea, transform);
                    //GenerateRoof(InitShape, transform, CurrentBuildingLevel, BuildArea.perUnitMultiplier, meshChoices, BuildArea);
                    GenerateTopRoof(InitShape, transform, CurrentBuildingLevel, BuildArea.perUnitMultiplier, meshChoices, BuildArea, meshVersionIndex);
                    GenerateLevel(InitShape, transform, CurrentBuildingLevel, BuildArea.perUnitMultiplier, meshChoices, meshVersionIndex);
                    break;

                case KitschBuildingOpt.Shrinker:
                    GenerateTopRoof(InitShape, transform, CurrentBuildingLevel, BuildArea.perUnitMultiplier, meshChoices, BuildArea, meshVersionIndex);
                    //GenerateRoof(InitShape, transform, CurrentBuildingLevel, BuildArea.perUnitMultiplier, meshChoices,BuildArea);
                    AttemptVerticesShrink(InitShape, BuildArea, MinSize, transform);
                    GenerateLevel(InitShape, transform, CurrentBuildingLevel, BuildArea.perUnitMultiplier, meshChoices, meshVersionIndex); 
                    break;
            }



            CurrentBuildingLevel++;
        }



        //-----ROOF TOP ADS-------//

        Utils.PlaceRoofObjects(transform, BuildArea.perUnitMultiplier, RoofAddChance, meshChoices, (int)KitschBuildingMesh.RoofAd, 1,-1);

        //List<Shape2D> leftoverShapes = new List<Shape2D>();

        //Utils.FindShape2DInTransform(transform, leftoverShapes, int.MinValue);

        //foreach (Shape2D shape in leftoverShapes)
        //{
        //    for (int i = 0; i < shape.vertices.Length; i++)
        //    {
        //        Vector3 Direction = Utils.GetVertexDirectionInPolygon(i, shape.vertices);

        //        //find how many units are needed to cover face

        //        float distance = Vector3.Distance(Vector3.zero, Direction);
        //        int UnitBuildingCount = (Mathf.RoundToInt(distance) / BuildArea.perUnitMultiplier);

        //        Direction.Normalize();
        //        Vector3 normal = Utils.Get2DNormal(Direction);
        //        Quaternion LookRot = Quaternion.LookRotation(normal, Vector3.up);

        //        float roofAddDecision = Random.Range(0, 1.0f);

        //        if(roofAddDecision < RoofAddChance)
        //        {
        //            int generateInt = Random.Range(0,UnitBuildingCount+1);
        //            Vector3 position = shape.vertices[i] + generateInt * Direction + normal * BuildArea.perUnitMultiplier;

        //            Utils.SpawnMesh(transform, meshChoices, (int)KitschBuildingMesh.RoofAd, position, LookRot);

        //        }

        //    }

        //}


    }

    

    static void GenerateEnlargerShape2D(Shape2D initShape, BuildArea2D buildArea)
    {
        Vector3[] vertices = new Vector3[4];

        vertices = buildArea.GetSquareVertices();

        initShape.Init(vertices, (int)KitschBuildingOpt.Base);
    }

    static void GenerateShrinkerShape2D(Shape2D initShape,BuildArea2D buildArea,Transform transform)
    {
        //check i
        bool widthIsOdd = buildArea.Width % 2 != 0;
        int perUnitMultiplier = buildArea.perUnitMultiplier;

        Vector3 startPosition = transform.position;

        int minWidth = widthIsOdd ? 3 : 2;
        int maxWidth = buildArea.Width-2;

        int minBreadth = 1;
        int maxBreadth = buildArea.Breadth-2;

        Debug.Log("minWidth " + minWidth);
        

        int width = Random.Range(minWidth, maxWidth + 1);
        int breadth = Random.Range(minBreadth, maxBreadth + 1);

        Vector3 halfRightDirection = width / 2.0f * transform.right * perUnitMultiplier;
        Vector3 halfForwardDirection = breadth / 2.0f * transform.forward * perUnitMultiplier;

        startPosition += halfRightDirection + halfForwardDirection;

        Vector3[] vertices = new Vector3[4];

        vertices[0] = startPosition;
        vertices[1] = startPosition - halfRightDirection * 2;
        vertices[2] = startPosition - halfRightDirection * 2 - halfForwardDirection * 2;
        vertices[3] = startPosition - halfForwardDirection * 2;


        initShape.Init(vertices,(int)KitschBuildingOpt.Base);




    }

    static Queue<KitschBuildingOpt> GenerateBuildingInstructions(bool isEnlarger,int BuildingHeight)
    {
        Queue<KitschBuildingOpt> ResultingInstructions = new Queue<KitschBuildingOpt>();
        ResultingInstructions.Clear();

        const float percentageThreshold = 0.35f;

        //add base
        ResultingInstructions.Enqueue(KitschBuildingOpt.Base);

        //decide if i would want one more level
        int i = 1;
        float currentPercentageThreshold = (float)i / BuildingHeight;

        while (currentPercentageThreshold < percentageThreshold)
        {
            
            ResultingInstructions.Enqueue(KitschBuildingOpt.Level);
            i++;
            currentPercentageThreshold = (float)i / BuildingHeight;
        }


        //if(enlarger)
        if (isEnlarger)
        {
            ResultingInstructions.Enqueue(KitschBuildingOpt.Enlarger);

        }
        else
        {
            
            ResultingInstructions.Enqueue(KitschBuildingOpt.Shrinker);
        }

        KitschBuildingOpt[] BuildChoices = { KitschBuildingOpt.Shrinker, KitschBuildingOpt.Enlarger, KitschBuildingOpt.Level };


        while(ResultingInstructions.Count < BuildingHeight)
        {
            int index = Random.Range(0, BuildChoices.Length);

            ResultingInstructions.Enqueue(BuildChoices[index]);
            
        }
        ResultingInstructions.Enqueue(KitschBuildingOpt.Roof);
        return ResultingInstructions;
    }

    static void GenerateBaseLevel(Shape2D shape,Transform transform,int CurrentBuildingLevel,int perUnitMultiplier,GameObject[] MeshChoices,int meshVersionIndex)
    {
        GameObject Level = new GameObject("BaseFloor " + CurrentBuildingLevel);

        Level.transform.position = new Vector3(transform.position.x, shape.vertices[0].y, transform.position.z);
        Level.transform.parent = transform;

        int[] choice = { (int)KitschBuildingMesh.Wall, (int)KitschBuildingMesh.Window };

        for (int i =0; i < shape.vertices.Length;i++)
        {
            KitschBuildingMesh MeshChoice = (KitschBuildingMesh)Utils.ChooseFromArray(choice);

            Vector3 Direction = Utils.GetVertexDirectionInPolygon(i, shape.vertices);

            //find how many units are needed to cover face

            float distance = Vector3.Distance(Vector3.zero, Direction);
            int UnitBuildingCount = (Mathf.RoundToInt(distance) / perUnitMultiplier);

            Direction.Normalize();
            Vector3 normal = Utils.Get2DNormal(Direction);
            Quaternion LookRot = Quaternion.LookRotation(normal, Vector3.up);

            for (int j = 0; j < UnitBuildingCount; j++)
            {
                if (j == 0)
                {
                    GameObject Corner = Utils.SpawnMesh(transform, MeshChoices, (int)KitschBuildingMesh.CornerWall, shape.vertices[i] + Direction * (j) * perUnitMultiplier, LookRot, meshVersionIndex);
                    Corner.transform.parent = Level.transform;
                }

                KitschBuildingMesh rawChoice = MeshChoice;

                if (i == 0 && j == UnitBuildingCount / 2)
                {

                    rawChoice = KitschBuildingMesh.Door;
                }

                GameObject newMesh = Utils.SpawnMesh(transform, MeshChoices, (int)rawChoice, shape.vertices[i] + Direction * (j + 1) * perUnitMultiplier, LookRot, meshVersionIndex);
                newMesh.transform.parent = Level.transform;
            }

        }

        shape.vertices = Utils.RaiseVerticesInY(shape.vertices, perUnitMultiplier);
    }

    static void GenerateLevel(Shape2D shape, Transform transform, int CurrentBuildingLevel, int perUnitMultiplier, GameObject[] MeshChoices,int meshVersionIndex)
    {
        GameObject Level = new GameObject("Floor " + CurrentBuildingLevel);

        Level.transform.position = new Vector3(transform.position.x, shape.vertices[0].y, transform.position.z);
        Level.transform.parent = transform;

        int[] choice = { (int)KitschBuildingMesh.Wall, (int)KitschBuildingMesh.Window };



        for (int i = 0; i < shape.vertices.Length; i++)
        {
            KitschBuildingMesh MeshChoice = (KitschBuildingMesh)Utils.ChooseFromArray(choice);

            Vector3 Direction = Utils.GetVertexDirectionInPolygon(i, shape.vertices);

            //find how many units are needed to cover face

            float distance = Vector3.Distance(Vector3.zero, Direction);
            int UnitBuildingCount = (Mathf.RoundToInt(distance) / perUnitMultiplier);

            Direction.Normalize();
            Vector3 normal = Utils.Get2DNormal(Direction);
            Quaternion LookRot = Quaternion.LookRotation(normal, Vector3.up);

            for (int j = 0; j < UnitBuildingCount; j++)
            {
                if(j == 0)
                {
                    GameObject Corner = Utils.SpawnMesh(transform, MeshChoices, (int)KitschBuildingMesh.CornerWall, shape.vertices[i] + Direction * (j ) * perUnitMultiplier, LookRot, meshVersionIndex);
                    AttemptPlaceAd( transform, MeshChoices, shape,i ,  normal, perUnitMultiplier,  LookRot,meshVersionIndex);
                    //Corner.transform.parent = transform;
                }
                GameObject newMesh = Utils.SpawnMesh(transform, MeshChoices, (int)MeshChoice, shape.vertices[i] + Direction * (j + 1) * perUnitMultiplier, LookRot, meshVersionIndex);
                //newMesh.transform.parent = transform;
            }

        }

        shape.vertices = Utils.RaiseVerticesInY(shape.vertices, perUnitMultiplier);
    }

    static void AttemptPlaceAd(Transform transform,GameObject[] MeshChoices,Shape2D shape,int i,Vector3 normal, int perUnitMultiplier,Quaternion LookRot,int meshVersion)
    {
        if(Random.Range(0,1.0f) < AddChance)
        {

            int max = MeshChoices[(int)KitschBuildingMesh.Ad].GetComponent<MeshVersions>().GetCount();
            int index = Mathf.Clamp(meshVersion, -1, max);

            if (meshVersion >0)
            {
                index = Random.Range(1, max);
            }

            GameObject Add = Utils.SpawnMesh(transform, MeshChoices, (int)KitschBuildingMesh.Ad, shape.vertices[i] + normal * (perUnitMultiplier), LookRot, index);
            Add.transform.parent = transform;
        }

        
    }
    static void GenerateTopRoof(Shape2D shape, Transform transform, int CurrentBuildingLevel, int perUnitMultiplier, GameObject[] MeshChoices, BuildArea2D BuildArea,int meshVersionIndex)
    {
        GenerateRoofsOnOuterShape(shape, transform, CurrentBuildingLevel, perUnitMultiplier,  MeshChoices, meshVersionIndex);
        GenerateRoof(shape, transform, CurrentBuildingLevel, perUnitMultiplier, MeshChoices, BuildArea, meshVersionIndex);
    }

    static void GenerateRoof(Shape2D shape, Transform transform, int CurrentBuildingLevel, int perUnitMultiplier, GameObject[] MeshChoices,BuildArea2D BuildArea, int meshVersionIndex)
    {
        GameObject Level = new GameObject("Roof " + CurrentBuildingLevel);

        Level.transform.position = new Vector3(transform.position.x, shape.vertices[0].y, transform.position.z);
        Level.transform.parent = transform;

        Vector3 startPosition = shape.vertices[0] - transform.forward * perUnitMultiplier - transform.right * perUnitMultiplier;

        int width = (int)Vector3.Distance(shape.vertices[0], shape.vertices[1]) / perUnitMultiplier;
        int height = (int)Vector3.Distance(shape.vertices[0], shape.vertices[3]) / perUnitMultiplier;

        int YOffset = 0;
        for (int j = 0; j < height; j++)
        {
            int XOffset = 0;
            Vector3 forwardShift = transform.forward * YOffset * BuildArea.perUnitMultiplier;


            for (int i = 0; i < width; i++)
            {
                Vector3 rightShift = transform.right * XOffset * BuildArea.perUnitMultiplier;
                Vector3 position = startPosition - rightShift - forwardShift;
                Utils.SpawnMesh(Level.transform, MeshChoices, (int)KitschBuildingMesh.Roof, position, transform.rotation);
                
                XOffset++;
            }

            YOffset++;
        }


        shape.vertices = Utils.RaiseVerticesInY(shape.vertices, 1);



    }

    static void GenerateRoofsOnOuterShape(Shape2D shape, Transform transform, int CurrentBuildingLevel, int perUnitMultiplier, GameObject[] MeshChoices, int meshVersionIndex)
    {
        GameObject Level = new GameObject("Roof " + CurrentBuildingLevel);

        Level.transform.position = new Vector3(transform.position.x, shape.vertices[0].y, transform.position.z);
        Level.transform.parent = transform;

        int cornerIndex = 0;

        for (int i = 0; i < shape.vertices.Length; i++)
        {
            KitschBuildingMesh MeshChoice = (KitschBuildingMesh.Roof);

            Vector3 Direction = Utils.GetVertexDirectionInPolygon(i, shape.vertices);

            //find how many units are needed to cover face

            float distance = Vector3.Distance(Vector3.zero, Direction);
            int UnitBuildingCount = (Mathf.RoundToInt(distance) / perUnitMultiplier);

            Direction.Normalize();
            Vector3 normal = Utils.Get2DNormal(Direction);
            Quaternion LookRot = Quaternion.LookRotation(normal, Vector3.up);

            for (int j = 0; j < UnitBuildingCount; j++)
            {

                if( j ==0)
                {
                    GameObject FirstCorner = Utils.SpawnMesh(transform, MeshChoices, (int)KitschBuildingMesh.CornerRoof, shape.vertices[i] , LookRot, meshVersionIndex);
                    

                    FirstCorner.transform.parent = Level.transform;
                    
                    cornerIndex++;
                }


                GameObject newMesh = Utils.SpawnMesh(transform, MeshChoices, (int)MeshChoice, shape.vertices[i] + Direction * (j + 1) * perUnitMultiplier, LookRot, meshVersionIndex);
                newMesh.transform.parent = Level.transform;
            }

        }

       
    }

    static void AttemptVerticesExpand(Shape2D shape,BuildArea2D BuildArea,Transform transform)
    {
        Vector3 startPosition = shape.vertices[0];
        Vector3 ToWidth = shape.vertices[1];
        Vector3 ToBreadth = shape.vertices[3];

        //get current width
        float currentWidth = Vector3.Distance(startPosition, ToWidth);

        //get current breadth
        float currentBreadth = Vector3.Distance(startPosition, ToBreadth);

        //get width that shape can expand
        float MaxWidth = (BuildArea.Width-2) * BuildArea.perUnitMultiplier;

        //get breadth that shape can expand 
        float MaxBreadth = (BuildArea.Breadth-2) * BuildArea.perUnitMultiplier;


        int maxWidthExpand = (int)((MaxWidth - currentWidth) / BuildArea.perUnitMultiplier);
        int maxBreadthExpand = (int)((MaxBreadth - currentBreadth) / BuildArea.perUnitMultiplier);





        Vector3[] vertices = shape.vertices;

        if(maxWidthExpand > 1)
        {
            
            int DecidedWidthExpansion = Random.Range(2, maxWidthExpand) /2;

            vertices = ResizeSquareShapeInRight(transform.right, vertices, DecidedWidthExpansion, BuildArea.perUnitMultiplier);

        }
        
        if(maxBreadthExpand > 1)
        {
      
            int DecidedBreadthExpansion = Random.Range(2, maxBreadthExpand) / 2;

            vertices = ResizeSquareShapeInForward(transform.forward, vertices, DecidedBreadthExpansion, BuildArea.perUnitMultiplier);
        }

        shape.vertices = vertices;

    }

    static void AttemptVerticesShrink(Shape2D shape,BuildArea2D BuildArea,Vector2 MinSize,Transform transform)
    {
        Vector3 startPosition = shape.vertices[0];
        Vector3 ToWidth = shape.vertices[1];
        Vector3 ToBreadth = shape.vertices[3];

        //get current width
        float currentWidth = Vector3.Distance(startPosition, ToWidth);

        //get current breadth
        float currentBreadth = Vector3.Distance(startPosition, ToBreadth);

        //get width that shape can expand
        float MinWidth = MinSize.x * BuildArea.perUnitMultiplier;

        //get breadth that shape can expand 
        float MinBreadth = MinSize.y * BuildArea.perUnitMultiplier;


        int maxWidthShrink = (int)((currentWidth - MinWidth) / BuildArea.perUnitMultiplier);
        int maxBreadthShrink = (int)((currentBreadth - MinBreadth) / BuildArea.perUnitMultiplier);

        Vector3[] vertices = shape.vertices;

        if (maxWidthShrink > 1)
        {

            int DecidedWidthExpansion = Random.Range(2, maxWidthShrink) / 2;

            vertices = ResizeSquareShapeInRight(-transform.right, vertices, DecidedWidthExpansion, BuildArea.perUnitMultiplier);
        }

        if(maxBreadthShrink > 1)
        {
            int DecidedBreadthExpansion = Random.Range(2, maxBreadthShrink) / 2;

            vertices = ResizeSquareShapeInForward(-transform.forward, vertices, DecidedBreadthExpansion,BuildArea.perUnitMultiplier);
        }

        shape.vertices = vertices;

    }


    static Vector3[] ResizeSquareShapeInForward(Vector3 forward,Vector3[] vertices, int UnitsExpand, int perUnitMultiplier)
    {
        Vector3[] ExpandResult = new Vector3[vertices.Length];

        ExpandResult = vertices;

        Vector3 directionNorm = forward.normalized;

        
        ExpandResult[0] += directionNorm * UnitsExpand * perUnitMultiplier;
        ExpandResult[1] += directionNorm * UnitsExpand * perUnitMultiplier;
        ExpandResult[2] -= directionNorm * UnitsExpand * perUnitMultiplier;
        ExpandResult[3] -= directionNorm * UnitsExpand * perUnitMultiplier;

        return ExpandResult;
    }


    static Vector3[] ResizeSquareShapeInRight(Vector3 right, Vector3[] vertices, int UnitsExpand, int perUnitMultiplier)
    {
        Vector3[] ExpandResult = new Vector3[vertices.Length];

        ExpandResult = vertices;

        Vector3 directionNorm = right.normalized;


        ExpandResult[0] += directionNorm * UnitsExpand * perUnitMultiplier;
        ExpandResult[3] += directionNorm * UnitsExpand * perUnitMultiplier;
        ExpandResult[1] -= directionNorm * UnitsExpand * perUnitMultiplier;
        ExpandResult[2] -= directionNorm * UnitsExpand * perUnitMultiplier;

        return ExpandResult;
    }


  
}

public enum KitschBuildingOpt
{
    Base,
    Level,
    Enlarger,
    Shrinker,
    Roof
}
public enum KitschBuildingMesh
{
    Door,
    Window,
    Wall,
    CornerRoof,
    Roof,
    CornerWall,
    Ad,
    RoofAd

}