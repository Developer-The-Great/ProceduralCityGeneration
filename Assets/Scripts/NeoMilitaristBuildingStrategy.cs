using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class NeoMilitaristBuildingStrategy : MonoBehaviour
{
    const float ShrinkChance = 0.7f;
    const float SideTrapeziumChance = 0.7f;
    const float AdChance = 0.8f;
    const float CompanySignChance = 0.6f;

    public static void generateNeoMilitaristBuilding(GameObject[] meshChoices, Transform transform,
        BuildArea2D BuildArea, Shape2D InitShape, int seed,int minLevel,int maxLevel,ref int CurrentBuildingLevel,bool hasAds,bool hasCompanySigns)
    {
        CurrentBuildingLevel = 0;
        Random.InitState(seed);

        //pick a random building height(based on range)
        int BuildingHeight = Random.Range(minLevel, maxLevel + 1);

        

        float currentShrinkDecision = Random.Range(0, 1.0f);
        float currentSideTrapeziumDecision = Random.Range(0, 1.0f);

        int ExtrusionHeight = -1;

        //picks a level where the building shrinks, if the building does not shrink, shrinkLevel is set to -1
        int ShrinkLevel = currentShrinkDecision > (1.0f - ShrinkChance) ?
            Mathf.RoundToInt(Random.Range(BuildingHeight * 0.3f, BuildingHeight * 0.5f)) :
            -1;

        //decides if the building has a side extrusion
        bool hasSideExtrusion = currentSideTrapeziumDecision > (1.0f - SideTrapeziumChance) && BuildArea.Width >= (BuildArea2D.minimumWidth + 2);

        if (hasSideExtrusion)
        {
            generateShape2DExtrusion(transform, BuildArea, InitShape, BuildingHeight, ShrinkLevel,ref ExtrusionHeight);
        }

        List<Shape2D> shapesFound = new List<Shape2D>();

        int meshVersionWindow = Random.Range(0, meshChoices[(int)NeoMilitarismMesh.Windows].GetComponent<MeshVersions>().GetCount());

        //while building is not at requested building height
        while (CurrentBuildingLevel < BuildingHeight)
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
                switch ((NeoMilitBuildingOpt)shape.Identifier)
                {
                    case NeoMilitBuildingOpt.NormalLevel:
                        generateLevel(meshChoices, shape, transform, CurrentBuildingLevel, ShrinkLevel, BuildArea.perUnitMultiplier, meshVersionWindow);
                        shape.Identifier = -1;
                        break;

                    case NeoMilitBuildingOpt.ShrinkLevel:
                        generateShrinkLevel(shape, BuildArea.perUnitMultiplier, meshChoices, transform);
                        shape.Identifier = -1;
                        break;

                    case NeoMilitBuildingOpt.SideExtrusion:
                        generateSideExtrusions(meshChoices, shape, transform, CurrentBuildingLevel, ExtrusionHeight, BuildArea.perUnitMultiplier);
                        shape.Identifier = -1;
                        break;

                    default:
                        break;
                }
                Destroy(shape.gameObject);

            }
            shapesFound.Clear();
            CurrentBuildingLevel++;
        }

        //---------------Generate Roof----------------//
        bool isShrinkerBuilding = ShrinkLevel != -1;


        //set start point
        Vector3 startPosition = isShrinkerBuilding? InitShape.vertices[0] - transform.right * BuildArea.perUnitMultiplier : InitShape.vertices[0];


        //quick fix, real problem is that buildings with side extrusion are one unit higher
        int heightMultiplier = isShrinkerBuilding ? (BuildingHeight + 1) : BuildingHeight;


        
        startPosition.y =  BuildArea.perUnitMultiplier * heightMultiplier;


        bool hasCornerRoofs = hasAds;
        int meshVersion = -1;

        if (hasCornerRoofs)
        {
            meshVersion = Random.Range(0, 2);
        }


        int width = BuildArea.Width;
        int height = BuildArea.Breadth;

        if(isShrinkerBuilding) { width -= 2; }
        if(hasSideExtrusion) { width -= 2; }

        int YOffset = 0;
        for(int j = 0; j < height; j++)
        {
            int XOffset = 0;
            Vector3 forwardShift = transform.forward * YOffset * BuildArea.perUnitMultiplier;


            for (int i = 0; i < width; i++)
            {
                Vector3 rightShift = transform.right * XOffset * BuildArea.perUnitMultiplier;
                Vector3 position = startPosition - rightShift - forwardShift;

                NeoMilitarismMesh roof = meshVersion == 0 && (i ==0 || i == width-1) && (j == 0 || j == height-1)? NeoMilitarismMesh.CornerRoof : NeoMilitarismMesh.Roof;

                Quaternion lookRot = transform.rotation;
                //hasAds indicate that this is a NeoMilitaristCopy for neo kitsch
                if(hasAds)
                {
                    if(j == 0 && i == width - 1)
                    {
                        lookRot = Quaternion.LookRotation(-transform.right);
                        position += -transform.right * BuildArea.perUnitMultiplier;
                    }

                    if(i == width - 1 && j != 0 && j < height-1)
                    {
                        lookRot = Quaternion.LookRotation(-transform.right);
                        position += -transform.right * BuildArea.perUnitMultiplier;
                    }

                    if(i == width - 1 && j == height -1)
                    {
                        lookRot = Quaternion.LookRotation(-transform.forward);
                        position += -transform.right * BuildArea.perUnitMultiplier + -transform.forward * BuildArea.perUnitMultiplier;

                    }

                    if(j == height - 1 && i < width -1 && i > 0)
                    {
                        lookRot = Quaternion.LookRotation(-transform.forward);
                        position += -transform.right * BuildArea.perUnitMultiplier + -transform.forward * BuildArea.perUnitMultiplier;
                    }
                    if(j == height - 1 && i ==0)
                    {
                        lookRot = Quaternion.LookRotation(transform.right);
                        position += -transform.forward * BuildArea.perUnitMultiplier;
                    }
                    if(i == 0 && j < height -1 && j > 0)
                    {
                        lookRot = Quaternion.LookRotation(transform.right);
                        position += -transform.forward * BuildArea.perUnitMultiplier;
                    }

                }


                
                Utils.SpawnMesh(transform, meshChoices, (int)roof, position, lookRot, meshVersion);

                XOffset++;
            }

            YOffset++;
        }
        
        if(hasAds && Random.Range(0,1.0f) < AdChance)
        {

            GameObject Ad = meshChoices[(int)NeoMilitarismMesh.BuildingAd];

            Shape2D TopShape = Utils.GetTopShape2DInTransform(transform);

            Vector3 Direction = Utils.GetVertexDirectionInPolygon(0, TopShape.vertices);

            //find how many units are needed to cover face
            float distance = Vector3.Distance(Vector3.zero, Direction);
            int UnitBuildingCount = (Mathf.RoundToInt(distance) /  BuildArea.perUnitMultiplier);

            Direction.Normalize();
            Vector3 normal = Utils.Get2DNormal(Direction);
            Quaternion LookRot = Quaternion.LookRotation(normal, Vector3.up);

            int buildingWorldHeight = BuildingHeight * BuildArea.perUnitMultiplier;


            //find add thats suited for building
            

            Vector2[] adSize = Ad.GetComponent<AdSize>().AddSizes;

            int smallestDifference = 2;
            Vector3 AddStart = Vector3.zero;
            int UnUsedWidth = UnitBuildingCount;
            int UsedWidth = 0;

            Dictionary<int, bool> AdToAlreadyUsed = new Dictionary<int, bool>();

            for (int i = 0; i < adSize.Length; i++)
            {
                AdToAlreadyUsed.Add(i, false);
            }

                while (UnUsedWidth > 0)
            {
                int AdWidthCover = Random.Range(UnUsedWidth/2, UnUsedWidth + 1);

                UnUsedWidth -= AdWidthCover;
                

                Dictionary<int, float> SuitableAdToMultiplier = new Dictionary<int, float>();

                for (int i = 0; i < adSize.Length; i++)
                {
                    int difference = AdWidthCover - (int)adSize[i].x;

                    if (difference < smallestDifference)
                    {
                        //check if its suitable
                        float multiplier = difference> 0 ? adSize[i].x / AdWidthCover:AdWidthCover / adSize[i].x   ;

                        float newHeight = (int)adSize[i].y * multiplier;


                        if (newHeight < BuildingHeight && !AdToAlreadyUsed[i])
                        {
                            SuitableAdToMultiplier.Add(i, multiplier);
                            AdToAlreadyUsed[i] = true;
                        }

                    }



                }

                if (SuitableAdToMultiplier.Keys.Count > 0)
                {
                    
                    int[] arrayOfAllKeys = new int[SuitableAdToMultiplier.Keys.Count];

                    SuitableAdToMultiplier.Keys.CopyTo(arrayOfAllKeys, 0);

                    AddStart = TopShape.vertices[0] + UsedWidth * BuildArea.perUnitMultiplier * Direction;

                    int keyIndex = Random.Range(0, arrayOfAllKeys.Length);
                    int chosenKey = arrayOfAllKeys[keyIndex];
                    float multiplier = SuitableAdToMultiplier[chosenKey];

                    Vector3 position = AddStart + Vector3.down * (adSize[chosenKey].y * multiplier )* BuildArea.perUnitMultiplier;


                    GameObject placedAdd = Utils.SpawnMesh(transform, meshChoices, (int)NeoMilitarismMesh.BuildingAd, position, LookRot, chosenKey);
                    
                    placedAdd.transform.localScale = new Vector3(multiplier, multiplier, 1);

                    UsedWidth += AdWidthCover;

                }
                else { break; }

            }


           




        }


        if(hasCompanySigns && Random.Range(0,1.0f) < CompanySignChance)
        {
            Shape2D topShape = Utils.GetTopShape2DInTransform(transform);

            Debug.Log("(meshChoices.Length " + meshChoices.Length);
            int meshVersionsCount = meshChoices[(int)NeoMilitarismMesh.RoofSign].GetComponent<MeshVersions>().GetCount();

            for (int i = 0; i < topShape.vertices.Length; i++)
            {
                

                Vector3 Direction = Utils.GetVertexDirectionInPolygon(i, topShape.vertices);

                //find how many units are needed to cover face

                float distance = Vector3.Distance(Vector3.zero, Direction);
                int UnitBuildingCount = (Mathf.RoundToInt(distance) / BuildArea.perUnitMultiplier);

                if ( UnitBuildingCount <= 4) { continue; }

                Direction.Normalize();
                Vector3 normal = Utils.Get2DNormal(Direction);
                Quaternion LookRot = Quaternion.LookRotation(normal, Vector3.up);

                float roofAddDecision = Random.Range(0, 1.0f);

                if (roofAddDecision < CompanySignChance)
                {

                    Vector3 position = topShape.vertices[i];

                    Utils.SpawnMesh(transform, meshChoices, (int)NeoMilitarismMesh.RoofSign, position, LookRot, Random.Range(0, meshVersionsCount));

                }

            }

        }




    }

    static void generateLevel(GameObject[] meshChoices,Shape2D shape,Transform transform, int CurrentBuildingLevel, int ShrinkLevel,int perUnitMultiplier,int meshVersionWIndow)
    {
        GameObject Level = new GameObject("Floor " + CurrentBuildingLevel);

        Level.transform.position = new Vector3(transform.position.x, shape.vertices[0].y, transform.position.z);
        Level.transform.parent = transform;

        bool isShrinkLevel = CurrentBuildingLevel == ShrinkLevel;



        for (int i = 0; i < shape.vertices.Length; i++)
        {
            //
            NeoMilitarismMesh MeshChoice;

            if (i % 2 == 0)
            {
                MeshChoice = NeoMilitarismMesh.Windows;

            }
            else
            {
                MeshChoice = NeoMilitarismMesh.Concrete;
            }

            //get build direction

            Vector3 Direction = Utils.GetVertexDirectionInPolygon(i, shape.vertices);

            //find how many units are needed to cover face

            float distance = Vector3.Distance(Vector3.zero, Direction);
            int UnitBuildingCount = (Mathf.RoundToInt(distance) / perUnitMultiplier);

            Direction.Normalize();
            Vector3 normal = Utils.Get2DNormal(Direction);
            Quaternion LookRot = Quaternion.LookRotation(normal, Vector3.up);

            for (int j = 0; j < UnitBuildingCount; j++)
            {
                int meshVersion = MeshChoice == NeoMilitarismMesh.Windows ?  meshVersionWIndow : -1;
                GameObject newMesh = Utils.SpawnMesh(transform, meshChoices,(int)MeshChoice, shape.vertices[i] + Direction * (j + 1) * perUnitMultiplier, LookRot, meshVersion);
                newMesh.transform.parent = Level.transform;
            }

        }


        Vector3[] newVertices = shape.vertices;
        int mult = 1;

        if (isShrinkLevel)
        {
            Vector3[] shrinkVertices = new Vector3[shape.vertices.Length];
            for (int i = 0; i < shape.vertices.Length; i++)
            {
                shrinkVertices[i] += shape.vertices[i] + new Vector3(0, (perUnitMultiplier), 0);
            }

            Utils.SpawnShape(transform,shrinkVertices, shrinkVertices[0], (int)NeoMilitBuildingOpt.ShrinkLevel);

            mult *= 2;

            Vector3 shrinkAdjustment = transform.right * perUnitMultiplier;
            //
            newVertices[0] -= shrinkAdjustment;
            newVertices[3] -= shrinkAdjustment;

            newVertices[1] += shrinkAdjustment;
            newVertices[2] += shrinkAdjustment;




        }


        newVertices = Utils.RaiseVerticesInY(newVertices, perUnitMultiplier * mult);

        Utils.SpawnShape(transform,newVertices, newVertices[0], (int)NeoMilitBuildingOpt.NormalLevel);


    }

    static void generateShrinkLevel(Shape2D shape,int perUnitMultiplier,GameObject[] meshChoices,Transform transform)
    {
        GameObject Level = new GameObject("ShrinkFloor ");
        Level.transform.parent = transform;
        NeoMilitarismMesh MeshChoice = NeoMilitarismMesh.Concrete;

        for (int i = 0; i < shape.vertices.Length; i++)
        {

            Vector3 Direction = Utils.GetVertexDirectionInPolygon(i, shape.vertices);

            float distance = Vector3.Distance(Vector3.zero, Direction);
            int UnitBuildingCount = (Mathf.RoundToInt(distance) / perUnitMultiplier);

            Direction.Normalize();
            Vector3 normal = Utils.Get2DNormal(Direction);
            Quaternion LookRot = Quaternion.LookRotation(normal, Vector3.up);

            if (i % 2 == 0)
            {
                for (int j = 0; j < UnitBuildingCount; j++)
                {
                    Vector3 meshPosition = shape.vertices[i] + Direction * (j + 1) * perUnitMultiplier;
                    if (j == 0)
                    {
                        GameObject newTriangleMesh = Utils.SpawnMesh(transform,meshChoices,(int)NeoMilitarismMesh.TriangularSide, meshPosition - Direction * perUnitMultiplier, LookRot);
                        newTriangleMesh.transform.Rotate(new Vector3(0, 180, 0));
                        newTriangleMesh.transform.parent = Level.transform;
                        continue;
                    }
                    else if (j == UnitBuildingCount - 1)
                    {
                        Quaternion LastTriangleLookRot = LookRot;
                        GameObject newTriangleMesh = Utils.SpawnMesh(transform, meshChoices, (int)NeoMilitarismMesh.TriangularSide, meshPosition, LastTriangleLookRot);
                        newTriangleMesh.transform.parent = Level.transform;
                        continue;
                    }
                    GameObject newMesh =Utils.SpawnMesh(transform, meshChoices, (int)MeshChoice, meshPosition, LookRot);
                    newMesh.transform.parent = Level.transform;
                }
            }
            else
            {

                for (int j = 0; j < UnitBuildingCount; j++)
                {

                    Vector3 meshPosition = shape.vertices[i] + Direction * (j + 1) * perUnitMultiplier;
                    GameObject newMesh = Utils.SpawnMesh(transform, meshChoices, (int)MeshChoice, meshPosition, LookRot);
                    newMesh.transform.Rotate(new Vector3(-45, 0, 0));
                    newMesh.transform.localScale = new Vector3(1, Mathf.Sqrt(2), 1);
                    newMesh.transform.parent = Level.transform;


                }
            }

        }
        Vector3[] newVertices = shape.vertices;
         newVertices = Utils.RaiseVerticesInY(newVertices, perUnitMultiplier);


        Vector3 shrinkDirection = perUnitMultiplier * transform.right;
        newVertices[0] -= shrinkDirection;
        newVertices[1] += shrinkDirection;
        newVertices[2] += shrinkDirection;
        newVertices[3] -= shrinkDirection;

        Utils.SpawnShape(transform, newVertices, newVertices[0], (int)NeoMilitBuildingOpt.NormalLevel);
        
    }

    static void generateSideExtrusions(GameObject[] meshChoices, Shape2D shape,Transform transform, int currentBuildLevel, int maxExtrusionHeight,int perUnitMultiplier)
    {

        GameObject Level = new GameObject("Extrusion ");
        Level.transform.parent = transform;

        NeoMilitarismMesh MeshChoice = NeoMilitarismMesh.Concrete;

        for (int i = 0; i < shape.vertices.Length; i++)
        {
            Vector3 Direction = Utils.GetVertexDirectionInPolygon(i, shape.vertices);
            float length = Direction.magnitude;

            Direction.Normalize();
            Vector3 normal = Utils.Get2DNormal(Direction);
            Quaternion LookRot = Quaternion.LookRotation(normal, Vector3.up);



            if (i == 0 || i == (shape.vertices.Length - 2))
            {

                GameObject newMesh = Utils.SpawnMesh(transform,meshChoices,(int)MeshChoice, shape.vertices[i] + Direction * length, LookRot);
                newMesh.transform.localScale = new Vector3(length / perUnitMultiplier, 1, 1);
                newMesh.transform.parent = Level.transform;
            }
            else if (i == (shape.vertices.Length - 1))
            {
                continue;
            }
            else
            {
                int UnitBuildingCount = (Mathf.RoundToInt(length) / perUnitMultiplier);
                for (int j = 0; j < UnitBuildingCount; j++)
                {
                    //int offset;

                    //if(j ==0 || j == UnitBuildingCount-1)
                    //{
                    //    offset = j;
                    //}
                    //else
                    //{
                    //    offset = j + 1;
                    //}

                    GameObject newMesh = Utils.SpawnMesh(transform,meshChoices,(int)MeshChoice, shape.vertices[i] + Direction * (j + 1) * perUnitMultiplier, LookRot);
                    newMesh.transform.parent = Level.transform;
                }
            }

        }

        Vector3[] newVertices = Utils.RaiseVerticesInY(shape.vertices, perUnitMultiplier);
        if (currentBuildLevel < maxExtrusionHeight)
        {

            Utils.SpawnShape(transform,newVertices, newVertices[0], (int)NeoMilitBuildingOpt.SideExtrusion);

        }
        else if (currentBuildLevel == maxExtrusionHeight)
        {
            NeoMilitaristBuildingStrategy.generateRoofExtrusion(transform,meshChoices, newVertices, Level.transform,perUnitMultiplier);
        }
    }

    static void generateRoofExtrusion(Transform transform, GameObject[] meshChoices,Vector3[] vertices, Transform transformToAttach,int perUnitMultiplier)
    {
        Vector3 Direction = Utils.GetVertexDirectionInPolygon(1, vertices);
        float length = Direction.magnitude;
        int UnitBuildingCount = (Mathf.RoundToInt(length) / perUnitMultiplier);

        Direction.Normalize();
        Vector3 normal = Utils.Get2DNormal(Direction);
        Quaternion LookRot = Quaternion.LookRotation(normal, Vector3.up);



        for (int j = 0; j < UnitBuildingCount; j++)
        {


            GameObject newMesh = Utils.SpawnMesh(transform,meshChoices,(int)NeoMilitarismMesh.Concrete, vertices[1] + Direction * (j + 1) *perUnitMultiplier, LookRot);
            newMesh.transform.Rotate(new Vector3(-45, 0, 0));
            newMesh.transform.parent = transformToAttach;

        }


        Vector3 FirstTriDirection = Utils.GetVertexDirectionInPolygon(0, vertices);
        float FirstTriLength = FirstTriDirection.magnitude;

        FirstTriDirection.Normalize();
        Vector3 FirstTrinormal = new Vector3(-FirstTriDirection.z, 0, FirstTriDirection.x);
        Quaternion FirstTriLookRot = Quaternion.LookRotation(FirstTrinormal, Vector3.up);
        GameObject Firsttriangle = Utils.SpawnMesh(transform,meshChoices,(int)NeoMilitarismMesh.TriangularSide, vertices[0] + FirstTriDirection * FirstTriLength, FirstTriLookRot);
        Firsttriangle.transform.Rotate(new Vector3(0, -180, 0)); //TODO: FIX THIS HACKY SHIT


        /////////

        Vector3 SecondTriDirection = Utils.GetVertexDirectionInPolygon(vertices.Length - 2, vertices);
        float SecondTriLength = FirstTriDirection.magnitude;

        SecondTriDirection.Normalize();
        Vector3 SecondTrinormal = new Vector3(-Direction.z, 0, Direction.x);
        Quaternion SecondTriLookRot = Quaternion.LookRotation(SecondTrinormal, Vector3.up);
        GameObject Secondtriangle = Utils.SpawnMesh(transform,meshChoices,(int)NeoMilitarismMesh.TriangularSide, vertices[vertices.Length - 2], SecondTriLookRot);
        Secondtriangle.transform.Rotate(new Vector3(0, -90, 0)); //TODO: FIX THIS HACKY SHIT


    }

    static void generateShape2DExtrusion(Transform transform, BuildArea2D BuildArea, Shape2D InitShape, int BuildingHeight,int ShrinkLevel,ref int ExtrusionHeight)
    {
        //shrink width of initShape by 1
        Vector3 shrinkDirection = BuildArea.perUnitMultiplier * transform.right;
        float ZFightingFix = 0.01f;

        //find maxExtrusionHeight
        ExtrusionHeight = ShrinkLevel == -1 ?
            Mathf.RoundToInt(Random.Range(1, BuildingHeight * 0.8f)) :
            Mathf.RoundToInt(Random.Range(1, ShrinkLevel));


        InitShape.vertices[0] -= shrinkDirection;
        InitShape.vertices[1] += shrinkDirection;
        InitShape.vertices[2] += shrinkDirection;
        InitShape.vertices[3] -= shrinkDirection;

        //create shape2D for the side extrusions
        Vector3[] leftExtrusion = new Vector3[4];

        int breadthUnitLength = Random.Range(3, BuildArea.Breadth);//TODO make it possible to have an extrusion with the size breadth
        


        Vector3 BreadthDirection = breadthUnitLength * BuildArea.perUnitMultiplier * transform.forward;

        Vector3 newWidthDirection = (BuildArea.Width - 2) * BuildArea.perUnitMultiplier * transform.right;

        Vector3 extrusionWidth = (BuildArea.perUnitMultiplier * transform.right) * Random.Range(0.5f, 0.9f);
        //LEFT EXTRUSION vertices

        //1-----------0
        //|-----------|
        //|-----------|
        //|-----------|
        //|-----------|
        //2-----------3

        leftExtrusion[0] = transform.position + (BreadthDirection / 2.0f) - (newWidthDirection / 2.0f);
        leftExtrusion[1] = leftExtrusion[0] - extrusionWidth;
        leftExtrusion[2] = leftExtrusion[1] - BreadthDirection;
        leftExtrusion[3] = leftExtrusion[2] + extrusionWidth;

        Utils.SpawnShape(transform, leftExtrusion, transform.position, (int)NeoMilitBuildingOpt.SideExtrusion);
        //RIGHT EXTRUSION vertices
        //3-----------2
        //|-----------|
        //|-----------|
        //|-----------|
        //|-----------|
        //0-----------1

        Vector3[] rightExtrusion = new Vector3[4];
        rightExtrusion[0] = transform.position - (BreadthDirection / 2.0f) + (newWidthDirection / 2.0f);
        rightExtrusion[1] = rightExtrusion[0] + extrusionWidth;
        rightExtrusion[2] = rightExtrusion[1] + BreadthDirection;
        rightExtrusion[3] = rightExtrusion[2] - extrusionWidth;

        Utils.SpawnShape(transform, rightExtrusion, transform.position, (int)NeoMilitBuildingOpt.SideExtrusion);

    }

}

public enum NeoMilitarismMesh
{
    Concrete,
    Windows,
    TriangularSide,
    SideTrapezoidalPrism,
    Roof,
    BuildingAd,
    RoofSign,
    CornerRoof
}

public enum NeoMilitBuildingOpt
{
    NormalLevel = 1,
    ShrinkLevel,
    SideExtrusion

}

